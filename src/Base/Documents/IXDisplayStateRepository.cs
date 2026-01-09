//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Base;

namespace Xarial.XCad.Documents
{
    /// <summary>
    /// Collection of display states
    /// </summary>
    public interface IXDisplayStateRepository : IXRepository<IXDisplayState> 
    {
        /// <summary>
        /// Gets or sets active display state
        /// </summary>
        IXDisplayState Active { get; set; }
    }
}
