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
    /// Exception indicates that the version of the parameters of macro feature
    /// belongs of a never version of the add-in
    /// </summary>
    /// <remarks>Suggest users to upgrade the add-in version to support the feature</remarks>
    public class FutureVersionParametersException : Exception
    {
        internal FutureVersionParametersException(Type paramType, Version curParamVersion, Version paramVersion)
            : base($"Future version of parameters '{paramType.FullName}' {paramVersion} are stored in macro feature. Current version: {curParamVersion}")
        {
        }
    }
}