//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.XCad.Toolkit.Utils
{
    /// <summary>
    /// Represents the thumbnai
    /// </summary>
    public interface IShellThumbnail : IDisposable 
    {
        /// <summary>
        /// hBitmap of the thumbnail image
        /// </summary>
        IntPtr BitmapHandle { get; }
    }

    /// <inheritdoc/>
    public class ShellThumbnail : IShellThumbnail
    {
        /// <inheritdoc/>
        public IntPtr BitmapHandle { get; }

        #region Windows API

        [ComImport]
        [Guid("bcc18b79-ba16-442f-80c4-8a59c30c463b")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IShellItemImageFactory
        {
            [PreserveSig]
            void GetImage(
                [In, MarshalAs(UnmanagedType.Struct)] SIZE size,
                [In] int flags,
                [Out] out IntPtr phbm);
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        private static extern void SHCreateItemFromParsingName(
            [In][MarshalAs(UnmanagedType.LPWStr)] string pszPath,
            [In] IntPtr pbc,
            [In][MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [Out][MarshalAs(UnmanagedType.Interface, IidParameterIndex = 2)] out IShellItemImageFactory ppv);

        [StructLayout(LayoutKind.Sequential)]
        private struct SIZE
        {
            public int cx;
            public int cy;

            public SIZE(int cx, int cy)
            {
                this.cx = cx;
                this.cy = cy;
            }
        }

        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteObject([In] IntPtr hObject);

        #endregion

        public ShellThumbnail(string filePath, int width = 1024, int height = 1024) 
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File '{filePath}' not found");
            }

            var shellItem2Guid = new Guid("7E9FB0D3-919F-4307-AB2E-9B1860310C93");

            SHCreateItemFromParsingName(filePath, IntPtr.Zero, shellItem2Guid, out var imgFactShellItem);

            var size = new SIZE(width, height);

            imgFactShellItem.GetImage(size, 0, out IntPtr hBitmap);

            BitmapHandle = hBitmap;
        }

        public void Dispose()
        {
            try
            {
                if (!IntPtr.Zero.Equals(BitmapHandle))
                {
                    DeleteObject(BitmapHandle);
                }
            }
            catch 
            {
            }
        }
    }
}
