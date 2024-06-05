//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;

namespace Xarial.XCad.Extensions.Attributes
{
    /// <summary>
    /// Attribute of <see cref="IXExtension"/> indicates that this add-in should not automatically register
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class SkipRegistrationAttribute : Attribute
    {
        /// <summary>
        /// True to skip the registration
        /// </summary>
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