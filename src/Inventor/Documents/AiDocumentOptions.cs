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
