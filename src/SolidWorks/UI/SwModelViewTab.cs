//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.UI;
using SolidWorks.Interop.sldworks;
using Xarial.XCad.Documents;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.Base;
using Xarial.XCad.SolidWorks.UI.Toolkit;
using Xarial.XCad.UI.Exceptions;

namespace Xarial.XCad.SolidWorks.UI
{
    public interface ISwModelViewTab<TControl> : IXCustomPanel<TControl>, IDisposable 
    {
        string TabName { get; }
    }

    internal class SwModelViewTab<TControl> : DocumentAttachedCustomPanel<TControl>, ISwModelViewTab<TControl>
    {
        private readonly ModelViewTabCreator<TControl> m_CtrlCreator;
        private readonly ModelViewManager m_ModelViewMgr;

        public string TabName 
        {
            get 
            {
                if (IsControlCreated)
                {
                    return m_CurTabTitle;
                }
                else 
                {
                    throw new CustomPanelControlNotCreatedException();
                }
            }
        }

        private string m_CurTabTitle;

        internal SwModelViewTab(ModelViewTabCreator<TControl> ctrlCreator, 
            ModelViewManager modelViewManager, SwDocument doc, ISwApplication app, IXLogger logger) : base(doc, app, logger)
        {
            m_CtrlCreator = ctrlCreator;

            m_ModelViewMgr = modelViewManager;
        }

        protected override TControl CreateControl()
        {
            m_CurTabTitle = m_CtrlCreator.CreateControl(typeof(TControl), out TControl ctrl);
            return ctrl;
        }

        protected override void DeleteControl()
        {
            if (IsActive)
            {
                m_ModelViewMgr.ActivateModelTab();
            }

            var res = m_ModelViewMgr.DeleteControlTab(m_CurTabTitle);

            if (!res)
            {
                m_Logger.Log("Failed to delete model view tab", XCad.Base.Enums.LoggerMessageSeverity_e.Error);
            }
        }

        protected override bool GetIsActive() => m_ModelViewMgr.IsControlTabActive(m_CurTabTitle);

        protected override void SetIsActive(bool active)
        {
            if (!m_ModelViewMgr.ActivateControlTab(m_CurTabTitle))
            {
                throw new Exception("Failed to activate the model view tab");
            }
        }
    }
}
