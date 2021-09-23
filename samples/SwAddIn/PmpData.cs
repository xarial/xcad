//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Collections.Generic;
using System.Runtime.InteropServices;
using Xarial.XCad.Features.CustomFeature.Attributes;
using Xarial.XCad.Features.CustomFeature.Enums;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.Base.Enums;
using Xarial.XCad.SolidWorks.UI.PropertyPage;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Geometry;
using System;
using Xarial.XCad;
using Xarial.XCad.SolidWorks.UI.PropertyPage.Services;
using Xarial.XCad.SolidWorks.Documents;
using System.Collections.ObjectModel;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Base.Attributes;
using SwAddInExample.Properties;
using System.Linq;
using System.ComponentModel;
using Xarial.XCad.UI.PropertyPage.Services;
using Xarial.XCad.UI.PropertyPage.Enums;

namespace SwAddInExample
{
    public enum Opts
    {
        Opt1,
        Opt2,
        Opt3
    }

    [Flags]
    public enum OptsFlag 
    {
        Opt1 = 1,
        Opt2 = 2,
        Opt3 = 4,
        Opt4 = 8
    }

    public class CustomControlDataContext 
    {
        public string Value { get; set; }
        public ObservableCollection<Item> Items { get; set; } = new ObservableCollection<Item>();
    }

    public class Item 
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class MyItem 
    {
        public static MyItem[] All { get; } = new MyItem[]
        {
            new MyItem()
            {
                Name = "A",
                Id = 1
            },
            new MyItem()
            {
                Name = "B",
                Id = 2
            }
        };

        public string Name { get; set; }
        public int Id { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            if (obj is MyItem) 
            {
                return (obj as MyItem).Id == Id;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }

    public class MyCustomItemsProvider : SwCustomItemsProvider<MyItem>
    {
        public override IEnumerable<MyItem> ProvideItems(ISwApplication app, IControl[] dependencies)
            => MyItem.All;
    }

    public class MyCustomItems1Provider : SwCustomItemsProvider<string>
    {
        public override IEnumerable<string> ProvideItems(ISwApplication app, IControl[] dependencies) 
        {
            var item = dependencies.First()?.GetValue() as MyItem;

            if (item != null)
            {
                return new string[]
                {
                    "1_" + item.Name,
                    "2_" + item.Name,
                    "3_" + item.Name,
                    "4_" + item.Name
                };
            }
            else 
            {
                return null;
            }
        }
    }

    [ComVisible(true)]
    public class PmpData : SwPropertyManagerPageHandler, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [CustomControl(typeof(WpfUserControl))]
        //[CustomControl(typeof(WinUserControl))]
        [ControlOptions(height: 200)]
        public CustomControlDataContext CustomControl { get; set; } = new CustomControlDataContext();

        public ISwSelObject AnyObject { get; set; }

        public List<ISwComponent> Components { get; set; }

        [SelectionBoxOptions(Focused = true)]
        public ISwBody Body { get; set; }

        public ISwCircularEdge CircEdge { get; set; }

        private string m_TextBlockText = "Hello World";

        [TextBlock]
        [TextBlockOptions(TextAlignment_e.Center, FontStyle_e.Bold | FontStyle_e.Italic)]
        [ControlOptions(backgroundColor: System.Drawing.KnownColor.Yellow, textColor: System.Drawing.KnownColor.Green)]
        public string TextBlockText => m_TextBlockText;

        [BitmapButton(typeof(Resources), nameof(Resources.vertical), 96, 96)]
        public bool CheckBox1 { get; set; }

        [BitmapButton(typeof(Resources), nameof(Resources.horizontal), 48, 48)]
        public bool CheckBox { get; set; }

        [BitmapButton(typeof(Resources), nameof(Resources.xarial))]
        public Action Button { get; }

        [Title("Action Button")]
        [Description("Sample button")]
        public Action Button1 { get; }

        [DynamicControls("_Test_")]
        public Dictionary<string, object> DynamicControls { get; }

        //public List<string> List { get; set; }

        [ComboBox(1, 2, 3, 4, 5)]
        [Label("Static Combo Box:", ControlLeftAlign_e.Indent)]
        public int StaticComboBox { get; set; }

        [Metadata("_SRC_")]
        public string[] Source => new string[] { "X", "Y", "Z" };

        [ComboBox(ItemsSource = "_SRC_")]
        public string ItemsSourceComboBox { get; set; }

        [ListBox(ItemsSource = "_SRC_")]
        [Label("List Box1:", ControlLeftAlign_e.LeftEdge, FontStyle_e.Bold)]
        [ControlOptions(align: ControlLeftAlign_e.Indent)]
        public string ListBox1 { get; set; }

        [ListBox("A1", "A2", "A3")]
        public string ListBox2 { get; set; }

        [ListBox(1, 2, 3, 4)]
        public List<int> ListBox3 { get; set; }

        //[ListBox]
        [OptionBox]
        [Label("Sample List Box 4:", fontStyle: FontStyle_e.Underline)]
        public Opts ListBox4 { get; set; }

        [ListBox]
        public OptsFlag ListBox5 { get; set; } = OptsFlag.Opt1 | OptsFlag.Opt3;

        private void ReduceComponents() 
        {
            if (Components?.Any() == true) 
            {
                Components.RemoveAt(Components.Count - 1);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Components)));
            }
        }

        public PmpData() 
        {
            Button = ReduceComponents;
            DynamicControls = new Dictionary<string, object>()
            {
                { "A", "Hello" }
            };

            Button1 = () =>
            {
                m_TextBlockText = "Hello World - " + Guid.NewGuid().ToString();
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TextBlockText)));
            };
        }
    }

    [ComVisible(true)]
    public class PmpMacroFeatData : SwPropertyManagerPageHandler
    {
        public string Text { get; set; }

        [ParameterDimension(CustomFeatureDimensionType_e.Linear)]
        [ExcludeControl]
        public double Number { get; set; } = 0.1;

        public Opts Options { get; set; }

        public ISwCircularEdge Selection { get; set; }
        
        [ParameterExclude]
        [ComboBox(typeof(MyCustomItemsProvider))]
        public MyItem Option2 { get; set; }

        [ParameterDimension(CustomFeatureDimensionType_e.Angular)]
        [ExcludeControl]
        public double Angle { get; set; } = Math.PI / 9;

        public PmpMacroFeatData() 
        {
            Option2 = MyItem.All.Last();
        }
    }

    [ComVisible(true)]
    public class PmpComboBoxData : SwPropertyManagerPageHandler, INotifyPropertyChanged
    {
        private MyItem[] m_List1;
        private MyItem m_Option3Set;

        [ComboBox(typeof(MyCustomItemsProvider))]
        public MyItem Option1Default { get; set; }

        [ComboBox(typeof(MyCustomItemsProvider))]
        public MyItem Option1Set { get; set; }

        public Opts Option2Default { get; set; }

        public Opts Option2Set { get; set; }

        [Metadata(nameof(List1))]
        public MyItem[] List1
        {
            get => m_List1;
            set
            {
                m_List1 = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(List1)));
            }
        }

        [ComboBox(ItemsSource = nameof(List1))]
        public MyItem Option3Default { get; set; }

        [ComboBox(ItemsSource = nameof(List1))]
        [ControlTag(nameof(Option3Set))]
        public MyItem Option3Set
        {
            get => m_Option3Set;
            set
            {
                m_Option3Set = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Option3Set)));
            }
        }

        [ComboBox(1, 2, 3)]
        [ControlTag(nameof(Option4Default))]
        public int Option4Default { get; set; }

        [ComboBox(1, 2, 3)]
        public int Option4Set { get; set; }

        [ComboBox(typeof(MyCustomItemsProvider), nameof(Option4Default))]
        public MyItem Option5Default { get; set; }

        [ComboBox(typeof(MyCustomItemsProvider), nameof(Option4Default))]
        public MyItem Option5Set { get; set; }

        [ComboBox(typeof(MyCustomItems1Provider), nameof(Option3Set))]
        public string Option6 { get; set; }

        public Action Button { get; }

        public PmpComboBoxData()
        {
            Button = new Action(() =>
            {
                List1 = new MyItem[] { new MyItem() { Name = "_", Id = -1 }, new MyItem() { Name = "-", Id = -2 } };
                Option3Set = List1.Last();
            });

            List1 = MyItem.All;
            Option1Set = MyItem.All.Last();
            Option2Set = Opts.Opt2;
            m_Option3Set = MyItem.All.Last();
            Option4Set = 2;
            Option5Set = MyItem.All.Last();

            //Option1Set = new MyItem() { Name = "_", Id = -1 };
            //Option2Set = (Opts)5;
            //Option3Set = new MyItem() { Name = "-", Id = -2 };
            //Option4Set = 5;
            //Option5Set = new MyItem() { Name = "+", Id = -3 };
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    [ComVisible(true)]
    public class ToggleGroupPmpData : SwPropertyManagerPageHandler
    {
        public class IsCheckedDepHandler : IMetadataDependencyHandler
        {
            public void UpdateState(IXApplication app, IControl source, IMetadata[] metadata)
            {
                source.Enabled = !((bool)metadata.First().Value);
            }
        }

        public class Group : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            private bool m_IsChecked;

            [Metadata(nameof(IsChecked))]
            public bool IsChecked 
            {
                get => m_IsChecked;
                set 
                {
                    m_IsChecked = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsChecked)));
                }
            }

            public string TextBox { get; set; }
            public int Number { get; set; }
            public Action Button { get; }

            public Group() 
            {
                m_IsChecked = true;
                Button = new Action(() => IsChecked = !IsChecked);
            }
        }

        [CheckableGroupBox(nameof(Group.IsChecked))]
        //[GroupBoxOptions(GroupBoxOptions_e.Collapsed)]
        public Group Grp { get; set; }

        [DependentOnMetadata(typeof(IsCheckedDepHandler), nameof(Group.IsChecked))]
        public double Number1 { get; set; }

        public ToggleGroupPmpData() 
        {
            Grp = new Group();
        }
    }
}
