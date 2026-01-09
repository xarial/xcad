using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Xarial.XCad.Toolkit.Base;
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.UI;

namespace Xarial.XCad.Toolkit.Windows.UI
{
    internal class WpfIcon : IIconDescriptor
    {
        public static BitmapImage CreateBitmapImage(IXImage icon, IIconsCreator iconConv)
        {
            using (var img = iconConv.ConvertIcon(new WpfIcon(icon))[0].Image)
            {
                using (var stream = new MemoryStream())
                {
                    img.Save(stream, ImageFormat.Png);
                    stream.Seek(0, SeekOrigin.Begin);

                    var bmpImg = new BitmapImage();
                    bmpImg.BeginInit();
                    bmpImg.CacheOption = BitmapCacheOption.OnLoad;
                    bmpImg.StreamSource = stream;
                    bmpImg.EndInit();

                    if (bmpImg.CanFreeze && !bmpImg.IsFrozen)
                    {
                        bmpImg.Freeze();
                    }

                    return bmpImg;
                }
            }
        }

        internal IXImage Icon { get; }

        public Color TransparencyKey => Color.Transparent;

        public bool IsPermanent => false;

        public IconImageFormat_e Format => IconImageFormat_e.Png;

        internal WpfIcon(IXImage icon)
        {
            Icon = icon;
            IconSizes = new IIconSpec[]
            {
                new IconSpec(Icon, new Size(64, 64))
            };
        }

        public IIconSpec[] IconSizes { get; }
    }
}
