//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Enums;

namespace Xarial.XCad.UI.PropertyPage.Attributes
{
    public enum LabelFontStyle_e 
    {
        Default,
        Bold,
        Italic,
        Underline
    }

    public class LabelAttribute : Attribute, IAttribute
    {
        public string Caption { get; }
        public ControlLeftAlign_e Align { get; }
        public LabelFontStyle_e FontStyle { get; }

        public LabelAttribute(string caption, ControlLeftAlign_e align = ControlLeftAlign_e.LeftEdge, LabelFontStyle_e fontStyle = LabelFontStyle_e.Default) 
        {
            Caption = caption;
            Align = align;
            FontStyle = fontStyle;
        }
    }
}
