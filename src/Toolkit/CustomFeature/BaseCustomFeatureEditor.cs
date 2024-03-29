﻿//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
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
using Xarial.XCad.Features;
using System.Drawing;

namespace Xarial.XCad.Utils.CustomFeature
{
    public delegate void CustomFeatureStateChangedDelegate<TData, TPage>(IXApplication app, IXDocument doc, IXCustomFeature<TData> feat, TPage page)
        where TData : class
        where TPage : class;

    public delegate void CustomFeaturePageParametersChangedDelegate<TData, TPage>(IXApplication app, IXDocument doc, IXCustomFeature<TData> feat, TPage page)
        where TData : class
        where TPage : class;

    public delegate void CustomFeatureInsertedDelegate<TData, TPage>(IXApplication app, IXDocument doc, IXCustomFeature<TData> feat, TPage page)
        where TData : class
        where TPage : class;

    public delegate void CustomFeatureEditingCompletedDelegate<TData, TPage>(IXApplication app, IXDocument doc, IXCustomFeature<TData> feat, TPage page, PageCloseReasons_e reason)
        where TData : class
        where TPage : class;

    public delegate bool ShouldUpdatePreviewDelegate<TData, TPage>(TData oldData, TData newData, TPage page, bool dataChanged)
        where TData : class
        where TPage : class;

    public delegate TData HandleEditingExceptionDelegate<TData>(IXCustomFeature<TData> feat, Exception ex)
        where TData : class;

    /// <summary>
    /// Additional behaviors defined in the macro feature editor
    /// </summary>
    [Flags]
    public enum CustomFeatureEditorBehavior_e 
    {
        /// <summary>
        /// Default behavior
        /// </summary>
        Default = 0,

        /// <summary>
        /// If editor page has a pushpin button this option and it is applied,
        /// this option will close and reopen page instead of keeping the page open
        /// </summary>
        /// <remarks>Some of the feature migth not be able to be created while page is open thus making pushpin not usable</remarks>
        ReopenOnApply = 1
    }

    public abstract class BaseCustomFeatureEditor<TData, TPage> 
        where TData : class
        where TPage : class
    {
        public event CustomFeatureStateChangedDelegate<TData, TPage> EditingStarted;
        public event CustomFeatureEditingCompletedDelegate<TData, TPage> EditingCompleting;
        public event CustomFeatureEditingCompletedDelegate<TData, TPage> EditingCompleted;
        public event CustomFeatureInsertedDelegate<TData, TPage> FeatureInserting;
        public event CustomFeaturePageParametersChangedDelegate<TData, TPage> PreviewUpdated;
        public event ShouldUpdatePreviewDelegate<TData, TPage> ShouldUpdatePreview;
        public event HandleEditingExceptionDelegate<TData> HandleEditingException;

        protected readonly IXApplication m_App;
        protected readonly IServiceProvider m_SvcProvider;
        protected readonly IXLogger m_Logger;

        private readonly XObjectEqualityComparer<IXBody> m_BodiesComparer;
        private readonly CustomFeatureParametersParser m_ParamsParser;
        private readonly Type m_DefType;

        private readonly IXPropertyPage<TPage> m_PmPage;
        private readonly Lazy<IXCustomFeatureDefinition<TData, TPage>> m_DefinitionLazy;

        private TPage m_CurPageData;
        private IXBody[] m_HiddenEditBodies;
        protected IXCustomFeature<TData> m_CurrentFeature;
        private Exception m_LastError;
        private IXMemoryBody[] m_PreviewBodies;

        protected IXDocument CurrentDocument { get; private set; }
        
        private bool m_IsPageActive;

        private readonly CustomFeatureEditorBehavior_e m_Behavior;

        public BaseCustomFeatureEditor(IXApplication app,
            Type featDefType,
            IServiceProvider svcProvider, IXPropertyPage<TPage> page, CustomFeatureEditorBehavior_e behavior)
        {
            m_App = app;
            m_SvcProvider = svcProvider;
            m_Logger = svcProvider.GetService<IXLogger>();
            m_DefType = featDefType;
            m_BodiesComparer = new XObjectEqualityComparer<IXBody>();
            m_ParamsParser = new CustomFeatureParametersParser();

            m_Behavior = behavior;

            m_DefinitionLazy = new Lazy<IXCustomFeatureDefinition<TData, TPage>>(
                () => (IXCustomFeatureDefinition<TData, TPage>)CustomFeatureDefinitionInstanceCache.GetInstance(m_DefType));

            m_PmPage = page;

            m_PmPage.Closing += OnPageClosing;
            m_PmPage.DataChanged += OnDataChanged;
            m_PmPage.Closed += OnPageClosed;
        }
        
        private IXCustomFeatureDefinition<TData, TPage> Definition => m_DefinitionLazy.Value;

        private IEditor<IXFeature> m_CurEditor;

        public void Edit(IXDocument model, IXCustomFeature<TData> feature)
        {
            if (feature == null) 
            {
                throw new ArgumentNullException(nameof(feature));
            }

            m_IsPageActive = true;

            CurrentDocument = model;
            m_CurrentFeature = feature;
            m_CurEditor = m_CurrentFeature.Edit();

            try
            {
                TData featData;

                try
                {
                    featData = m_CurrentFeature.Parameters;
                }
                catch (Exception ex)
                {
                    featData = HandleEditingException.Invoke(m_CurrentFeature, ex);
                    m_CurrentFeature.Parameters = featData;
                }

                m_CurPageData = Definition.ConvertParamsToPage(m_App, model, featData);

                EditingStarted?.Invoke(m_App, model, feature, m_CurPageData);

                m_PmPage.Show(m_CurPageData);

                UpdatePreview();
            }
            catch(Exception ex)
            {
                m_Logger.Log(ex);
                m_CurEditor.Cancel = true;
                m_CurEditor.Dispose();
                throw;
            }
        }

        public void Insert(IXDocument doc, TData data)
        {
            m_IsPageActive = true;
            
            CurrentDocument = doc;

            m_CurrentFeature = CurrentDocument.Features.PreCreateCustomFeature<TData>();
            m_CurrentFeature.DefinitionType = m_DefType;
            m_CurrentFeature.Parameters = data;

            m_CurPageData = Definition.ConvertParamsToPage(m_App, doc, data);

            EditingStarted?.Invoke(m_App, doc, m_CurrentFeature, m_CurPageData);

            m_PmPage.Show(m_CurPageData);

            UpdatePreview();
        }

        protected virtual IXObject CurrentPreviewContext => CurrentDocument;

        private void DisplayPreview(IXMemoryBody[] bodies, AssignPreviewBodyColorDelegate assignPreviewBodyColorDelegateFunc)
        {
            if (bodies?.Any() == true)
            {
                var previewContext = CurrentPreviewContext;

                if (previewContext == null)
                {
                    throw new Exception("Preview context is not specified");
                }

                foreach (var body in bodies)
                {
                    assignPreviewBodyColorDelegateFunc.Invoke(body, out Color color);

                    body.Preview(previewContext, color);
                }
            }
        }

        private void HidePreview(IXMemoryBody[] bodies)
        {
            if (bodies != null)
            {
                for (int i = 0; i < bodies.Length; i++)
                {
                    try
                    {
                        bodies[i].Visible = false;
                        bodies[i].Dispose();
                    }
                    catch (Exception ex)
                    {
                        m_Logger.Log(ex);
                    }

                    bodies[i] = null;
                }
            }
        }

        private void HideEditBodies(ShouldHidePreviewEditBodyDelegate<TData, TPage> shouldHidePreviewEditBodyFunc)
        {
            IXBody[] editBodies;

            m_ParamsParser.Parse(m_CurrentFeature.Parameters, out _, out _, out _, out _, out editBodies);

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
                    hide &= shouldHidePreviewEditBodyFunc.Invoke(body, m_CurrentFeature.Parameters, m_CurPageData);
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
            if (m_IsPageActive)
            {
                var oldParams = m_CurrentFeature.Parameters;
                var newParams = Definition.ConvertPageToParams(m_App, CurrentDocument, m_CurPageData, oldParams);

                var dataChanged = AreParametersChanged(oldParams, newParams);

                var needUpdatePreview = ShouldUpdatePreview.Invoke(oldParams, newParams, m_CurPageData, dataChanged);

                m_CurrentFeature.Parameters = newParams;

                if (needUpdatePreview)
                {
                    UpdatePreview();

                    PreviewUpdated?.Invoke(m_App, CurrentDocument, m_CurrentFeature, m_CurPageData);
                }
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

            m_ParamsParser.Parse(oldParams, out CustomFeatureAttribute[] oldAtts, out IXSelObject[] oldSels, out _, out double[] oldDimVals, out IXBody[] oldEditBodies);
            m_ParamsParser.Parse(newParams, out CustomFeatureAttribute[] newAtts, out IXSelObject[] newSels, out _, out double[] newDimVals, out IXBody[] newEditBodies);

            return !(AreArraysEqual(oldAtts, newAtts, (o, n) => string.Equals(o.Name, n.Name) && object.Equals(o.Value, n.Value) && Type.Equals(o.Type, n.Type))
                    && AreArraysEqual(oldSels, newSels, (o, n) => o.Equals(n))
                    && AreArraysEqual(oldDimVals, newDimVals, (o, n) => double.Equals(o, n))
                    && AreArraysEqual(oldEditBodies, newEditBodies, (o, n) => o.Equals(n)));
        }

        private void OnPageClosed(PageCloseReasons_e reason)
        {
            if (m_IsApplying)
            {
                reason = PageCloseReasons_e.Apply;
            }

            var cachedParams = m_CurrentFeature.Parameters;

            m_IsPageActive = false;

            CompleteFeature(reason);

            TData reusableParams;

            if (m_IsApplying)
            {
                reusableParams = Definition.ConvertPageToParams(
                    m_App, CurrentDocument, m_CurPageData, cachedParams);
            }
            else 
            {
                reusableParams = default;
            }

            m_CurEditor?.Dispose();

            m_CurPageData = null;
            m_HiddenEditBodies = null;
            m_CurrentFeature = null;
            m_LastError = null;
            m_PreviewBodies = null;
            m_CurEditor = null;

            if (!m_IsApplying)
            {
                CurrentDocument = null;
            }
            else
            {
                m_IsApplying = false;
                Insert(CurrentDocument, reusableParams);
                m_PmPage.IsPinned = true;
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
            if (!m_IsApplying)
            {
                if (EditingCompleting != null)
                {
                    try
                    {
                        EditingCompleting.Invoke(m_App, CurrentDocument, m_CurrentFeature, m_CurPageData, reason);
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
                        if (m_Behavior.HasFlag(CustomFeatureEditorBehavior_e.ReopenOnApply))
                        {
                            m_IsApplying = true;
                            m_PmPage.Close(true);
                        }
                        else
                        {
                            CompleteFeature(reason);

                            m_CurrentFeature.Parameters = Definition.ConvertPageToParams(m_App, CurrentDocument, m_CurPageData, m_CurrentFeature.Parameters);

                            //page stays open
                            UpdatePreview();
                        }
                    }
                }
            }
        }

        private bool m_IsApplying;

        private void DefaultAssignPreviewBodyColor(IXBody body, out Color color)
            => color = Color.FromArgb(100, Color.Yellow);

        private void UpdatePreview()
        {
            if (m_IsPageActive)
            {
                using (CurrentDocument.ModelViews.Active.Freeze(true))
                {
                    try
                    {
                        m_LastError = null;

                        HidePreviewBodies();

                        m_PreviewBodies = Definition.CreatePreviewGeometry(m_App, CurrentDocument,
                            m_CurrentFeature, m_CurPageData, out var shouldHidePreviewEdit,
                            out var assignPreviewColor);

                        if (assignPreviewColor == null)
                        {
                            assignPreviewColor = DefaultAssignPreviewBodyColor;
                        }

                        HideEditBodies(shouldHidePreviewEdit);

                        if (m_PreviewBodies?.Any() == true)
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

        protected virtual void CompleteFeature(PageCloseReasons_e reason)
        {
            EditingCompleted?.Invoke(m_App, CurrentDocument, m_CurrentFeature, m_CurPageData, reason);

            ShowEditBodies();

            HidePreviewBodies();

            m_PreviewBodies = null;

            if (reason == PageCloseReasons_e.Okay || reason == PageCloseReasons_e.Apply)
            {
                if (!m_CurrentFeature.IsCommitted)
                {
                    FeatureInserting?.Invoke(m_App, CurrentDocument, m_CurrentFeature, m_CurPageData);
                }
            }
            else
            {
                if (m_CurrentFeature.IsCommitted)
                {
                    m_CurEditor.Cancel = true;
                }
            }
        }
    }
}