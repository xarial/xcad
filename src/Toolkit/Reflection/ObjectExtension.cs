//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;

namespace Xarial.XCad.Utils.Reflection
{
    /// <summary>
    /// Extension methods of <see cref="object"/>
    /// </summary>
    public static class ObjectExtension
    {
        /// <summary>
        /// Casts object to specific type
        /// </summary>
        /// <param name="value">value</param>
        /// <param name="type">Taget type</param>
        /// <returns>Cast value</returns>
        /// <exception cref="InvalidCastException"></exception>
        public static object Cast(this object value, Type type)
        {
            object destVal = null;

            if (value != null)
            {
                if (!type.IsAssignableFrom(value.GetType()))
                {
                    try
                    {
                        if (typeof(IConvertible).IsAssignableFrom(type))
                        {
                            destVal = Convert.ChangeType(value, type);
                        }
                        else if (type.IsEnum)
                        {
                            destVal = Enum.Parse(type, value?.ToString());
                        }
                        else
                        {
                            throw new InvalidCastException(
                                $"{value.GetType().Name} cannot be cast to {type.Name}");
                        }
                    }
                    catch (InvalidCastException)
                    {
                        throw;
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
            else 
            {
                if (type.IsValueType)
                {
                    return Activator.CreateInstance(type);
                }
            }

            return destVal;
        }
    }
}