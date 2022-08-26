//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.SolidWorks.Graphics;

namespace Xarial.XCad.SolidWorks.Services
{
    /// <summary>
    /// Handler for the callout when used with <see cref="XCad.Documents.IXDocumentGraphics.PreCreateTriad"/>
    /// </summary>
    public interface ITriadHandlerProvider
    {
        ///<summary> This function is called when new handler instance needs to be created</summary>
        /// <param name="app">Pointer to SOLIDWORKS application</param>
        /// <returns>Triad handler</returns>
        /// <remarks>The class must be com-visible. Provide new instance of the handler with each call</remarks>
        SwTriadHandler CreateHandler(ISwApplication app);
    }

    internal class NotSetTriadHandlerProvider : ITriadHandlerProvider
    {
        public SwTriadHandler CreateHandler(ISwApplication app)
            => throw new Exception($"{nameof(ITriadHandlerProvider)} service is not registered. Configure this service within the {nameof(SwAddInEx)}::OnConfigureServices which returns the COM-visible instance of {nameof(SwTriadHandler)}");
    }
}
