//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.SolidWorks.Utils
{
    internal class SwPointerEqualityComparer<T> : IEqualityComparer<T>
        where T : class
    {
        private readonly ISldWorks m_App;

        internal SwPointerEqualityComparer(ISldWorks app) 
        {
            m_App = app;
        }

        public bool Equals(T x, T y)
        {
            if (object.ReferenceEquals(x, y)) 
            {
                return true;
            }

            if (x == null || y == null) 
            {
                return false;
            }

            return m_App.IsSame(x, y) == (int)swObjectEquality.swObjectSame;
        }

        public int GetHashCode(T obj)
        {
            return 0;
        }
    }
}
