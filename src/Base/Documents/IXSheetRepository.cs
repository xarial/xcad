//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Base;
using Xarial.XCad.Documents.Delegates;

namespace Xarial.XCad.Documents
{
    /// <summary>
    /// Represents sheets collection
    /// </summary>
    public interface IXSheetRepository : IXRepository<IXSheet>
    {
        /// <summary>
        /// Fired when sheet is activated
        /// </summary>
        event SheetActivatedDelegate SheetActivated;

        /// <summary>
        /// Fired when new sheet is created
        /// </summary>
        event SheetCreatedDelegate SheetCreated;

        /// <summary>
        /// Returns or sets the active sheet in this sheets repository
        /// </summary>
        IXSheet Active { get; set; }
    }
}
