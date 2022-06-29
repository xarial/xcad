//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Xarial.XCad.SolidWorks.Base;
using Xarial.XCad.SolidWorks.Exceptions;
using Xarial.XCad.UI;

namespace Xarial.XCad.SolidWorks.Services
{
    public class BaseIconsCreator : IIconsCreator
    {
        private readonly string m_DefaultFolder;

        private readonly List<string> m_TempFolders;
        private readonly List<string> m_TempIcons;

        public BaseIconsCreator()
            : this(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()))
        {
        }

        /// <param name="iconsDir">Directory to store the icons</param>
        /// <param name="disposeIcons">True to remove the icons when class is disposed</param>
        public BaseIconsCreator(string iconsDir)
        {
            m_DefaultFolder = iconsDir;
            m_TempFolders = new List<string>();
            m_TempIcons = new List<string>();
        }

        /// <summary>
        /// Replaces the pixels in the image based on the custom replacer handler
        /// </summary>
        /// <param name="icon">Image to replace</param>
        /// <param name="mask">Handler to replace which is called for each pixel</param>
        /// <returns>Resulting image</returns>
        protected Image ReplaceColor(Image icon, ColorMaskDelegate mask)
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
                mask.Invoke(ref rgba[i + 2], ref rgba[i + 1], ref rgba[i], ref rgba[i + 3]);
            }

            Marshal.Copy(rgba, 0, bmpData.Scan0, rgba.Length);

            maskImg.UnlockBits(bmpData);

            return maskImg;
        }

        public string[] ConvertIcon(IIcon icon, string folder = "")
        {
            var iconsFolder = GetIconsFolder(folder, icon.IsPermanent);

            var sizes = icon.GetIconSizes().ToArray();

            var bitmapPaths = new string[sizes.Length];

            for(int i = 0; i< sizes.Length; i++)
            {
                bitmapPaths[i] = Path.Combine(iconsFolder, sizes[i].Name);

                CreateBitmap(new IXImage[] { sizes[i].SourceImage },
                    bitmapPaths[i], sizes[i].TargetSize, sizes[i].Offset, icon.TransparencyKey, sizes[i].Mask, icon.IsPermanent);
            }

            return bitmapPaths;
        }
        
        /// <inheritdoc/>
        public string[] ConvertIconsGroup(IIcon[] icons, string folder = "")
        {
            if (icons == null || !icons.Any())
            {
                throw new ArgumentNullException(nameof(icons));
            }
            
            IIconSpec[,] iconsDataGroup = null;

            var transparencyKey = icons.First().TransparencyKey;
            var permanent = icons.First().IsPermanent;

            var iconsFolder = GetIconsFolder(folder, permanent);

            for (int i = 0; i < icons.Length; i++)
            {
                if (icons[i].TransparencyKey != transparencyKey)
                {
                    throw new IconTransparencyMismatchException(i);
                }

                var data = icons[i].GetIconSizes().ToArray();

                if (iconsDataGroup == null)
                {
                    iconsDataGroup = new IIconSpec[data.Length, icons.Length];
                }

                for (int j = 0; j < data.Length; j++)
                {
                    iconsDataGroup[j, i] = data[j];
                }
            }

            var iconsPaths = new string[iconsDataGroup.GetLength(0)];

            for (int i = 0; i < iconsDataGroup.GetLength(0); i++)
            {
                var imgs = new IXImage[iconsDataGroup.GetLength(1)];
                for (int j = 0; j < iconsDataGroup.GetLength(1); j++)
                {
                    imgs[j] = iconsDataGroup[i, j].SourceImage;
                }

                iconsPaths[i] = Path.Combine(iconsFolder, iconsDataGroup[i, 0].Name);

                CreateBitmap(imgs, iconsPaths[i],
                    iconsDataGroup[i, 0].TargetSize, iconsDataGroup[i, 0].Offset, transparencyKey, iconsDataGroup[i, 0].Mask, permanent);
            }

            return iconsPaths;
        }

        private string GetIconsFolder(string folder, bool permanent)
        {
            var iconsFolder = string.IsNullOrEmpty(folder) ? m_DefaultFolder : folder;

            if (!permanent)
            {
                if (!m_TempFolders.Contains(iconsFolder, StringComparer.CurrentCultureIgnoreCase))
                {
                    m_TempFolders.Add(iconsFolder);
                }
            }

            return iconsFolder;
        }

        private void CreateBitmap(IXImage[] sourceIcons,
            string targetIcon, Size size, int offset, Color background, ColorMaskDelegate mask, bool permanent)
        {
            var width = size.Width * sourceIcons.Length;
            var height = size.Height;

            using (var bmp = new Bitmap(width,
                height, PixelFormat.Format24bppRgb))
            {
                using (var graph = System.Drawing.Graphics.FromImage(bmp))
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
                        var targSize = new Size(size.Width - offset * 2, size.Height - offset * 2);

                        var sourceIcon = CreateImage(sourceIcons[i], targSize, mask, background);

                        if (bmp.HorizontalResolution != sourceIcon.HorizontalResolution
                                || bmp.VerticalResolution != sourceIcon.VerticalResolution)
                        {
                            bmp.SetResolution(
                                sourceIcon.HorizontalResolution,
                                sourceIcon.VerticalResolution);
                        }

                        var widthScale = (double)targSize.Width / (double)sourceIcon.Width;
                        var heightScale = (double)targSize.Height / (double)sourceIcon.Height;
                        var scale = Math.Min(widthScale, heightScale);

                        if (scale < 0)
                        {
                            throw new Exception("Target size of the icon cannot be calculated due to offset constraint");
                        }

                        var destX = (int)(size.Width - sourceIcon.Width * scale) / 2;
                        var destY = (int)(size.Height - sourceIcon.Height * scale) / 2;

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

                if (!permanent) 
                {
                    if (!m_TempIcons.Contains(targetIcon, StringComparer.CurrentCultureIgnoreCase))
                    {
                        m_TempIcons.Add(targetIcon);
                    }
                }
            }
        }

        protected virtual Image CreateImage(IXImage icon, 
            Size size, ColorMaskDelegate mask, Color background)
        {
            var img = FromXImage(icon);

            if (mask != null)
            {
                img = ReplaceColor(img, mask);
            }
            else 
            {
                void ConflictingBackgroundPixelMask(ref byte r, ref byte g, ref byte b, ref byte a) 
                {
                    if (r == background.R && g == background.G && b == background.B && a == background.A)
                    {
                        b = (byte)((b == 0) ? 1 : (b - 1));
                    }
                }

                img = ReplaceColor(img, ConflictingBackgroundPixelMask);
            }

            return img;
        }

        private Image FromXImage(IXImage img)
        {
            using (var str = new MemoryStream(img.Buffer))
            {
                return Image.FromStream(str);
            }
        }

        public void Clear()
        {
            foreach (var tempIcon in m_TempIcons)
            {
                try
                {
                    if (File.Exists(tempIcon))
                    {
                        File.Delete(tempIcon);
                    }
                }
                catch
                {
                }
            }

            m_TempIcons.Clear();

            foreach (var tempDir in m_TempFolders)
            {
                try
                {
                    if (Directory.Exists(tempDir))
                    {
                        if (!Directory.EnumerateFiles(tempDir, "*.*", SearchOption.AllDirectories).Any())
                        {
                            Directory.Delete(tempDir, true);
                        }
                    }
                }
                catch
                {
                }
            }

            m_TempFolders.Clear();
        }

        /// <summary>
        /// Disposing temp icon files
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Clear();
            }
        }
    }
}
