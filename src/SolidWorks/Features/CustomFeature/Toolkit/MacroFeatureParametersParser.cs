//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Xarial.XCad.Annotations;
using Xarial.XCad.Documents;
using Xarial.XCad.Features;
using Xarial.XCad.Features.CustomFeature;
using Xarial.XCad.Features.CustomFeature.Enums;
using Xarial.XCad.Geometry;
using Xarial.XCad.SolidWorks.Annotations;
using Xarial.XCad.SolidWorks.Geometry;
using Xarial.XCad.Utils.CustomFeature;

namespace Xarial.XCad.SolidWorks.Features.CustomFeature.Toolkit
{
    internal class MacroFeatureParametersParser : CustomFeatureParametersParser
    {
        internal IMathUtility MathUtils { get; }

        internal MacroFeatureParametersParser() : this(SwMacroFeatureDefinition.Application.Sw)
        {
        }

        internal MacroFeatureParametersParser(ISldWorks app)
        {
            MathUtils = app.IGetMathUtility();
        }

        protected override void ExtractRawParameters(IXCustomFeature feat, out Dictionary<string, object> parameters,
            out IXDimension[] dimensions, out IXSelObject[] selection, out IXBody[] editBodies)
        {
            object retParamNames = null;
            object retParamValues = null;
            object paramTypes = null;
            object retSelObj;
            object selObjType;
            object selMarks;
            object selDrViews;
            object compXforms;

            var featData = ((SwMacroFeature)feat).FeatureData;

            featData.GetParameters(out retParamNames, out paramTypes, out retParamValues);
            featData.GetSelections3(out retSelObj, out selObjType, out selMarks, out selDrViews, out compXforms);

            dimensions = GetDimensions(feat);

            var editBodiesObj = featData.EditBodies as object[];

            if (editBodiesObj != null)
            {
                editBodies = editBodiesObj.Cast<IBody2>().Select(b => SwObject.FromDispatch<SwBody>(b)).ToArray();
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
                    parameters.Add(paramNames[i], paramValues[i]);
                }
            }
            else
            {
                parameters = null;
            }

            var selObjects = retSelObj as object[];

            if (selObjects != null)
            {
                selection = selObjects.Select(s => SwObject.FromDispatch<SwSelObject>(s)).ToArray();
            }
            else
            {
                selection = null;
            }
        }

        protected override IXDimension[] GetDimensions(IXCustomFeature feat)
        {
            var macroFeat = (SwMacroFeature)feat;

            var dispDimsObj = macroFeat.FeatureData.GetDisplayDimensions() as object[];

            if (dispDimsObj != null)
            {
                var dimensions = new IXDimension[dispDimsObj.Length];

                for (int i = 0; i < dispDimsObj.Length; i++)
                {
                    dimensions[i] = new SwDimension(macroFeat.Model.Model, dispDimsObj[i] as IDisplayDimension);
                    dispDimsObj[i] = null;
                }

                return dimensions;
            }
            else
            {
                return null;
            }
        }

        protected override CustomFeatureOutdateState_e GetState(IXCustomFeature featData, IXDimension[] dispDims)
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

        protected override Version GetVersion(IXFeature featData, string name)
        {
            Version dimsVersion;
            string versVal;
            ((SwMacroFeature)featData).FeatureData.GetStringByName(name, out versVal);

            if (!Version.TryParse(versVal, out dimsVersion))
            {
                dimsVersion = new Version();
            }

            return dimsVersion;
        }

        internal void ConvertParameters(CustomFeatureParameter[] param, out string[] paramNames, out int[] paramTypes, out string[] paramValues)
        {
            if (param != null)
            {
                paramNames = param.Select(p => p.Name).ToArray();
                paramTypes = param.Select(p =>
                {
                    swMacroFeatureParamType_e paramType;

                    if (p.Type == typeof(int))
                    {
                        paramType = swMacroFeatureParamType_e.swMacroFeatureParamTypeInteger;
                    }
                    else if (p.Type == typeof(double))
                    {
                        paramType = swMacroFeatureParamType_e.swMacroFeatureParamTypeDouble;
                    }
                    else
                    {
                        paramType = swMacroFeatureParamType_e.swMacroFeatureParamTypeString;
                    }

                    return (int)paramType;
                }).ToArray();

                paramValues = param.Select(p => (string)p.Value).ToArray();
            }
            else
            {
                paramNames = null;
                paramTypes = null;
                paramValues = null;
            }
        }

        protected override void SetParametersToFeature(IXCustomFeature feat, IXSelObject[] selection, IXBody[] editBodies,
            IXDimension[] dims, double[] dimValues, CustomFeatureParameter[] param)
        {
            try
            {
                var featData = ((SwMacroFeature)feat).FeatureData;

                if (selection != null && selection.Any())
                {
                    var dispWraps = selection.Cast<SwSelObject>().Select(s => new DispatchWrapper(s.Dispatch)).ToArray();

                    featData.SetSelections2(dispWraps, new int[selection.Length], new IView[selection.Length]);
                }

                if (editBodies != null)
                {
                    featData.EditBodies = editBodies.Cast<SwBody>().Select(b => b.Body).ToArray();
                }

                if (dims != null)
                {
                    for (int i = 0; i < dims.Length; i++)
                    {
                        dims[i].SetValue(dimValues[i]);
                        ((SwDimension)dims[i]).Dispose();
                    }
                }

                var state = GetState(feat, dims);

                if (param.Any())
                {
                    //macro feature dimensions cannot be changed in the existing feature
                    //reverting the dimensions version
                    if (state.HasFlag(CustomFeatureOutdateState_e.Dimensions))
                    {
                        var vers = GetVersion(feat, VERSION_DIMENSIONS_NAME);

                        var par = param.First(p => p.Name == VERSION_DIMENSIONS_NAME);
                        par.Value = vers.ToString();
                    }

                    string[] paramNames;
                    string[] paramValues;
                    int[] paramTypes;

                    ConvertParameters(param, out paramNames, out paramTypes, out paramValues);

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

        public override object GetParameters(IXCustomFeature feat, IXDocument model, Type paramsType, out IXDimension[] dispDims, out string[] dispDimParams, out IXBody[] editBodies, out IXSelObject[] sels, out CustomFeatureOutdateState_e state)
        {
            dispDims = null;

            try
            {
                return base.GetParameters(feat, model, paramsType, out dispDims, out dispDimParams, out editBodies, out sels, out state);
            }
            catch
            {
                if (dispDims != null)
                {
                    foreach (SwDimension dim in dispDims)
                    {
                        dim.Dispose();
                    }
                }

                throw;
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
                            featData.SetDoubleByName(paramName, double.Parse(val));
                            break;
                    }
                }
            }
        }
    }
}