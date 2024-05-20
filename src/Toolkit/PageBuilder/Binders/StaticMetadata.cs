//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.UI.PropertyPage.Base;

namespace Xarial.XCad.Toolkit.PageBuilder.Binders
{
    /// <summary>
    /// Represents the static metadata value
    /// </summary>
    public class StaticMetadata : IMetadata
    {
        /// <inheritdoc/>
        public event Action<IMetadata, object> Changed;

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public object Tag => null;

        /// <inheritdoc/>
        public object Value { get; set; }
        
        public StaticMetadata(object value, string name) 
        {
            Value = value;
            Name = name;
        }
    }
}
