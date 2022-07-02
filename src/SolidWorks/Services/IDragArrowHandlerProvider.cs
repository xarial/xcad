using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.SolidWorks.Graphics;

namespace Xarial.XCad.SolidWorks.Services
{
    /// <summary>
    /// Handler for the drag arrow when used with <see cref="XCad.Documents.IXDocumentGraphics.PreCreateDragArrow"/>
    /// </summary>
    public interface IDragArrowHandlerProvider
    {
        ///<summary> This function is called when new handler instance needs to be created</summary>
        /// <param name="app">Pointer to SOLIDWORKS application</param>
        /// <returns>Drag arrow handler</returns>
        /// <remarks>The class must be com-visible. Provide new instance of the handler with each call</remarks>
        SwDragArrowHandler CreateHandler(ISwApplication app);
    }

    internal class NotSetDragArrowHandlerProvider : IDragArrowHandlerProvider
    {
        public SwDragArrowHandler CreateHandler(ISwApplication app)
            => throw new Exception($"{nameof(IDragArrowHandlerProvider)} service is not registered. Configure this service within the {nameof(SwAddInEx)}::{nameof(SwAddInEx.OnConfigureServices)} which returns the COM-visible instance of {nameof(SwDragArrowHandler)}");
    }
}
