//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
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

namespace Xarial.XCad.Utils.CustomFeature
{
    public abstract class BaseCustomFeatureEditor<TData, TPage> 
        where TData : class, new()
        where TPage : class, new()
    {
        protected readonly IXApplication m_App;
        protected readonly IXLogger m_Logger;

        private readonly XObjectEqualityComparer<IXBody> m_BodiesComparer;
        private readonly CustomFeatureParametersParser m_ParamsParser;
        private readonly Type m_DefType;

        private TPage m_CurData;
        private IXBody[] m_EditBodies;
        private IXCustomFeature<TData> m_EditingFeature;
        private Exception m_LastError;
        private IXPropertyPage<TPage> m_PmPage;
        private IXBody[] m_PreviewBodies;

        protected IXDocument CurModel { get; private set; }

        public BaseCustomFeatureEditor(IXApplication app,
            Type featDefType,
            CustomFeatureParametersParser paramsParser,
            IXLogger logger)
        {
            m_App = app;
            m_Logger = logger;

            m_DefType = featDefType;

            m_BodiesComparer = new XObjectEqualityComparer<IXBody>();

            m_PmPage = CreatePage();

            m_ParamsParser = paramsParser;

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

                m_CurData = Definition.ConvertParamsToPage(featParam);

                m_PmPage.Show(m_CurData);
                UpdatePreview();
            }
            catch
            {
                m_EditingFeature.Parameters = null;
            }
        }

        public void Insert(IXDocument model)
        {
            m_CurData = new TPage();

            CurModel = model;

            m_EditingFeature = null;

            m_PmPage.Show(m_CurData);
        }

        protected abstract void DisplayPreview(IXBody[] bodies);

        protected abstract void HidePreview(IXBody[] bodies);

        protected abstract IXPropertyPage<TPage> CreatePage();

        private void HideEditBodies()
        {
            IXBody[] editBodies;

            m_ParamsParser.Parse(m_CurData, out _, out _, out _, out _, out editBodies);

            var bodiesToShow = m_EditBodies.ValueOrEmpty().Except(editBodies.ValueOrEmpty(), m_BodiesComparer);

            foreach (var body in bodiesToShow)
            {
                body.Visible = true;
            }

            var bodiesToHide = editBodies.ValueOrEmpty().Except(m_EditBodies.ValueOrEmpty(), m_BodiesComparer);

            foreach (var body in bodiesToHide)
            {
                body.Visible = false;
            }

            m_EditBodies = editBodies;
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
        }

        private void OnPageClosed(PageCloseReasons_e reason)
        {
            foreach (var body in m_EditBodies.ValueOrEmpty())
            {
                body.Visible = true;
            }

            HidePreviewBodies();

            m_EditBodies = null;
            m_PreviewBodies = null;

            if (reason == PageCloseReasons_e.Okay)
            {
                if (m_EditingFeature == null)
                {
                    var feat = CurModel.Features.PreCreateCustomFeature<TData>();
                    feat.DefinitionType = m_DefType;
                    feat.Parameters = Definition.ConvertPageToParams(m_CurData);
                    CurModel.Features.Add(feat);

                    if (feat == null)
                    {
                        throw new NullReferenceException("Failed to create custom feature");
                    }
                }
                else
                {
                    m_EditingFeature.Parameters = Definition.ConvertPageToParams(m_CurData);
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

        private void OnPageClosing(PageCloseReasons_e reason, PageClosingArg arg)
        {
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
                    Definition.ConvertPageToParams(m_CurData), true, out _);

                HideEditBodies();

                if (m_PreviewBodies != null)
                {
                    DisplayPreview(m_PreviewBodies);
                }
            }
            catch (Exception ex)
            {
                HidePreviewBodies();
                m_LastError = ex;
            }
        }
    }
}