//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.UI.PropertyPage.Base;

namespace Xarial.XCad.UI.PropertyPage.Base
{
    /// <summary>
    /// Represents binding between UI control and data model
    /// </summary>
    public interface IBinding
    {
        /// <summary>
        /// Event raised when binding is changed
        /// </summary>
        event Action<IBinding> Changed;

        /// <summary>
        /// Event raised when model of the binding is changed
        /// </summary>
        event Action<IBinding> ModelUpdated;

        /// <summary>
        /// Event raised when control value is changed
        /// </summary>
        event Action<IBinding> ControlUpdated;

        /// <summary>
        /// Indicates that this binding is silent and <see cref="IXPropertyPage.DataChanged"/> should not be raised
        /// </summary>
        bool Silent { get; }

        /// <summary>
        /// Metadata associated with this binding
        /// </summary>
        IMetadata[] Metadata { get; }

        /// <summary>
        /// Descriptor of the control
        /// </summary>
        IControlDescriptor ControlDescriptor { get; }

        /// <summary>
        /// Control associated with the binding
        /// </summary>
        IControl Control { get; }

        /// <summary>
        /// Update the control
        /// </summary>
        void UpdateControl();

        /// <summary>
        /// Updated data model
        /// </summary>
        void UpdateDataModel();

        /// <summary>
        /// Sets the value for the binding
        /// </summary>
        /// <param name="val">Value</param>
        void SetValue(string val);
    }
}