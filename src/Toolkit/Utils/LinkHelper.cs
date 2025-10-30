using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Exceptions;
using Xarial.XCad.Toolkit.Exceptions;
using Xarial.XCad.UI.PropertyPage.Services;

namespace Xarial.XCad.Toolkit.Utils
{
    internal static class LinkHelper
    {
        internal static void TryOpenLink(string link, IXApplication app, string customLinkHelperName)
        {
            try
            {
                if (!string.IsNullOrEmpty(link))
                {
                    if (!IsUrl(link))
                    {
                        if (!Path.IsPathRooted(link))
                        {
                            throw new Exception($"Path to link is not rooted. Register custom {customLinkHelperName} service to handle help links");
                        }
                    }

                    try
                    {
                        System.Diagnostics.Process.Start(link);
                    }
                    catch (Exception ex)
                    {
                        throw new OpenHelpLinkException("Help link is not available", ex);
                    }
                }
                else
                {
                    throw new OpenHelpLinkException("Help link is not specified");
                }
            }
            catch (Exception ex)
            {
                string err;

                if (ex is IUserException)
                {
                    err = ex.Message;
                }
                else
                {
                    err = "Failed to open help link";
                }

                app.ShowMessageBox(err, XCad.Base.Enums.MessageBoxIcon_e.Warning);
            }
        }

        private static bool IsUrl(string input)
        {
            if (Uri.TryCreate(input, UriKind.Absolute, out var uri))
            {
                return !uri.IsFile;
            }

            return false;
        }
    }
}
