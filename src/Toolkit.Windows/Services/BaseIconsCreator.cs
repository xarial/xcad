//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Xarial.XCad.Toolkit.Base;
using Xarial.XCad.Toolkit.Exceptions;
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.UI;

namespace Xarial.XCad.Toolkit.Windows.Services
{
    public class BaseIcon : IIcon
    {
        public Image Image => m_Bmp;

        public string FilePath 
        {
            get 
            {
                if (!m_IsFileCreated) 
                {
                    CreateFile();
                    m_IsFileCreated = true;
                }

                return m_FilePath;
            }
        }

        private void CreateFile()
        {
            var dir = Path.GetDirectoryName(m_FilePath);

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            ImageFormat imgFormat;

            switch (m_Format)
            {
                case IconImageFormat_e.Bmp:
                    imgFormat = ImageFormat.Bmp;
                    break;

                case IconImageFormat_e.Png:
                    imgFormat = ImageFormat.Png;
                    break;

                case IconImageFormat_e.Jpeg:
                    imgFormat = ImageFormat.Jpeg;
                    break;

                default:
                    throw new NotSupportedException();
            }

            m_Bmp.Save(m_FilePath, imgFormat);
        }

        private string m_FilePath;

        private bool m_IsFileCreated;

        private readonly bool m_IsPermanent;

        private bool m_IsDisposed;

        private readonly IconImageFormat_e m_Format;

        private readonly Bitmap m_Bmp;

        public BaseIcon(Bitmap bmp, string filePath, IconImageFormat_e format, bool isPermanent)
        {
            m_Bmp = bmp;
            m_FilePath = filePath;
            m_Format = format;

            m_IsPermanent = isPermanent;

            m_IsDisposed = isPermanent;

            m_IsFileCreated = false;
            m_IsDisposed = false;
        }

        public void Dispose()
        {
            if (!m_IsDisposed)
            {
                m_IsDisposed = true;

                m_Bmp.Dispose();

                if (m_IsFileCreated)
                {
                    try
                    {
                        if (File.Exists(m_FilePath))
                        {
                            File.Delete(m_FilePath);
                        }
                    }
                    catch
                    {
                    }
                }
            }
        }
    }

    /// <inheritdoc/>
    public class IconCollection : IIconCollection
    {
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private bool m_IsDisposed;

        internal string TempDirectory { get; }

        /// <inheritdoc/>
        public IIcon this[int index] => m_Images[index];

        private readonly bool m_IsPermanent;

        private readonly IReadOnlyList<IIcon> m_Images;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dir">Temp directory to store temp images</param>
        /// <param name="imgs">List of images</param>
        /// <param name="permanent">True if images are permanent and should not be deleted</param>
        public IconCollection(string dir, IReadOnlyList<IIcon> imgs, bool permanent)
        {
            TempDirectory = dir;
            m_IsPermanent = permanent;
            m_Images = imgs ?? Array.Empty<IIcon>();
        }

        public void Dispose()
        {
            if (!m_IsDisposed) 
            {
                m_IsDisposed = true;

                foreach (var img in m_Images)
                {
                    img.Dispose();
                }

                if (!m_IsPermanent)
                {
                    try
                    {
                        if (Directory.Exists(TempDirectory))
                        {
                            if (!Directory.EnumerateFiles(TempDirectory, "*.*", SearchOption.AllDirectories).Any())
                            {
                                Directory.Delete(TempDirectory, true);
                            }
                        }
                    }
                    catch
                    {
                    }
                }
            }
        }

        public IEnumerator<IIcon> GetEnumerator() => m_Images.GetEnumerator();
    }

    public static class IconCollectionExtension
    {
        public static string[] FilePaths(this IIconCollection imgColl)
        {
            if (imgColl != null)
            {
                return imgColl.Select(i => i.FilePath).ToArray();
            }
            else
            {
                return null;
            }
        }
    }

    public class BaseIconsCreator : IIconsCreator
    {
        private readonly string m_DefaultFolder;

        private readonly List<IconCollection> m_CreatedImages;

        public BaseIconsCreator()
            : this(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()))
        {
        }

        /// <param name="iconsDir">Directory to store the icons</param>
        public BaseIconsCreator(string iconsDir)
        {
            m_DefaultFolder = iconsDir;
            m_CreatedImages = new List<IconCollection>();
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

        public IIconCollection ConvertIcon(Base.IIconDescriptor icon, string folder = "")
        {
            var iconsFolder = GetIconsFolder(folder);

            var sizes = icon.IconSizes;

            var images = new List<BaseIcon>();

            for(int i = 0; i< sizes.Length; i++)
            {
                var bitmapPath = Path.Combine(iconsFolder, IconSpec.CreateFileName(sizes[i].BaseName, sizes[i].TargetSize, icon.Format));

                var bitmap = CreateBitmap(new IXImage[] { sizes[i].SourceImage },
                    sizes[i].TargetSize, sizes[i].Margin, icon.TransparencyKey, sizes[i].Mask);

                images.Add(new BaseIcon(bitmap, bitmapPath, icon.Format, icon.IsPermanent));
            }

            var imgsColl = new IconCollection(iconsFolder, images, icon.IsPermanent);

            m_CreatedImages.Add(imgsColl);

            return imgsColl;
        }
        
        /// <inheritdoc/>
        public IIconCollection ConvertIconsGroup(Base.IIconDescriptor[] icons, string folder = "")
        {
            if (icons == null || !icons.Any())
            {
                throw new ArgumentNullException(nameof(icons));
            }
            
            IIconSpec[,] iconsDataGroup = null;

            var transparencyKey = icons.First().TransparencyKey;
            var format = icons.First().Format;
            var isPermanent = icons.First().IsPermanent;

            var iconsFolder = GetIconsFolder(folder);

            for (int i = 0; i < icons.Length; i++)
            {
                if (icons[i].TransparencyKey != transparencyKey)
                {
                    throw new IconTransparencyMismatchException(i);
                }

                var data = icons[i].IconSizes;

                if (iconsDataGroup == null)
                {
                    iconsDataGroup = new IIconSpec[data.Length, icons.Length];
                }

                for (int j = 0; j < data.Length; j++)
                {
                    iconsDataGroup[j, i] = data[j];
                }
            }

            var images = new List<BaseIcon>();

            for (int i = 0; i < iconsDataGroup.GetLength(0); i++)
            {
                var imgs = new IXImage[iconsDataGroup.GetLength(1)];

                for (int j = 0; j < iconsDataGroup.GetLength(1); j++)
                {
                    imgs[j] = iconsDataGroup[i, j].SourceImage;
                }

                var bitmapPath = Path.Combine(iconsFolder, IconSpec.CreateFileName(iconsDataGroup[i, 0].BaseName, iconsDataGroup[i, 0].TargetSize, format));

                var bitmap = CreateBitmap(imgs,
                    iconsDataGroup[i, 0].TargetSize, iconsDataGroup[i, 0].Margin, transparencyKey, iconsDataGroup[i, 0].Mask);

                images.Add(new BaseIcon(bitmap, bitmapPath, format, isPermanent));
            }

            var imgsColl = new IconCollection(iconsFolder, images, isPermanent);

            m_CreatedImages.Add(imgsColl);

            return imgsColl;
        }

        private string GetIconsFolder(string folder)
            => string.IsNullOrEmpty(folder) ? m_DefaultFolder : folder;

        private Bitmap CreateBitmap(IXImage[] sourceIcons, Size size, int margin, Color background, ColorMaskDelegate mask)
        {
            var width = size.Width * sourceIcons.Length;
            var height = size.Height;

            var pixelFormat = background == Color.Transparent ? PixelFormat.Format32bppArgb : PixelFormat.Format24bppRgb;

            var bmp = new Bitmap(width, height, pixelFormat);

            using (var graph = System.Drawing.Graphics.FromImage(bmp))
            {
                graph.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graph.SmoothingMode = SmoothingMode.HighQuality;
                graph.PixelOffsetMode = PixelOffsetMode.HighQuality;

                if (background != Color.Transparent)
                {
                    using (var brush = new SolidBrush(background))
                    {
                        graph.FillRectangle(brush, 0, 0, bmp.Width, bmp.Height);
                    }
                }

                for (int i = 0; i < sourceIcons.Length; i++)
                {
                    var targSize = new Size(size.Width - margin * 2, size.Height - margin * 2);

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

            return bmp;
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
                if (background != Color.Transparent)
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

        private void Clear()
        {
            foreach (var createdImages in m_CreatedImages)
            {
                createdImages.Dispose();
            }

            foreach (var tempDir in m_CreatedImages.Select(i => i.TempDirectory).Distinct(StringComparer.CurrentCultureIgnoreCase))
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

            m_CreatedImages.Clear();
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
