//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Linq;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Extensions;
using Xarial.XCad.Features.CustomFeature;
using Xarial.XCad.Features.CustomFeature.Delegates;
using Xarial.XCad.Geometry;
using Xarial.XCad.Services;
using Xarial.XCad.Toolkit.CustomFeature;
using Xarial.XCad.UI.PropertyPage;
using Xarial.XCad.UI.PropertyPage.Enums;
using Xarial.XCad.UI.PropertyPage.Structures;
using Xarial.XCad.Utils.Diagnostics;
using Xarial.XCad.Utils.Reflection;
using Xarial.XCad.Toolkit;
using Xarial.XCad.UI.PropertyPage.Delegates;

namespace Xarial.XCad.Utils.CustomFeature
{
    public delegate void CustomFeatureStateChangedDelegate<TData>(IXApplication app, IXDocument doc, IXCustomFeature<TData> feat)
        where TData : class, new();

    public delegate void CustomFeaturePageParametersChangedDelegate<TPageData, TData>(IXApplication app, IXDocument doc, IXCustomFeature<TData> feat, TPageData pageData)
        where TPageData : class, new()
        where TData : class, new();

    public delegate void CustomFeatureInsertedDelegate<TData>(IXApplication app, IXDocument doc, IXCustomFeature<TData> feat, TData data)
        where TData : class, new();

    public delegate void CustomFeatureEditingCompletedDelegate<TData>(IXApplication app, IXDocument doc, IXCustomFeature<TData> feat, TData data, PageCloseReasons_e reason)
        where TData : class, new();

    public abstract class BaseCustomFeatureEditor<TData, TPage> 
        where TData : class, new()
        where TPage : class, new()
    {
        public event CustomFeatureStateChangedDelegate<TData> EditingStarted;
        public event CustomFeatureEditingCompletedDelegate<TData> EditingCompleting;
        public event CustomFeatureEditingCompletedDelegate<TData> EditingCompleted;
        public event CustomFeatureInsertedDelegate<TData> FeatureInserted;
        public event CustomFeaturePageParametersChangedDelegate<TPage, TData> PageParametersChanged;

        protected readonly IXApplication m_App;
        protected readonly IServiceProvider m_SvcProvider;
        protected readonly IXLogger m_Logger;

        private readonly XObjectEqualityComparer<IXBody> m_BodiesComparer;
        private readonly CustomFeatureParametersParser m_ParamsParser;
        private readonly Type m_DefType;

        private TPage m_CurPageData;
        private IXBody[] m_HiddenEditBodies;
        private IXCustomFeature<TData> m_EditingFeature;
        private Exception m_LastError;
        private IXPropertyPage<TPage> m_PmPage;
        private IXBody[] m_PreviewBodies;

        protected IXDocument CurModel { get; private set; }

        public BaseCustomFeatureEditor(IXApplication app,
            Type featDefType,
            CustomFeatureParametersParser paramsParser,
            IServiceProvider svcProvider, CreateDynamicControlsDelegate createDynCtrlHandler)
            : this(app, featDefType, paramsParser, svcProvider)
        {
            InitPage(createDynCtrlHandler);
        }

        protected BaseCustomFeatureEditor(IXApplication app,
            Type featDefType,
            CustomFeatureParametersParser paramsParser,
            IServiceProvider svcProvider)
        {
            m_App = app;
            m_SvcProvider = svcProvider;
            m_Logger = svcProvider.GetService<IXLogger>();
            m_DefType = featDefType;
            m_BodiesComparer = new XObjectEqualityComparer<IXBody>();
            m_ParamsParser = paramsParser;
        }

        protected void InitPage(CreateDynamicControlsDelegate createDynCtrlHandler)
        {
            m_PmPage = CreatePage(createDynCtrlHandler);

            m_PmPage.Closing += OnPageClosing;
            m_PmPage.DataChanged += OnDataChanged;
            m_PmPage.Closed += OnPageClosed;
        }

        private IXCustomFeatureDefinition<TData, TPage> m_Definition;

        private IXCustomFeatureDefinition<TData, TPage> Definition 
        {
            get 
            {
                return m_Definition ?? (m_Definition = (IXCustomFeatureDefinition<TData, TPage>)CustomFeatureDefinitionInstanceCache.GetInstance(m_DefType));
            }
        }

        public void Edit(IXDocument model, IXCustomFeature<TData> feature)
        {
            CurModel = model;
            m_EditingFeature = feature;

            try
            {
                var featParam = m_EditingFeature.Parameters;

                m_CurPageData = Definition.ConvertParamsToPage(featParam);

                EditingStarted?.Invoke(m_App, model, feature);
                m_PmPage.Show(m_CurPageData);

                UpdatePreview();
            }
            catch
            {
                m_EditingFeature.Parameters = null;
            }
        }

        public void Insert(IXDocument model)
        {
            m_CurPageData = new TPage();

            CurModel = model;

            m_EditingFeature = null;

            EditingStarted?.Invoke(m_App, model, null);

            m_PmPage.Show(m_CurPageData);

            UpdatePreview();
        }

        protected abstract void DisplayPreview(IXBody[] bodies);

        protected abstract void HidePreview(IXBody[] bodies);

        protected abstract IXPropertyPage<TPage> CreatePage(CreateDynamicControlsDelegate createDynCtrlHandler);

        private void HideEditBodies()
        {
            IXBody[] editBodies;

            m_ParamsParser.Parse(Definition.ConvertPageToParams(m_CurPageData), out _, out _, out _, out _, out editBodies);

            var bodiesToShow = m_HiddenEditBodies.ValueOrEmpty().Except(editBodies.ValueOrEmpty(), m_BodiesComparer);

            foreach (var body in bodiesToShow)
            {
                body.Visible = true;
            }

            var bodiesToHide = editBodies.ValueOrEmpty().Except(m_HiddenEditBodies.ValueOrEmpty(), m_BodiesComparer);

            foreach (var body in bodiesToHide)
            {
                body.Visible = false;
            }

            m_HiddenEditBodies = editBodies;
        }

        private void HidePreviewBodies()
        {
            if (m_PreviewBodies != null)
            {
                HidePreview(m_PreviewBodies);
            }

            m_PreviewBodies = null;
        }

        private void OnDataChanged()
        {
            UpdatePreview();

            PageParametersChanged?.Invoke(m_App, CurModel, m_EditingFeature, m_CurPageData);
        }

        private void OnPageClosed(PageCloseReasons_e reason)
        {
            var data = Definition.ConvertPageToParams(m_CurPageData);

            EditingCompleted?.Invoke(m_App, CurModel, m_EditingFeature, data, reason);

            ShowEditBodies();

            HidePreviewBodies();

            m_PreviewBodies = null;

            if (reason == PageCloseReasons_e.Okay)
            {
                if (m_EditingFeature == null)
                {
                    var feat = CurModel.Features.PreCreateCustomFeature<TData>();
                    feat.DefinitionType = m_DefType;
                    feat.Parameters = data;
                    CurModel.Features.Add(feat);

                    if (feat == null)
                    {
                        throw new NullReferenceException("Failed to create custom feature");
                    }

                    FeatureInserted?.Invoke(m_App, CurModel, feat, data);
                }
                else
                {
                    m_EditingFeature.Parameters = data;
                }
            }
            else
            {
                if (m_EditingFeature != null)
                {
                    m_EditingFeature.Parameters = null;
                }
            }
        }

        private void ShowEditBodies()
        {
            foreach (var body in m_HiddenEditBodies.ValueOrEmpty())
            {
                body.Visible = true;
            }

            m_HiddenEditBodies = null;
        }

        private void OnPageClosing(PageCloseReasons_e reason, PageClosingArg arg)
        {
            if (EditingCompleting != null)
            {
                try
                {
                    var data = Definition.ConvertPageToParams(m_CurPageData);
                    EditingCompleting.Invoke(m_App, CurModel, m_EditingFeature, data, reason);
                }
                catch (Exception ex)
                {
                    m_LastError = ex;
                }
            }

            if (m_LastError != null)
            {
                arg.ErrorMessage = m_LastError.Message;
                arg.Cancel = true;
            }
        }

        private void UpdatePreview()
        {
            try
            {
                m_LastError = null;

                HidePreviewBodies();

                m_PreviewBodies = Definition.CreateGeometry(m_App, CurModel,
                    Definition.ConvertPageToParams(m_CurPageData), true, out _);

                HideEditBodies();

                if (m_PreviewBodies != null)
                {
                    DisplayPreview(m_PreviewBodies);
                }
            }
            catch (Exception ex)
            {
                HidePreviewBodies();
                ShowEditBodies();
                m_LastError = ex;
            }
        }
    }
}