//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Enums;

namespace Xarial.XCad.UI.PropertyPage.Attributes
{
    /// <summary>
    /// Indicates that property page should be locked
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class LockedPageAttribute : Attribute, IAttribute
    {
        /// <summary>
        /// Strategy for the locked page
        /// </summary>
        public LockPageStrategy_e Strategy { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="strategy">Lock page strategy</param>
        public LockedPageAttribute(LockPageStrategy_e strategy) 
        {
            Strategy = strategy;
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public LockedPageAttribute() : this(LockPageStrategy_e.Blocked) 
        {
        }
    }
}
