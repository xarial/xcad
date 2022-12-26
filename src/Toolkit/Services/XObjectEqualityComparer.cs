//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Collections.Generic;

namespace Xarial.XCad.Services
{
    /// <summary>
    /// Represents the generic equality of the <see cref="IXObject"/>
    /// </summary>
    /// <typeparam name="TObj">Specific type of <see cref="IXObject"/></typeparam>
    public class XObjectEqualityComparer<TObj> : IEqualityComparer<TObj>
        where TObj : IXObject
    {
        public bool Equals(TObj x, TObj y)
        {
            if (object.ReferenceEquals(x, y))
            {
                return true;
            }

            if (x == null || y == null)
            {
                return false;
            }

            return x.Equals(y);
        }

        public int GetHashCode(TObj obj) => 0;
    }
}