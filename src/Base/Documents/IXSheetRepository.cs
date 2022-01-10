//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
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
        /// Returns or sets the active sheet in this sheets repository
        /// </summary>
        IXSheet Active { get; set; }
    }
}
