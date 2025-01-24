//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Documents;
using Xarial.XCad.SolidWorks.Documents;

namespace Xarial.XCad.SolidWorks.Utils
{
    internal static class SwLayerHelper
    {
        public static void SetLayer<T>(T ent, IXLayer layer, Action<T, string> setter)
            where T : IHasLayer
        {
            if (ent.OwnerDocument is ISwDrawing)
            {
                setter.Invoke(ent, layer?.Name);
            }
            else
            {
                throw new NotSupportedException("Layers are only supported in drawings");
            }
        }

        public static IXLayer GetLayer<T>(T ent, Func<T, string> getter)
            where T : IHasLayer
        {
            var layerName = getter.Invoke(ent);

            if (!string.IsNullOrEmpty(layerName))
            {
                return null;
            }
            else
            {
                if (ent.OwnerDocument is ISwDrawing)
                {
                    return ((ISwDrawing)ent.OwnerDocument).Layers[layerName];
                }
                else
                {
                    throw new NotSupportedException("Layers are only supported in drawings");
                }
            }
        }
    }
}
