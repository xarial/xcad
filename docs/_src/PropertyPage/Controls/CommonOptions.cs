using SolidWorks.Interop.swconst;
using System.ComponentModel;
using System.Drawing;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.Documentation.Properties;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Enums;

public class DataModelCommonOpts
{
    #region Style
    [ControlOptions(backgroundColor: KnownColor.Green, textColor: KnownColor.Yellow)]
    public string TextField { get; set; } = "Sample Text";
    #endregion

    #region StandardIcon
    [Description("Depth Value")]
    [StandardControlIcon(BitmapLabelType_e.Depth)]
    public string Depth { get; set; }
    #endregion

    #region CustomIcon
    [Icon(typeof(Resources), nameof(Resources.OffsetImage))]
    public double Offset { get; set; }
    #endregion
}
