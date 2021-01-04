//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.swdocumentmgr;
using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Documents;

namespace Xarial.XCad.SwDocumentManager.Documents
{
    public interface ISwDmAssembly : ISwDmDocument3D, IXAssembly
    {
    }

    internal class SwDmAssembly : SwDmDocument3D, ISwDmAssembly
    {
        public SwDmAssembly(ISwDmApplication dmApp, ISwDMDocument doc, bool isCreated,
            Action<ISwDmDocument> createHandler, Action<ISwDmDocument> closeHandler, bool? isReadOnly = null)
            : base(dmApp, doc, isCreated, createHandler, closeHandler, isReadOnly)
        {
        }

        public IXComponentRepository Components => new SwDmComponentCollection(this, Configurations.Active);
    }
}
