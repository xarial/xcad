//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Data.Delegates;

namespace Xarial.XCad.Data
{
    public interface IXProperty
    {
        event PropertyValueChangedDelegate ValueChanged;

        string Name { get; set; }
        object Value { get; set; }
    }
}
