//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Documents;
using Xarial.XCad.SolidWorks.Documents;

namespace Xarial.XCad.SolidWorks.Services
{
    public interface IFilePathResolver
    {
        string ResolvePath(string parentDocPath, string path);
    }
}
