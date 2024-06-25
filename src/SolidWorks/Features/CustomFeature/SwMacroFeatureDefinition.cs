//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using SolidWorks.Interop.swpublished;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;
using Xarial.XCad.Annotations;
using Xarial.XCad.Base;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.Base.Enums;
using Xarial.XCad.Delegates;
using Xarial.XCad.Documents;
using Xarial.XCad.Exceptions;
using Xarial.XCad.Features.CustomFeature;
using Xarial.XCad.Features.CustomFeature.Attributes;
using Xarial.XCad.Features.CustomFeature.Enums;
using Xarial.XCad.Features.CustomFeature.Structures;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Reflection;
using Xarial.XCad.SolidWorks.Annotations;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Enums;
using Xarial.XCad.SolidWorks.Features.CustomFeature.Delegates;
using Xarial.XCad.SolidWorks.Features.CustomFeature.Toolkit;
using Xarial.XCad.SolidWorks.Features.CustomFeature.Toolkit.Icons;
using Xarial.XCad.SolidWorks.Geometry;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.SolidWorks.UI.PropertyPage;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.Toolkit;
using Xarial.XCad.Toolkit.CustomFeature;
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.UI;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Enums;
using Xarial.XCad.Utils.CustomFeature;
using Xarial.XCad.Utils.Diagnostics;
using Xarial.XCad.Utils.Reflection;

namespace Xarial.XCad.SolidWorks.Features.CustomFeature
{
    /// <inheritdoc/>
    public abstract class SwMacroFeatureDefinition : IXCustomFeatureDefinition, ISwComFeature, IXServiceConsumer
    {
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        internal class MacroFeatureRegenerateData 
        {
            internal ISwApplication Application { get; set; }
            internal ISwDocument Document { get; set; }
            internal ISwMacroFeature Feature { get; set; }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static ISwTempBody CreateEditBody(IBody2 body, ISwDocument doc, ISwApplication app, bool isPreview)
        {
            var bodyType = (swBodyType_e)body.GetType();

            switch (bodyType)
            {
                case swBodyType_e.swSheetBody:
                    if (body.GetFaceCount() == 1 && body.IGetFirstFace().IGetSurface().IsPlane())
                    {
                        return new SwPlanarSheetMacroFeatureEditBody(body, (SwDocument)doc, (SwApplication)app, isPreview);
                    }
                    else
                    {
                        return new SwSheetMacroFeatureEditBody(body, (SwDocument)doc, (SwApplication)app, isPreview);
                    }

                case swBodyType_e.swSolidBody:
                    return new SwSolidMacroFeatureEditBody(body, (SwDocument)doc, (SwApplication)app, isPreview);

                case swBodyType_e.swWireBody:
                    return new SwWireMacroFeatureEditBody(body, (SwDocument)doc, (SwApplication)app, isPreview);

                default:
                    throw new NotSupportedException();
            }
        }

        public event ConfigureServicesDelegate ConfigureServices;

        /// <summary>
        /// Called when macro feature is rebuild
        /// </summary>
        public event PostRebuildMacroFeatureDelegate PostRebuild 
        {
            add => m_PostRebuild += value;
            remove => m_PostRebuild -= value;
        }

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

        private PostRebuildMacroFeatureDelegate m_PostRebuild;

        #region Initiation

        private readonly string m_Provider;

        /// <summary>
        /// Logger
        /// </summary>
        public IXLogger Logger { get; }

        /// <summary>
        /// DI services
        /// </summary>
        protected IServiceProvider Services { get; }

        internal readonly List<MacroFeatureRegenerateData> m_RebuildFeaturesQueue;

        private bool m_IsSubscribedToIdle;

        private readonly Lazy<IMathUtility> m_MathUtilsLazy;

        public SwMacroFeatureDefinition() 
        {
            string provider = "";
            this.GetType().TryGetAttribute<MissingDefinitionErrorMessage>(a =>
            {
                provider = a.Message;
            });

            m_Provider = provider;

            m_MathUtilsLazy = new Lazy<IMathUtility>(Application.Sw.IGetMathUtility);

            m_RebuildFeaturesQueue = new List<MacroFeatureRegenerateData>();

            m_IsSubscribedToIdle = false;

            var svcColl = Application.CustomServices.Clone();

            svcColl.Add<IXLogger>(() => new TraceLogger($"xCad.MacroFeature.{this.GetType().FullName}"), ServiceLifetimeScope_e.Singleton, false);
            svcColl.Add<IIconsCreator, BaseIconsCreator>(ServiceLifetimeScope_e.Singleton, false);

            OnConfigureServices(svcColl);

            Services = svcColl.CreateProvider();

            Logger = Services.GetService<IXLogger>();

            CustomFeatureDefinitionInstanceCache.RegisterInstance(this);

            TryCreateIcons(Services.GetService<IIconsCreator>(), MacroFeatureIconInfo.GetLocation(this.GetType()));
        }

        private void TryCreateIcons(IIconsCreator iconsConverter, string folder)
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
                iconsConverter.ConvertIcon(new MacroFeatureIcon(icon, MacroFeatureIconInfo.RegularName), folder);
                iconsConverter.ConvertIcon(new MacroFeatureIcon(icon, MacroFeatureIconInfo.HighlightedName), folder);
                iconsConverter.ConvertIcon(new MacroFeatureSuppressedIcon(icon, MacroFeatureIconInfo.SuppressedName), folder);
                iconsConverter.ConvertIcon(new MacroFeatureHighResIcon(icon, MacroFeatureIconInfo.RegularName), folder);
                iconsConverter.ConvertIcon(new MacroFeatureHighResIcon(icon, MacroFeatureIconInfo.HighlightedName), folder);
                iconsConverter.ConvertIcon(new MacroFeatureSuppressedHighResIcon(icon, MacroFeatureIconInfo.SuppressedName), folder);
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
            try
            {
                var doc = (SwDocument)Application.Documents[modelDoc as IModelDoc2];
                return OnEditDefinition(Application, doc, CreateMacroFeatureInstance((IFeature)feature, doc, Application));
            }
            catch(Exception ex) 
            {
                Logger.Log(ex);
                return HandleEditException(ex);
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public object Regenerate(object app, object modelDoc, object feature)
        {
            try
            {
                SetProvider(app as ISldWorks, feature as IFeature);

                var doc = (SwDocument)Application.Documents[modelDoc as IModelDoc2];

                SwDocument contextDoc;

                var comp = (IComponent2)(feature as IEntity).GetComponent();

                if (comp != null)
                {
                    var assmName = comp.GetSelectByIDString().Split('@').Last() + ".sldasm";
                    contextDoc = (SwDocument)Application.Documents[assmName];
                }
                else
                {
                    contextDoc = doc;
                }
                
                var macroFeatInst = CreateMacroFeatureInstance((IFeature)feature, contextDoc, Application);

                var userIdsMgr = CreateUserIdsManager(Application, doc, macroFeatInst);

                var res = OnRebuild(Application, doc, macroFeatInst, userIdsMgr);

                if (HandlePostRebuild)
                {
                    AddDataToRebuildQueue(Application, doc, macroFeatInst);

                    if (!m_IsSubscribedToIdle)
                    {
                        m_IsSubscribedToIdle = true;
                        ((SldWorks)Application.Sw).OnIdleNotify += OnIdleNotify;
                    }
                }

                if (res != null)
                {
                    return ParseMacroFeatureResult(res, (ISldWorks)app, (IModelDoc2)modelDoc, macroFeatInst.FeatureData, userIdsMgr);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);

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
            try
            {
                var doc = (SwDocument)Application.Documents[modelDoc as IModelDoc2];
                return (int)OnUpdateState(Application, doc, CreateMacroFeatureInstance((IFeature)feature, doc, Application));
            }
            catch(Exception ex) 
            {
                Logger.Log(ex);
                return HandleStateException(ex);
            }
        }

        /// <summary>
        /// Handles the exception raised while editing feature
        /// </summary>
        /// <param name="ex">Raised exception</param>
        /// <returns>Value to return</returns>
        protected virtual object HandleEditException(Exception ex) => throw ex;

        /// <summary>
        /// Handles the exception raised while feature update state
        /// </summary>
        /// <param name="ex">Raised exception</param>
        /// <returns>Value to return</returns>
        protected virtual object HandleStateException(Exception ex) => throw ex;

        internal virtual void AddDataToRebuildQueue(ISwApplication app, ISwDocument doc, ISwMacroFeature macroFeatInst)
        {
            m_RebuildFeaturesQueue.Add(new MacroFeatureRegenerateData()
            {
                Application = app,
                Document = doc,
                Feature = macroFeatInst
            });
        }

        private int OnIdleNotify()
        {
            m_IsSubscribedToIdle = false;
            ((SldWorks)Application.Sw).OnIdleNotify -= OnIdleNotify;

            foreach (var data in m_RebuildFeaturesQueue) 
            {
                DispatchPostBuildData(data);
            }

            m_RebuildFeaturesQueue.Clear();

            return HResult.S_OK;
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

        #endregion Overrides

        /// <inheritdoc/>
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

            var refPts = pts.Select(p => m_MathUtilsLazy.Value.CreatePoint(p.ToArray()) as IMathPoint).ToArray();

            if (dir != null)
            {
                var dimDirVec = m_MathUtilsLazy.Value.CreateVector(dir.ToArray()) as MathVector;
                ((SwDimension)dim).Dimension.DimensionLineDirection = dimDirVec;
            }

            if (extDir != null)
            {
                var extDirVec = m_MathUtilsLazy.Value.CreateVector(extDir.ToArray()) as MathVector;
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

        bool IXCustomFeatureDefinition.OnEditDefinition(IXApplication app, IXDocument doc, IXCustomFeature feature)
            => OnEditDefinition((ISwApplication)app, (ISwDocument)doc, (SwMacroFeature)feature);

        CustomFeatureRebuildResult IXCustomFeatureDefinition.OnRebuild(IXApplication app, IXDocument doc, IXCustomFeature feature)
            => OnRebuild((ISwApplication) app, (ISwDocument) doc, (ISwMacroFeature)feature);

        CustomFeatureState_e IXCustomFeatureDefinition.OnUpdateState(IXApplication app, IXDocument doc, IXCustomFeature feature) 
            => OnUpdateState((ISwApplication)app, (ISwDocument)doc, (SwMacroFeature)feature);

        public virtual bool OnEditDefinition(ISwApplication app, ISwDocument model, ISwMacroFeature feature) => true;

        public virtual CustomFeatureRebuildResult OnRebuild(ISwApplication app, ISwDocument doc, ISwMacroFeature feature)
            => null;

        public virtual CustomFeatureRebuildResult OnRebuild(ISwApplication app, ISwDocument doc, ISwMacroFeature feature, IUserIdsManager userIdsMgr) 
            => OnRebuild(app, doc, feature);

        protected virtual IUserIdsManager CreateUserIdsManager(ISwApplication app, ISwDocument doc, ISwMacroFeature feature) 
            => new UserIdsManager((SwMacroFeature)feature);

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        internal virtual SwMacroFeature CreateMacroFeatureInstance(IFeature feat, SwDocument doc, SwApplication app)
            => new SwMacroFeature(feat, doc, app, feat != null);

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        internal virtual void DispatchPostBuildData(MacroFeatureRegenerateData data)
            => m_PostRebuild?.Invoke(data.Application, data.Document, data.Feature);

        /// <summary>
        /// This handle is called everytime macro feature needs to update the state
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="doc">Document</param>
        /// <param name="feature">Feature to update state</param>
        /// <returns></returns>
        public virtual CustomFeatureState_e OnUpdateState(ISwApplication app, ISwDocument doc, ISwMacroFeature feature)
            => CustomFeatureState_e.Default;

        private object ParseMacroFeatureResult(CustomFeatureRebuildResult res, ISldWorks app, IModelDoc2 model,
            IMacroFeatureData featData, IUserIdsManager userIdsMgr)
        {
            switch (res)
            {
                case CustomFeatureBodyRebuildResult bodyRes:
                    
                    if (bodyRes.Bodies?.Any() == true)
                    {
                        return GetBodyResult(app, model, bodyRes.Bodies, featData, userIdsMgr);
                    }
                    else 
                    {
                        return GetStatusResult(true, "");
                    }

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

        private object GetBodyResult(ISldWorks app, IModelDoc2 model, IReadOnlyList<IXBody> bodies,
            IMacroFeatureData featData, IUserIdsManager userIdsMgr)
        {
            if (bodies != null)
            {
                if (CompatibilityUtils.IsVersionNewerOrEqual(app, SwVersion_e.Sw2013, 5))
                {
                    featData.EnableMultiBodyConsume = true;
                }

                foreach(var body in bodies) 
                {
                    userIdsMgr.AssignUserIds(body);
                }
                
                if (bodies.Count == 1)
                {
                    return bodies.Cast<ISwBody>().First().Body;
                }
                else
                {
                    return bodies.Cast<ISwBody>().Select(b => b.Body).ToArray();
                }
            }
            else
            {
                throw new ArgumentNullException(nameof(bodies));
            }
        }

        internal virtual bool HandlePostRebuild => m_PostRebuild != null;

        /// <summary>
        /// Register Dependency Injection services
        /// </summary>
        /// <param name="svcColl">Services collection</param>
        /// <remarks>Typically add-in is loaded before the instance of the macro feature definition service is created
        /// In this case macro feature services will inherit services configured within the <see cref="ISwApplication"/> and <see cref="SwAddInEx"/> and overriding of this method or handling the <see cref="ConfigureServices"/> event is not required
        /// However macro feature definition is independent COM server which means it can be loaded without the add-in. In this case add-in services will not be automatically inherited
        /// It is recommended to haev independent helper class which registers all services and shares between the <see cref="ISwApplication"/>, <see cref="SwAddInEx"/> and <see cref="SwMacroFeatureDefinition"/></remarks>
        protected virtual void OnConfigureServices(IXServiceCollection svcColl)
        {
            ConfigureServices?.Invoke(this, svcColl);
        }
    }

    /// <inheritdoc/>
    public abstract class SwMacroFeatureDefinition<TParams> : SwMacroFeatureDefinition, IXCustomFeatureDefinition<TParams>
        where TParams : class
    {
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        internal class MacroFeatureParametersRegenerateData : MacroFeatureRegenerateData
        {
            internal TParams Parameters { get; set; }
        }

        private PostRebuildMacroFeatureDelegate<TParams> m_PostRebuild;

        /// <inheritdoc/>
        public new event PostRebuildMacroFeatureDelegate<TParams> PostRebuild
        {
            add => m_PostRebuild += value;
            remove => m_PostRebuild -= value;
        }

        internal override bool HandlePostRebuild => m_PostRebuild != null;

        CustomFeatureRebuildResult IXCustomFeatureDefinition<TParams>.OnRebuild(IXApplication app, IXDocument doc, IXCustomFeature<TParams> feature)
            => OnRebuild((ISwApplication)app, (ISwDocument)doc, (ISwMacroFeature)feature, 
                CreateUserIdsManager((ISwApplication)app, (ISwDocument)doc, (ISwMacroFeature)feature));

        /// <inheritdoc/>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override bool OnEditDefinition(ISwApplication app, ISwDocument doc, ISwMacroFeature feature)
            => OnEditDefinition(app, doc, (ISwMacroFeature<TParams>)feature);

        /// <inheritdoc/>
        public virtual CustomFeatureRebuildResult OnRebuild(ISwApplication app, ISwDocument doc, ISwMacroFeature<TParams> feature,
            IUserIdsManager userIdsMgr) => OnRebuild(app, doc, feature);

        /// <inheritdoc/>
        public virtual CustomFeatureRebuildResult OnRebuild(ISwApplication app, ISwDocument doc, ISwMacroFeature<TParams> feature)
            => throw new NotImplementedException("Rebuild routine is not implemented");

        /// <inheritdoc/>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override CustomFeatureRebuildResult OnRebuild(ISwApplication app, ISwDocument doc, ISwMacroFeature feature, IUserIdsManager userIdsMgr)
        {
            var paramsFeat = (SwMacroFeature<TParams>)feature;
            paramsFeat.UseCachedParameters = true;

            IXDimension[] dims;
            string[] dimParamNames;

            paramsFeat.ReadParameters(out dims, out dimParamNames,
                out var _, out var _, out var _);

            var cachedParamsUpdateState = paramsFeat.CachedParametersUpdateState;

            var res = OnRebuild(app, doc, paramsFeat, userIdsMgr);

            if (dims?.Any() == true)
            {
                for (int i = 0; i < dims.Length; i++)
                {
                    OnAlignDimension(paramsFeat, dimParamNames[i], dims[i]);

                    //IMPORTANT: need to dispose otherwise SW will crash once the document is closed
                    ((IDisposable)dims[i]).Dispose();
                }
            }

            if (cachedParamsUpdateState != paramsFeat.CachedParametersUpdateState) 
            {
                paramsFeat.WriteParameters(paramsFeat.Parameters, out _);
            }

            if (HandlePostRebuild)
            {
                AddDataToRebuildQueue(app, doc, (ISwMacroFeature<TParams>)feature, paramsFeat.Parameters);
            }

            return res;
        }

        /// <inheritdoc/>
        public virtual bool OnEditDefinition(ISwApplication app, ISwDocument doc, ISwMacroFeature<TParams> feature) => true;

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        internal override SwMacroFeature CreateMacroFeatureInstance(IFeature feat, SwDocument doc, SwApplication app)
            => new SwMacroFeature<TParams>(feat, doc, app, feat != null);

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        internal override void AddDataToRebuildQueue(ISwApplication app, ISwDocument doc, ISwMacroFeature macroFeatInst)
        {
            //Do nothing, this method is overriden
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        internal virtual void AddDataToRebuildQueue(ISwApplication app, ISwDocument doc, ISwMacroFeature<TParams> macroFeatInst, TParams parameters)
        {
            m_RebuildFeaturesQueue.Add(new MacroFeatureParametersRegenerateData()
            {
                Application = app,
                Document = doc,
                Feature = macroFeatInst,
                Parameters = parameters
            });
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        internal override void DispatchPostBuildData(MacroFeatureRegenerateData data)
        {
            var paramData = (MacroFeatureParametersRegenerateData)data;

            m_PostRebuild?.Invoke(paramData.Application, paramData.Document, (ISwMacroFeature<TParams>)paramData.Feature, paramData.Parameters);
        }

        /// <inheritdoc/>
        public virtual void OnAlignDimension(IXCustomFeature<TParams> feat, string paramName, IXDimension dim)
        {
        }
    }

    /// <inheritdoc/>
    public abstract class SwMacroFeatureDefinition<TParams, TPage> : SwMacroFeatureDefinition<TParams>, IXCustomFeatureDefinition<TParams, TPage>
        where TParams : class
        where TPage : class
    {
        IXBody[] IXCustomFeatureDefinition<TParams, TPage>.CreateGeometry(
            IXApplication app, IXDocument doc, IXCustomFeature<TParams> feat)
            => CreateGeometry((ISwApplication)app, (ISwDocument)doc, (ISwMacroFeature<TParams>)feat)?.Cast<SwBody>().ToArray();

        /// <inheritdoc/>
        void IXCustomFeatureEditorDefinition.Insert(IXDocument doc, object data) => this.Insert(doc, (TParams)data);

        private readonly Lazy<SwMacroFeatureEditor<TParams, TPage>> m_Editor;

        private readonly CustomFeatureParametersParser m_ParamsParser;

        /// <summary>
        /// Default constructor
        /// </summary>
        public SwMacroFeatureDefinition()
        {
            m_ParamsParser = new CustomFeatureParametersParser();

            m_Editor = new Lazy<SwMacroFeatureEditor<TParams, TPage>>(() => 
            {
                var page = new SwPropertyManagerPage<TPage>(Application, Services, CreatePageHandler(), CreateDynamicControls);

                var editor = new SwMacroFeatureEditor<TParams, TPage>(
                    Application, this.GetType(),
                    Services, page, EditorBehavior);

                editor.EditingStarted += OnEditingStarted;
                editor.EditingCompleting += OnEditingCompleting;
                editor.EditingCompleted += OnEditingCompleted;
                editor.FeatureInserting += OnFeatureInserting;
                editor.PreviewUpdated += OnPreviewUpdated;
                editor.ShouldUpdatePreview += OnShouldUpdatePreview;
                editor.ProvidePreviewContext += ProvidePreviewContext;
                editor.HandleEditingException += HandleEditingException;
                return editor;
            });
        }

        /// <summary>
        /// Behavior of macro feature editor
        /// </summary>
        protected virtual CustomFeatureEditorBehavior_e EditorBehavior => CustomFeatureEditorBehavior_e.Default;

        /// <summary>
        /// Override this method to handle the exception reading the macro feature parameters on editing of the macro feature
        /// </summary>
        /// <param name="feat">Feature being edited</param>
        /// <param name="ex">Exception</param>
        /// <returns>Parameters to use for feature editing</returns>
        protected virtual TParams HandleEditingException(IXCustomFeature<TParams> feat, Exception ex) => throw ex;

        /// <inheritdoc/>
        public virtual bool OnShouldUpdatePreview(IXCustomFeature<TParams> feat, TParams oldData, TPage page, bool dataChanged) => true;

        /// <summary>
        /// Create custom page handler
        /// </summary>
        /// <returns>Page handler</returns>
        public virtual SwPropertyManagerPageHandler CreatePageHandler()
            => Services.GetService<IPropertyPageHandlerProvider>().CreateHandler(Application, typeof(TPage));

        /// <inheritdoc/>
        public virtual TParams CreateParameters(IXApplication app, IXDocument doc, TPage page, TParams curParams)
        {
            if (typeof(TParams).IsAssignableFrom(typeof(TPage)))
            {
                return (TParams)(object)page;
            }
            else
            {
                throw new Exception($"Override {nameof(CreateParameters)} to provide the converter from TPage to TParams");
            }
        }

        /// <inheritdoc/>
        public virtual TPage CreatePropertyPage(IXApplication app, IXDocument doc, IXCustomFeature<TParams> feat)
        {
            if (typeof(TPage).IsAssignableFrom(typeof(TParams)))
            {
                return (TPage)(object)feat.Parameters;
            }
            else
            {
                throw new Exception($"Override {nameof(CreatePropertyPage)} to provide the converter from TParams to TPage");
            }
        }

        /// <inheritdoc/>
        public virtual ISwBody[] CreateGeometry(ISwApplication app, ISwDocument doc, ISwMacroFeature<TParams> feat)
            => Array.Empty<ISwTempBody>();

        /// <inheritdoc/>
        public virtual ISwBody[] CreateGeometry(ISwApplication app, ISwDocument doc, ISwMacroFeature<TParams> feat,
            IUserIdsManager userIdsMgr) => CreateGeometry(app, doc, feat);

        /// <inheritdoc/>
        public virtual ISwTempBody[] CreatePreviewGeometry(ISwApplication app, ISwDocument doc, ISwMacroFeature<TParams> feat, TPage page)
            => CreateGeometry(app, doc, feat, CreateEmptyUserIdsManager())
                ?.Cast<ISwTempBody>().ToArray();

        /// <inheritdoc/>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public IXMemoryBody[] CreatePreviewGeometry(IXApplication app, IXDocument doc, IXCustomFeature<TParams> feat, TPage page)
        {
            var data = feat.Parameters;

            //see the description of SwMacroFeatureEditBody for the explanation
            m_ParamsParser.TraverseParametersDefinition(data, (obj, prp) => { }, (dim, obj, prp) => { }, 
                (obj, prp) =>
                {
                    var objData = prp.GetValue(obj);

                    if (objData is IList)
                    {
                        for(int i = 0; i < ((IList)objData).Count; i++)
                        {
                            var body = ((IList)objData)[i];

                            if (body is SwBody) 
                            {
                                ((IList)objData)[i] = CreateEditBody(((SwBody)body).Body, (SwDocument)doc, (SwApplication)app, true);
                            }
                        }
                    }
                    else if(objData is SwBody)
                    {
                        prp.SetValue(obj, CreateEditBody(((SwBody)objData).Body, (SwDocument)doc, (SwApplication)app, true));
                    }
                },
                (obj, prp) => { });

            return CreatePreviewGeometry((ISwApplication)app, (ISwDocument)doc, (ISwMacroFeature<TParams>)feat, page);
        }

        /// <inheritdoc/>
        public void Insert(IXDocument doc, TParams data) => m_Editor.Value.Insert(doc, data);

        /// <inheritdoc/>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override bool OnEditDefinition(ISwApplication app, ISwDocument doc, ISwMacroFeature<TParams> feature)
        {
            ((SwMacroFeature<TParams>)feature).UseCachedParameters = true;
            m_Editor.Value.Edit(doc, feature);
            return true;
        }

        /// <inheritdoc/>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override CustomFeatureRebuildResult OnRebuild(ISwApplication app, ISwDocument doc,
            ISwMacroFeature<TParams> feature, IUserIdsManager userIdsMgr)
            => new CustomFeatureBodyRebuildResult()
            {
                Bodies = CreateGeometry(app, doc, feature, userIdsMgr)?.ToArray()
            };

        /// <inheritdoc/>
        public virtual void OnEditingStarted(IXApplication app, IXDocument doc, IXCustomFeature<TParams> feat, TPage page)
        {
        }

        /// <inheritdoc/>
        public virtual void OnEditingCompleting(IXApplication app, IXDocument doc, IXCustomFeature<TParams> feat, TPage page, PageCloseReasons_e reason)
        {
        }

        /// <inheritdoc/>
        public virtual void OnEditingCompleted(IXApplication app, IXDocument doc, IXCustomFeature<TParams> feat, TPage page, PageCloseReasons_e reason)
        {
        }

        /// <inheritdoc/>
        public virtual void OnFeatureInserting(IXApplication app, IXDocument doc, IXCustomFeature<TParams> feat, TPage page)
            => feat.Commit();

        /// <inheritdoc/>
        public virtual void OnPreviewUpdated(IXApplication app, IXDocument doc, IXCustomFeature<TParams> feat, TPage page)
        {
        }

        /// <inheritdoc/>
        public virtual IControlDescriptor[] CreateDynamicControls(object tag) => null;

        /// <inheritdoc/>
        public virtual void OnAssignPreviewBodyColor(IXCustomFeature<TParams> feat, IXBody body, out System.Drawing.Color color)
        {
            color = System.Drawing.Color.FromArgb(100, System.Drawing.Color.Yellow);
        }

        /// <inheritdoc/>
        public virtual bool OnShouldHidePreviewEditBody(IXCustomFeature<TParams> feat, IXBody body, TPage page)
            => true;

        /// <summary>
        /// Context for the preview of this document
        /// </summary>
        /// <param name="doc">Current document</param>
        /// <param name="feat">Feature being edited</param>
        /// <returns>Either <see cref="IXPart"/> or <see cref="IXComponent"/></returns>
        protected virtual ISwObject ProvidePreviewContext(IXDocument doc, IXCustomFeature<TParams> feat)
        {
            switch (doc)
            {
                case ISwPart part:
                    return part;

                case ISwAssembly assm:
                    return assm.EditingComponent;

                default:
                    throw new NotSupportedException("Not supported preview context");
            }
        }

        /// <summary>
        /// Creates an instance of empty User IDs manager (usually used in the preview)
        /// </summary>
        /// <returns></returns>
        protected virtual IUserIdsManager CreateEmptyUserIdsManager() => new EmptyUserIdsManager();
    }
}