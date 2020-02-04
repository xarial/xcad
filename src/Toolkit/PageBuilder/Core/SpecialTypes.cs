//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;

namespace Xarial.XCad.Utils.PageBuilder.Core
{
    public static class SpecialTypes
    {
        internal interface ISpecialType
        {
        }

        public class AnyType : ISpecialType
        {
        }

        public class ComplexType : ISpecialType
        {
        }

        public class EnumType : ISpecialType
        {
        }

        internal static IEnumerable<Type> FindMathingSpecialTypes(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (type.IsEnum)
            {
                yield return typeof(EnumType);
            }
            else if (IsComplexType(type))
            {
                yield return typeof(ComplexType);
            }
            else
            {
                yield return typeof(AnyType);
            }
        }

        private static bool IsComplexType(Type type)
        {
            return !(type.IsPrimitive
                || type.IsEnum
                || type == typeof(string)
                || type == typeof(decimal));
        }
    }
}