using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.XCad.Documents.Delegates
{
    /// <summary>
    /// Delegate of <see cref="IXSheetRepository.SheetCreated"/> event
    /// </summary>
    /// <param name="drawing">Sheet where view is created</param>
    /// <param name="sheet">Created drawing sheet</param>
    public delegate void SheetCreatedDelegate(IXDrawing drawing, IXSheet sheet);
}
