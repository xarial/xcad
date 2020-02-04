//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

namespace Xarial.XCad.Geometry
{
    public interface IXBody : IXSelObject
    {
        bool Visible { get; set; }

        IXBody Add(IXBody other);

        IXBody[] Substract(IXBody other);

        IXBody[] Common(IXBody other);
    }
}