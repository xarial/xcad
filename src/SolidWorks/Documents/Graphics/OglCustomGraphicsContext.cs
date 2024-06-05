using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Documents;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.Toolkit.Graphics;

namespace Xarial.XCad.SolidWorks.Documents.Graphics
{
    internal class OglCustomGraphicsContext : CustomGraphicsContext
    {
        private readonly ISwModelView m_View;

        internal OglCustomGraphicsContext(ISwModelView view) : base(view)
        {
            m_View = view;
        }

        public override void RegisterRenderer(IXCustomGraphicsRenderer renderer)
        {
            if (IsEmpty)
            {
                ((ModelView)m_View.View).BufferSwapNotify += OnBufferSwapNotify;
            }

            base.RegisterRenderer(renderer);
        }

        public override void UnregisterRenderer(IXCustomGraphicsRenderer renderer)
        {
            base.UnregisterRenderer(renderer);

            if (IsEmpty)
            {
                ((ModelView)m_View.View).BufferSwapNotify -= OnBufferSwapNotify;
            }
        }

        private int OnBufferSwapNotify()
        {
            foreach (var renderer in this) 
            {
                renderer.Render();
            }

            return HResult.S_OK;
        }

        public override void Dispose()
        {
            base.Dispose();

            ((ModelView)m_View.View).BufferSwapNotify -= OnBufferSwapNotify;
        }
    }
}
