using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.XCad.Documents.Delegates
{
    /// <summary>
    /// Delegate of <see cref="IXComponent.Moved"/> event
    /// </summary>
    /// <param name="comp">Component being moved</param>
    public delegate void ComponentMovedDelegate(IXComponent comp);
}
