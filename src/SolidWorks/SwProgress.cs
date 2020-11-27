using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.SolidWorks
{
    internal class SwProgress : IXProgress
    {
        private readonly IUserProgressBar m_PrgBar;

        internal SwProgress(IUserProgressBar prgBar)
        {
            m_PrgBar = prgBar;
            m_PrgBar.Start(0, 1000, "...");
        }

        public void Report(double value) => m_PrgBar.UpdateProgress((int)(value * 1000));
        public void Dispose() => m_PrgBar.End();
        public void SetStatus(string status) => m_PrgBar.UpdateTitle(status);
    }
}
