using Xarial.XCad.Base.Attributes;
using Xarial.XCad.UI.PropertyPage.Attributes;

public class OptionBoxDataModel
{
    public enum Options_e
    {
        Option1,
        Option2,
        [Title("Third Option")]
        Option3
    }

    [OptionBox]
    public Options_e Options { get; set; }
}