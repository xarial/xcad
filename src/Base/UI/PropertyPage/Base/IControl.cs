//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;

namespace Xarial.XCad.UI.PropertyPage.Base
{
    /// <summary>
    /// Delegate of <see cref="IControl.ValueChanged"/> event
    /// </summary>
    /// <param name="sender">Control</param>
    /// <param name="newValue">New value of the control</param>
    public delegate void ControlObjectValueChangedDelegate(IControl sender, object newValue);

    /// <summary>
    /// Represents the control in the page
    /// </summary>
    public interface IControl : IDisposable
    {
        /// <summary>
        /// Fired when the value of the control has been changed
        /// </summary>
        event ControlObjectValueChangedDelegate ValueChanged;

        /// <summary>
        /// Metadata attached to this control
        /// </summary>
        IMetadata[] Metadata { get; }

        /// <summary>
        /// Manages the enable state of the control
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Manages the visibility of the control
        /// </summary>
        bool Visible { get; set; }

        /// <summary>
        /// Identifier of this control
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Custom tag associated with this control via <see cref="IControlTagAttribute"/>
        /// </summary>
        object Tag { get; }

        /// <summary>
        /// Gets the value from this control
        /// </summary>
        /// <returns></returns>
        object GetValue();

        /// <summary>
        /// Sets the value to this control
        /// </summary>
        /// <param name="value"></param>
        void SetValue(object value);

        /// <summary>
        /// Shows tooltip for this control
        /// </summary>
        /// <param name="title">Title of the tooltip</param>
        /// <param name="msg">Message to show in the tooltip</param>
        void ShowTooltip(string title, string msg);

        /// <summary>
        /// Updated this control
        /// </summary>
        void Update();

        /// <summary>
        /// Sets the focus to the current control
        /// </summary>
        void Focus();

        /// <summary>
        /// Specific type of the value for this control
        /// </summary>
        /// <remarks>This is a type of the property this control value is bound to</remarks>
        Type ValueType { get; }
    }
}