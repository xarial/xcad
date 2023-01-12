//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.UI.PropertyPage.Base;

namespace Xarial.XCad.UI.PropertyPage.Attributes
{
    /// <summary>
    /// Group box options
    /// </summary>
    [Flags]
    public enum GroupBoxOptions_e 
    {
        /// <summary>
        /// Display the group collapsed by default
        /// </summary>
        Collapsed = 1
    }

    /// <summary>
    /// Additional options for the group box control
    /// </summary>
    public interface IGroupBoxOptionsAttribute : IAttribute 
    {
        /// <summary>
        /// Options of the group box
        /// </summary>
        GroupBoxOptions_e Options { get; }
    }

    /// <inheritdoc/>>
    public class GroupBoxOptionsAttribute : Attribute, IGroupBoxOptionsAttribute
    {
        /// <inheritdoc/>>
        public GroupBoxOptions_e Options { get; }

        public GroupBoxOptionsAttribute(GroupBoxOptions_e opts) 
        {
            Options = opts;
        }
    }
}
