//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Enums;

namespace Xarial.XCad.UI.PropertyPage.Attributes
{
    /// <summary>
    /// Additional options for text block control
    /// </summary>
    /// <remarks>Applied to property of type <see cref="string"/></remarks>
    public class TextBlockOptionsAttribute : Attribute, IAttribute
    {
        /// <summary>
        /// Specific text box style
        /// </summary>
        public TextAlignment_e TextAlignment { get; }

        /// <summary>
        /// Font style of the label
        /// </summary>
        public FontStyle_e FontStyle { get; }

        /// <summary>
        /// Text font
        /// </summary>
        public string Font { get; }

        /// <summary>
        /// Constructor for text block options
        /// </summary>
        /// <param name="textAlignment">Text block font alignment</param>
        /// <param name="fontStyle">Font style</param>
        /// <param name="font">Font name</param>
        public TextBlockOptionsAttribute(TextAlignment_e textAlignment = TextAlignment_e.Default, FontStyle_e fontStyle = FontStyle_e.Default, string font = "")
        {
            TextAlignment = textAlignment;
            FontStyle = fontStyle;
            Font = font;
        }
    }
}