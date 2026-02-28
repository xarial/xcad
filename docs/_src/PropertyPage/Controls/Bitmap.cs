#region Control
using System.Drawing;
using Xarial.XCad.Documentation.Properties;
using Xarial.XCad.UI.PropertyPage.Attributes;

public class BitmapDataModel
{
    public Image Bitmap { get; set; } = Resources.BitmapSample;
    #endregion Control
    #region Size
    [BitmapOptions(48, 48)]
    public Image BitmapLarge { get; set; } = Resources.BitmapSample;
    #endregion Size
    #region Control2
}
#endregion Control2