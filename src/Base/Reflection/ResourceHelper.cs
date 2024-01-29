//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using Xarial.XCad.UI;

namespace Xarial.XCad.Reflection
{
    /// <summary>
    /// Helper class to work with resources
    /// </summary>
    /// <remarks>Use this method in attributes to provide the reference to the data from the resources (i.e. text and image)</remarks>
    public static class ResourceHelper
    {
        private static object m_ImageFormatPng;

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

            if (typeof(IXImage) == typeof(T)) 
            {
                if (val is byte[])
                {
                    val = ImageFromBytes(val as byte[]);
                }
                else if (val is string) 
                {
                    using (var memStr = new MemoryStream())
                    {
                        using (var streamWriter = new StreamWriter(memStr))
                        {
                            streamWriter.Write(val as string);
                        }

                        memStr.Seek(0, SeekOrigin.Begin);
                        val = ImageFromBytes(memStr.ToArray());
                    }
                }
                else if (val is Bitmap)
                {
                    using (var stream = new MemoryStream())
                    {
                        var img = (Bitmap)val;
                        img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);

                        stream.Seek(0, SeekOrigin.Begin);
                        val = ImageFromBytes(stream.ToArray());
                    }
                }
            }
            
            return (T)val;
        }

        private static IXImage ImageFromBytes(byte[] buffer)
            => new BaseImage(buffer);

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