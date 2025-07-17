//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.IO;
using Xarial.XCad.Exceptions;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.Core;

namespace Xarial.XCad.Utils.PageBuilder.PageElements
{
    /// <inheritdoc/>
    public abstract class Page : Group, IPage
    {
        private IBindingManager m_Binding;

        /// <summary>
        /// Default constructor
        /// </summary>
        public Page() : base(-1, null, null)
        {
        }

        /// <inheritdoc/>
        public IBindingManager Binding
        {
            get
            {
                return m_Binding ?? (m_Binding = new BindingManager());
            }
        }

        public override void Focus()
        {
        }
    }
}