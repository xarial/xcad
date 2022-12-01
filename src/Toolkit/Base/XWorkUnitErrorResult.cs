using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Base;

namespace Xarial.XCad.Toolkit.Base
{
    public class XWorkUnitErrorResult : IXWorkUnitErrorResult
    {
        public Exception Error { get; }

        public XWorkUnitErrorResult(Exception error)
        {
            Error = error;
        }
    }
}
