using System;
using System.Drawing;
using Xarial.XCad.Documentation.Properties;
using Xarial.XCad.UI.PropertyPage.Attributes;

public class BitmapButtonDataModel
{
    //--- Button
    [BitmapButton(typeof(Resources), nameof(Resources.BitmapSample))]
    public Action Button1 { get; set; } = new Action(()=> { });
    //---

    //--- Toggle
    [BitmapButton(typeof(Resources), nameof(Resources.BitmapSample))]
    public bool Toggle1 { get; set; } = false;

    [BitmapButton(typeof(Resources), nameof(Resources.BitmapSample))]
    public bool Toggle2 { get; set; } = true;
    //---

    //--- Size
    [BitmapButton(typeof(Resources), nameof(Resources.BitmapSample), 48, 48)]
    public Action Button2 { get; set; } = new Action(() => { });
    //---

    //--- Standard
    [BitmapButton(Xarial.XCad.UI.PropertyPage.Enums.BitmapButtonLabelType_e.AlongZ)]
    public bool Standard1 { get; set; }

    [BitmapButton(Xarial.XCad.UI.PropertyPage.Enums.BitmapButtonLabelType_e.Draft)]
    public bool Standard2 { get; set; }
    //---
}
