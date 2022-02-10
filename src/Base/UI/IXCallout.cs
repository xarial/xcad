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
using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.UI
{
    /// <summary>
    /// Represents the visual key-value element which can be attached to visual elements
    /// </summary>
    public interface IXCalloutBase : IXObject, IXTransaction, IDisposable
    {
        /// <summary>
        /// Rows of this callout
        /// </summary>
        IXCalloutRow[] Rows { get; set; }

        /// <summary>
        /// Shows this callout
        /// </summary>
        void Show();

        /// <summary>
        /// Pre creates new callout row
        /// </summary>
        /// <returns>New row template</returns>
        IXCalloutRow PreCreateRow();
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

        /// <summary>
        /// Hides this callout
        /// </summary>
        void Hide();
    }

    /// <summary>
    /// Represents the callout associated with the selection
    /// </summary>
    public interface IXSelCallout : IXCalloutBase 
    {
        /// <summary>
        /// Attached selection object
        /// </summary>
        IXSelObject Owner { get; }
    }

    /// <summary>
    /// Represents the callout row
    /// </summary>
    public interface IXCalloutRow : IXTransaction
    {
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
