//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

namespace Xarial.XCad.Utils.PageBuilder.Base
{
    public interface IDependencyHandler
    {
        void UpdateState(IBinding binding, IBinding[] dependencies);
    }
}