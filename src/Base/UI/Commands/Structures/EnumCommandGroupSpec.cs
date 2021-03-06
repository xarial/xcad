﻿//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;

namespace Xarial.XCad.UI.Commands.Structures
{
    internal class EnumCommandGroupSpec : CommandGroupSpec
    {
        internal Type CmdGrpEnumType { get; }

        internal EnumCommandGroupSpec(Type cmdGrpEnumType, int id) : base(id)
        {
            CmdGrpEnumType = cmdGrpEnumType;
        }
    }
}