//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.XCad.SolidWorks
{
    public interface ISwMaterial : IXMaterial
    {
    }

    internal class SwMaterial : ISwMaterial
    {
        public string Database { get; }
        public string Name { get; }

        internal SwMaterial(string name, string database) 
        {
            Name = name;
            Database = database;
        }
    }
}
