using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Toolkit.Exceptions
{
    public class ElementAlreadyCommittedException : Exception
    {
        public ElementAlreadyCommittedException() : base("This element already committed") 
        {
        }
    }
}
