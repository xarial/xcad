using Xarial.XCad.Base.Attributes;
using Xarial.XCad.Documentation.Properties;
using Xarial.XCad.UI.PropertyPage.Attributes;

public class TabDataModel
{
    [Tab]
    [Icon(typeof(Resources), nameof(Resources.OffsetImage))]
    public class TabControl1
    {
        public string Field1 { get; set; }
    }

    public TabControl1 Tab1 { get; set; }

    //--- WithGroup
    public class TabControl2
    {
        public class Group1
        {
            public int Field2 { get; set; }
        }

        public Group1 Group { get; set; }
        public bool Field3 { get; set; }
    }

    [Tab]
    public TabControl2 Tab2 { get; set; }
    //---
}
