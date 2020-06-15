//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xarial.XCad.Base.Enums;
using Xarial.XCad.Documents;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry;
using Xarial.XCad.Sketch;
using Xarial.XCad.UI.PropertyPage;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Enums;
using Xarial.XCad.UI.PropertyPage.Services;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.Reflection;

namespace Xarial.XCad.Toolkit.PageBuilder.Constructors
{
    public static class SelectionBoxConstructorHelper
    {
        internal class TypeSelectionCustomFilter : ISelectionCustomFilter
        {
            private readonly Type m_TargetType;

            internal TypeSelectionCustomFilter(Type targType)
            {
                if (targType == null)
                {
                    throw new ArgumentNullException(nameof(targType));
                }

                m_TargetType = targType;
            }

            public bool Filter(IControl selBox, IXSelObject selection, SelectType_e selType, ref string itemText)
            {
                return m_TargetType.IsAssignableFrom(selection.GetType());
            }
        }

        public static SelectType_e[] GetDefaultFilters(IAttributeSet atts, out ISelectionCustomFilter customFilter)
        {
            var filters = new List<SelectType_e>();

            customFilter = null;

            if (atts == null)
            {
                throw new ArgumentNullException(nameof(atts));
            }

            var type = atts.BoundType;

            if (type.IsAssignableToGenericType(typeof(IEnumerable<>))) 
            {
                type = type.GetArgumentsOfGenericType(typeof(IEnumerable<>)).First();
            }

            if (IsOfType<IXEdge>(type))
            {
                filters.Add(SelectType_e.Edges);
            }
            else if (IsOfType<IXFace>(type))
            {
                filters.Add(SelectType_e.Faces);
            }
            else if (IsOfType<IXSketchBase>(type))
            {
                filters.Add(SelectType_e.Sketches);
            }
            else if (IsOfType<IXSketchPoint>(type))
            {
                filters.Add(SelectType_e.SketchPoints);
            }
            else if (IsOfType<IXSketchSegment>(type))
            {
                filters.Add(SelectType_e.SketchSegments);
            }
            else if (IsOfType<IXComponent>(type))
            {
                filters.Add(SelectType_e.Components);
            }
            else if (IsOfType<IXBody>(type))
            {
                filters.Add(SelectType_e.SolidBodies);
                filters.Add(SelectType_e.SurfaceBodies);
            }

            if (!filters.Any())
            {
                filters.Add(SelectType_e.Everything);
                customFilter = new TypeSelectionCustomFilter(type);
            }

            return filters.ToArray();
        }

        public static BitmapLabelType_e? GetDefaultBitmapLabel(IAttributeSet atts)
        {
            var type = atts.BoundType;

            if (type.IsAssignableToGenericType(typeof(IEnumerable<>)))
            {
                type = type.GetArgumentsOfGenericType(typeof(IEnumerable<>)).First();
            }

            if (IsOfType<IXFace>(type))
            {
                return BitmapLabelType_e.SelectFace;
            }
            else if (IsOfType<IXEdge>(type))
            {
                return BitmapLabelType_e.SelectEdge;
            }
            else if (IsOfType<IXComponent>(type))
            {
                return BitmapLabelType_e.SelectComponent;
            }

            return null;
        }

        private static bool IsOfType<T>(Type t)
        {
            return typeof(T).IsAssignableFrom(t);
        }
    }
}
