using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.XCad.UI.PropertyPage.Services
{
    /// <summary>
    /// Service for handling the links in the <see cref="Attributes.HelpAttribute"/>
    /// </summary>
    public interface IHelpLinkHandler
    {
        /// <summary>
        /// Called when user clicks the help link
        /// </summary>
        /// <param name="link">Help link</param>
        void OpenHelpLink(string link);

        /// <summary>
        /// Called when user clicks the what's new link
        /// </summary>
        /// <param name="whatsNewLink"></param>
        void OpenWhatsNewLink(string whatsNewLink);
    }
}
