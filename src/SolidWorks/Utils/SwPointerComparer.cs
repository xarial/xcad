//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

            try
            {
                //Note: ISldWorks::IsSame can crash if pointer is disconnected

                if (IsAlive(x) && IsAlive(y))
                {
                    return m_App.IsSame(x, y) == (int)swObjectEquality.swObjectSame;
                }
                else 
                {
                    return false;
                }
            }
            catch 
            {
                return false;
            }
        }

        protected virtual bool IsAlive(T obj) => true;

        public int GetHashCode(T obj)
        {
            return 0;
        }
    }

    internal class SwModelPointerEqualityComparer : IEqualityComparer<IModelDoc2>
    {
        public bool Equals(IModelDoc2 x, IModelDoc2 y)
        {
            if (object.ReferenceEquals(x, y))
            {
                return true;
            }

            try
            {
                return string.Equals(
                    x.GetTitle(),
                    y.GetTitle(),
                    StringComparison.CurrentCultureIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        public int GetHashCode(IModelDoc2 obj)
            => 0;
    }
}
