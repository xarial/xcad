using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Base;

namespace Xarial.XCad.Data
{
    public static class IXPropertyRepositoryExtension
    {
        public static void Set(this IXPropertyRepository prps, string prpName, object prpVal)
        {
            var prp = prps.GetOrPreCreate(prpName);
            prp.Value = prpVal;
            if (!prp.Exists)
            {
                prps.Add(prp);
            }
        }
    }
}
