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
        //--- NetFramework
        [ComVisible(true)]
        public class SampleAddIn : SwAddInEx
        {
            public override void OnConnect()
            {
            }
        }
        //---
    }

    namespace SkipReg
    {
        //--- SkipReg
        [ComVisible(true)]
        [Extensions.Attributes.SkipRegistration]
        public class SampleAddIn : SwAddInEx
        {
            //--- 
        }
    }

    //--- NetCore
    [ComVisible(true), Guid("612378E1-C962-468C-9810-AF5AE1245EB7")]
    public class SampleAddIn : SwAddInEx
    {
        [ComRegisterFunction]
        public static void RegisterFunction(Type t)
        {
            SwAddInEx.RegisterFunction(t);
        }

        [ComUnregisterFunction]
        public static void UnregisterFunction(Type t)
        {
            SwAddInEx.UnregisterFunction(t);
        }

        public override void OnConnect()
        {
        }
    }
    //---
}
