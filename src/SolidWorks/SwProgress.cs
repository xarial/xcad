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
using Xarial.XCad.SolidWorks.Services;

namespace Xarial.XCad.SolidWorks
{
    internal class SwProgress : IXProgress
    {
        private readonly IUserProgressBar m_PrgBar;
        private readonly IProgressUserCancellationHandler m_CancellationHandler;

        internal SwProgress(IUserProgressBar prgBar, IProgressUserCancellationHandler cancellationHandler)
        {
            m_CancellationHandler = cancellationHandler;

            m_PrgBar = prgBar;
            m_PrgBar.Start(0, 1000, "...");
        }

        public void Report(double value)
        {
            var res = (swUpdateProgressError_e)m_PrgBar.UpdateProgress((int)(value * 1000));

            if (res == swUpdateProgressError_e.swUpdateProgressError_UserCancel) 
            {
                m_CancellationHandler.Handle(this);
            }
        }

        public void Dispose() => m_PrgBar.End();
        public void SetStatus(string status) => m_PrgBar.UpdateTitle(status);
    }
}
