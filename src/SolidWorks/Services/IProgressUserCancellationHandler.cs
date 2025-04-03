using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.XCad.SolidWorks.Services
{
    /// <summary>
    /// Service to provide a handler for the user cancellation of the <see cref="IXProgress"/> via ESC button click
    /// </summary>
    public interface IProgressUserCancellationHandler
    {
        /// <summary>
        /// Handle user cancellation
        /// </summary>
        /// <param name="sender">Progress bar</param>
        void Handle(IXProgress sender);
    }
}
