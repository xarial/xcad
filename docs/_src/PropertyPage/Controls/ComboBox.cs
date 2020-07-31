using SolidWorks.Interop.swconst;
using System.Collections.Generic;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.Documentation.Properties;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.UI.PropertyPage.Services;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Enums;

public class ComboBoxDataModel
{
    //--- Simple
    public enum Options_e
    {
        Option1,
        Option2,
        Option3
    }

    [ComboBoxOptions(ComboBoxStyle_e.Sorted)]
    public Options_e Options { get; set; }
    //---
    //--- ItemsText
    public enum OptionsCustomized_e
    {
        [Title("First Option")] //static title
        Option1,

        [Title(typeof(Resources), nameof(Resources.Option2Title))] //title loaded from resources
        Option2
    }

    public OptionsCustomized_e Options2 { get; set; }
    //---
    //--- CustomItemsProvider
    public class CustomStringItemsProvider : SwCustomItemsProvider<string>
    {
        public override IEnumerable<string> ProvideItems(SwApplication app)
            => new string[] { "A", "B", "C" };
    }

    public class CustomIntItemsProvider : SwCustomItemsProvider<int>
    {
        public override IEnumerable<int> ProvideItems(SwApplication app)
            => new int[] { 1, 2, 3 };
    }

    [CustomItems(typeof(CustomStringItemsProvider))]
    public string Options3 { get; set; } = "B";

    [CustomItems(typeof(CustomIntItemsProvider))]
    public int Options4 { get; set; }
    //---
}
