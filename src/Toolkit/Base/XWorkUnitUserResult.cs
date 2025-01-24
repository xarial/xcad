//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Base;

namespace Xarial.XCad.Toolkit.Base
{
    public class XWorkUnitUserResult<TRes> : IXWorkUnitUserResult<TRes>
    {
        public TRes Result { get; }

        public XWorkUnitUserResult(TRes result)
        {
            Result = result;
        }
    }
}
