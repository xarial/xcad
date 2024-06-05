//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Data.Delegates
{
    /// <summary>
    /// Delegate for <see cref="IXProperty.ValueChanged"/> event
    /// </summary>
    /// <param name="prp">Event sender</param>
    /// <param name="newValue">New value assigned to the property</param>
    public delegate void PropertyValueChangedDelegate(IXProperty prp, object newValue);
}
