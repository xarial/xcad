//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Documents;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.SolidWorks.UI;
using Xarial.XCad.UI;
using Xarial.XCad.Toolkit;
using Xarial.XCad.SolidWorks.Graphics;
using Xarial.XCad.Graphics;

namespace Xarial.XCad.SolidWorks.Documents
{
    /// <inheritdoc/>
    public interface ISwDocumentGraphics : IXDocumentGraphics
    {
        ISwCallout PreCreateCallout<T>()
            where T : SwCalloutBaseHandler, new();

        ISwTriad PreCreateTriad<T>()
            where T : SwTriadHandler, new();

        ISwDragArrow PreCreateDragArrow<T>()
            where T : SwDragArrowHandler, new();
    }

    internal class SwDocumentGraphics : ISwDocumentGraphics
    {
        private readonly SwDocument3D m_Doc;

        internal SwDocumentGraphics(SwDocument3D doc) 
        {
            m_Doc = doc;
        }

        public IXCallout PreCreateCallout() 
            => new SwCallout(m_Doc, m_Doc.OwnerApplication.Services.GetService<ICalloutHandlerProvider>().CreateHandler(m_Doc.OwnerApplication));

        public ISwCallout PreCreateCallout<T>() where T : SwCalloutBaseHandler, new()
            => new SwCallout(m_Doc, new T());

        public IXDragArrow PreCreateDragArrow()
            => new SwDragArrow(m_Doc, m_Doc.OwnerApplication.Services.GetService<IDragArrowHandlerProvider>().CreateHandler(m_Doc.OwnerApplication));

        public ISwDragArrow PreCreateDragArrow<T>() where T : SwDragArrowHandler, new()
            => new SwDragArrow(m_Doc, new T());

        public IXTriad PreCreateTriad() 
            => new SwTriad(m_Doc, m_Doc.OwnerApplication.Services.GetService<ITriadHandlerProvider>().CreateHandler(m_Doc.OwnerApplication));

        public ISwTriad PreCreateTriad<T>() where T : SwTriadHandler, new()
            => new SwTriad(m_Doc, new T());
    }
}
