//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.Documents;

namespace Xarial.XCad.Features.CustomFeature
{
    public interface IXCustomFeature : IXFeature
    {
        Type DefinitionType { get; set; }
        IXConfiguration Configuration { get; }
    }

    public interface IXCustomFeature<TParams> : IXCustomFeature
        where TParams : class, new()
    {
        TParams Parameters { get; set; }
    }
}