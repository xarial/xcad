using System;
using System.Runtime.InteropServices;
using SolidWorks.Interop.sldworks;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Enums;

namespace Xarial.XCad.Documentation
{
    [ComVisible(true), Guid("E574D882-4223-4FB3-9160-0F54655FB6D9")]
    public class MultiTargetAddIn : SwAddInEx
    {
        //--- Major
        public void ReadDescriptionProperty()
        {
            var prpMgr = Application.Sw.IActiveDoc2.Extension.CustomPropertyManager[""];
            var prpName = "Description";

            string val;
            string resVal;
            
            if (Application.IsVersionNewerOrEqual(SwVersion_e.Sw2018))
            {
                bool wasRes;
                bool linkToPrp;
                prpMgr.Get6(prpName, false, out val, out resVal, out wasRes, out linkToPrp);
            }
            else if (Application.IsVersionNewerOrEqual(SwVersion_e.Sw2014))
            {
                bool wasRes;
                prpMgr.Get5(prpName, false, out val, out resVal, out wasRes);
            }
            else
            {
                prpMgr.Get4(prpName, false, out val, out resVal);
            }
            
            //Logger.Log($"{prpName} = {resVal} [{val}]");
        }
        //---

        //--- Minor
        public void GetTolerance(IDimension dim)
        {
            var dimTol = dim.Tolerance;

            double maxTol;
            double minTol;

            if (Application.IsVersionNewerOrEqual(SwVersion_e.Sw2015, 3))
            {
                dimTol.GetMinValue2(out minTol);
                dimTol.GetMaxValue2(out maxTol);
            }
            else
            {
                minTol = dimTol.GetMinValue();
                maxTol = dimTol.GetMaxValue();
            }
        }
        //---
    }
}
