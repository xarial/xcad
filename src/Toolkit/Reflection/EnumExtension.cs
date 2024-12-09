//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Xarial.XCad.Reflection;

namespace Xarial.XCad.Utils.Reflection
{
    /// <summary>
    /// Provides extension classes for the <see cref="Enum"/> enumerator
    /// </summary>
    public static class EnumExtension
    {
        /// <summary>
        /// Get the specified attribute from the enumerator field
        /// </summary>
        /// <typeparam name="TAtt">Attribute type</typeparam>
        /// <param name="enumer">Enumerator field</param>
        /// <returns>Attribute</returns>
        /// <exception cref="NullReferenceException"/>
        /// <remarks>This method throws an exception if attribute is missing</remarks>
        public static TAtt GetAttribute<TAtt>(this Enum enumer)
            where TAtt : Attribute
        {
            TAtt att = default;

            if (!enumer.TryGetAttribute<TAtt>(a => att = a))
            {
                throw new NullReferenceException($"Attribute of type {typeof(TAtt)} is not fond on {enumer}");
            }

            return att;
        }

        /// <summary>
        /// Attempts to the attribute from enumeration
        /// </summary>
        /// <typeparam name="TAtt">Type of the attribute</typeparam>
        /// <param name="enumer">Enumerator field</param>
        /// <returns>Attribute or null if not found</returns>
        public static TAtt TryGetAttribute<TAtt>(this Enum enumer)
            where TAtt : Attribute
        {
            TAtt thisAtt = null;
            enumer.TryGetAttribute<TAtt>(a => thisAtt = a);
            return thisAtt;
        }
    }
}