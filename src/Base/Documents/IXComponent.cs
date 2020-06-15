using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Documents
{
    public interface IXComponent : IXSelObject
    {
        string Name { get; }
        IXDocument3D Document { get; }
        IXComponentRepository Children { get; }
    }
}
