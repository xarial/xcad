//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;

namespace Xarial.XCad.UI.Commands.Structures
{
    internal class EnumCommandGroupSpec : CommandGroupSpec
    {
        internal Type CmdGrpEnumType { get; }

        internal EnumCommandGroupSpec(Type cmdGrpEnumType)
        {
            CmdGrpEnumType = cmdGrpEnumType;
        }
    }
}