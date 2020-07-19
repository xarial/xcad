using System.Drawing;
using Xarial.XCad.Documentation.Properties;
using Xarial.XCad.UI.PropertyPage.Attributes;

public class BitmapDataModel
{
    public Image Bitmap { get; set; } = Resources.BitmapSample;

    //--- Size
    [BitmapOptions(48, 48)]
    public Image BitmapLarge { get; set; } = Resources.BitmapSample;
    //---
}
