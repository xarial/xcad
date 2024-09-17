using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Enums;

public class NumberBoxDataModel
{
    #region Simple
    public int Number { get; set; }
    public double FloatingNumber { get; set; }
    #endregion

    #region Style
    [NumberBoxOptions(NumberBoxUnitType_e.Length, 0, 1000, 0.01, true, 0.02, 0.001,
        NumberBoxStyle_e.Thumbwheel)]
    public double Length { get; set; }
    #endregion
}