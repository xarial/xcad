//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.IO;
using Xarial.XCad.Exceptions;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.Core;

namespace Xarial.XCad.Utils.PageBuilder.PageElements
{
    public abstract class Page : Group, IPage
    {
        private IBindingManager m_Binding;

        public Page() : base(-1, null, null)
        {
        }

        public IBindingManager Binding
        {
            get
            {
                return m_Binding ?? (m_Binding = new BindingManager());
            }
        }

        public override void Focus()
        {
        }
    }

    public static class PageExtension 
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

        public static void TryOpenLink(this Page page, string link, IXApplication app)
        {
            try
            {
                if (!string.IsNullOrEmpty(link))
                {
                    if (!IsUrl(link))
                    {
                        if (!Path.IsPathRooted(link)) 
                        {
                            link = Path.Combine(Path.GetDirectoryName(page.GetType().Assembly.Location), link);
                        }
                    }

                    try
                    {
                        System.Diagnostics.Process.Start(link);
                    }
                    catch(Exception ex)
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