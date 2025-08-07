//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Xarial.XCad.Services;
using Xarial.XCad.SolidWorks.Services;

namespace Xarial.XCad.SolidWorks
{
    internal class SwProgress : IXProgress
    {
        private readonly IUserProgressBar m_PrgBar;
        private readonly IProgressUserCancellationHandler m_CancellationHandler;
        private readonly CancellationTokenSource m_Cts;

        internal SwProgress(IUserProgressBar prgBar, CancellationTokenSource cts, IProgressUserCancellationHandler cancellationHandler)
        {
            m_CancellationHandler = cancellationHandler;
            m_Cts = cts;

            m_PrgBar = prgBar;

            if (!m_PrgBar.Start(0, 1000, "...")) 
            {
                throw new Exception("Failed to start progress bar");
            }
        }

        public void Report(double value)
        {
            var res = (swUpdateProgressError_e)m_PrgBar.UpdateProgress((int)(value * 1000));

            if (res == swUpdateProgressError_e.swUpdateProgressError_UserCancel) 
            {
                m_CancellationHandler.Handle(this, m_Cts);
            }
        }

        public void SetStatus(string status) => m_PrgBar.UpdateTitle(status);

        public void Dispose()
        {
            if (!m_PrgBar.End())
            {
                throw new Exception("Failed to end progress bar");
            }
        }
    }
}
