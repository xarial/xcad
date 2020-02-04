//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Base;

namespace Xarial.XCad.Sketch
{
    public interface IXSketchEntityRepository : IXRepository<IXSketchEntity>
    {
        IXSketchLine NewLine();

        IXSketchPoint NewPoint();
    }
}