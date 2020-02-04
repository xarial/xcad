//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.Utils.PageBuilder.Base.Attributes;

namespace Xarial.XCad.Utils.PageBuilder.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class DefaultTypeAttribute : Attribute, IDefaultTypeAttribute
    {
        public Type Type { get; private set; }

        public DefaultTypeAttribute(Type type)
        {
            Type = type;
        }
    }
}