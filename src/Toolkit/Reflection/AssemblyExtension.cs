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
    public static class AssemblyExtension
    {
        /// <summary>
        /// Tries to get attribute from the assembly
        /// </summary>
        /// <typeparam name="TAtt">Type of attribute to get</typeparam>
        /// <param name="assm">Assembly</param>
        /// <param name="attProc">Action to process attribute</param>
        /// <returns>True if attribute exists</returns>
        public static bool TryGetAttribute<TAtt>(this Assembly assm, Action<TAtt> attProc)
            where TAtt : Attribute
        {
            var atts = assm.GetCustomAttributes(typeof(TAtt), true);

            if (atts != null && atts.Any())
            {
                var att = atts.First() as TAtt;
                attProc?.Invoke(att);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}