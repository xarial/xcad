//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Collections.Generic;

namespace Xarial.XCad.Geometry
{
    public interface IXEntity : IXSelObject
    {
        IXBody Body { get; }
        IEnumerable<IXEntity> AdjacentEntities { get; }
    }
}