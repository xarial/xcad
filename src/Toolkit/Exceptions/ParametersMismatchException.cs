//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;

namespace Xarial.XCad.Exceptions
{
    //TODO: this might need to go to base

    /// <summary>
    /// Exception indicates that the macro feature parameters have not been updated via <see cref="Services.IParametersVersionConverter"/>
    /// </summary>
    public class ParametersMismatchException : Exception
    {
        internal ParametersMismatchException(string reason)
            : base($"{reason}. Please reinsert the feature as changing the dimensions in parameters is not supported")
        {
        }
    }
}