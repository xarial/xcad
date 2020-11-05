using System.Collections.Generic;
using Xarial.XCad;
using Xarial.XCad.Base.Enums;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Geometry;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Services;

//--- Single
public class SelectionBoxDataModel
{
    public ISwBody Body { get; set; }

    [SelectionBoxOptions(SelectType_e.Edges, SelectType_e.Notes, SelectType_e.CoordinateSystems)]
    public ISwSelObject Dispatch { get; set; }
}
//---

//--- List
public class SelectionBoxListDataModel
{
    public List<ISwBody> Bodies { get; set; } = new List<ISwBody>();

    [SelectionBoxOptions(SelectType_e.Edges, SelectType_e.Notes, SelectType_e.CoordinateSystems)]
    public List<ISwSelObject> Dispatches { get; set; } = new List<ISwSelObject>();
}
//---

//--- CustomFilter
public class SelectionBoxCustomSelectionFilterDataModel
{
    public class DataGroup
    {
        [SelectionBoxOptions(typeof(PlanarFaceFilter), SelectType_e.Faces)] //setting the standard filter to faces and custom filter to only filter planar faces
        public ISwFace PlanarFace { get; set; }
    }

    public class PlanarFaceFilter : ISelectionCustomFilter
    {
        public bool Filter(IControl selBox, IXSelObject selection, SelectType_e selType, ref string itemText)
        {
            itemText = "Planar Face";

            return (selection as ISwFace).Face.IGetSurface().IsPlane(); //validating the selection and only allowing planar face
        }
    }
}
//---