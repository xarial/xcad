using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Annotations;
using Xarial.XCad.Base;

namespace Xarial.XCad.Annotations
{
    public interface IXDimensionsRepository : IXRepository<IXDimension>
    {
        IXDimension this[string name] { get; }
    }
}
