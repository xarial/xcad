//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Drawing;
using Xarial.XCad.Base.Enums;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Enums;
using Xarial.XCad.UI.PropertyPage.Services;

namespace Xarial.XCad.UI.PropertyPage.Attributes
{
    /// <summary>
    /// Represents the standard selection colors
    /// </summary>
    public enum StandardSelectionColor_e 
    {
        /// <summary>
        /// Primary standard selection color
        /// </summary>
        Primary = 104,

        /// <summary>
        /// Secondary standard selection color
        /// </summary>
        Secondary = 105,

        /// <summary>
        /// Tertiary standard selection color
        /// </summary>
        Tertiary = 106,
    }

    /// <summary>
    /// Additional options for selection box control
    /// </summary>
    public class SelectionBoxOptionsAttribute : Attribute, IAttribute
    {
        /// <summary>
        /// Style of this selection box
        /// </summary>
        public SelectionBoxStyle_e Style { get; set; }

        /// <summary>
        /// Standard color of selection box
        /// </summary>
        public StandardSelectionColor_e SelectionColor { get; set; }

        /// <summary>
        /// Allowed entities filter for the selection
        /// </summary>
        public SelectType_e[] Filters { get; set; }

        /// <summary>
        /// Selection mark associated with this selection box
        /// </summary>
        public int SelectionMark { get; set; }

        /// <summary>
        /// Custom filter for this selection box
        /// </summary>
        public Type CustomFilter { get; set; }

        /// <summary>
        /// Sets the current selection box as default focus
        /// </summary>
        public bool Focused { get; set; }

        /// <summary>
        /// Constructor for selection box options
        /// </summary>
        /// <param name="style">Selection box style</param>
        /// <param name="selColor">Color of the selections in this selection box</param>
        public SelectionBoxOptionsAttribute(
            SelectionBoxStyle_e style = SelectionBoxStyle_e.None,
            StandardSelectionColor_e selColor = 0) : this(-1, null, style, selColor)
        {
        }

        /// <summary>
        /// Constructor for selection box options
        /// </summary>
        /// <param name="filters">Filters allowed for selection into this selection box</param>
        public SelectionBoxOptionsAttribute(params SelectType_e[] filters)
            : this(-1, filters)
        {
        }

        /// <inheritdoc cref="SelectionBoxOptionsAttribute(SelectType_e[])"/>
        /// <param name="mark">Selection mark. If multiple selections box are used - use different selection marks for each of them
        /// to differentiate the selections</param>
        public SelectionBoxOptionsAttribute(int mark, params SelectType_e[] filters)
            : this(mark, null, SelectionBoxStyle_e.None, 0, filters)
        {
        }

        /// <inheritdoc cref="SelectionBoxOptionsAttribute(int, Type, SelectionBoxStyle_e, int, SelectType_e[])"/>
        public SelectionBoxOptionsAttribute(Type customFilter, params SelectType_e[] filters)
            : this(-1, customFilter, SelectionBoxStyle_e.None, 0, filters)
        {
        }

        /// <inheritdoc cref="SelectionBoxOptionsAttribute(int, SelectType_e[])"/>
        /// <param name="customFilter">Type of custom filter of <see cref="ISelectionCustomFilter"/> for custom logic for filtering selection objects</param>
        /// <exception cref="InvalidCastException"/>
        public SelectionBoxOptionsAttribute(int mark, Type customFilter, SelectionBoxStyle_e style,
            StandardSelectionColor_e selColor, params SelectType_e[] filters)
        {
            Style = style;
            SelectionColor = selColor;

            Filters = filters;
            SelectionMark = mark;

            if (customFilter != null)
            {
                if (!typeof(ISelectionCustomFilter).IsAssignableFrom(customFilter))
                {
                    throw new InvalidCastException($"{customFilter.FullName} doesn't implement {typeof(ISelectionCustomFilter).FullName}");
                }

                CustomFilter = customFilter;
            }
        }
    }
}