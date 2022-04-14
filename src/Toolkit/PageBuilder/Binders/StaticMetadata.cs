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
