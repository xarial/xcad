//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.Documents;

namespace Xarial.XCad.Inventor.Documents
{
    internal class AiDocumentOptions : IXDocumentOptions
    {
        public IXViewEntityKindVisibilityOptions ViewEntityKindVisibility => throw new NotImplementedException();

        internal AiDocumentOptions() 
        {
        }
    }
}
