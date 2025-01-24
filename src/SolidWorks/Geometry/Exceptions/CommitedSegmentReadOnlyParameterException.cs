//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Exceptions;

namespace Xarial.XCad.SolidWorks.Geometry.Exceptions
{
    public class CommitedSegmentReadOnlyParameterException : CommitedElementReadOnlyParameterException
    {
        public CommitedSegmentReadOnlyParameterException() : base("Parameter cannot be modified after entity is committed") 
        {
        }
    }
}
