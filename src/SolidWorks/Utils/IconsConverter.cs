//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Xarial.XCad.SolidWorks.Base;
using Xarial.XCad.SolidWorks.Exceptions;
using Xarial.XCad.UI;

namespace Xarial.XCad.SolidWorks.Utils
{
    internal class IconsConverter : IDisposable
    {
        internal static Image FromXImage(IXImage img) 
        {
            using (var str = new MemoryStream(img.Buffer)) 
            {
                return Image.FromStream(str);
            }
        }

        /// <summary>
        /// Icon data
        /// </summary>
        private class IconData
        {
            /// <summary>
            /// Source image in original format (not scaled, not modified)
            /// </summary>
            internal Image SourceIcon { get; set; }

            /// <summary>
            /// Path where the icon needs to be saved
            /// </summary>
            internal string TargetIconPath { get; private set; }

            /// <summary>
            /// Required target size for the image
            /// </summary>
            internal Size TargetSize { get; set; }

            internal IconData(string iconsDir, Image sourceIcon, Size targetSize, string name)
            {
                SourceIcon = sourceIcon;
                TargetSize = targetSize;
                TargetIconPath = Path.Combine(iconsDir, name);
            }
        }

        /// <summary>
        /// Custom handler for the image replace function <see cref="IconsConverter.ReplaceColor(Image, ColorReplacerDelegate)"/>
        /// </summary>
        /// <param name="r">Red component of pixel</param>
        /// <param name="g">Green component of pixel</param>
        /// <param name="b">Blue component of pixel</param>
        /// <param name="a">Alpha component of pixel</param>
        internal delegate void ColorReplacerDelegate(ref byte r, ref byte g, ref byte b, ref byte a);

        private readonly bool m_DisposeIcons;
        private readonly string m_IconsDir;

        internal IconsConverter()
            : this(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()), true)
        {
        }

        /// <param name="iconsDir">Directory to store the icons</param>
        /// <param name="disposeIcons">True to remove the icons when class is disposed</param>
        internal IconsConverter(string iconsDir,
            bool disposeIcons = true)
        {
            m_IconsDir = iconsDir;
            m_DisposeIcons = disposeIcons;

            if (!Directory.Exists(m_IconsDir))
            {
                Directory.CreateDirectory(m_IconsDir);
            }
        }

        /// <summary>
        /// Disposing temp icon files
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Replaces the pixels in the image based on the custom replacer handler
        /// </summary>
        /// <param name="icon">Image to replace</param>
        /// <param name="replacer">Handler to replace which is called for each pixel</param>
        /// <returns>Resulting image</returns>
        internal static Image ReplaceColor(Image icon, ColorReplacerDelegate replacer)
        {
            var maskImg = new Bitmap(icon);

            var rect = new Rectangle(0, 0, maskImg.Width, maskImg.Height);

            var bmpData = maskImg.LockBits(rect, ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb);

            var ptr = bmpData.Scan0;

            var rgba = new byte[Math.Abs(bmpData.Stride) * maskImg.Height];

            Marshal.Copy(ptr, rgba, 0, rgba.Length);

            for (int i = 0; i < rgba.Length; i += 4)
            {
                replacer.Invoke(ref rgba[i + 2], ref rgba[i + 1], ref rgba[i], ref rgba[i + 3]);
            }

            Marshal.Copy(rgba, 0, bmpData.Scan0, rgba.Length);

            maskImg.UnlockBits(bmpData);

            return maskImg;
        }

        internal string[] ConvertIcon(IIcon icon)
        {
            var iconsData = CreateIconData(icon);

            foreach (var iconData in iconsData)
            {
                CreateBitmap(new Image[] { iconData.SourceIcon },
                    iconData.TargetIconPath,
                    iconData.TargetSize, icon.TransparencyKey);
            }

            return iconsData.Select(i => i.TargetIconPath).ToArray();
        }

        /// <inheritdoc/>
        internal string[] ConvertIconsGroup(IIcon[] icons)
        {
            if (icons == null || !icons.Any())
            {
                throw new ArgumentNullException(nameof(icons));
            }

            IconData[,] iconsDataGroup = null;

            var transparencyKey = icons.First().TransparencyKey;

            for (int i = 0; i < icons.Length; i++)
            {
                if (icons[i].TransparencyKey != transparencyKey)
                {
                    throw new IconTransparencyMismatchException(i);
                }

                var data = CreateIconData(icons[i]);

                if (iconsDataGroup == null)
                {
                    iconsDataGroup = new IconData[data.Length, icons.Length];
                }

                for (int j = 0; j < data.Length; j++)
                {
                    iconsDataGroup[j, i] = data[j];
                }
            }

            var iconsPaths = new string[iconsDataGroup.GetLength(0)];

            for (int i = 0; i < iconsDataGroup.GetLength(0); i++)
            {
                var imgs = new Image[iconsDataGroup.GetLength(1)];
                for (int j = 0; j < iconsDataGroup.GetLength(1); j++)
                {
                    imgs[j] = iconsDataGroup[i, j].SourceIcon;
                }

                iconsPaths[i] = iconsDataGroup[i, 0].TargetIconPath;
                CreateBitmap(imgs, iconsPaths[i],
                    iconsDataGroup[i, 0].TargetSize, transparencyKey);
            }

            return iconsPaths;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (m_DisposeIcons)
            {
                try
                {
                    Directory.Delete(m_IconsDir, true);
                }
                catch
                {
                }
            }
        }

        private void CreateBitmap(Image[] sourceIcons,
            string targetIcon, Size size, Color background)
        {
            var width = size.Width * sourceIcons.Length;
            var height = size.Height;

            using (var bmp = new Bitmap(width,
                height, PixelFormat.Format24bppRgb))
            {
                using (var graph = Graphics.FromImage(bmp))
                {
                    graph.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graph.SmoothingMode = SmoothingMode.HighQuality;
                    graph.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    using (var brush = new SolidBrush(background))
                    {
                        graph.FillRectangle(brush, 0, 0, bmp.Width, bmp.Height);
                    }

                    for (int i = 0; i < sourceIcons.Length; i++)
                    {
                        var sourceIcon = ReplaceColor(sourceIcons[i],
                            new ColorReplacerDelegate((ref byte r, ref byte g, ref byte b, ref byte a) =>
                            {
                                if (r == background.R && g == background.G && b == background.B && a == background.A)
                                {
                                    b = (byte)((b == 0) ? 1 : (b - 1));
                                }
                            }));

                        if (bmp.HorizontalResolution != sourceIcon.HorizontalResolution
                            || bmp.VerticalResolution != sourceIcon.VerticalResolution)
                        {
                            bmp.SetResolution(
                                sourceIcon.HorizontalResolution,
                                sourceIcon.VerticalResolution);
                        }

                        var widthScale = (double)size.Width / (double)sourceIcon.Width;
                        var heightScale = (double)size.Height / (double)sourceIcon.Height;
                        var scale = Math.Min(widthScale, heightScale);

                        int destX = 0;
                        int destY = 0;

                        if (heightScale < widthScale)
                        {
                            destX = (int)(size.Width - sourceIcon.Width * scale) / 2;
                        }
                        else
                        {
                            destY = (int)(size.Height - sourceIcon.Height * scale) / 2;
                        }

                        int destWidth = (int)(sourceIcon.Width * scale);
                        int destHeight = (int)(sourceIcon.Height * scale);

                        destX += i * size.Width;

                        graph.DrawImage(sourceIcon,
                            new Rectangle(destX, destY, destWidth, destHeight),
                            new Rectangle(0, 0, sourceIcon.Width, sourceIcon.Height),
                            GraphicsUnit.Pixel);
                    }
                }

                var dir = Path.GetDirectoryName(targetIcon);

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                bmp.Save(targetIcon, ImageFormat.Bmp);
            }
        }

        private IconData[] CreateIconData(IIcon icon)
        {
            if (icon == null)
            {
                throw new ArgumentNullException(nameof(icon));
            }

            var sizes = icon.GetIconSizes();

            if (sizes == null || !sizes.Any())
            {
                throw new NullReferenceException($"Specified icon '{icon.GetType().FullName}' doesn't provide any sizes");
            }

            var iconsData = sizes.Select(s => new IconData(m_IconsDir, s.SourceImage, s.TargetSize, s.Name)).ToArray();

            return iconsData;
        }
    }
}