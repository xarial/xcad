//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
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
using Xarial.XCad.Toolkit.Utils;
using Xarial.XCad.UI;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Enums;
using Xarial.XCad.Utils.CustomFeature;
using Xarial.XCad.Utils.Diagnostics;
using Xarial.XCad.Utils.Reflection;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

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
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        protected class MacroFeatureRegenerateData 
        {
            internal ISwApplication Application { get; set; }
            internal ISwDocument Document { get; set; }
            internal ISwMacroFeature Feature { get; set; }
        }

        private static SwMacroFeature CreateMacroFeatureInstance(SwMacroFeatureDefinition sender, IFeature feat, SwDocument doc, SwApplication app)
            => new SwMacroFeature(feat, doc, app, true);

        public event ConfigureServicesDelegate ConfigureServices;

        /// <summary>
        /// Called when macro feature is rebuild
        /// </summary>
        public event PostRebuildMacroFeatureDelegate PostRebuild 
        {
            add 
            {
                m_PostRebuild += value;
                m_HandlePostRebuild = m_PostRebuild != null;
            }
            remove 
            {
                m_PostRebuild -= value;
                m_HandlePostRebuild = m_PostRebuild != null;
            }
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
        protected readonly IXLogger m_Logger;

        public IXLogger Logger
        {
            get
            {
                return m_Logger;
            }
        }

        protected readonly IServiceProvider m_SvcProvider;

        protected readonly List<MacroFeatureRegenerateData> m_RebuildFeaturesQueue;

        private bool m_IsSubscribedToIdle;

        private readonly Func<SwMacroFeatureDefinition, IFeature, SwDocument, SwApplication, SwMacroFeature> m_MacroFeatInstFact;

        internal SwMacroFeatureDefinition(Func<SwMacroFeatureDefinition, IFeature, SwDocument, SwApplication, SwMacroFeature> macroFeatInstFact) 
        {
            m_MacroFeatInstFact = macroFeatInstFact;

            string provider = "";
            this.GetType().TryGetAttribute<MissingDefinitionErrorMessage>(a =>
            {
                provider = a.Message;
            });

            m_Provider = provider;

            m_RebuildFeaturesQueue = new List<MacroFeatureRegenerateData>();

            m_IsSubscribedToIdle = false;

            var svcColl = Application.CustomServices.Clone();

            svcColl.Add<IXLogger>(() => new TraceLogger($"xCad.MacroFeature.{this.GetType().FullName}"), ServiceLifetimeScope_e.Singleton, false);
            svcColl.Add<IIconsCreator, BaseIconsCreator>(ServiceLifetimeScope_e.Singleton, false);

            OnConfigureServices(svcColl);

            m_SvcProvider = svcColl.CreateProvider();

            m_Logger = m_SvcProvider.GetService<IXLogger>();

            CustomFeatureDefinitionInstanceCache.RegisterInstance(this);

            TryCreateIcons(m_SvcProvider.GetService<IIconsCreator>(), MacroFeatureIconInfo.GetLocation(this.GetType()));
        }

        public SwMacroFeatureDefinition() : this(CreateMacroFeatureInstance)
        {
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
                LogOperation("Editing feature", app as ISldWorks, modelDoc as IModelDoc2, feature as IFeature);

                var doc = (SwDocument)Application.Documents[modelDoc as IModelDoc2];
                return OnEditDefinition(Application, doc, m_MacroFeatInstFact.Invoke(this, feature as IFeature, doc, Application));
            }
            catch(Exception ex) 
            {
                m_Logger.Log(ex);
                return HandleEditException(ex);
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public object Regenerate(object app, object modelDoc, object feature)
        {
            try
            {
                LogOperation("Regenerating feature", app as ISldWorks, modelDoc as IModelDoc2, feature as IFeature);

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
                
                var macroFeatInst = m_MacroFeatInstFact.Invoke(this, feature as IFeature, contextDoc, Application);

                var res = OnRebuild(Application, doc, macroFeatInst);

                if (m_HandlePostRebuild)
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
            try
            {
                var doc = (SwDocument)Application.Documents[modelDoc as IModelDoc2];
                return (int)OnUpdateState(Application, doc, m_MacroFeatInstFact.Invoke(this, feature as IFeature, doc, Application));
            }
            catch(Exception ex) 
            {
                m_Logger.Log(ex);
                return HandleStateException(ex);
            }
        }

        protected virtual object HandleEditException(Exception ex) => throw ex;

        protected virtual object HandleStateException(Exception ex) => throw ex;

        protected virtual void AddDataToRebuildQueue(ISwApplication app, ISwDocument doc, ISwMacroFeature macroFeatInst)
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

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual void DispatchPostBuildData(MacroFeatureRegenerateData data)
            => m_PostRebuild?.Invoke(data.Application, data.Document, data.Feature);

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

        protected bool m_HandlePostRebuild;

        public virtual void AssignFaceIds(ISldWorks app, IModelDoc2 model, IReadOnlyDictionary<Face2, MacroFeatureEntityId> faces) 
        {
            int nextId = 0;

            foreach (var face in faces)
            {
                face.Value.FirstId = nextId++;
                face.Value.SecondId = 0;
            }
        }

        public virtual void AssignEdgeIds(ISldWorks app, IModelDoc2 model, IReadOnlyDictionary<Edge, MacroFeatureEntityId> edges)
        {
            int nextId = 0;

            foreach (var edge in edges)
            {
                edge.Value.FirstId = nextId++;
                edge.Value.SecondId = 0;
            }
        }

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
        private static SwMacroFeature CreateMacroFeatureInstance(SwMacroFeatureDefinition sender, IFeature feat, SwDocument doc, SwApplication app)
            => new SwMacroFeature<TParams>(feat, doc, app, ((SwMacroFeatureDefinition<TParams>)sender).m_ParamsParser, true);

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        protected class MacroFeatureParametersRegenerateData : MacroFeatureRegenerateData
        {
            internal TParams Parameters { get; set; }
        }

        private PostRebuildMacroFeatureDelegate<TParams> m_PostRebuild;

        public new event PostRebuildMacroFeatureDelegate<TParams> PostRebuild
        {
            add
            {
                m_PostRebuild += value;
                m_HandlePostRebuild = m_PostRebuild != null;
            }
            remove
            {
                m_PostRebuild -= value;
                m_HandlePostRebuild = m_PostRebuild != null;
            }
        }

        private readonly MacroFeatureParametersParser m_ParamsParser;

        CustomFeatureRebuildResult IXCustomFeatureDefinition<TParams>.OnRebuild(IXApplication app, IXDocument doc, IXCustomFeature feature, TParams parameters, out AlignDimensionDelegate<TParams> alignDim)
            => OnRebuild((ISwApplication)app, (ISwDocument)doc, (ISwMacroFeature<TParams>)feature, parameters, out alignDim);

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override bool OnEditDefinition(ISwApplication app, ISwDocument doc, ISwMacroFeature feature)
            => OnEditDefinition(app, doc, (ISwMacroFeature<TParams>)feature);

        public SwMacroFeatureDefinition() : this(new MacroFeatureParametersParser())
        {
        }

        internal SwMacroFeatureDefinition(MacroFeatureParametersParser paramsParser) : base(CreateMacroFeatureInstance)
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

        public abstract CustomFeatureRebuildResult OnRebuild(ISwApplication app, ISwDocument doc, ISwMacroFeature<TParams> feature,
            TParams parameters, out AlignDimensionDelegate<TParams> alignDim);

        public override CustomFeatureRebuildResult OnRebuild(ISwApplication app, ISwDocument doc, ISwMacroFeature feature)
        {
            IXDimension[] dims;
            string[] dimParamNames;
            var param = (TParams)m_ParamsParser.GetParameters(feature, doc, typeof(TParams), out dims, out dimParamNames,
                out IXBody[] _, out IXSelObject[] _, out CustomFeatureOutdateState_e _);

            AlignDimensionDelegate<TParams> alignDimsDel;
            var res = OnRebuild(app, doc, (ISwMacroFeature<TParams>)feature, param, out alignDimsDel);

            if (dims?.Any() == true)
            {
                if (alignDimsDel != null)
                {
                    for (int i = 0; i < dims.Length; i++)
                    {
                        alignDimsDel.Invoke(dimParamNames[i], dims[i]);

                        //IMPORTANT: need to dispose otherwise SW will crash once the document is closed
                        ((IDisposable)dims[i]).Dispose();
                    }
                }
            }

            if (m_HandlePostRebuild)
            {
                AddDataToRebuildQueue(app, doc, (ISwMacroFeature<TParams>)feature, param);
            }

            return res;
        }

        public virtual bool OnEditDefinition(ISwApplication app, ISwDocument doc, ISwMacroFeature<TParams> feature) 
        {
            return true;
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        protected override void AddDataToRebuildQueue(ISwApplication app, ISwDocument doc, ISwMacroFeature macroFeatInst)
        {
            //Do nothing, this method is overriden
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual void AddDataToRebuildQueue(ISwApplication app, ISwDocument doc, ISwMacroFeature<TParams> macroFeatInst, TParams parameters)
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
        protected override void DispatchPostBuildData(MacroFeatureRegenerateData data)
        {
            var paramData = (MacroFeatureParametersRegenerateData)data;

            m_PostRebuild?.Invoke(paramData.Application, paramData.Document, (ISwMacroFeature<TParams>)paramData.Feature, paramData.Parameters);
        }
    }

    /// <inheritdoc/>
    public abstract class SwMacroFeatureDefinition<TParams, TPage> : SwMacroFeatureDefinition<TParams>, IXCustomFeatureDefinition<TParams, TPage>
        where TParams : class
        where TPage : class
    {
        IXBody[] IXCustomFeatureDefinition<TParams, TPage>.CreateGeometry(
            IXApplication app, IXDocument doc, TParams data, out AlignDimensionDelegate<TParams> alignDim)
            => CreateGeometry((ISwApplication)app, (ISwDocument)doc, data, out alignDim).Cast<SwBody>().ToArray();

        private readonly MacroFeatureParametersParser m_ParamsParser;

        private readonly Lazy<SwMacroFeatureEditor<TParams, TPage>> m_Editor;

        public SwMacroFeatureDefinition() : this(new MacroFeatureParametersParser())
        {
        }

        internal SwMacroFeatureDefinition(MacroFeatureParametersParser parser) : base(parser)
        {
            m_ParamsParser = parser;

            m_Editor = new Lazy<SwMacroFeatureEditor<TParams, TPage>>(() => 
            {
                var page = new SwPropertyManagerPage<TPage>(Application, m_SvcProvider, CreatePageHandler(), CreateDynamicControls);

                var editor = new SwMacroFeatureEditor<TParams, TPage>(
                    Application, this.GetType(),
                    m_ParamsParser, m_SvcProvider, page, EditorBehavior);

                editor.EditingStarted += OnEditingStarted;
                editor.EditingCompleting += OnEditingCompleting;
                editor.EditingCompleted += OnEditingCompleted;
                editor.FeatureInserted += OnFeatureInserted;
                editor.PreviewUpdated += OnPreviewUpdated;
                editor.ShouldUpdatePreview += ShouldUpdatePreview;
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

        /// <summary>
        /// Checks if the preview should be updated
        /// </summary>
        /// <param name="oldData">Old parameters</param>
        /// <param name="newData">New parameters</param>
        /// <param name="page">Current page data</param>
        /// <param name="dataChanged">Indicates if the parameters of the data have changed</param>
        /// <remarks>This method is called everytime property manager page data is changed, however this is not always require preview update</remarks>
        public virtual bool ShouldUpdatePreview(TParams oldData, TParams newData, TPage page, bool dataChanged) => true;

        /// <summary>
        /// Create custom page handler
        /// </summary>
        /// <returns>Page handler</returns>
        public virtual SwPropertyManagerPageHandler CreatePageHandler()
            => m_SvcProvider.GetService<IPropertyPageHandlerProvider>().CreateHandler(Application, typeof(TPage));

        /// <inheritdoc/>
        public virtual TParams ConvertPageToParams(IXApplication app, IXDocument doc, TPage page, TParams curParams)
        {
            if (typeof(TParams).IsAssignableFrom(typeof(TPage)))
            {
                return (TParams)((object)page);
            }
            else
            {
                throw new Exception($"Override {nameof(ConvertPageToParams)} to provide the converter from TPage to TParams");
            }
        }

        /// <inheritdoc/>
        public virtual TPage ConvertParamsToPage(IXApplication app, IXDocument doc, TParams par)
        {
            if (typeof(TPage).IsAssignableFrom(typeof(TParams)))
            {
                return (TPage)((object)par);
            }
            else
            {
                throw new Exception($"Override {nameof(ConvertParamsToPage)} to provide the converter from TParams to TPage");
            }
        }

        /// <inheritdoc/>
        public virtual ISwBody[] CreateGeometry(ISwApplication app, ISwDocument doc, TParams data, out AlignDimensionDelegate<TParams> alignDim) 
        {
            alignDim = null;
            return CreateGeometry(app, doc, data);
        }

        /// <inheritdoc/>
        public virtual ISwBody[] CreatePreviewGeometry(ISwApplication app, ISwDocument doc, TParams data, TPage page,
            out ShouldHidePreviewEditBodyDelegate<TParams, TPage> shouldHidePreviewEdit,
            out AssignPreviewBodyColorDelegate assignPreviewColor)
        {
            shouldHidePreviewEdit = null;
            assignPreviewColor = null;

            return CreatePreviewGeometry(app, doc, data, page);
        }

        /// <inheritdoc/>
        public virtual ISwBody[] CreateGeometry(ISwApplication app, ISwDocument doc, TParams data) => new ISwBody[0];

        /// <inheritdoc/>
        public virtual ISwBody[] CreatePreviewGeometry(ISwApplication app, ISwDocument doc, TParams data, TPage page)
            => CreateGeometry(app, doc, data, out _);

        /// <inheritdoc/>
        public IXBody[] CreatePreviewGeometry(IXApplication app, IXDocument doc, TParams data, TPage page,
            out ShouldHidePreviewEditBodyDelegate<TParams, TPage> shouldHidePreviewEdit,
            out AssignPreviewBodyColorDelegate assignPreviewColor)
        {
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
                                ((IList)objData)[i] = SwMacroFeatureEditBody.CreateMacroFeatureEditBody(((SwBody)body).Body, (SwDocument)doc, (SwApplication)app, true);
                            }
                        }
                    }
                    else if(objData is SwBody)
                    {
                        prp.SetValue(obj, SwMacroFeatureEditBody.CreateMacroFeatureEditBody(((SwBody)objData).Body, (SwDocument)doc, (SwApplication)app, true));
                    }
                },
                (obj, prp) => { });

            return CreatePreviewGeometry((ISwApplication)app, (ISwDocument)doc, data, page,
                out shouldHidePreviewEdit, out assignPreviewColor).Cast<SwBody>().ToArray();
        }

        /// <inheritdoc/>
        public void Insert(IXDocument doc, TParams data)
            => m_Editor.Value.Insert(doc, data);

        public override bool OnEditDefinition(ISwApplication app, ISwDocument doc, ISwMacroFeature<TParams> feature)
        {
            m_Editor.Value.Edit(doc, feature);
            return true;
        }

        public override CustomFeatureRebuildResult OnRebuild(ISwApplication app, ISwDocument doc,
            ISwMacroFeature<TParams> feature, TParams parameters, out AlignDimensionDelegate<TParams> alignDim)
            => new CustomFeatureBodyRebuildResult()
            {
                Bodies = CreateGeometry(app, doc, parameters, out alignDim).ToArray()
            };

        /// <summary>
        /// Called when macro feature is about to be edited before Property Manager Page is opened
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="doc">Document</param>
        /// <param name="feat">Feature being edited (null if feature is being inserted)</param>
        /// <param name="data">Macro feature data (null if feature is being inserted)</param>
        /// <param name="page">Page data</param>
        public virtual void OnEditingStarted(IXApplication app, IXDocument doc, IXCustomFeature<TParams> feat, TParams data, TPage page)
        {
        }

        /// <summary>
        /// Called when macro feature is finishing editing and Property Manager Page is about to be closed
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="doc">Document</param>
        /// <param name="feat">Feature being edited</param>
        /// <param name="data">Macro feature data</param>
        /// <param name="page">Page data</param>
        /// <param name="reason">Closing reason</param>
        public virtual void OnEditingCompleting(IXApplication app, IXDocument doc, IXCustomFeature<TParams> feat, TParams data, TPage page, PageCloseReasons_e reason)
        {
        }

        /// <summary>
        /// Called when macro feature is finished editing and Property Manager Page is closed
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="doc">Document</param>
        /// <param name="feat">Feature being edited</param>
        /// <param name="data">Macro feature data</param>
        /// <param name="page">Page data</param>
        /// <param name="reason">Closing reason</param>
        public virtual void OnEditingCompleted(IXApplication app, IXDocument doc, IXCustomFeature<TParams> feat, TParams data, TPage page, PageCloseReasons_e reason)
        {
        }

        /// <summary>
        /// Called when macro feature is created
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="doc">Document</param>
        /// <param name="feat">Feature which is created</param>
        /// <param name="data">Macro feature data</param>
        /// <param name="page">Page data</param>
        public virtual void OnFeatureInserted(IXApplication app, IXDocument doc, IXCustomFeature<TParams> feat, TParams data, TPage page)
        {
        }

        /// <summary>
        /// Called when the preview of the macro feature updated
        /// </summary>
        /// <param name="app">Application</param>
        /// <param name="doc">Document</param>
        /// <param name="feat">Feature being edited</param>
        /// <param name="page">Current page data</param>
        /// <remarks>Use <see cref="ShouldUpdatePreview(TParams, TParams, TPage, bool)"/> to control if preview needs to be updated</remarks>
        public virtual void OnPreviewUpdated(IXApplication app, IXDocument doc, IXCustomFeature<TParams> feat, TPage page)
        {
        }

        public virtual IControlDescriptor[] CreateDynamicControls(object tag) => null;

        /// <summary>
        /// Context for the preview of this document
        /// </summary>
        /// <param name="doc">Current document</param>
        /// <returns>Either <see cref="IXPart"/> or <see cref="IXComponent"/></returns>
        protected virtual ISwObject ProvidePreviewContext(IXDocument doc)
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
    }
}