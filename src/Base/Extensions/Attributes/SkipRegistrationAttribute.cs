//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;

namespace Xarial.XCad.Extensions.Attributes
{
    public class SkipRegistrationAttribute : Attribute
    {
        public bool Skip { get; private set; }

        public SkipRegistrationAttribute() : this(true)
        {
        }

        public SkipRegistrationAttribute(bool skip)
        {
            Skip = skip;
        }
    }
}