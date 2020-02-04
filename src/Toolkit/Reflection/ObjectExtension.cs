//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;

namespace Xarial.XCad.Utils.Reflection
{
    internal static class ObjectExtension
    {
        internal static TVal Cast<TVal>(this object obj)
        {
            return (TVal)obj.Cast(typeof(TVal));
        }

        internal static object Cast(this object value, Type type)
        {
            object destVal = null;

            if (value != null)
            {
                if (!type.IsAssignableFrom(value.GetType())
                    && (typeof(IConvertible)).IsAssignableFrom(type))
                {
                    try
                    {
                        destVal = Convert.ChangeType(value, type);
                    }
                    catch
                    {
                        throw new InvalidCastException(
                            $"Specified constructor for {type.Name} type is invalid as value cannot be cast from {value.GetType().Name}");
                    }
                }
                else
                {
                    //TODO: change this - validate that cast is possible otherwise throw exception
                    destVal = value;
                }
            }

            return destVal;
        }
    }
}