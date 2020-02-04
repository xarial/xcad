//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.UI.PropertyPage.Base;

namespace Xarial.XCad.UI.PropertyPage.Attributes
{
    public class DependentOnAttribute : Attribute, IDependentOnAttribute
    {
        public object[] Dependencies { get; private set; }

        public Type DependencyHandler { get; private set; }

        public DependentOnAttribute(Type dependencyHandler, params object[] dependencies)
        {
            DependencyHandler = dependencyHandler;
            Dependencies = dependencies;
        }
    }
}