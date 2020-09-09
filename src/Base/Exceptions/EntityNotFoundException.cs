//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Exceptions
{
    public class EntityNotFoundException : KeyNotFoundException
    {
        public EntityNotFoundException(string name) : base($"Entity '{name}' is not found in the repository") 
        {
        }
    }
}
