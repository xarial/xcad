using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Enums;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Enums;

namespace Xarial.XCad.Tests.Common
{
    public class SwStarter : IDisposable
    {
        public ISwApplication Application { get; }

        private readonly Process m_Process;

        private readonly bool m_CloseSw;

        public SwStarter(int prcId)
        {
            if (prcId > 0)
            {
                m_Process = Process.GetProcessById(prcId);

                if (m_Process == null) 
                {
                    throw new NullReferenceException("No process found");
                }
            }
            else
            {
                m_Process = Process.GetProcessesByName("SLDWORKS").FirstOrDefault();

                if (m_Process == null) 
                {
                    m_Process = CreateApplication(null).Process;
                }
            }

            Application = SwApplicationFactory.FromProcess(m_Process);
        }

        public SwStarter(SwVersion_e? version)
        {
            Application = CreateApplication(version);

            m_CloseSw = true;

            m_Process = Application.Process;
        }

        private ISwApplication CreateApplication(SwVersion_e? version)
        {
            SwApplicationFactory.DisableAllAddInsStartup(out var disabledStartupAddIns);

            var app = SwApplicationFactory.Create(version,
                ApplicationState_e.Background | ApplicationState_e.Safe | ApplicationState_e.Silent);

            if (disabledStartupAddIns?.Any() == true)
            {
                SwApplicationFactory.EnableAddInsStartup(disabledStartupAddIns);
            }

            return app;
        }

        public void Dispose()
        {
            if (m_CloseSw)
            {
                m_Process.Kill();
            }
        }
    }
}
