using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Annotations;
using Xarial.XCad.Documents;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry;
using Xarial.XCad.Sketch;

namespace Xarial.XCad.SolidWorks.Utils
{
    internal static class SwSelectionHelper
    {
        internal static IReadOnlyList<swSelectType_e> GetSelectionType(Type type)
        {
            if (IsOfType<IXEdge>(type))
            {
                return new swSelectType_e[]
                {
                    swSelectType_e.swSelEDGES
                };
            }
            else if (IsOfType<IXFace>(type))
            {
                return new swSelectType_e[]
                {
                    swSelectType_e.swSelFACES
                };
            }
            else if (IsOfType<IXVertex>(type))
            {
                return new swSelectType_e[]
                {
                    swSelectType_e.swSelVERTICES
                };
            }
            else if (IsOfType<IXPlane>(type)) 
            {
                return new swSelectType_e[]
                {
                    swSelectType_e.swSelDATUMPLANES
                };
            }
            else if (IsOfType<IXNote>(type))
            {
                return new swSelectType_e[]
                {
                    swSelectType_e.swSelNOTES
                };
            }
            else if (IsOfType<IXSheet>(type))
            {
                return new swSelectType_e[]
                {
                    swSelectType_e.swSelSHEETS
                };
            }
            else if (IsOfType<IXCoordinateSystem>(type))
            {
                return new swSelectType_e[]
                {
                    swSelectType_e.swSelCOORDSYS
                };
            }
            else if (IsOfType<IXSketchBase>(type))
            {
                return new swSelectType_e[]
                {
                    swSelectType_e.swSelSKETCHES
                };
            }
            else if (IsOfType<IXSketchPicture>(type))
            {
                return new swSelectType_e[]
                {
                    swSelectType_e.swSelSKETCHBITMAP
                };
            }
            else if (IsOfType<IXSketchRegion>(type))
            {
                return new swSelectType_e[]
                {
                    swSelectType_e.swSelSKETCHREGION
                };
            }
            else if (IsOfType<IXSketchRegion>(type))
            {
                return new swSelectType_e[]
                {
                    swSelectType_e.swSelSKETCHREGION
                };
            }
            else if (IsOfType<IXSketchPoint>(type))
            {
                return new swSelectType_e[]
                {
                    swSelectType_e.swSelSKETCHPOINTS,
                    swSelectType_e.swSelEXTSKETCHPOINTS
                };
            }
            else if (IsOfType<IXSketchSegment>(type))
            {
                return new swSelectType_e[]
                {
                    swSelectType_e.swSelSKETCHSEGS,
                    swSelectType_e.swSelEXTSKETCHSEGS
                };
            }
            else if (IsOfType<IXSketchBlockInstance>(type))
            {
                return new swSelectType_e[]
                {
                    swSelectType_e.swSelBLOCKINST,
                    swSelectType_e.swSelSUBSKETCHINST
                };
            }
            else if (IsOfType<IXSketchEntity>(type))
            {
                return new swSelectType_e[]
                {
                    swSelectType_e.swSelSKETCHSEGS,
                    swSelectType_e.swSelEXTSKETCHSEGS,
                    swSelectType_e.swSelSKETCHPOINTS,
                    swSelectType_e.swSelEXTSKETCHPOINTS,
                    swSelectType_e.swSelBLOCKINST,
                    swSelectType_e.swSelSUBSKETCHINST
                };
            }
            else if (IsOfType<IXComponent>(type))
            {
                return new swSelectType_e[]
                {
                    swSelectType_e.swSelCOMPONENTS
                };
            }
            else if (IsOfType<IXSolidBody>(type))
            {
                return new swSelectType_e[]
                {
                    swSelectType_e.swSelSOLIDBODIES
                };
            }
            else if (IsOfType<IXSheetBody>(type))
            {
                return new swSelectType_e[]
                {
                    swSelectType_e.swSelSURFACEBODIES
                };
            }
            else if (IsOfType<IXBody>(type))
            {
                return new swSelectType_e[]
                {
                    swSelectType_e.swSelSOLIDBODIES,
                    swSelectType_e.swSelSURFACEBODIES
                };
            }
            else if (IsOfType<IXDimension>(type))
            {
                return new swSelectType_e[]
                {
                    swSelectType_e.swSelDIMENSIONS
                };
            }
            else if (IsOfType<IXDrawingView>(type))
            {
                return new swSelectType_e[]
                {
                    swSelectType_e.swSelDRAWINGVIEWS
                };
            }
            else
            {
                return null;
            }
        }

        private static bool IsOfType<T>(Type t) => typeof(T).IsAssignableFrom(t);
    }
}
