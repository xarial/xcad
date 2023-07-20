//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.UI.PropertyPage.Base;

namespace Xarial.XCad.Toolkit.PageBuilder.Binders
{
    public class StaticMetadata : IMetadata
    {
        public event Action<IMetadata, object> Changed;

        public object Tag => null;

        public object Value { get; set; }
        
        public StaticMetadata(object value) 
        {
            Value = value;
        }
    }
}
