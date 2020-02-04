//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;

namespace Xarial.XCad.Utils.PageBuilder.Exceptions
{
    public class ConstructorNotFoundException : Exception
    {
        internal ConstructorNotFoundException(Type type, string message = "")
            : base($"Constructor for type {type.FullName} is not found. {message}")
        {
        }
    }
}