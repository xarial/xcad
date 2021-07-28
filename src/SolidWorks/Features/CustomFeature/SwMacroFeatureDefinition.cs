//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
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
using Xarial.XCad.Base.Enums;
using Xarial.XCad.Delegates;
using Xarial.XCad.Documents;
using Xarial.XCad.Exceptions;
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
using Xarial.XCad.SolidWorks.UI.PropertyPage;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.Toolkit;
using Xarial.XCad.Toolkit.CustomFeature;
using Xarial.XCad.UI;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.Diagnostics;
using Xarial.XCad.Utils.Reflection;

namespace Xarial.XCad.SolidWorks.Features.CustomFeature
{
    public class MacroFeatureEntityId 
    {
        public int FirstId { get; set; }
        public int SecondId { get; set; }
    }

    /// <inheritdoc/>
    public abstract class SwMacroFeatureDefinition : IXCustomFeatureDefinition, ISwComFeature, IXServiceConsumer
    {
        public event ConfigureServicesDelegate ConfigureServices;

        private static SwApplication m_Application;

        internal static SwApplication Application
        {
            get
            {
                if (m_Application == null)
                {
                    m_Application = (SwApplication)SwApplicationFactory.FromProcess(Process.GetCurrentProcess());
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
            svcColl.AddOrReplace<IIconsCreator>(() => new BaseIconsCreator());

            ConfigureServices?.Invoke(this, svcColl);
            OnConfigureServices(svcColl);

            m_SvcProvider = svcColl.CreateProvider();

            m_Logger = m_SvcProvider.GetService<IXLogger>();

            CustomFeatureDefinitionInstanceCache.RegisterInstance(this);

            var iconsConv = m_SvcProvider.GetService<IIconsCreator>();
            iconsConv.KeepIcons = true;
            iconsConv.IconsFolder = MacroFeatureIconInfo.GetLocation(this.GetType());
            TryCreateIcons(iconsConv);
        }

        event ConfigureServicesDelegate IXServiceConsumer.ConfigureServices
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
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

            //Creation of icons may fail if user doesn't have write permissions or icon is locked
            try
            {
                iconsConverter.ConvertIcon(new MacroFeatureIcon(icon, MacroFeatureIconInfo.RegularName));
                iconsConverter.ConvertIcon(new MacroFeatureIcon(icon, MacroFeatureIconInfo.HighlightedName));
                iconsConverter.ConvertIcon(new MacroFeatureSuppressedIcon(icon, MacroFeatureIconInfo.SuppressedName));
                iconsConverter.ConvertIcon(new MacroFeatureHighResIcon(icon, MacroFeatureIconInfo.RegularName));
                iconsConverter.ConvertIcon(new MacroFeatureHighResIcon(icon, MacroFeatureIconInfo.HighlightedName));
                iconsConverter.ConvertIcon(new MacroFeatureSuppressedHighResIcon(icon, MacroFeatureIconInfo.SuppressedName));
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

            var doc = (SwDocument)Application.Documents[modelDoc as IModelDoc2];
            return OnEditDefinition(Application, doc, CreateMacroFeatureInstance(feature as IFeature, doc, Application));
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public object Regenerate(object app, object modelDoc, object feature)
        {
            try
            {
                LogOperation("Regenerating feature", app as ISldWorks, modelDoc as IModelDoc2, feature as IFeature);

                SetProvider(app as ISldWorks, feature as IFeature);

                var doc = (SwDocument)Application.Documents[modelDoc as IModelDoc2];

                var macroFeatInst = (SwMacroFeature)CreateMacroFeatureInstance(feature as IFeature, doc, Application);

                var res = OnRebuild(Application, doc, macroFeatInst);

                if (res != null)
                {
                    return ParseMacroFeatureResult(res, app as ISldWorks, modelDoc as IModelDoc2, macroFeatInst.FeatureData);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                m_Logger.Log(ex);

                if (ex is IUserException)
                {
                    return ex.Message;
                }
                else 
                {
                    return "Unknown regeneration error";
                }
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public object Security(object app, object modelDoc, object feature)
        {
            var doc = (SwDocument)Application.Documents[modelDoc as IModelDoc2];
            return OnUpdateState(Application, doc, CreateMacroFeatureInstance(feature as IFeature, doc, Application));
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
            => Logger.Log($"{operName}: {feature?.Name} in {modelDoc?.GetTitle()} of SOLIDWORKS session: {app?.GetProcessID()}", LoggerMessageSeverity_e.Debug);

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
            => CustomFeatureState_e.Default;

        private object ParseMacroFeatureResult(CustomFeatureRebuildResult res, ISldWorks app, IModelDoc2 model, IMacroFeatureData featData)
        {
            switch (res)
            {
                case CustomFeatureBodyRebuildResult bodyRes:
                    //TODO: validate if any non SwBody in the array
                    //TODO: get updateEntityIds from the parameters
                    return GetBodyResult(app, model, bodyRes.Bodies?.OfType<SwBody>().Select(b => b.Body), featData, true);

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

        private object GetBodyResult(ISldWorks app, IModelDoc2 model, IEnumerable<IBody2> bodies,
            IMacroFeatureData featData, bool updateEntityIds)
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
                            var faceIds = (faces as object[]).ToDictionary(x => (Face2)x, x => new MacroFeatureEntityId());
                            
                            AssignFaceIds(app, model, faceIds);

                            foreach (var faceId in faceIds)
                            {
                                featData.SetFaceUserId(faceId.Key, faceId.Value.FirstId, faceId.Value.SecondId);
                            }
                        }

                        if (edges is object[])
                        {
                            var edgeIds = (edges as object[]).ToDictionary(x => (Edge)x, x => new MacroFeatureEntityId());

                            AssignEdgeIds(app, model, edgeIds);

                            foreach (var edgeId in edgeIds)
                            {
                                featData.SetEdgeUserId(edgeId.Key, edgeId.Value.FirstId, edgeId.Value.SecondId);
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

        protected virtual void AssignFaceIds(ISldWorks app, IModelDoc2 model, IReadOnlyDictionary<Face2, MacroFeatureEntityId> faces) 
        {
            int nextId = 0;

            foreach (var face in faces)
            {
                face.Value.FirstId = nextId++;
                face.Value.SecondId = 0;
            }
        }

        protected virtual void AssignEdgeIds(ISldWorks app, IModelDoc2 model, IReadOnlyDictionary<Edge, MacroFeatureEntityId> edges)
        {
            int nextId = 0;

            foreach (var edge in edges)
            {
                edge.Value.FirstId = nextId++;
                edge.Value.SecondId = 0;
            }
        }

        public virtual void OnConfigureServices(IXServiceCollection collection)
        {
        }

        protected virtual ISwMacroFeature CreateMacroFeatureInstance(IFeature feat, ISwDocument doc, ISwApplication app)
            => doc.CreateObjectFromDispatch<SwMacroFeature>(feat);
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
            => OnEditDefinition(app, model, (ISwMacroFeature<TParams>)feature);

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

        //NOTE: using this to avoid overflow of OnUpdateState as calling the IMacroFeatureData from IFeature invokes Security and thus causing infinite loop
        protected override ISwMacroFeature CreateMacroFeatureInstance(IFeature feat, ISwDocument doc, ISwApplication app)
            => new SwMacroFeature<TParams>(feat, (SwDocument)doc, app, m_ParamsParser, true);
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
                Application, this.GetType(), CreatePageHandler(),
                m_ParamsParser, m_SvcProvider, CreateDynamicControls, AssignPreviewBodyColor);

            m_Editor.EditingStarted += OnEditingStarted;
            m_Editor.EditingCompleted += OnEditingCompleted;
            m_Editor.FeatureInserted += OnFeatureInserted;
        }

        protected virtual void AssignPreviewBodyColor(IXBody body, out System.Drawing.Color color)
            => color = System.Drawing.Color.Yellow;

        protected virtual SwPropertyManagerPageHandler CreatePageHandler() 
            => (SwPropertyManagerPageHandler)Activator.CreateInstance(typeof(TPage));

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

        /// <summary>
        /// Called when macro feature is about to be edited before Property Manager Page is opened
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="doc">Document</param>
        /// <param name="feat">Feature being edited</param>
        protected virtual void OnEditingStarted(IXApplication app, IXDocument doc, IXCustomFeature feat)
        {
        }

        /// <summary>
        /// Called when macro feature is finished editing and Property Manager Page is closed
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="doc">Document</param>
        /// <param name="feat">Feature being edited</param>
        protected virtual void OnEditingCompleted(IXApplication app, IXDocument doc, IXCustomFeature feat)
        {
        }

        /// <summary>
        /// Called when macro feature is created
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="doc">Document</param>
        /// <param name="feat">Feature which is created</param>
        protected virtual void OnFeatureInserted(IXApplication app, IXDocument doc, IXCustomFeature feat)
        {
        }

        protected virtual IControlDescriptor[] CreateDynamicControls(object tag)
            => null;
    }
}