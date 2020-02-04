//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

namespace Xarial.XCad.Annotations
{
    public interface IXDimension : IXSelObject
    {
        double GetValue(string confName = "");

        void SetValue(double val, string confName = "");
    }
}