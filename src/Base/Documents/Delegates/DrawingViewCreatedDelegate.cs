//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.XCad.Documents.Delegates
{
    /// <summary>
    /// Delegate of <see cref="IXDrawingViewRepository.ViewCreated"/> event
    /// </summary>
    /// <param name="drawing">Drawing where the view is created</param>
    /// <param name="sheet">Sheet where view is created</param>
    /// <param name="view">Created drawing view</param>
    public delegate void DrawingViewCreatedDelegate(IXDrawing drawing, IXSheet sheet, IXDrawingView view);
}
