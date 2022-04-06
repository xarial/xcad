//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Base;
using Xarial.XCad.Enums;
using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.Graphics
{
    /// <summary>
    /// Represents the visual key-value element which can be attached to visual elements
    /// </summary>
    public interface IXCalloutBase : IXObject, IXTransaction, IDisposable
    {
        /// <summary>
        /// Rows of this callout
        /// </summary>
        IXCalloutRow[] Rows { get; }

        /// <summary>
        /// Background color of the callout
        /// </summary>
        StandardSelectionColor_e? Background { get; set; }

        /// <summary>
        /// Foreground color of the callout
        /// </summary>
        StandardSelectionColor_e? Foreground { get; set; }

        /// <summary>
        /// Controls the visibility of this callout
        /// </summary>
        bool Visible { get; set; }

        /// <summary>
        /// Pre creates new callout row
        /// </summary>
        /// <returns>New row template</returns>
        IXCalloutRow AddRow();
    }

    /// <summary>
    /// Represents the selection independent callout
    /// </summary>
    public interface IXCallout : IXCalloutBase 
    {
        /// <summary>
        /// Location of the callout box
        /// </summary>
        Point Location { get; set; }

        /// <summary>
        /// Anchor point (attachment) of this callout
        /// </summary>
        Point Anchor { get; set; }
    }

    /// <summary>
    /// Represents the callout associated with the selection
    /// </summary>
    public interface IXSelCallout : IXCalloutBase 
    {
        /// <summary>
        /// Attached selection object
        /// </summary>
        IXSelObject Owner { get; set; }
    }

    /// <summary>
    /// Delegate of <see cref="IXCalloutRow.ValueChanged"/> event
    /// </summary>
    /// <param name="callout">Callout where value is changed</param>
    /// <param name="row">Changed row</param>
    /// <param name="newValue">New value</param>
    /// <returns>True to accept value, False to cancel the value change</returns>
    public delegate bool CalloutRowValueChangedDelegate(IXCalloutBase callout, IXCalloutRow row, string newValue);

    /// <summary>
    /// Represents the callout row
    /// </summary>
    public interface IXCalloutRow
    {
        /// <summary>
        /// Fired when the value of the callout is changed
        /// </summary>
        event CalloutRowValueChangedDelegate ValueChanged;

        /// <summary>
        /// True if value of this callout cannot be changed
        /// </summary>
        bool IsReadOnly { get; set; }

        /// <summary>
        /// Name of the key in this row
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Value of this key
        /// </summary>
        string Value { get; set; }
    }
}
