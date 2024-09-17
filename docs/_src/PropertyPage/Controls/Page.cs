using System.ComponentModel;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.Documentation.Properties;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Enums;

#region Options
[PageOptions(PageOptions_e.CancelButton
| PageOptions_e.OkayButton)]
public class DataModelPageOpts
{
}
#endregion

#region Attribution
[Icon(typeof(Resources), nameof(Resources.PageIcon))]
[Message("Sample message for property page", "Sample Page")]
[DisplayName("Sample Page")]
public class DataModelPageAtts
{
}
#endregion

#region HelpLinks
[Help("<Help URL>", "<What's New URL>")]
public class DataModelHelpLinks
{
}
#endregion
