using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.UI;

namespace Xarial.XCad.Toolkit.Graphics
{
    /// <summary>
    /// Represents the image class
    /// </summary>
    public class XDrawingImage : BaseImage
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="img">PNG image</param>
        public XDrawingImage(Image img) : this(img, ImageFormat.Png)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="img">Image</param>
        /// <param name="format">Format</param>
        public XDrawingImage(Image img, ImageFormat format) : base(ImageToByteArray(img, format))
        {
        }

        private static byte[] ImageToByteArray(Image bmp, ImageFormat format)
        {
            using (var ms = new MemoryStream())
            {
                bmp.Save(ms, format);
                return ms.ToArray();
            }
        }
    }
}
