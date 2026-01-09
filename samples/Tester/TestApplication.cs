using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using Xarial.XCad;
using Xarial.XCad.Base.Enums;
using Xarial.XCad.Delegates;
using Xarial.XCad.Documents;
using Xarial.XCad.Enums;
using Xarial.XCad.Geometry;

namespace Tester
{
    public class TestApplication : IXApplication
    {
        public IXVersion Version { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public ApplicationState_e State { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool IsAlive => throw new NotImplementedException();

        public Rectangle WindowRectangle => throw new NotImplementedException();

        public IntPtr WindowHandle => throw new NotImplementedException();

        public Process Process => throw new NotImplementedException();

        public IXApplicationOptions Options => throw new NotImplementedException();

        public IXDocumentRepository Documents => throw new NotImplementedException();

        public IXMemoryGeometryBuilder MemoryGeometryBuilder => throw new NotImplementedException();

        public IXMaterialsDatabaseRepository MaterialDatabases => throw new NotImplementedException();

        public bool IsCommitted => throw new NotImplementedException();

        public event ApplicationStartingDelegate Starting;
        public event ApplicationIdleDelegate Idle;

        public void Close()
        {
            throw new NotImplementedException();
        }

        public void Commit(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public IXObjectTracker CreateObjectTracker(string name)
        {
            throw new NotImplementedException();
        }

        public IXProgress CreateProgress(CancellationTokenSource cts = null)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public IXMacro OpenMacro(string path)
        {
            throw new NotImplementedException();
        }

        public MessageBoxResult_e ShowMessageBox(string msg, MessageBoxIcon_e icon = MessageBoxIcon_e.Info, MessageBoxButtons_e buttons = MessageBoxButtons_e.Ok)
        {
            throw new NotImplementedException();
        }

        public void ShowTooltip(Xarial.XCad.Base.ITooltipSpec spec)
        {
            throw new NotImplementedException();
        }
    }
}
