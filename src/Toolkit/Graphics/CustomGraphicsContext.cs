//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xarial.XCad.Documents;

namespace Xarial.XCad.Toolkit.Graphics
{
    /// <summary>
    /// Default context for custom graphics
    /// </summary>
    /// <remarks>This is placeholder context and does not render any graphics. Override this class to implement actual rendering</remarks>
    public abstract class CustomGraphicsContext : IXCustomGraphicsContext
    {
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private readonly List<IXCustomGraphicsRenderer> m_Renderers;

        /// <summary>
        /// Checks if this context has any renderers
        /// </summary>
        protected bool IsEmpty => m_Renderers.Count == 0;

        private readonly IXModelView m_View;

        /// <summary>
        /// Default constructor
        /// </summary>
        public CustomGraphicsContext(IXModelView view) 
        {
            m_View = view;
            m_Renderers = new List<IXCustomGraphicsRenderer>();
        }

        /// <inheritdoc/>
        public virtual IEnumerator<IXCustomGraphicsRenderer> GetEnumerator() => m_Renderers.GetEnumerator();

        /// <inheritdoc/>
        public virtual void RegisterRenderer(IXCustomGraphicsRenderer renderer)
        {
            if (!m_Renderers.Contains(renderer))
            {
                m_Renderers.Add(renderer);
                m_View.Update();
            }
            else 
            {
                throw new Exception("Renderer already registered");
            }
        }

        /// <inheritdoc/>
        public virtual void UnregisterRenderer(IXCustomGraphicsRenderer renderer)
        {
            if (m_Renderers.Contains(renderer))
            {
                renderer.Dispose();

                m_Renderers.Remove(renderer);

                m_View.Update();
            }
            else
            {
                throw new Exception("Renderer is not registered");
            }
        }

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            foreach (var renderer in m_Renderers)
            {
                renderer.Dispose();
            } 
        }
    }
}
