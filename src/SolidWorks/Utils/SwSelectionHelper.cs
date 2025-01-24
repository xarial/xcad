//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
        //NOTE: this should be a list so we keep the order correct (not the case for the Dictionary)
        //this would allow to proritize the grouped selection types, like IXBody (both surface and solid),
        //but still return the specific selection type (e.g. IXSolidBody)
        private static readonly List<KeyValuePair<Type, swSelectType_e[]>> m_Map;

        static SwSelectionHelper() 
        {
            m_Map = new List<KeyValuePair<Type, swSelectType_e[]>>();

            AddToMap<IXEdge>(swSelectType_e.swSelEDGES);
            AddToMap<IXFace>(swSelectType_e.swSelFACES);
            AddToMap<IXVertex>(swSelectType_e.swSelVERTICES);
                        
            AddToMap<IXPlane>(swSelectType_e.swSelDATUMPLANES);
            AddToMap<IXCoordinateSystem>(swSelectType_e.swSelCOORDSYS);
            AddToMap<IXSketchBase>(swSelectType_e.swSelSKETCHES);
            
            AddToMap<IXSketchPicture>(swSelectType_e.swSelSKETCHBITMAP);
            AddToMap<IXSketchRegion>(swSelectType_e.swSelSKETCHREGION);
            AddToMap<IXSketchPoint>(swSelectType_e.swSelSKETCHPOINTS, swSelectType_e.swSelEXTSKETCHPOINTS);
            AddToMap<IXSketchText>(swSelectType_e.swSelSKETCHTEXT, swSelectType_e.swSelEXTSKETCHTEXT);
            AddToMap<IXSketchSegment>(swSelectType_e.swSelSKETCHSEGS, swSelectType_e.swSelEXTSKETCHSEGS);
            AddToMap<IXSketchBlockInstance>(swSelectType_e.swSelBLOCKINST, swSelectType_e.swSelSUBSKETCHINST);
            AddToMap<IXSketchBlockDefinition>(swSelectType_e.swSelBLOCKDEF, swSelectType_e.swSelSUBSKETCHDEF);
            AddToMap<IXSketchEntity>(swSelectType_e.swSelSKETCHSEGS, swSelectType_e.swSelEXTSKETCHSEGS, swSelectType_e.swSelSKETCHPOINTS, swSelectType_e.swSelEXTSKETCHPOINTS, swSelectType_e.swSelBLOCKINST, swSelectType_e.swSelSUBSKETCHINST);

            AddToMap<IXNote>(swSelectType_e.swSelNOTES);
            AddToMap<IXDimension>(swSelectType_e.swSelDIMENSIONS);
            
            AddToMap<IXSheet>(swSelectType_e.swSelSHEETS);
            AddToMap<IXDrawingView>(swSelectType_e.swSelDRAWINGVIEWS);

            AddToMap<IXComponent>(swSelectType_e.swSelCOMPONENTS);
            AddToMap<IXSolidBody>(swSelectType_e.swSelSOLIDBODIES);
            AddToMap<IXSheetBody>(swSelectType_e.swSelSURFACEBODIES);
            AddToMap<IXBody>(swSelectType_e.swSelSOLIDBODIES, swSelectType_e.swSelSURFACEBODIES);
        }

        private static void AddToMap<T>(params swSelectType_e[] selTypes) where T : IXSelObject 
            => m_Map.Add(new KeyValuePair<Type, swSelectType_e[]>(typeof(T), selTypes));

        internal static IReadOnlyList<swSelectType_e> GetSelectionType(Type type)
        {
            foreach (var map in m_Map) 
            {
                if(map.Key.IsAssignableFrom(type))
                {
                    return map.Value;
                }
            }

            return null;
        }
    }
}
