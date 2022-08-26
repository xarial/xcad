//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Annotations.Delegates
{
    /// <summary>
    /// Delegate of <see cref="IXDimension.ValueChanged"/> event
    /// </summary>
    /// <param name="dim">Sender</param>
    /// <param name="newVal">New value of the dimension</param>
    public delegate void DimensionValueChangedDelegate(IXDimension dim, double newVal);
}
