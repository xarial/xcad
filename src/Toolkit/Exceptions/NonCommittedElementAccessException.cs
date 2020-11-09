//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

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
