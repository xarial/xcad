//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Threading;
using Xarial.XCad.Base;
using Xarial.XCad.Base.Enums;
using Xarial.XCad.Services;

namespace Xarial.XCad.Toolkit.Services
{
    /// <summary>
    /// Default implementation of progress user cancellation handler
    /// </summary>
    public class DefaultProgressUserCancellationHandler : IProgressUserCancellationHandler
    {
        private readonly IXLogger m_Logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger">Logger</param>
        public DefaultProgressUserCancellationHandler(IXLogger logger) 
        {
            m_Logger = logger;
        }

        /// <inheritdoc/>
        public void Handle(IXProgress sender, CancellationTokenSource cts)
        {
            m_Logger.Log("Cancellation requested by user", LoggerMessageSeverity_e.Debug);

            if (cts != null) 
            {
                cts.Cancel();
            }
        }
    }
}
