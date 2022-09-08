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

        internal XDrawingImage(Image img)
        {
            Buffer = ImageToByteArray(img);
        }

        private byte[] ImageToByteArray(Image bmp)
        {
            using (var ms = new MemoryStream())
            {
                bmp.Save(ms, ImageFormat.Png);
                return ms.ToArray();
            }
        }
    }
}
