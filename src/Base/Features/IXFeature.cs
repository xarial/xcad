//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Annotations;
using Xarial.XCad.Base;

namespace Xarial.XCad.Features
{
    public interface IXFeature : IXSelObject, IXColorizable, IXTransaction
    {
        string Name { get; set; }
        IXDimensionRepository Dimensions { get; }
    }
}