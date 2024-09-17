//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using Xarial.XCad.Annotations;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Features;
using Xarial.XCad.Features.CustomFeature;
using Xarial.XCad.Features.CustomFeature.Attributes;
using Xarial.XCad.Features.CustomFeature.Enums;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Reflection;
using Xarial.XCad.Services;
using Xarial.XCad.SolidWorks.Annotations;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Features.CustomFeature.Exceptions;
using Xarial.XCad.SolidWorks.Features.CustomFeature.Toolkit;
using Xarial.XCad.SolidWorks.Geometry;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.Toolkit.Exceptions;
using Xarial.XCad.Utils.CustomFeature;
using Xarial.XCad.Utils.Reflection;
using System.Runtime.InteropServices;
using Xarial.XCad.Exceptions;
using Xarial.XCad.Features.CustomFeature.Structures;
using System.Globalization;

namespace Xarial.XCad.SolidWorks.Features.CustomFeature
{
    public interface ISwMacroFeature : ISwFeature, IXCustomFeature
    {
        new ISwConfiguration Configuration { get; }
    }

    internal class SwMacroFeature : SwFeature, ISwMacroFeature
    {
        IXConfiguration IXCustomFeature.Configuration => Configuration;

        private IMacroFeatureData m_FeatData;

        private Type m_DefinitionType;

        public Type DefinitionType
        {
            get
            {
                if (IsCommitted)
                {
                    if (m_DefinitionType == null)
                    {
                        var progId = FeatureData.GetProgId();

                        if (!string.IsNullOrEmpty(progId))
                        {
                            m_DefinitionType = Type.GetTypeFromProgID(progId);
                        }
                    }
                }

                return m_DefinitionType;
            }
            set
            {
                if (!IsCommitted)
                {
                    m_DefinitionType = value;
                }
                else
                {
                    throw new CommittedElementPropertyChangeNotSupported();
                }
            }
        }

        public IMacroFeatureData FeatureData => m_FeatData ?? (m_FeatData = Feature.GetDefinition() as IMacroFeatureData);

        private readonly IFeatureManager m_FeatMgr;

        internal SwMacroFeature(IFeature feat, SwDocument doc, SwApplication app, bool created)
            : base(feat, doc, app, created)
        {
            m_FeatMgr = doc.Model.FeatureManager;
        }

        //TODO: check constant context disconnection exception
        public ISwConfiguration Configuration
            => OwnerDocument.CreateObjectFromDispatch<SwConfiguration>(FeatureData.CurrentConfiguration);

        public TransformMatrix TargetTransformation
        {
            get
            {
                if (IsCommitted)
                {
                    var featTransform = FeatureData.GetEditTargetTransform();

                    if (featTransform != null)
                    {
                        return TransformConverter.ToTransformMatrix(featTransform);
                    }
                    else
                    {
                        return TransformMatrix.Identity;
                    }
                }
                else
                {
                    if (OwnerDocument is IXAssembly)
                    {
                        var editComp = ((IXAssembly)OwnerDocument).EditingComponent;

                        if (editComp != null)
                        {
                            return editComp.Transformation;
                        }
                    }

                    return TransformMatrix.Identity;
                }
            }
        }

        protected override IFeature InsertFeature(CancellationToken cancellationToken)
            => InsertComFeatureBase(null, null, null, null, null, null, null);

        protected IFeature InsertComFeatureBase(string[] paramNames, int[] paramTypes, string[] paramValues,
            int[] dimTypes, double[] dimValues, object[] selection, object[] editBodies)
        {
            ValidateDefinitionType();

            var options = CustomFeatureOptions_e.Default;
            var provider = "";

            DefinitionType.TryGetAttribute<CustomFeatureOptionsAttribute>(a =>
            {
                options = a.Flags;
            });

            DefinitionType.TryGetAttribute<MissingDefinitionErrorMessage>(a =>
            {
                provider = a.Message;
            });

            var baseName = MacroFeatureInfo.GetBaseName(DefinitionType);

            var progId = MacroFeatureInfo.GetProgId(DefinitionType);

            if (string.IsNullOrEmpty(progId))
            {
                throw new NullReferenceException("Prog id for macro feature cannot be extracted");
            }

            var icons = MacroFeatureIconInfo.GetIcons(DefinitionType,
                CompatibilityUtils.SupportsHighResIcons(SwMacroFeatureDefinition.Application.Sw, CompatibilityUtils.HighResIconsScope_e.MacroFeature));

            using (var selSet = new SelectionGroup(OwnerDocument, false))
            {
                if (selection != null && selection.Any())
                {
                    selSet.AddRange(selection);
                }

                var feat = (IFeature)m_FeatMgr.InsertMacroFeature3(baseName,
                    progId, null, paramNames, paramTypes,
                    paramValues, dimTypes, dimValues, editBodies, icons, (int)options);

                return feat;
            }
        }

        protected virtual void ValidateDefinitionType()
        {
            if (!typeof(SwMacroFeatureDefinition).IsAssignableFrom(DefinitionType))
            {
                throw new MacroFeatureDefinitionTypeMismatch(DefinitionType, typeof(SwMacroFeatureDefinition));
            }
        }
    }

    public interface ISwMacroFeature<TParams> : ISwMacroFeature, IXCustomFeature<TParams>
        where TParams : class
    {
    }

    internal class SwMacroFeatureEditor : SwFeatureEditor<IMacroFeatureData>
    {
        public SwMacroFeatureEditor(SwFeature feat, IMacroFeatureData featData) : base(feat, featData)
        {
        }

        protected override void CancelEdit(IMacroFeatureData featData) => featData.ReleaseSelectionAccess();

        protected override bool StartEdit(IMacroFeatureData featData, ISwDocument doc, ISwComponent comp)
            => featData.AccessSelections(doc?.Model, comp?.Component);
    }

    internal class SwMacroFeature<TParams> : SwMacroFeature, ISwMacroFeature<TParams>
        where TParams : class
    {
        private readonly CustomFeatureParametersParser m_ParamsParser;
        private TParams m_ParametersCache;

        internal static SwMacroFeature CreateSpecificInstance(IFeature feat, SwDocument doc, SwApplication app, Type paramType)
        {
            var macroFeatType = typeof(SwMacroFeature<>).MakeGenericType(paramType);

#if DEBUG
            //NOTE: this is a test to ensure that if constructor is changed the reflection will not be broken and this call will fail at compile time
            var test = new SwMacroFeature<object>(feat, doc, app, true);
#endif
            var constr = macroFeatType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null,
                new Type[] { typeof(IFeature), typeof(SwDocument), typeof(SwApplication), typeof(bool) }, null);

            if (constr == null)
            {
                Debug.Assert(false, "Modify the parameters above");
                throw new Exception("Failed to create instance of the macro feature - incorrect parameters");
            }

            return (SwMacroFeature)constr.Invoke(new object[] { feat, doc, app, feat != null });
        }

        /// <summary>
        /// Indicates that the parameters should be read from the cache
        /// </summary>
        /// <remarks>This is used when consumer will be accessing the parameters multiple time
        /// and macro feature definition is not changed during this time (e.g. while regenerating)</remarks>
        internal bool UseCachedParameters { get; set; }

        private Dictionary<IXSelObject, TransformMatrix> m_EntitiesTransformsCache;

        //NOTE: this constructor is used in the reflection of SwObjectFactory
        internal SwMacroFeature(IFeature feat, SwDocument doc, SwApplication app, bool created)
            : base(feat, doc, app, created)
        {
            m_ParamsParser = new CustomFeatureParametersParser();
        }

        public override IEditor<IXFeature> Edit() => new SwMacroFeatureEditor(this, FeatureData);

        public TParams Parameters
        {
            get
            {
                if (IsCommitted && (!UseCachedParameters || m_ParametersCache == null))
                {
                    m_ParametersCache = ReadParameters(out _, out _, out _, out _, out _);
                }

                return m_ParametersCache;
            }
            set
            {
                m_ParametersCache = value;

                if (IsCommitted && !UseCachedParameters)
                {
                    WriteParameters(value, out _);
                }
            }
        }

        public TransformMatrix GetEntityTransformation(IXSelObject entity)
        {
            if (m_EntitiesTransformsCache?.TryGetValue(entity, out var transform) == true)
            {
                return transform;
            }
            else
            {
                if (entity is IXEntity)
                {
                    var ownerComp = ((IXEntity)entity).Component;

                    if (ownerComp != null)
                    {
                        return ownerComp.Transformation;
                    }
                }

                return TransformMatrix.Identity;
            }
        }

        internal TParams ReadParameters(out IXDimension[] dispDims, out string[] dispDimParams, out IXBody[] editBodies,
            out CustomFeatureSelectionInfo[] sels, out CustomFeatureOutdateState_e state)
        {
            dispDims = null;

            try
            {
                ExtractRawParameters(out var rawParams, out dispDims, out sels, out editBodies);

                m_ParamsParser.ConvertParameters(typeof(TParams), OwnerDocument, this, ref rawParams, ref sels, ref dispDims, ref editBodies);

                var parameters = (TParams)m_ParamsParser.BuildParameters(typeof(TParams), ref rawParams, ref dispDims, ref editBodies, ref sels, out dispDimParams);

                m_EntitiesTransformsCache = (sels ?? new CustomFeatureSelectionInfo[0]).Where(s => s != null)
                    .ToDictionary(s => s.Selection, s => s.Transformation, new XObjectEqualityComparer<IXSelObject>());

                state = GetState(dispDims);

                m_ParametersCache = parameters;

                return parameters;
            }
            catch (Exception ex)
            {
                if (dispDims != null)
                {
                    foreach (SwDimension dim in dispDims)
                    {
                        dim.Dispose();
                    }
                }

                OwnerApplication.Logger.Log(ex);

                throw;
            }
        }

        internal void ApplyParametersCache()
        {
            if (!IsCommitted)
            {
                throw new Exception("Feature is not committed");
            }

            if (!UseCachedParameters)
            {
                throw new Exception("Feature is not editing");
            }

            if (m_ParametersCache == null)
            {
                throw new Exception("Feature does not have parameters cache");
            }

            WriteParameters(m_ParametersCache, out _);
        }

        protected override IFeature InsertFeature(CancellationToken cancellationToken)
            => InsertComFeatureWithParameters();

        protected override void ValidateDefinitionType()
        {
            if (!typeof(SwMacroFeatureDefinition`< TParams >`).IsAssignableFrom(DefinitionType))
            {
                throw new MacroFeatureDefinitionTypeMismatch(DefinitionType, typeof(SwMacroFeatureDefinition`< TParams >`));
            }
        }

        private IFeature InsertComFeatureWithParameters()
        {
            CustomFeatureAttribute[] atts;
            IXSelObject[] selection;
            CustomFeatureDimensionType_e[] dimTypes;
            double[] dimValues;
            IXBody[] editBodies;

            m_ParamsParser.Parse(Parameters,
                out atts, out selection, out dimTypes, out dimValues,
                out editBodies);

            string[] paramNames;
            string[] paramValues;
            int[] paramTypes;

            SeparateParameters(atts, out paramNames, out paramTypes, out paramValues);

            //TODO: add dim types conversion

            return InsertComFeatureBase(
                paramNames, paramTypes, paramValues,
                dimTypes?.Select(d => (int)d)?.ToArray(), dimValues,
                selection?.Cast<SwSelObject>()?.Select(s => s.Dispatch)?.ToArray(),
                editBodies?.Cast<SwBody>()?.Select(b => b.Body)?.ToArray());
        }

        private void WriteParameters(object parameters, out CustomFeatureOutdateState_e state)
        {
            CustomFeatureAttribute[] param;
            IXSelObject[] selection;
            double[] dimValues;
            IXBody[] bodies;

            m_ParamsParser.Parse(parameters,
                out param,
                out selection, out _, out dimValues, out bodies);

            var dispDims = GetDimensions();

            //var dimsVersion = GetDimensionsVersion(feat);

            //ConvertParameters(parameters.GetType(), dimsVersion, conv =>
            //{
            //    dispDims = conv.ConvertDisplayDimensions(model, feat, dispDims);
            //});

            if (dispDims != null)
            {
                if (dispDims.Length != dimValues.Length)
                {
                    throw new ParametersMismatchException("Dimensions mismatch");
                }
            }

            state = GetState(dispDims);

            SetParametersToFeature(selection, bodies, dispDims, dimValues, param);
        }

        private void ExtractRawParameters(out Dictionary<string, object> parameters,
            out IXDimension[] dimensions, out CustomFeatureSelectionInfo[] selection, out IXBody[] editBodies)
        {
            object retParamNames;
            object retParamValues;
            object paramTypes;
            object retSelObj;
            object selObjType;
            object selMarks;
            object selDrViews;
            object retCompXforms;

            var featData = FeatureData;

            featData.GetParameters(out retParamNames, out paramTypes, out retParamValues);
            featData.GetSelections3(out retSelObj, out selObjType, out selMarks, out selDrViews, out retCompXforms);

            //TODO: if entity is missing then the order of the retSelObj will be incorrect (null references are always at the end) which may break the indices

            dimensions = GetDimensions();

            var editBodiesObj = featData.EditBodies as object[];

            if (editBodiesObj != null)
            {
                editBodies = editBodiesObj.Cast<IBody2>()
                    .Select(b => SwMacroFeatureDefinition.CreateEditBody(b, OwnerDocument, OwnerApplication, false)).ToArray();
            }
            else
            {
                editBodies = null;
            }

            var paramNames = retParamNames as string[];
            var paramValues = retParamValues as string[];

            if (paramNames != null)
            {
                parameters = new Dictionary<string, object>();

                for (int i = 0; i < paramNames.Length; i++)
                {
                    object paramValue;

                    switch ((swMacroFeatureParamType_e)((int[])paramTypes)[i])
                    {
                        case swMacroFeatureParamType_e.swMacroFeatureParamTypeInteger:
                            try
                            {
                                paramValue = int.Parse(paramValues[i]);
                            }
                            catch
                            {
                                paramValue = int.MinValue;
                            }
                            break;
                        case swMacroFeatureParamType_e.swMacroFeatureParamTypeDouble:
                            try
                            {
                                paramValue = double.Parse(paramValues[i], CultureInfo.InvariantCulture);
                            }
                            catch
                            {
                                paramValue = double.NaN;
                            }
                            break;
                        case swMacroFeatureParamType_e.swMacroFeatureParamTypeString:
                            paramValue = paramValues[i];
                            break;
                        default:
                            throw new NotSupportedException("Macro feature parameter type is not supported");
                    }

                    parameters.Add(paramNames[i], paramValue);
                }
            }
            else
            {
                parameters = null;
            }

            var selObjects = retSelObj as object[];
            var compXforms = retCompXforms as object[];

            if (selObjects != null)
            {
                selection = selObjects.Select((s, i) =>
                {
                    if (s != null)
                    {
                        var transform = (IMathTransform)compXforms[i];

                        var matrix = TransformMatrix.Identity;

                        if (transform != null)
                        {
                            matrix = TransformConverter.ToTransformMatrix(transform);
                        }

                        return new CustomFeatureSelectionInfo(OwnerDocument.CreateObjectFromDispatch<SwSelObject>(s), matrix);
                    }
                    else
                    {
                        return null;
                    }
                }).ToArray();
            }
            else
            {
                selection = null;
            }
        }

        private IXDimension[] GetDimensions()
        {
            var dispDimsObj = (object[])FeatureData.GetDisplayDimensions();

            if (dispDimsObj != null)
            {
                var dimensions = new IXDimension[dispDimsObj.Length];

                for (int i = 0; i < dispDimsObj.Length; i++)
                {
                    var dim = new SwMacroFeatureDimension((IDisplayDimension)dispDimsObj[i], OwnerDocument, OwnerApplication);

                    dimensions[i] = dim;
                    dispDimsObj[i] = null;
                }

                return dimensions;
            }
            else
            {
                return null;
            }
        }

        private CustomFeatureOutdateState_e GetState(IXDimension[] dispDims)
        {
            var state = CustomFeatureOutdateState_e.UpToDate;

            if (dispDims != null && dispDims.Any(d => d is SwDimensionPlaceholder))
            {
                state |= CustomFeatureOutdateState_e.Dimensions;
            }

            //TODO: fix icons

            //if (m_CurrentIcons != null)
            //{
            //    var curIcons = featData.IconFiles as string[];

            //    if (curIcons == null || !m_CurrentIcons.SequenceEqual(curIcons,
            //        StringComparer.CurrentCultureIgnoreCase))
            //    {
            //        state |= CustomFeatureOutdateState_e.Icons;
            //    }
            //}

            return state;
        }

        private Version GetVersion(string name)
        {
            Version dimsVersion;
            string versVal;

            FeatureData.GetStringByName(name, out versVal);

            if (!Version.TryParse(versVal, out dimsVersion))
            {
                dimsVersion = new Version();
            }

            return dimsVersion;
        }

        private void SeparateParameters(CustomFeatureAttribute[] param, out string[] paramNames, out int[] paramTypes, out string[] paramValues)
        {
            if (param != null)
            {
                paramNames = new string[param.Length];
                paramTypes = new int[param.Length];
                paramValues = new string[param.Length];

                for (int i = 0; i < param.Length; i++)
                {
                    paramNames[i] = param[i].Name;

                    if (param[i].Type == typeof(int))
                    {
                        paramTypes[i] = (int)swMacroFeatureParamType_e.swMacroFeatureParamTypeInteger;
                        paramValues[i] = Convert.ToString(param[i].Value);
                    }
                    else if (param[i].Type == typeof(double))
                    {
                        paramTypes[i] = (int)swMacroFeatureParamType_e.swMacroFeatureParamTypeDouble;
                        paramValues[i] = Convert.ToString(param[i].Value, CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        paramTypes[i] = (int)swMacroFeatureParamType_e.swMacroFeatureParamTypeString;
                        paramValues[i] = Convert.ToString(param[i].Value);
                    }
                }
            }
            else
            {
                paramNames = null;
                paramTypes = null;
                paramValues = null;
            }
        }

        private void SetParametersToFeature(IXSelObject[] selection, IXBody[] editBodies,
            IXDimension[] dims, double[] dimValues, CustomFeatureAttribute[] param)
        {
            try
            {
                var featData = FeatureData;

                if (selection?.Any() == true)
                {
                    var dispWraps = selection.Cast<SwSelObject>().Select(s => new DispatchWrapper(s.Dispatch)).ToArray();

                    featData.SetSelections2(dispWraps, new int[selection.Length], new IView[selection.Length]);
                }
                else
                {
                    featData.SetSelections2(null, null, null);
                }

                if (editBodies?.Any() == true)
                {
                    featData.EditBodies = editBodies.Cast<SwBody>().Select(b => b.Body).ToArray();
                }
                else
                {
                    //TODO: this seems to be not working and old edit bodies will still be assigned
                    featData.EditBodies = null;
                }

                if (dims != null)
                {
                    for (int i = 0; i < dims.Length; i++)
                    {
                        dims[i].Value = dimValues[i];
                        ((SwDimension)dims[i]).Dispose();
                    }
                }

                var state = GetState(dims);

                if (param.Any())
                {
                    //macro feature dimensions cannot be changed in the existing feature
                    //reverting the dimensions version
                    if (state.HasFlag(CustomFeatureOutdateState_e.Dimensions))
                    {
                        var vers = GetVersion(CustomFeatureParametersParser.VERSION_DIMENSIONS_NAME);

                        var paramIndex = Array.FindIndex(param, p => p.Name == CustomFeatureParametersParser.VERSION_DIMENSIONS_NAME);

                        param[paramIndex] = new CustomFeatureAttribute(param[paramIndex].Name, param[paramIndex].Type, vers.ToString());
                    }

                    string[] paramNames;
                    string[] paramValues;
                    int[] paramTypes;

                    SeparateParameters(param, out paramNames, out paramTypes, out paramValues);

                    OwnerApplication.Logger.Log($"Writing macro feature parameters: {string.Join(", ", paramNames)} of types {string.Join(", ", paramTypes)} to values {string.Join(", ", paramValues)}", XCad.Base.Enums.LoggerMessageSeverity_e.Debug);

                    featData.SetParameters(paramNames, paramTypes, paramValues);

                    UpdateParameters(featData, paramNames, paramTypes, paramValues);
                }
            }
            finally
            {
                if (dims != null)
                {
                    foreach (SwDimension dim in dims)
                    {
                        dim.Dispose();
                    }
                }
            }
        }

        /// <summary>
        /// Parameters are not updated when SetParameters is called from OnRebuild method, updating one by one fixes the issue
        /// </summary>
        private void UpdateParameters(IMacroFeatureData featData,
            string[] paramNames, int[] paramTypes, string[] paramValues)
        {
            if (paramNames != null && paramTypes != null && paramValues != null)
            {
                for (int i = 0; i < paramNames.Length; i++)
                {
                    var paramName = paramNames[i];
                    var val = paramValues[i];

                    switch ((swMacroFeatureParamType_e)paramTypes[i])
                    {
                        case swMacroFeatureParamType_e.swMacroFeatureParamTypeString:
                            featData.SetStringByName(paramName, val);
                            break;

                        case swMacroFeatureParamType_e.swMacroFeatureParamTypeInteger:
                            featData.SetIntegerByName(paramName, int.Parse(val));
                            break;

                        case swMacroFeatureParamType_e.swMacroFeatureParamTypeDouble:
                            featData.SetDoubleByName(paramName, double.Parse(val, CultureInfo.InvariantCulture));
                            break;
                    }
                }
            }
        }
    }
}