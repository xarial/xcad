using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.XCad.Toolkit.Windows.Utils
{
    /// <summary>
    /// Utility tools for File Explorer
    /// </summary>
    public static class FileExplorer
    {
        /// <summary>
        /// Returns if the file extensions shown for known file types
        /// </summary>
        public static bool IsFileExtensionShown
        {
            get
            {
                try
                {
                    const string REG_KEY = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced";
                    const int UNCHECKED = 0;
                    var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(REG_KEY);

                    if (key != null)
                    {
                        return (int)key.GetValue("HideFileExt") == UNCHECKED;
                    }
                }
                catch
                {
                }

                return false;
            }
        }
    }
}
