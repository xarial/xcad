﻿//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Reflection;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.Binders;

namespace Xarial.XCad.Utils.PageBuilder.Base
{
    public interface IAttributeSet
    {
        int Id { get; }
        object Tag { get; }
        string Name { get; }
        string Description { get; }
        Type ContextType { get; }
        IControlDescriptor ControlDescriptor { get; }

        bool Has<TAtt>() where TAtt : IAttribute;

        TAtt Get<TAtt>() where TAtt : IAttribute;

        IEnumerable<TAtt> GetAll<TAtt>() where TAtt : IAttribute;

        void Add<TAtt>(TAtt att) where TAtt : IAttribute;
    }
}