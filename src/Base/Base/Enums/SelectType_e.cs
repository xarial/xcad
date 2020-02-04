//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

namespace Xarial.XCad.Base.Enums
{
    public enum SelectType_e
    {
        Everything = -3,
        Nothing = 0,
        Edges = 1,
        Faces = 2,
        Vertices = 3,
        Planes = 4,
        Axes = 5,
        Points = 6,
        Sketches = 9,
        SketchSegments = 10,
        SketchPoints = 11,
        DrawingViews = 12,
        Dimensions = 14,
        Notes = 15,
        Sheets = 19,
        Components = 20,
        Mates = 21,
        CoordinateSystems = 61,
        SurfaceBodies = 75,
        SolidBodies = 76,
        SketchRegion = 95,
        SketchContour = 96,
    }
}