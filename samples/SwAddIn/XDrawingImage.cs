using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.UI;

namespace SwAddInExample
{
    internal class XDrawingImage : IXImage
    {
        /// <inheritdoc/>
        public byte[] Buffer { get; }

        internal XDrawingImage(Image img) : this(img, ImageFormat.Png)
        {
        }

        internal XDrawingImage(Image img, ImageFormat format)
        {
            Buffer = ImageToByteArray(img, format);
        }

        private byte[] ImageToByteArray(Image bmp, ImageFormat format)
        {
            using (var ms = new MemoryStream())
            {
                bmp.Save(ms, format);
                return ms.ToArray();
            }
        }
    }
}
