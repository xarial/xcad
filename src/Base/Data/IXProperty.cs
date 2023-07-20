//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Base;
using Xarial.XCad.Data.Delegates;

namespace Xarial.XCad.Data
{
    /// <summary>
    /// Represents the custom property
    /// </summary>
    public interface IXProperty : IXTransaction
    {
        /// <summary>
        /// Raised when the value of this property is changed
        /// </summary>
        event PropertyValueChangedDelegate ValueChanged;
        
        /// <summary>
        /// Name of the property
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Property value
        /// </summary>
        object Value { get; set; }

        /// <summary>
        /// Property equation
        /// </summary>
        string Expression { get; set; }
    }


    /// <summary>
    /// Additional methods for property
    /// </summary>
    public static class XPropertyExtension 
    {
        /// <summary>
        /// True if this property exists
        /// </summary>
        public static bool Exists(this IXProperty prp) => prp.IsCommitted;
    }
}
