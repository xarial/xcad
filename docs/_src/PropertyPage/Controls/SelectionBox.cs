using System.Collections.Generic;
using Xarial.XCad;
using Xarial.XCad.Annotations;
using Xarial.XCad.Base.Enums;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Geometry;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Services;
using Xarial.XCad.UI.PropertyPage.Structures;

//--- Single
public class SelectionBoxDataModel
{
    public ISwBody Body { get; set; }

    [SelectionBoxOptions(typeof(IXEdge), typeof(IXNote), typeof(IXCoordinateSystem))]
    public ISwSelObject Dispatch { get; set; }
}
//---

//--- List
public class SelectionBoxListDataModel
{
    public List<ISwBody> Bodies { get; set; } = new List<ISwBody>();

    [SelectionBoxOptions(typeof(IXEdge), typeof(IXNote), typeof(IXCoordinateSystem))]
    public List<ISwSelObject> Dispatches { get; set; } = new List<ISwSelObject>();
}
//---

//--- CustomFilter
public class SelectionBoxCustomSelectionFilterDataModel
{
    public class DataGroup
    {
        [SelectionBoxOptions(typeof(PlanarFaceFilter), typeof(IXFace))] //setting the standard filter to faces and custom filter to only filter planar faces
        public ISwFace PlanarFace { get; set; }
    }

    public class PlanarFaceFilter : ISelectionCustomFilter
    {
        public void Filter(IControl selBox, IXSelObject selection, SelectionCustomFilterArguments args)
        {
            args.Filter = (selection as ISwFace).Face.IGetSurface().IsPlane(); //validating the selection and only allowing planar face
            
            if (args.Filter)
            {
                args.ItemText = "Planar Face";
            }
            else 
            {
                args.Reason = "Only planar faces can be selected";
            }
        }
    }
}
//---