//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

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
using Xarial.XCad.Toolkit.Utils;
using Xarial.XCad.UI;
using Xarial.XCad.UI.PropertyPage.Enums;

namespace Xarial.XCad.Toolkit.Windows.UI
{
    internal class WpfIcon : IIconDescriptor
    {
        public static BitmapImage CreateBitmapImage(IXImage icon, IIconsCreator iconConv, BitmapEffect_e effect = BitmapEffect_e.None)
        {
            using (var img = iconConv.ConvertIcon(new WpfIcon(icon, effect))[0].Image)
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

        internal BitmapEffect_e Effect { get; }

        private WpfIcon(IXImage icon, BitmapEffect_e effect = BitmapEffect_e.None)
        {
            Icon = icon;
            Effect = effect;
            IconSizes = new IIconSpec[]
            {
                new IconSpec(Icon, new Size(64, 64), ApplyEffect)
            };
        }

        public IIconSpec[] IconSizes { get; }

        private void ApplyEffect(ref byte r, ref byte g, ref byte b, ref byte a)
        {
            if (Effect.HasFlag(BitmapEffect_e.Grayscale))
            {
                ColorUtils.ConvertPixelToGrayscale(ref r, ref g, ref b);
            }

            if (Effect.HasFlag(BitmapEffect_e.Transparent))
            {
                a = (byte)((double)a / 2);
            }
        }
    }
}
