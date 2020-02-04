//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;

namespace Xarial.XCad.UI.PropertyPage.Base
{
    public interface IDependentOnAttribute : IAttribute
    {
        Type DependencyHandler { get; }
        object[] Dependencies { get; }
    }
}