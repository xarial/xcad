//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using SolidWorks.Interop.swpublished;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Xarial.XCad;
using Xarial.XCad.Annotations;
using Xarial.XCad.Base;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.Documents;
using Xarial.XCad.Features.CustomFeature;
using Xarial.XCad.Features.CustomFeature.Attributes;
using Xarial.XCad.Features.CustomFeature.Delegates;
using Xarial.XCad.Features.CustomFeature.Enums;
using Xarial.XCad.Features.CustomFeature.Structures;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Reflection;
using Xarial.XCad.SolidWorks.Annotations;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Enums;
using Xarial.XCad.SolidWorks.Features.CustomFeature.Toolkit;
using Xarial.XCad.SolidWorks.Features.CustomFeature.Toolkit.Icons;
using Xarial.XCad.SolidWorks.Geometry;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.Toolkit;
using Xarial.XCad.Toolkit.CustomFeature;
using Xarial.XCad.UI;
using Xarial.XCad.Utils.Diagnostics;
using Xarial.XCad.Utils.Reflection;

namespace Xarial.XCad.SolidWorks.Features.CustomFeature
{
    /// <inheritdoc/>
    public abstract class SwMacroFeatureDefinition : IXCustomFeatureDefinition, ISwComFeature, IXServiceConsumer
    {
        private static ISwApplication m_Application;

        internal static ISwApplication Application
        {
            get
            {
                if (m_Application == null)
                {
                    m_Application = SwApplicationFactory.FromProcess(Process.GetCurrentProcess());
                }

                return m_Application;
            }
            set
            {
                m_Application = value;
            }
        }

        #region Initiation

        private readonly string m_Provider;
        protected readonly IXLogger m_Logger;

        public IXLogger Logger
        {
            get
            {
                return m_Logger;
            }
        }

        protected readonly IServiceProvider m_SvcProvider;

        public SwMacroFeatureDefinition()
        {
            string provider = "";
            this.GetType().TryGetAttribute<MissingDefinitionErrorMessage>(a =>
            {
                provider = a.Message;
            });

            m_Provider = provider;
            
            var svcColl = new ServiceCollection();
            
            svcColl.AddOrReplace<IXLogger>(() => new TraceLogger($"xCad.MacroFeature.{this.GetType().FullName}"));
            svcColl.AddOrReplace<IIconsCreator>(() => new BaseIconsCreator(
                MacroFeatureIconInfo.GetLocation(this.GetType()))
                {
                    KeepIcons = true
                });

            ConfigureServices(svcColl);

            m_SvcProvider = svcColl.CreateProvider();

            m_Logger = m_SvcProvider.GetService<IXLogger>();

            CustomFeatureDefinitionInstanceCache.RegisterInstance(this);

            TryCreateIcons(m_SvcProvider.GetService<IIconsCreator>());
        }

        private void TryCreateIcons(IIconsCreator iconsConverter)
        {
            IXImage icon = null;

            this.GetType().TryGetAttribute<IconAttribute>(a =>
            {
                icon = a.Icon;
            });

            if (icon == null)
            {
                icon = Defaults.Icon;
            }

            //TODO: create different icons for highlighted and suppressed
            var regular = icon;
            var highlighted = icon;
            var suppressed = icon;

            //Creation of icons may fail if user doesn't have write permissions or icon is locked
            try
            {
                iconsConverter.ConvertIcon(new MacroFeatureIcon(icon, MacroFeatureIconInfo.RegularName));
                iconsConverter.ConvertIcon(new MacroFeatureIcon(highlighted, MacroFeatureIconInfo.HighlightedName));
                iconsConverter.ConvertIcon(new MacroFeatureIcon(suppressed, MacroFeatureIconInfo.SuppressedName));
                iconsConverter.ConvertIcon(new MacroFeatureHighResIcon(icon, MacroFeatureIconInfo.RegularName));
                iconsConverter.ConvertIcon(new MacroFeatureHighResIcon(highlighted, MacroFeatureIconInfo.HighlightedName));
                iconsConverter.ConvertIcon(new MacroFeatureHighResIcon(suppressed, MacroFeatureIconInfo.SuppressedName));
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        #endregion Initiation

        #region Overrides

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public object Edit(object app, object modelDoc, object feature)
        {
            LogOperation("Editing feature", app as ISldWorks, modelDoc as IModelDoc2, feature as IFeature);

            var doc = Application.Documents[modelDoc as IModelDoc2];
            return OnEditDefinition(Application, doc, new SwMacroFeature(doc, (modelDoc as IModelDoc2).FeatureManager, feature as IFeature, true));
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public object Regenerate(object app, object modelDoc, object feature)
        {
            LogOperation("Regenerating feature", app as ISldWorks, modelDoc as IModelDoc2, feature as IFeature);

            SetProvider(app as ISldWorks, feature as IFeature);

            var doc = Application.Documents[modelDoc as IModelDoc2];

            var macroFeatInst = new SwMacroFeature(doc, (modelDoc as IModelDoc2).FeatureManager, feature as IFeature, true);

            var res = OnRebuild(Application, doc, macroFeatInst);

            if (res != null)
            {
                return ParseMacroFeatureResult(res, app as ISldWorks, macroFeatInst.FeatureData);
            }
            else
            {
                return null;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public object Security(object app, object modelDoc, object feature)
        {
            var doc = Application.Documents[modelDoc as IModelDoc2];
            return OnUpdateState(Application, doc, new SwMacroFeature(doc, (modelDoc as IModelDoc2).FeatureManager, feature as IFeature, true));
        }

        private void SetProvider(ISldWorks app, IFeature feature)
        {
            if (!string.IsNullOrEmpty(m_Provider))
            {
                if (app.IsVersionNewerOrEqual(SwVersion_e.Sw2016))
                {
                    var featData = feature.GetDefinition() as IMacroFeatureData;

                    if (featData.Provider != m_Provider)
                    {
                        featData.Provider = m_Provider;
                    }
                }
            }
        }

        private void LogOperation(string operName, ISldWorks app, IModelDoc2 modelDoc, IFeature feature)
        {
            Logger.Log($"{operName}: {feature?.Name} in {modelDoc?.GetTitle()} of SOLIDWORKS session: {app?.GetProcessID()}");
        }

        #endregion Overrides

        bool IXCustomFeatureDefinition.OnEditDefinition(IXApplication app, IXDocument model, IXCustomFeature feature)
            => OnEditDefinition((ISwApplication)app, (ISwDocument)model, (SwMacroFeature)feature);

        CustomFeatureRebuildResult IXCustomFeatureDefinition.OnRebuild(IXApplication app, IXDocument model, IXCustomFeature feature)
            => OnRebuild((ISwApplication) app, (ISwDocument) model, (ISwMacroFeature)feature);

        CustomFeatureState_e IXCustomFeatureDefinition.OnUpdateState(IXApplication app, IXDocument model, IXCustomFeature feature) 
            => OnUpdateState((ISwApplication)app, (ISwDocument)model, (SwMacroFeature)feature);

        public virtual bool OnEditDefinition(ISwApplication app, ISwDocument model, ISwMacroFeature feature)
        {
            return true;
        }

        public virtual CustomFeatureRebuildResult OnRebuild(ISwApplication app, ISwDocument model, ISwMacroFeature feature)
        {
            return null;
        }

        public virtual CustomFeatureState_e OnUpdateState(ISwApplication app, ISwDocument model, ISwMacroFeature feature)
        {
            return CustomFeatureState_e.Default;
        }

        private object ParseMacroFeatureResult(CustomFeatureRebuildResult res, ISldWorks app, IMacroFeatureData featData)
        {
            switch (res)
            {
                case CustomFeatureBodyRebuildResult bodyRes:
                    //TODO: validate if any non SwBody in the array
                    //TODO: get updateEntityIds from the parameters
                    return GetBodyResult(app, bodyRes.Bodies?.OfType<SwBody>().Select(b => b.Body), featData, true);

                default:
                    return GetStatusResult(res.Result, res.ErrorMessage);
            }
        }

        private object GetStatusResult(bool status, string error = "")
        {
            if (status)
            {
                return status;
            }
            else
            {
                if (!string.IsNullOrEmpty(error))
                {
                    return error;
                }
                else
                {
                    return status;
                }
            }
        }

        private object GetBodyResult(ISldWorks app, IEnumerable<IBody2> bodies, IMacroFeatureData featData, bool updateEntityIds)
        {
            if (bodies != null)
            {
                if (CompatibilityUtils.IsVersionNewerOrEqual(app, SwVersion_e.Sw2013, 5))
                {
                    featData.EnableMultiBodyConsume = true;
                }

                if (updateEntityIds)
                {
                    if (featData == null)
                    {
                        throw new ArgumentNullException(nameof(featData));
                    }

                    foreach (var body in bodies)
                    {
                        object faces;
                        object edges;
                        featData.GetEntitiesNeedUserId(body, out faces, out edges);

                        if (faces is object[])
                        {
                            int nextId = 0;

                            foreach (Face2 face in faces as object[])
                            {
                                featData.SetFaceUserId(face, nextId++, 0);
                            }
                        }

                        if (edges is object[])
                        {
                            int nextId = 0;

                            foreach (Edge edge in edges as object[])
                            {
                                featData.SetEdgeUserId(edge, nextId++, 0);
                            }
                        }
                    }
                }

                if (bodies.Count() == 1)
                {
                    return bodies.First();
                }
                else
                {
                    return bodies.ToArray();
                }
            }
            else
            {
                throw new ArgumentNullException(nameof(bodies));
            }
        }

        public virtual void ConfigureServices(IXServiceCollection collection)
        {
        }
    }

    /// <inheritdoc/>
    public abstract class SwMacroFeatureDefinition<TParams> : SwMacroFeatureDefinition, IXCustomFeatureDefinition<TParams>
        where TParams : class, new()
    {
        private readonly MacroFeatureParametersParser m_ParamsParser;

        CustomFeatureRebuildResult IXCustomFeatureDefinition<TParams>.OnRebuild(IXApplication app, IXDocument model, IXCustomFeature feature, TParams parameters, out AlignDimensionDelegate<TParams> alignDim)
            => OnRebuild((ISwApplication)app, (ISwDocument)model, (SwMacroFeature)feature, parameters, out alignDim);

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override bool OnEditDefinition(ISwApplication app, ISwDocument model, ISwMacroFeature feature)
            => OnEditDefinition(app, model, feature.ToParameters<TParams>());

        public SwMacroFeatureDefinition() : this(new MacroFeatureParametersParser())
        {
        }

        internal SwMacroFeatureDefinition(MacroFeatureParametersParser paramsParser) : base()
        {
            m_ParamsParser = paramsParser;
        }

        public void AlignDimension(IXDimension dim, Point[] pts, Vector dir, Vector extDir)
        {
            if (pts != null)
            {
                if (pts.Length == 2)
                {
                    var newPts = new Point[3]
                    {
                        pts[0],
                        pts[1],
                        new Point(0, 0, 0)//3 points required for SOLIDWORKS even if not used
                    };
                }
            }

            var refPts = pts.Select(p => m_ParamsParser.MathUtils.CreatePoint(p.ToArray()) as IMathPoint).ToArray();

            if (dir != null)
            {
                var dimDirVec = m_ParamsParser.MathUtils.CreateVector(dir.ToArray()) as MathVector;
                ((SwDimension)dim).Dimension.DimensionLineDirection = dimDirVec;
            }

            if (extDir != null)
            {
                var extDirVec = m_ParamsParser.MathUtils.CreateVector(extDir.ToArray()) as MathVector;
                ((SwDimension)dim).Dimension.ExtensionLineDirection = extDirVec;
            }

            var swDim = ((SwDimension)dim).Dimension;

            swDim.ReferencePoints = refPts;

            var swDispDim = ((SwDimension)dim).DisplayDimension;
            if (swDispDim.Type2 == (int)swDimensionType_e.swAngularDimension) 
            {
                swDispDim.IGetAnnotation().SetPosition2(
                    (pts[1].X + pts[0].X) / 2,
                    (pts[1].Y + pts[0].Y) / 2,
                    (pts[1].Z + pts[0].Z) / 2);
            }
        }

        public abstract CustomFeatureRebuildResult OnRebuild(ISwApplication app, ISwDocument model, ISwMacroFeature feature,
            TParams parameters, out AlignDimensionDelegate<TParams> alignDim);

        public override CustomFeatureRebuildResult OnRebuild(ISwApplication app, ISwDocument model, ISwMacroFeature feature)
        {
            IXDimension[] dims;
            string[] dimParamNames;
            var param = (TParams)m_ParamsParser.GetParameters(feature, model, typeof(TParams), out dims, out dimParamNames,
                out IXBody[] _, out IXSelObject[] _, out CustomFeatureOutdateState_e _);

            AlignDimensionDelegate<TParams> alignDimsDel;
            var res = OnRebuild(app, model, feature, param, out alignDimsDel);

            m_ParamsParser.SetParameters(model, feature, param, out CustomFeatureOutdateState_e _);

            if (dims?.Any() == true)
            {
                if (alignDimsDel != null)
                {
                    for (int i = 0; i < dims.Length; i++)
                    {
                        alignDimsDel.Invoke(dimParamNames[i], dims[i]);

                        //IMPORTANT: need to dispose otherwise SW will crash once document is closed
                        ((IDisposable)dims[i]).Dispose();
                    }
                }
            }

            return res;
        }

        public virtual bool OnEditDefinition(ISwApplication app, ISwDocument model, ISwMacroFeature<TParams> feature) 
        {
            return true;
        }
    }

    /// <inheritdoc/>
    public abstract class SwMacroFeatureDefinition<TParams, TPage> : SwMacroFeatureDefinition<TParams>, IXCustomFeatureDefinition<TParams, TPage>
        where TParams : class, new()
        where TPage : class, new()
    {
        private readonly MacroFeatureParametersParser m_ParamsParser;

        private readonly SwMacroFeatureEditor<TParams, TPage> m_Editor;

        public SwMacroFeatureDefinition() : this(new MacroFeatureParametersParser())
        {
        }

        internal SwMacroFeatureDefinition(MacroFeatureParametersParser parser) : base(parser)
        {
            m_ParamsParser = parser;

            m_Editor = new SwMacroFeatureEditor<TParams, TPage>(
                Application, this.GetType(), m_ParamsParser, m_SvcProvider);
        }

        public virtual TParams ConvertPageToParams(TPage par)
        {
            if (typeof(TParams) == typeof(TPage)) 
            {
                return (TParams)((object)par);
            }

            throw new Exception($"Override {nameof(ConvertPageToParams)} to provide the converter");
        }

        public virtual TPage ConvertParamsToPage(TParams par)
        {
            if (typeof(TParams) == typeof(TPage))
            {
                return (TPage)((object)par);
            }

            throw new Exception($"Override {nameof(ConvertParamsToPage)} to provide the converter");
        }

        public abstract ISwBody[] CreateGeometry(ISwApplication app, ISwDocument model, TParams data, bool isPreview, out AlignDimensionDelegate<TParams> alignDim);

        IXBody[] IXCustomFeatureDefinition<TParams, TPage>.CreateGeometry(
            IXApplication app, IXDocument doc, TParams data, bool isPreview, out AlignDimensionDelegate<TParams> alignDim) 
            => CreateGeometry((ISwApplication)app, (ISwDocument)doc, data, isPreview, out alignDim).Cast<SwBody>().ToArray();

        public void Insert(IXDocument doc)
        {
            m_Editor.Insert(doc);
        }

        public override bool OnEditDefinition(ISwApplication app, ISwDocument model, ISwMacroFeature<TParams> feature)
        {
            m_Editor.Edit(model, feature);
            return true;
        }

        public override CustomFeatureRebuildResult OnRebuild(ISwApplication app, ISwDocument model,
            ISwMacroFeature feature, TParams parameters, out AlignDimensionDelegate<TParams> alignDim)
        {
            return new CustomFeatureBodyRebuildResult()
            {
                Bodies = CreateGeometry(app, model, parameters, false, out alignDim).ToArray()
            };
        }
    }
}