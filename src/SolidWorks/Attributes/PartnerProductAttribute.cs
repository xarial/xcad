﻿//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.SolidWorks.Enums;

namespace Xarial.XCad.SolidWorks.Attributes
{
    /// <summary>
    /// Registers add-in as the SOLIDWORKS partner product
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class PartnerProductAttribute : Attribute
    {
        /// <summary>
        /// Partner key of the product
        /// </summary>
        public string PartnerKey { get; }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="partnerKey">Partner key of the product</param>
        public PartnerProductAttribute(string partnerKey) 
        {
            PartnerKey = partnerKey;
        }
    }
}
