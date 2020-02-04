//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Linq;
using System.Reflection;

namespace Xarial.XCad.Utils.Reflection
{
    /// <summary>
    /// Provides extension methods for the <see cref="MemberInfo"/>
    /// </summary>
    public static class MemberInfoExtension
    {
        /// <summary>
        /// Attempts to get the attribute from the class member
        /// </summary>
        /// <typeparam name="TAtt">Attribute type</typeparam>
        /// <param name="membInfo">Pointer to member (field or property)</param>
        /// <returns>Pointer to attribute or null if not found</returns>
        public static TAtt TryGetAttribute<TAtt>(this MemberInfo membInfo)
            where TAtt : Attribute
        {
            var atts = membInfo.GetCustomAttributes(typeof(TAtt), true);

            if (atts != null && atts.Any())
            {
                return atts.First() as TAtt;
            }
            else
            {
                return null;
            }
        }
    }
}