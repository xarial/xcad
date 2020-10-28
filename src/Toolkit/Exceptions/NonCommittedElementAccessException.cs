using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Toolkit.Exceptions
{
    public class NonCommittedElementAccessException : Exception
    {
        public NonCommittedElementAccessException() 
            : base("This is a template feature and has not been created yet. Commit this feature by adding to the feature collection") 
        {
        }
    }
}
