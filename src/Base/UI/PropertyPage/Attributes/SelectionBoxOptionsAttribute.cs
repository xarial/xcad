//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Drawing;
using Xarial.XCad.Base.Enums;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Enums;

namespace Xarial.XCad.UI.PropertyPage.Attributes
{
    /// <summary>
    /// Additional options for selection box control
    /// </summary>
    public class SelectionBoxOptionsAttribute : Attribute, IAttribute
    {
        public SelectionBoxStyle_e Style { get; private set; }
        public KnownColor SelectionColor { get; private set; }

        public SelectType_e[] Filters { get; private set; }
        public int SelectionMark { get; private set; }
        public Type CustomFilter { get; private set; }

        /// <summary>
        /// Constructor for selection box options
        /// </summary>
        /// <param name="style">Selection box style</param>
        /// <param name="selColor">Color of the selections in this selection box</param>
        public SelectionBoxOptionsAttribute(
            SelectionBoxStyle_e style = SelectionBoxStyle_e.None,
            KnownColor selColor = 0) : this(-1, null, style, selColor)
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

        /// <inheritdoc cref="SelectionBoxAttribute(SelectType_e[])"/>
        /// <param name="mark">Selection mark. If multiple selections box are used - use different selection marks for each of them
        /// to differentiate the selections</param>
        public SelectionBoxOptionsAttribute(int mark, params SelectType_e[] filters)
            : this(mark, null, SelectionBoxStyle_e.None, 0, filters)
        {
        }

        /// <inheritdoc cref="SelectionBoxAttribute(int, Type, SelectType_e[])"/>
        public SelectionBoxOptionsAttribute(Type customFilter, params SelectType_e[] filters)
            : this(-1, customFilter, SelectionBoxStyle_e.None, 0, filters)
        {
        }

        /// <inheritdoc cref="SelectionBoxAttribute(int, SelectType_e[])"/>
        /// <param name="customFilter">Type of custom filter of <see cref="SelectionCustomFilter{TSelection}"/> for custom logic for filtering selection objects</param>
        /// <exception cref="InvalidCastException"/>
        public SelectionBoxOptionsAttribute(int mark, Type customFilter, SelectionBoxStyle_e style,
            KnownColor selColor, params SelectType_e[] filters)
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