//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.Data
{
    public interface ITagsManager
    {
        bool Contains(string name);
        void Put<T>(string name, T value);
        T Get<T>(string name);
        T Pop<T>(string name);
    }
}
