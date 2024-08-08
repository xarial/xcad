//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Base;

namespace Xarial.XCad.Data
{
    /// <summary>
    /// Additional methods for <see cref="IXPropertyRepository"/>
    /// </summary>
    public static class XPropertyRepositoryExtension
    {
        /// <summary>
        /// Sets the value for this poperty
        /// </summary>
        /// <param name="prps">Repository</param>
        /// <param name="prpName">Name of the property</param>
        /// <param name="prpVal">Property value</param>
        /// <remarks>This method will change the value of existing property or create new one if not exist</remarks>
        public static void Set(this IXPropertyRepository prps, string prpName, object prpVal)
        {
            var prp = prps.GetOrPreCreate(prpName);
            prp.Value = prpVal;
            if (!prp.Exists())
            {
                prps.Add(prp);
            }
        }

        /// <summary>
        /// Gets or pre creates property
        /// </summary>
        /// <param name="prps">Repository</param>
        /// <param name="name">Name of the property</param>
        /// <returns>Existing proeprty or non-commited property</returns>
        public static IXProperty GetOrPreCreate(this IXPropertyRepository prps, string name) 
        {
            IXProperty prp;

            if (!prps.TryGet(name, out prp)) 
            {
                prp = prps.PreCreate();
                prp.Name = name;
            }

            return prp;
        }
    }
}
