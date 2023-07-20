//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.UI;

namespace Xarial.XCad.Toolkit.Extensions
{
    /// <summary>
    /// Additional methods for <see cref="IXImage"/>
    /// </summary>
    public static class XImageExtension
    {
        /// <summary>
        /// Tries converts <see cref="IXImage"/> to <see cref="Image"/>
        /// </summary>
        /// <param name="img"></param>
        /// <returns>Imgage or null</returns>
        public static Image ToImage(this IXImage img) 
        {
            try
            {
                if (img != null && img.Buffer != null)
                {
                    using (var memStr = new MemoryStream(img.Buffer))
                    {
                        memStr.Seek(0, SeekOrigin.Begin);
                        return Image.FromStream(memStr);
                    }
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
