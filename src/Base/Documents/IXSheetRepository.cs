//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Base;
using Xarial.XCad.Documents.Delegates;

namespace Xarial.XCad.Documents
{
    public interface IXSheetRepository : IXRepository<IXSheet>
    {
        event SheetActivatedDelegate SheetActivated;

        IXSheet Active { get; }
    }
}
