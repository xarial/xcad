//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

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
