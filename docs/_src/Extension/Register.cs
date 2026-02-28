using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.SolidWorks;

namespace Xarial.XCad.Documentation.Extension
{
    namespace Framework
    {
        #region NetFramework
        [ComVisible(true)]
        public class SampleAddIn : SwAddInEx
        {
            public override void OnConnect()
            {
            }
        }
        #endregion NetFramework
    }

    namespace SkipReg
    {
        #region SkipReg
        [ComVisible(true)]
        [Extensions.Attributes.SkipRegistration]
        public class SampleAddIn : SwAddInEx
        {
            #endregion SkipReg
        }
    }

    #region NetCore
    [ComVisible(true), Guid("612378E1-C962-468C-9810-AF5AE1245EB7")]
    public class SampleAddIn : SwAddInEx
    {
        [ComRegisterFunction]
        public new static void RegisterFunction(Type t)
        {
            SwAddInEx.RegisterFunction(t);
        }

        [ComUnregisterFunction]
        public new static void UnregisterFunction(Type t)
        {
            SwAddInEx.UnregisterFunction(t);
        }

        public override void OnConnect()
        {
        }
    }
    #endregion NetCore
}
