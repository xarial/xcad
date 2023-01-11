//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using Xarial.XCad.Annotations;
using Xarial.XCad.Documents;
using Xarial.XCad.Features;
using Xarial.XCad.Features.CustomFeature;
using Xarial.XCad.Features.CustomFeature.Attributes;
using Xarial.XCad.Features.CustomFeature.Enums;
using Xarial.XCad.Geometry;
using Xarial.XCad.Reflection;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Features.CustomFeature.Exceptions;
using Xarial.XCad.SolidWorks.Features.CustomFeature.Toolkit;
using Xarial.XCad.SolidWorks.Geometry;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.Toolkit.Exceptions;
using Xarial.XCad.Utils.CustomFeature;
using Xarial.XCad.Utils.Reflection;

namespace Xarial.XCad.SolidWorks.Features.CustomFeature
{
    public interface ISwMacroFeature : ISwFeature, IXCustomFeature
    {
        new ISwConfiguration Configuration { get; }
    }

    internal class SwMacroFeature : SwFeature, ISwMacroFeature
    {
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

        IXConfiguration IXCustomFeature.Configuration => Configuration;

        //TODO: check constant context disconnection exception
        public ISwConfiguration Configuration 
            => OwnerDocument.CreateObjectFromDispatch<SwConfiguration>(FeatureData.CurrentConfiguration);

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
        private readonly MacroFeatureParametersParser m_ParamsParser;
        private TParams m_ParametersCache;

        internal static SwMacroFeature CreateSpecificInstance(IFeature feat, SwDocument doc, SwApplication app, Type paramType, MacroFeatureParametersParser paramsParser = null) 
        {
            var macroFeatType = typeof(SwMacroFeature<>).MakeGenericType(paramType);

            if (paramsParser == null)
            {
                paramsParser = new MacroFeatureParametersParser(app);
            }

#if DEBUG
            //NOTE: this is a test to ensure that if constructor is changed the reflection will not be broken and this call will fail at compile time
            var test = new SwMacroFeature<object>(feat, doc, app, paramsParser, true);
#endif
            var constr = macroFeatType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null,
                new Type[] { typeof(IFeature), typeof(SwDocument), typeof(SwApplication), typeof(MacroFeatureParametersParser), typeof(bool) }, null);

            if (constr == null) 
            {
                Debug.Assert(false, "Modify the parameters above");
                throw new Exception("Failed to create instance of the macro feature - incorrect parameters");
            }

            return (SwMacroFeature)constr.Invoke(new object[] { feat, doc, app, paramsParser, feat != null });
        }

        internal bool UseParametersCache { get; set; }

        //NOTE: this constructor is used in the reflection of SwObjectFactory
        internal SwMacroFeature(IFeature feat, SwDocument doc, SwApplication app, MacroFeatureParametersParser paramsParser, bool created)
            : base(feat, doc, app, created)
        {
            m_ParamsParser = paramsParser;
        }

        public override IEditor<IXFeature> Edit() => new SwMacroFeatureEditor(this, FeatureData);

        public TParams Parameters
        {
            get
            {
                if (IsCommitted && UseParametersCache && m_ParametersCache == null) 
                {
                    m_ParametersCache = (TParams)m_ParamsParser.GetParameters(this, OwnerDocument, typeof(TParams),
                            out _, out _, out _, out _, out _);
                }

                if (IsCommitted && !UseParametersCache)
                {
                    return (TParams)m_ParamsParser.GetParameters(this, OwnerDocument, typeof(TParams),
                            out _, out _, out _, out _, out _);
                }
                else
                {
                    return m_ParametersCache;
                }
            }
            set
            {
                if (IsCommitted && !UseParametersCache)
                {
                    m_ParamsParser.SetParameters(OwnerDocument, this, value, out _);
                }
                else
                {
                    m_ParametersCache = value;
                }
            }
        }

        internal void ApplyParametersCache() 
        {
            if (!IsCommitted)
            {
                throw new Exception("Feature is not committed");
            }

            if(!UseParametersCache)
            {
                throw new Exception("Feature is not editing");
            }

            if (m_ParametersCache == null) 
            {
                throw new Exception("Feature does not have parameters cache");
            }

            m_ParamsParser.SetParameters(OwnerDocument, this, m_ParametersCache, out _);
        }
        
        protected override IFeature InsertFeature(CancellationToken cancellationToken)
            => InsertComFeatureWithParameters();

        protected override void ValidateDefinitionType()
        {
            if (!typeof(SwMacroFeatureDefinition<TParams>).IsAssignableFrom(DefinitionType))
            {
                throw new MacroFeatureDefinitionTypeMismatch(DefinitionType, typeof(SwMacroFeatureDefinition<TParams>));
            }
        }

        private IFeature InsertComFeatureWithParameters()
        {
            CustomFeatureParameter[] atts;
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

            m_ParamsParser.ConvertParameters(atts, out paramNames, out paramTypes, out paramValues);

            //TODO: add dim types conversion

            return InsertComFeatureBase(
                paramNames, paramTypes, paramValues,
                dimTypes?.Select(d => (int)d)?.ToArray(), dimValues,
                selection?.Cast<SwSelObject>()?.Select(s => s.Dispatch)?.ToArray(),
                editBodies?.Cast<SwBody>()?.Select(b => b.Body)?.ToArray());
        }
    }
}