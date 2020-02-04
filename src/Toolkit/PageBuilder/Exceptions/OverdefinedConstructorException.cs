//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;

namespace Xarial.XCad.Utils.PageBuilder.Exceptions
{
    public class OverdefinedConstructorException : Exception
    {
        internal OverdefinedConstructorException(Type constrType, Type keyType)
            : base($"Constructor of type {constrType.FullName} is already registered for {keyType.FullName}")
        {
        }
    }
}