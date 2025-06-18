using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Enums;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Enums;
using Xarial.XCad.Documents.Extensions;
using SolidWorks.Interop.swconst;

namespace Xarial.XCad.Tests.Common
{
    public class SwStarter : IDisposable
    {
        private static ISwApplication CreateApplication(int prcId) 
        {
            Process process;

            if (prcId > 0)
            {
                process = Process.GetProcessById(prcId);

                if (process == null)
                {
                    throw new NullReferenceException("No process found");
                }
            }
            else
            {
                process = Process.GetProcessesByName("SLDWORKS").FirstOrDefault();

                if (process == null)
                {
                    process = CreateApplication(null).Process;
                }
            }

            return SwApplicationFactory.FromProcess(process);
        }

        private static ISwApplication CreateApplication(SwVersion_e? version)
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

        public ISwApplication Application { get; }

        private readonly Process m_Process;

        private readonly PopupKiller m_PopupKiller;

        private readonly bool m_CloseSw;

        /// <summary>
        /// Create reusable instance of SOLIDWORKS
        /// </summary>
        /// <param name="prcId">ID of the process, or -1 to create new instance</param>
        public SwStarter(int prcId) : this(CreateApplication(prcId))
        {
        }

        public SwStarter(SwVersion_e? version) : this(CreateApplication(version))
        {
            m_CloseSw = true;
        }

        private SwStarter(ISwApplication app)
        {
            Application = app;

            m_PopupKiller = new PopupKiller(Application);

            m_Process = Application.Process;
        }

        public void UpdateReferences(string filePath, string workDir)
        {
            using (var doc = (ISwDocument)Application.Documents.Open(filePath))
            {
                foreach (ISwDocument dep in doc.Dependencies)
                {
                    if (dep.IsCommitted)
                    {
                        RebuildAndSave(dep);
                    }
                }

                RebuildAndSave(doc);

                var deps = (doc.Model.Extension.GetDependencies(true, false, false, false, false) as string[]).Where((item, index) => index % 2 != 0).ToArray();

                if (!deps.All(d => d.Contains("^") || d.StartsWith(workDir, StringComparison.CurrentCultureIgnoreCase)))
                {
                    throw new Exception("Failed to setup source assemblies");
                }
            }
        }

        private void RebuildAndSave(ISwDocument dep)
        {
            dep.Model.ForceRebuild3(false);

            int errs = -1;
            int warns = -1;

            if (!dep.Model.Save3((int)(swSaveAsOptions_e.swSaveAsOptions_Silent | swSaveAsOptions_e.swSaveAsOptions_SaveReferenced), ref errs, ref warns))
            {
                throw new Exception();
            }
        }

        public void Dispose()
        {
            m_PopupKiller.Dispose();

            if (m_CloseSw)
            {
                try
                {
                    Application.Dispose();
                }
                catch 
                {
                }

                try
                {
                    m_Process.Kill();
                }
                catch 
                {
                }
            }
        }
    }
}
