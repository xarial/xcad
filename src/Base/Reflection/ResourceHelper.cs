//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Drawing;
using System.IO;
using System.Reflection;

namespace Xarial.XCad.Reflection
{
    /// <summary>
    /// Helper class to work with resources
    /// </summary>
    /// <remarks>Use this method in attributes to provide the reference to the data from the resources (i.e. text and image)</remarks>
    public static class ResourceHelper
    {
        /// <summary>
        /// Gets the specified resource by name
        /// </summary>
        /// <typeparam name="T">Type of the resource</typeparam>
        /// <param name="resType">Type of the resource class (usually Resources)</param>
        /// <param name="resName">Name of the resource</param>
        /// <returns>Value of the resource</returns>
        /// <remarks>Use nameof operator to get the resource name avoiding using the 'magic' strings</remarks>
        public static T GetResource<T>(Type resType, string resName)
        {
            var val = GetValue(null, resType, resName.Split('.'));

            if (val is byte[] && typeof(Image) == typeof(T))
            {
                val = FromBytes(val as byte[]);
            }

            return (T)val;
        }

        public static Image FromBytes(byte[] buffer)
        {
            using (var ms = new MemoryStream(buffer))
            {
                return Image.FromStream(ms);
            }
        }

        private static object GetValue(object obj, Type type, string[] prpsPath)
        {
            foreach (var prpName in prpsPath)
            {
                var prp = type.GetProperty(prpName,
                    BindingFlags.NonPublic | BindingFlags.Public
                    | BindingFlags.Static | BindingFlags.Instance);

                if (prp == null)
                {
                    throw new NullReferenceException($"Resource '{prpName}' is missing in '{type.Name}'");
                }

                obj = prp.GetValue(obj, null);

                if (obj != null)
                {
                    type = obj.GetType();
                }
            }

            return obj;
        }
    }
}