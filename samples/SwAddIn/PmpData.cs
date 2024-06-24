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
using Xarial.XCad.SolidWorks.Documents;
using System.Collections.ObjectModel;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Base.Attributes;
using System.Linq;
using System.ComponentModel;
using Xarial.XCad.UI.PropertyPage.Services;
using Xarial.XCad.UI.PropertyPage.Enums;
using Xarial.XCad.UI.PropertyPage.Structures;
using Xarial.XCad.Enums;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry;
using Xarial.XCad.SolidWorks.UI.PropertyPage.Attributes;
using SolidWorks.Interop.swconst;
using SwAddIn.Properties;

namespace SwAddInExample
{
    public enum Opts
    {
        Opt1,
        Opt2,
        Opt3
    }

    public enum Opts1
    {
        Opt4,
        Opt5,
        Opt6
    }

    [Flags]
    public enum OptsFlag 
    {
        Opt1 = 1,
        Opt2 = 2,
        Opt3 = 4,
        Opt4 = 8
    }

    [Flags]
    public enum OptsFlag2
    {
        None = 0,

        [Title("Option #1")]
        [Description("First Option")]
        Opt1 = 1,
        Opt2 = 2,

        [Title("Opt1 + Opt2")]
        Opt1_2 = Opt1 | Opt2,

        Opt3 = 4,
        Opt4 = 8
    }

    public class CustomControlDataContext : INotifyPropertyChanged
    {
        public event Action<CustomControlDataContext, OptsFlag> ValueChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        private OptsFlag m_Value;

        public OptsFlag Value 
        {
            get => m_Value;
            set 
            {
                m_Value = value;
                ValueChanged?.Invoke(this, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
            }
        }

        public CustomControlDataContext() 
        {
            Items = new ObservableCollection<Item>();
            Items.Add(new Item() { Name = "ABC", Value = "XYZ" });
            Items.Add(new Item() { Name = "ABC1", Value = "XYZ1" });
        }

        public ObservableCollection<Item> Items { get; }
    }

    public class Item 
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class MyItem 
    {
        [Title("Custom Item C")]
        [Description("Item C [ID = 3]")]
        private class MyCustomItem : MyItem
        {
            internal MyCustomItem() 
            {
                Name = "C";
                Id = 3;
            }
        }

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
            },
            new MyCustomItem()
        };

        public string Name { get; set; }
        public int Id { get; set; }

        public override string ToString() => Name;

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

    public class MyItem1 
    {
        public class MyItem1Name 
        {
            public string Name { get; }

            public MyItem1Name(string name)
            {
                Name = name;
            }
        }
        
        public string Value { get; }

        public MyItem1Name DisplayName { get; }

        public MyItem1(string val) 
        {
            Value = val;
            DisplayName = new MyItem1Name("[" + val + "]");
        }

        public override string ToString() => Value;
    }

    public class MyCustomItemsProvider : ICustomItemsProvider
    {
        public IEnumerable<object> ProvideItems(IXApplication app, IControl ctrl, IControl[] dependencies, object parameter)
            => MyItem.All;
    }

    public class MyCustomItems1Provider : ICustomItemsProvider
    {
        public IEnumerable<object> ProvideItems(IXApplication app, IControl ctrl, IControl[] dependencies, object parameter)
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


    public class PlanarFaceFilter : ISelectionCustomFilter
    {
        public void Filter(IControl selBox, IXSelObject selection, SelectionCustomFilterArguments args)
        {
            args.Filter = (selection as ISwFace).Face.IGetSurface().IsPlane(); //validating the selection and only allowing planar face

            if (args.Filter)
            {
                args.ItemText = "Planar Face";
            }
            else
            {
                args.Reason = "Only planar faces can be selected";
            }
        }
    }

    public class VisibilityHandler : IDependencyHandler
    {
        public void UpdateState(IXApplication app, IControl source, IControl[] dependencies, object parameter)
        {
            source.Visible = (bool)dependencies.First().GetValue();
        }
    }

    public class EnableHandler : IDependencyHandler
    {
        public void UpdateState(IXApplication app, IControl source, IControl[] dependencies, object parameter)
        {
            source.Enabled = (bool)dependencies.First().GetValue();
        }
    }

    public class CustomControlDependantHandler : IDependencyHandler
    {
        public void UpdateState(IXApplication app, IControl source, IControl[] dependencies, object parameter)
        {
            var val = (OptsFlag)dependencies.First().GetValue();

            source.Enabled = val.HasFlag(OptsFlag.Opt2);
        }
    }

    [ComVisible(true)]
    [Help("https://xcad.net/")]
    //[PageOptions(PageOptions_e.OkayButton | PageOptions_e.CancelButton | PageOptions_e.HandleKeystrokes)]
    public class PmpData : SwPropertyManagerPageHandler, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        //[SelectionBoxOptions(Filters = new Type[] { typeof(IXFace) })]
        [SwSelectionBoxOptions(Filters = new swSelectType_e[] { swSelectType_e.swSelANNOTATIONTABLES })]
        public ISwSelObject UnknownObject { get; set; }

        [CustomControl(typeof(WpfUserControl))]
        //[CustomControl(typeof(WinUserControl))]
        [ControlOptions(height: 300)]
        [ControlTag(nameof(CustomControl))]
        public OptsFlag CustomControl { get; set; }

        [DependentOn(typeof(CustomControlDependantHandler), nameof(CustomControl))]
        [Description("Any object selection")]
        public ISwSelObject AnyObject { get; set; }

        [SwSelectionBoxOptions(CustomFilter = typeof(PlanarFaceFilter), Filters = new swSelectType_e[] { swSelectType_e.swSelFACES })] //setting the standard filter to faces and custom filter to only filter planar faces
        [AttachMetadata(nameof(ComponentsMetadata))]
        [AttachMetadata(nameof(CircEdgeMetadata))]
        public ISwFace PlanarFace { get; set; }

        public List<ISwComponent> Components { get; set; }

        [SelectionBoxOptions(Focused = true)]
        public ISwBody Body { get; set; }

        [Metadata(nameof(ComponentsMetadata))]
        public List<ISwComponent> ComponentsMetadata => Components;

        [Metadata(nameof(CircEdgeMetadata))]
        public ISwCircularEdge CircEdgeMetadata => CircEdge;

        public ISwCircularEdge CircEdge { get; set; }

        private string m_TextBlockText = "Hello World";

        [TextBlock]
        [TextBlockOptions(TextAlignment_e.Center, FontStyle_e.Bold | FontStyle_e.Italic)]
        [ControlOptions(backgroundColor: System.Drawing.KnownColor.Yellow, textColor: System.Drawing.KnownColor.Green)]
        public string TextBlockText => m_TextBlockText;

        [BitmapToggleButton(typeof(Resources), nameof(Resources.vertical), nameof(Resources.horizontal), 96, 96)]
        [Description("Dynamic icon1")]
        public bool CheckBox1 { get; set; } = true;

        [BitmapToggleButton(typeof(Resources), nameof(Resources.vertical), BitmapEffect_e.Grayscale | BitmapEffect_e.Transparent, 24, 24)]
        [Description("Dynamic icon2")]
        public bool CheckBox2 { get; set; } = false;

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
        public MyItem1[] Source { get; } = new MyItem1[] { new MyItem1("X"), new MyItem1("Y"), new MyItem1("Z") };

        [ComboBox(ItemsSource = "_SRC_")]
        public MyItem1 ItemsSourceComboBox { get; set; }

        [ListBox(ItemsSource = "_SRC_", DisplayMemberPath = "DisplayName.Name")]
        [Label("List Box1:", ControlLeftAlign_e.LeftEdge, FontStyle_e.Bold)]
        [ControlOptions(align: ControlLeftAlign_e.Indent)]
        public MyItem1 ListBox1 { get; set; }

        [ListBox("A1", "A2", "A3")]
        public string ListBox2 { get; set; }

        [ListBox(1, 2, 3, 4)]
        public List<int> ListBox3 { get; set; }

        //[ListBox]
        [OptionBox]
        [Label("Sample Option Box 4:", fontStyle: FontStyle_e.Underline)]
        public Opts OptionBox4 { get; set; }

        [OptionBox]
        [Label("Sample Option Box 5:")]
        public Opts1 OptionBox5 { get; set; }

        [OptionBox(1, 2, 3, 4)]
        public int OptionBox6 { get; set; }

        [OptionBox(typeof(MyCustomItemsProvider))]
        public MyItem OptionBox7 { get; set; }

        [ListBox]
        public OptsFlag ListBox5 { get; set; } = OptsFlag.Opt1 | OptsFlag.Opt3;

        [CheckBoxList]
        [CheckBoxListOptions]
        [ControlOptions(align: ControlLeftAlign_e.Indent)]
        public OptsFlag2 FlagEnumCheckBoxes { get; set; }

        [CheckBoxList(1, 2, 3, 4)]
        public List<int> CheckBoxList2 { get; set; }

        [CheckBoxList(typeof(MyCustomItemsProvider))]
        public List<MyItem> CheckBoxList3 { get; set; }

        [ControlTag(nameof(Visible))]
        public bool Visible { get; set; }

        [ControlTag(nameof(Enabled))]
        public bool Enabled { get; set; }

        [DependentOn(typeof(VisibilityHandler), nameof(Visible))]
        [DependentOn(typeof(EnableHandler), nameof(Enabled))]
        [Label("Numeric Control")]
        public double Number { get; set; }

        public IXCoordinateSystem CoordSystem { get; set; }

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
            CustomControl = OptsFlag.Opt3 | OptsFlag.Opt4;

            FlagEnumCheckBoxes = OptsFlag2.Opt3 | OptsFlag2.Opt4;

            CheckBoxList3 = new List<MyItem>()
            {
                MyItem.All[0]
            };

            OptionBox6 = 3;
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

        [ComboBox(typeof(MyCustomItems1Provider), nameof(Option3Set), Parameter = "TestParam")]
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
            public void UpdateState(IXApplication app, IControl source, IMetadata[] metadata, object parameter)
            {
                source.Enabled = !(bool)metadata.First().Value;
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
