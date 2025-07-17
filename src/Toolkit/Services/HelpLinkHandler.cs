using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Exceptions;
using Xarial.XCad.UI.PropertyPage.Services;

namespace Xarial.XCad.Toolkit.Services
{
    /// <summary>
    /// Default help link handler which executes the link
    /// </summary>
    public class HelpLinkHandler : IHelpLinkHandler
    {
        private class OpenHelpLinkException : Exception, IUserException
        {
            internal OpenHelpLinkException(string err) : base(err)
            {
            }

            internal OpenHelpLinkException(string err, Exception inner) : base(err, inner)
            {
            }
        }

        private readonly IXApplication m_App;
        private readonly string m_WorkDir;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="app">Pointer to application</param>
        /// <param name="workDir">Current working directory</param>
        public HelpLinkHandler(IXApplication app, string workDir)
        {
            m_App = app;
            m_WorkDir = workDir;
        }

        /// <inheritdoc/>
        public void OpenHelpLink(string link) => TryOpenLink(link);

        /// <inheritdoc/>
        public void OpenWhatsNewLink(string whatsNewLink) => TryOpenLink(whatsNewLink);

        private void TryOpenLink(string link)
        {
            try
            {
                if (!string.IsNullOrEmpty(link))
                {
                    if (!IsUrl(link))
                    {
                        if (!Path.IsPathRooted(link))
                        {
                            link = Path.Combine(Path.GetDirectoryName(m_WorkDir), link);
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

                m_App.ShowMessageBox(err, XCad.Base.Enums.MessageBoxIcon_e.Warning);
            }
        }

        private bool IsUrl(string input)
        {
            if (Uri.TryCreate(input, UriKind.Absolute, out var uri))
            {
                return !uri.IsFile;
            }

            return false;
        }
    }
}
