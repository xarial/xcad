//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
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
using System.Collections.Generic;
using Xarial.XCad.Exceptions;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.Utils.CustomFeature
{
    public delegate void CustomFeatureStateChangedDelegate<TData, TPage>(IXApplication app, IXDocument doc, IXCustomFeature<TData> feat, TData data, TPage page)
        where TData : class, new()
        where TPage : class, new();

    public delegate void CustomFeaturePageParametersChangedDelegate<TData, TPage>(IXApplication app, IXDocument doc, IXCustomFeature<TData> feat, TPage page)
        where TData : class, new()
        where TPage : class, new();

    public delegate void CustomFeatureInsertedDelegate<TData, TPage>(IXApplication app, IXDocument doc, IXCustomFeature<TData> feat, TData data, TPage page)
        where TData : class, new()
        where TPage : class, new();

    public delegate void CustomFeatureEditingCompletedDelegate<TData, TPage>(IXApplication app, IXDocument doc, IXCustomFeature<TData> feat, TData data, TPage page, PageCloseReasons_e reason)
        where TData : class, new()
        where TPage : class, new();

    public delegate bool ShouldUpdatePreviewDelegate<TData, TPage>(TData oldData, TData newData, TPage page, bool dataChanged)
        where TData : class, new()
        where TPage : class, new();

    public abstract class BaseCustomFeatureEditor<TData, TPage> 
        where TData : class, new()
        where TPage : class, new()
    {
        public event CustomFeatureStateChangedDelegate<TData, TPage> EditingStarted;
        public event CustomFeatureEditingCompletedDelegate<TData, TPage> EditingCompleting;
        public event CustomFeatureEditingCompletedDelegate<TData, TPage> EditingCompleted;
        public event CustomFeatureInsertedDelegate<TData, TPage> FeatureInserted;
        public event CustomFeaturePageParametersChangedDelegate<TData, TPage> PreviewUpdated;
        public event ShouldUpdatePreviewDelegate<TData, TPage> ShouldUpdatePreview;

        protected readonly IXApplication m_App;
        protected readonly IServiceProvider m_SvcProvider;
        protected readonly IXLogger m_Logger;

        private readonly XObjectEqualityComparer<IXBody> m_BodiesComparer;
        private readonly CustomFeatureParametersParser m_ParamsParser;
        private readonly Type m_DefType;

        private readonly IXPropertyPage<TPage> m_PmPage;
        private readonly Lazy<IXCustomFeatureDefinition<TData, TPage>> m_DefinitionLazy;

        private TPage m_CurPageData;
        private TData m_CurData;
        private IXBody[] m_HiddenEditBodies;
        private IXCustomFeature<TData> m_EditingFeature;
        private Exception m_LastError;
        private IXBody[] m_PreviewBodies;

        protected IXDocument CurModel { get; private set; }
        
        private bool m_IsPageActive;

        public BaseCustomFeatureEditor(IXApplication app,
            Type featDefType,
            CustomFeatureParametersParser paramsParser,
            IServiceProvider svcProvider, IXPropertyPage<TPage> page)
        {
            m_App = app;
            m_SvcProvider = svcProvider;
            m_Logger = svcProvider.GetService<IXLogger>();
            m_DefType = featDefType;
            m_BodiesComparer = new XObjectEqualityComparer<IXBody>();
            m_ParamsParser = paramsParser;

            m_DefinitionLazy = new Lazy<IXCustomFeatureDefinition<TData, TPage>>(
                () => (IXCustomFeatureDefinition<TData, TPage>)CustomFeatureDefinitionInstanceCache.GetInstance(m_DefType));

            m_PmPage = page;

            m_PmPage.Closing += OnPageClosing;
            m_PmPage.DataChanged += OnDataChanged;
            m_PmPage.Closed += OnPageClosed;
        }
        
        private IXCustomFeatureDefinition<TData, TPage> Definition => m_DefinitionLazy.Value;

        public void Edit(IXDocument model, IXCustomFeature<TData> feature)
        {
            m_IsPageActive = true;

            CurModel = model;
            m_EditingFeature = feature;

            try
            {
                m_CurData = m_EditingFeature.Parameters;

                m_CurPageData = Definition.ConvertParamsToPage(m_App, model, m_CurData);

                EditingStarted?.Invoke(m_App, model, feature, m_CurData, m_CurPageData);

                m_PmPage.Show(m_CurPageData);

                UpdatePreview();
            }
            catch(Exception ex)
            {
                m_Logger.Log(ex);
                m_EditingFeature.Parameters = null;
                throw;
            }
        }

        public void Insert(IXDocument model, TData data)
        {
            m_IsPageActive = true;
            
            CurModel = model;

            m_EditingFeature = null;

            m_CurData = data;

            m_CurPageData = Definition.ConvertParamsToPage(m_App, model, m_CurData);

            EditingStarted?.Invoke(m_App, model, null, m_CurData, m_CurPageData);

            m_PmPage.Show(m_CurPageData);

            UpdatePreview();
        }

        protected abstract void DisplayPreview(IXBody[] bodies, AssignPreviewBodyColorDelegate assignPreviewBodyColorDelegateFunc);

        protected abstract void HidePreview(IXBody[] bodies);

        private void HideEditBodies(ShouldHidePreviewEditBodyDelegate<TData, TPage> shouldHidePreviewEditBodyFunc)
        {
            IXBody[] editBodies;

            m_ParamsParser.Parse(m_CurData, out _, out _, out _, out _, out editBodies);

            var bodiesToShow = m_HiddenEditBodies.ValueOrEmpty().Except(editBodies.ValueOrEmpty(), m_BodiesComparer);

            foreach (var body in bodiesToShow)
            {
                body.Visible = true;
            }

            var doNotHideBodies = new List<IXBody>();

            var bodiesToHide = editBodies.ValueOrEmpty().Except(m_HiddenEditBodies.ValueOrEmpty(), m_BodiesComparer);

            foreach (var body in bodiesToHide)
            {
                var hide = body.Visible;

                if (hide && shouldHidePreviewEditBodyFunc != null) 
                {
                    hide &= shouldHidePreviewEditBodyFunc.Invoke(body, m_CurData, m_CurPageData);
                }

                if (hide)
                {   
                    body.Visible = false;
                }
                else 
                {
                    doNotHideBodies.Add(body);
                }
            }

            if (editBodies != null)
            {
                m_HiddenEditBodies = editBodies.Except(doNotHideBodies).ToArray();
            }
            else 
            {
                m_HiddenEditBodies = null;
            }
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
            var oldParams = m_CurData;
            m_CurData = Definition.ConvertPageToParams(m_App, CurModel, m_CurPageData);

            var dataChanged = AreParametersChanged(oldParams, m_CurData);

            var needUpdatePreview = ShouldUpdatePreview.Invoke(oldParams, m_CurData, m_CurPageData, dataChanged);

            if (needUpdatePreview)
            {
                UpdatePreview();

                PreviewUpdated?.Invoke(m_App, CurModel, m_EditingFeature, m_CurPageData);
            }
        }

        private bool AreParametersChanged(TData oldParams, TData newParams) 
        {
            bool AreArraysEqual<T>(T[] oldArr, T[] newArr, Func<T, T, bool> comparer)
            {
                if (oldArr == null && newArr == null)
                {
                    return true;
                }
                else if (oldArr == null || newArr == null)
                {
                    return false;
                }
                else 
                {
                    for (int i = 0; i < oldArr.Length; i++) 
                    {
                        if (!comparer.Invoke(oldArr[i], newArr[i])) 
                        {
                            return false;
                        }
                    }
                }

                return true;
            }

            m_ParamsParser.Parse(oldParams, out CustomFeatureParameter[] oldAtts, out IXSelObject[] oldSels, out _, out double[] oldDimVals, out IXBody[] oldEditBodies);
            m_ParamsParser.Parse(newParams, out CustomFeatureParameter[] newAtts, out IXSelObject[] newSels, out _, out double[] newDimVals, out IXBody[] newEditBodies);

            return !(AreArraysEqual(oldAtts, newAtts, (o, n) => string.Equals(o.Name, n.Name) && object.Equals(o.Value, n.Value) && Type.Equals(o.Type, n.Type))
                    && AreArraysEqual(oldSels, newSels, (o, n) => o.Equals(n))
                    && AreArraysEqual(oldDimVals, newDimVals, (o, n) => double.Equals(o, n))
                    && AreArraysEqual(oldEditBodies, newEditBodies, (o, n) => o.Equals(n)));
        }

        private void OnPageClosed(PageCloseReasons_e reason)
        {
            m_IsPageActive = false;
            CompleteFeature(reason);

            m_CurPageData = null;
            m_CurData = null;
            m_HiddenEditBodies = null;
            m_EditingFeature = null;
            m_LastError = null;
            m_PreviewBodies = null;

            CurModel = null;
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
                    EditingCompleting.Invoke(m_App, CurModel, m_EditingFeature, m_CurData, m_CurPageData, reason);
                }
                catch (Exception ex)
                {
                    m_Logger.Log(ex);
                    m_LastError = ex;
                }
            }

            if (m_LastError != null)
            {
                arg.ErrorMessage = m_LastError is IUserException ? m_LastError.Message : "Unknown error. Please see log for more details";
                arg.Cancel = true;
            }
            else 
            {
                if (reason == PageCloseReasons_e.Apply)
                {
                    CompleteFeature(reason);

                    m_CurData = Definition.ConvertPageToParams(m_App, CurModel, m_CurPageData);

                    //page stays open
                    UpdatePreview();
                }
            }
        }

        private void DefaultAssignPreviewBodyColor(IXBody body, out System.Drawing.Color color)
            => color = System.Drawing.Color.Yellow;

        private void UpdatePreview()
        {
            if (m_IsPageActive)
            {
                using (var viewFreezer = new ViewFreezer(CurModel))
                {
                    try
                    {
                        m_LastError = null;

                        HidePreviewBodies();

                        m_PreviewBodies = Definition.CreatePreviewGeometry(m_App, CurModel,
                            m_CurData, m_CurPageData, out var shouldHidePreviewEdit,
                            out var assignPreviewColor);
                        
                        if (assignPreviewColor == null) 
                        {
                            assignPreviewColor = DefaultAssignPreviewBodyColor;
                        }

                        HideEditBodies(shouldHidePreviewEdit);

                        if (m_PreviewBodies != null)
                        {
                            DisplayPreview(m_PreviewBodies, assignPreviewColor);
                        }
                    }
                    catch (Exception ex)
                    {
                        HidePreviewBodies();
                        ShowEditBodies();
                        m_Logger.Log(ex);
                        m_LastError = ex;
                    }
                }
            }
        }

        private void CompleteFeature(PageCloseReasons_e reason)
        {
            EditingCompleted?.Invoke(m_App, CurModel, m_EditingFeature, m_CurData, m_CurPageData, reason);

            ShowEditBodies();

            HidePreviewBodies();

            m_PreviewBodies = null;

            if (reason == PageCloseReasons_e.Okay || reason == PageCloseReasons_e.Apply)
            {
                if (m_EditingFeature == null)
                {
                    var feat = CurModel.Features.PreCreateCustomFeature<TData>();
                    feat.DefinitionType = m_DefType;
                    feat.Parameters = m_CurData;
                    CurModel.Features.Add(feat);

                    if (feat == null)
                    {
                        throw new NullReferenceException("Failed to create custom feature");
                    }

                    FeatureInserted?.Invoke(m_App, CurModel, feat, m_CurData, m_CurPageData);
                }
                else
                {
                    m_EditingFeature.Parameters = m_CurData;
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
    }
}