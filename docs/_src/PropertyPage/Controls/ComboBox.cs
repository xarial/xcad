﻿using SolidWorks.Interop.swconst;
using System.Collections.Generic;
using System.Linq;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.Documentation.Properties;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.UI.PropertyPage.Services;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Enums;

public class ComboBoxDataModel
{
    #region Simple
    public enum Options_e
    {
        Option1,
        Option2,
        Option3
    }

    [ComboBoxOptions(ComboBoxStyle_e.Sorted)]
    public Options_e Options { get; set; }
    #endregion Simple
    #region ItemsText
    public enum OptionsCustomized_e
    {
        [Title("First Option")] //static title
        Option1,

        [Title(typeof(Resources), nameof(Resources.Option2Title))] //title loaded from resources
        Option2
    }

    public OptionsCustomized_e Options2 { get; set; }
    #endregion ItemsText
    #region CustomItemsProvider
    public class CustomStringItemsProvider : SwCustomItemsProvider<string>
    {
        public override IEnumerable<string> ProvideItems(ISwApplication app, IControl[] dependencies)
            => new string[] { "A", "B", "C" };
    }

    public class CustomIntItemsProvider : SwCustomItemsProvider<int>
    {
        public override IEnumerable<int> ProvideItems(ISwApplication app, IControl[] dependencies)
            => new int[] { 1, 2, 3 };
    }

    [ComboBox(typeof(CustomStringItemsProvider))]
    [ControlTag(nameof(Options3))]
    public string Options3 { get; set; } = "B";

    [ComboBox(typeof(CustomIntItemsProvider))]
    [ControlTag(nameof(Options4))]
    public int Options4 { get; set; }
    #endregion CustomItemsProvider
    #region CustomItemsProviderDependency
    public class CustomComboBoxItem
    {
        public string BaseName { get; }
        public string Name { get; }

        public CustomComboBoxItem(object dep1, object dep2, string name)
        {
            BaseName = $"{dep1}_{dep2}";
            Name = name;
        }

        public override string ToString() => $"Item-{BaseName}-{Name}";
    }

    public class CustomDependencyProvider : SwCustomItemsProvider<CustomComboBoxItem>
    {
        public override IEnumerable<CustomComboBoxItem> ProvideItems(ISwApplication app,
            IControl[] dependencies)
        {
            return new CustomComboBoxItem[]
            {
                new CustomComboBoxItem(dependencies[0]?.GetValue(), dependencies[1]?.GetValue(), "I1"),
                new CustomComboBoxItem(dependencies[0]?.GetValue(), dependencies[1]?.GetValue(), "I2"),
                new CustomComboBoxItem(dependencies[0]?.GetValue(), dependencies[1]?.GetValue(), "I3")
            };
        }
    }

    [ComboBox(typeof(CustomDependencyProvider), nameof(Options3), nameof(Options4))]
    public CustomComboBoxItem Options5 { get; set; }
    #endregion CustomItemsProviderDependency
}
