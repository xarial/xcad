//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Collections.Generic;
using Xarial.XCad.Documents;
using Xarial.XCad.Features.CustomFeature.Delegates;
using Xarial.XCad.Geometry;

namespace Xarial.XCad.Features.CustomFeature
{
    public interface IXCustomFeatureEditor<TData, TPage>
        where TData : class, new()
        where TPage : class, new()
    {
        void Insert(IXDocument model);

        void Edit(IXDocument model, IXCustomFeature<TData> feature);
    }
}