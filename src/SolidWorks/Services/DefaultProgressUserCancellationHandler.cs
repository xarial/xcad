using Xarial.XCad.Base;
using Xarial.XCad.Base.Enums;

namespace Xarial.XCad.SolidWorks.Services
{
    internal class DefaultProgressUserCancellationHandler : IProgressUserCancellationHandler
    {
        private readonly IXLogger m_Logger;

        public DefaultProgressUserCancellationHandler(IXLogger logger) 
        {
            m_Logger = logger;
        }

        public void Handle(IXProgress sender)
        {
            m_Logger.Log("Cancellation requested by user", LoggerMessageSeverity_e.Debug);
        }
    }
}
