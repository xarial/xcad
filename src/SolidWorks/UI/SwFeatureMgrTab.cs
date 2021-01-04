//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.UI.Toolkit;
using Xarial.XCad.UI;
using Xarial.XCad.UI.Exceptions;

namespace Xarial.XCad.SolidWorks.UI
{
    public interface ISwFeatureMgrTab<TControl> : IXCustomPanel<TControl>, IDisposable 
    {
        IFeatMgrView FeatureManagerView { get; }
    }
    
    internal class SwFeatureMgrTab<TControl> : DocumentAttachedCustomPanel<TControl>, ISwFeatureMgrTab<TControl> 
    {
        public IFeatMgrView FeatureManagerView
        {
            get
            {
                if (IsControlCreated)
                {
                    return m_CurFeatMgrView;
                }
                else
                {
                    throw new CustomPanelControlNotCreatedException();
                }
            }
        }

        private IFeatMgrView m_CurFeatMgrView;
        private readonly FeatureManagerTabCreator<TControl> m_CtrlCreator;

        internal SwFeatureMgrTab(FeatureManagerTabCreator<TControl> ctrlCreator, SwDocument doc, IXLogger logger)
            : base(doc, logger)
        {
            m_CtrlCreator = ctrlCreator;
        }

        protected override bool GetIsActive() => throw new NotSupportedException();

        protected override void SetIsActive(bool active)
        {
            if (active)
            {
                m_CurFeatMgrView.ActivateView();
            }
            else 
            {
                if (!m_CurFeatMgrView.DeActivateView()) 
                {
                    m_Logger.Log("Failed to deactivate view");
                }
            }
        }

        protected override TControl CreateControl()
        {
            m_CurFeatMgrView = m_CtrlCreator.CreateControl(typeof(TControl), out TControl ctrl);
            return ctrl;
        }

        protected override void DeleteControl()
        {
            if (!FeatureManagerView.DeleteView())
            {
                m_Logger.Log("Failed to delete feature manager view");
            }
        }
    }
}
