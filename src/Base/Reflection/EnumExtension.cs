//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Linq;

namespace Xarial.XCad.Reflection
{
    public static class EnumExtension
    {
        /// <summary>
        /// Tries to get attribute from the enumeration
        /// </summary>
        /// <typeparam name="TAtt">Type of attribute to get</typeparam>
        /// <param name="enumer">Enumerator value</param>
        /// <param name="attProc">Action to process attribute</param>
        /// <returns>True if attribute exists</returns>
        public static bool TryGetAttribute<TAtt>(this Enum enumer, Action<TAtt> attProc)
            where TAtt : Attribute
        {
            var enumType = enumer.GetType();
            var enumField = enumType.GetMember(enumer.ToString()).FirstOrDefault();
            var atts = enumField.GetCustomAttributes(typeof(TAtt), false);

            if (atts != null && atts.Any())
            {
                var att = atts.First() as TAtt;
                attProc.Invoke(att);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}