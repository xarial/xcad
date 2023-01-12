//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Documents;
using Xarial.XCad.Exceptions;

namespace Xarial.XCad.SolidWorks.Geometry.Exceptions
{
    /// <summary>
    /// SOLIDOWORKS API limitation of not-loaded components mass property calculation in SOLIDWORKS 2019 or older
    /// </summary>
    public class NotLoadedMassPropertyComponentException : NotSupportedException
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="comp">Component</param>
        public NotLoadedMassPropertyComponentException(IXComponent comp) 
            : base($"Reference document of the component '{comp.Name}' must be loaded in order to access this mass property in SOLIDWORKS 2019 or older") 
        {
        }
    }
}
