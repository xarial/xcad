//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Enums;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Enums;

namespace Xarial.XCad.UI.PropertyPage.Attributes
{
    /// <summary>
    /// Indicates that the control should have a label associated with it
    /// </summary>
    public class LabelAttribute : Attribute, IAttribute
    {
        /// <summary>
        /// Caption of the label
        /// </summary>
        public string Caption { get; }

        /// <summary>
        /// Label alighnment
        /// </summary>
        public ControlLeftAlign_e Align { get; }

        /// <summary>
        /// Font style of the label
        /// </summary>
        public FontStyle_e FontStyle { get; }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="caption">Label caption</param>
        /// <param name="align">Label alignment</param>
        /// <param name="fontStyle">Font style</param>
        public LabelAttribute(string caption, ControlLeftAlign_e align = ControlLeftAlign_e.LeftEdge, FontStyle_e fontStyle = FontStyle_e.Regular) 
        {
            Caption = caption;
            Align = align;
            FontStyle = fontStyle;
        }
    }
}
