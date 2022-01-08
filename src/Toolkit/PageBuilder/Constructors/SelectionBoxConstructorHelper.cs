//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using Xarial.XCad.Annotations;
using Xarial.XCad.Base.Enums;
using Xarial.XCad.Documents;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry;
using Xarial.XCad.Sketch;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Enums;
using Xarial.XCad.UI.PropertyPage.Services;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.Reflection;

namespace Xarial.XCad.Toolkit.PageBuilder.Constructors
{
    public static class SelectionBoxConstructorHelper
    {
        public static Type GetElementType(Type type) 
        {
            if (type.IsAssignableToGenericType(typeof(IEnumerable<>)))
            {
                return type.GetArgumentsOfGenericType(typeof(IEnumerable<>)).First();
            }
            else 
            {
                return type;
            }
        }

        public static SelectType_e[] GetDefaultFilters(IAttributeSet atts)
        {
            var filters = new List<SelectType_e>();
            
            if (atts == null)
            {
                throw new ArgumentNullException(nameof(atts));
            }

            var type = GetElementType(atts.ContextType);

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
            else if (IsOfType<IXDimension>(type)) 
            {
                filters.Add(SelectType_e.Dimensions);
            }

            if (filters.Any())
            {
                return filters.ToArray();
            }
            else
            {
                return null;
            }
        }

        public static BitmapLabelType_e? GetDefaultBitmapLabel(IAttributeSet atts)
        {
            var type = atts.ContextType;

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
            else if (IsOfType<IXDimension>(type))
            {
                return BitmapLabelType_e.LinearDistance;
            }

            return null;
        }

        private static bool IsOfType<T>(Type t)
            => typeof(T).IsAssignableFrom(t);
    }
}
