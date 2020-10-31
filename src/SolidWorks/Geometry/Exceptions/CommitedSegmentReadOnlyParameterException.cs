using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.SolidWorks.Geometry.Exceptions
{
    public class CommitedSegmentReadOnlyParameterException : Exception
    {
        public CommitedSegmentReadOnlyParameterException() : base("Parameter cannot be modified after entity is committed") 
        {
        }
    }
}
