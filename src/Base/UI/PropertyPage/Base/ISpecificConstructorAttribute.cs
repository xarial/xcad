﻿//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;

namespace Xarial.XCad.UI.PropertyPage.Base
{
    public interface ISpecificConstructorAttribute : IAttribute
    {
        Type ConstructorType { get; }
    }
}