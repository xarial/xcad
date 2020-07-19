using SolidWorks.Interop.swconst;
using System.ComponentModel;
using System.Drawing;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.Documentation.Properties;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Enums;

public class DataModelCommonOpts
{
    //--- Style
    [ControlOptions(backgroundColor: KnownColor.Green, textColor: KnownColor.Yellow)]
    public string TextField { get; set; } = "Sample Text";
    //---

    //--- StandardIcon
    [Description("Depth Value")]
    [StandardControlIcon(BitmapLabelType_e.Depth)]
    public string Depth { get; set; }
    //---

    //--- CustomIcon
    [Icon(typeof(Resources), nameof(Resources.OffsetImage))]
    public double Offset { get; set; }
    //---
}
