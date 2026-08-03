//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using Xarial.XCad.UI.PropertyPage.Base;

namespace Xarial.XCad.Toolkit.Blazor.Controls
{
    /// <summary>
    /// Blazor-specific surface of a rendered control
    /// </summary>
    public interface IBlazorPropertyManagerPageControl : IControl
    {
        /// <summary>
        /// Name of the control used as the row label
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Explicit label caption set via <see cref="Xarial.XCad.UI.PropertyPage.Attributes.LabelAttribute"/>
        /// </summary>
        string Label { get; }

        /// <summary>
        /// Tooltip / description of the control
        /// </summary>
        string Tooltip { get; }

        /// <summary>
        /// Raised whenever this control (or, for a host, any of its descendants) changes
        /// in a way that requires the hosting Razor component to re-render
        /// </summary>
        event Action Invalidated;
    }

    /// <summary>
    /// A control which hosts other controls (group, tab, page)
    /// </summary>
    public interface IBlazorPropertyManagerPageControlsHost : IBlazorPropertyManagerPageControl
    {
        /// <summary>
        /// Child controls of this host, in declaration order
        /// </summary>
        IReadOnlyList<IBlazorPropertyManagerPageControl> Controls { get; }

        /// <summary>
        /// Registers a child control with this host
        /// </summary>
        /// <param name="control">Control to add</param>
        void AddControl(IBlazorPropertyManagerPageControl control);
    }
}
