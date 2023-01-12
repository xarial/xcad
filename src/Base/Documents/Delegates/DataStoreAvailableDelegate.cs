//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Documents.Delegates
{
    /// <summary>
    /// Delegate of data store events of <see cref="IXDocument"/> (e.g. <see cref="IXDocument.StorageReadAvailable"/>
    /// </summary>
    /// <param name="doc">Sender document</param>
    public delegate void DataStoreAvailableDelegate(IXDocument doc);
}
