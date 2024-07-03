using System.ComponentModel;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.Documentation.Properties;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Enums;

//--- Options
[PageButtons(PageButtons_e.Cancel | PageButtons_e.Okay)]
public class DataModelPageOpts
{
}
//---

//--- Attribution
[Icon(typeof(Resources), nameof(Resources.PageIcon))]
[Message("Sample message for property page", "Sample Page")]
[DisplayName("Sample Page")]
public class DataModelPageAtts
{
}
//---

//--- HelpLinks
[Help("<Help URL>", "<What's New URL>")]
public class DataModelHelpLinks
{
}
//---
