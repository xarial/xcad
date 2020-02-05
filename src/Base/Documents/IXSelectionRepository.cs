using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Base;

namespace Xarial.XCad.Documents
{
    public interface IXSelectionRepository : IXRepository<IXSelObject>
    {
        void Clear();
    }
}
