using System.ComponentModel;
using System.Diagnostics;
using Xarial.XCad;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.Toolkit.Blazor.Demo.Controls;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Services;

namespace Web.Blazor.Models
{
    [Title("Sample Data")]
    [Description("Sample data page")]
    public class SampleData
    {
        public class SimpleControlsGroup
        {
            public enum Items_e 
            {
                [Title("First Item")]
                Item1,

                [Title("Second Item")]
                Item2,

                [Title("Third Item")]
                Item3
            }

            [Label("Text Box:")]
            public string TextBox { get; set; }

            [Label("Number Box:")]
            public double NumberBox { get; set; }

            [Label("Integer Box:")]
            public int IntegerBox { get; set; }

            [Label("Check Box")]
            public bool CheckBox { get; set; }

            [Label("Option Box:")]
            public Items_e OptionBox { get; set; }
        }

        [Title("Advanced Controls")]
        public class AdvancedControlsGroup : INotifyPropertyChanged
        {
            public class MyItem 
            {
                public string Name { get; }

                public MyItem(string name)
                {
                    Name = name;
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            [ComboBox(ItemsSource = nameof(ItemsSource), DisplayMemberPath = nameof(MyItem.Name))]
            public MyItem SelectedItem { get; set; }

            [Metadata(nameof(ItemsSource))]
            public MyItem[] ItemsSource 
            {
                get => m_ItemsSource;
                private set 
                {
                    m_ItemsSource = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ItemsSource)));
                }
            }

            [Title("Add Item")]
            public Action AddItemAction { get; }

            [Title("Attached Files")]
            [Description("Drag and drop reference files onto the box below")]
            [CustomControl(typeof(FileDropZone))]
            public IReadOnlyList<string> AttachedFiles{ get; set; }

            private MyItem[] m_ItemsSource;

            public AdvancedControlsGroup() 
            {
                m_ItemsSource = new MyItem[]
                {
                    new MyItem("Item #1"),
                    new MyItem("Item #2"),
                    new MyItem("Item #3")
                };

                SelectedItem = m_ItemsSource.First();

                AddItemAction = OnAddItem;
            }

            private void OnAddItem()
            {
                var selItem = SelectedItem;
                ItemsSource = m_ItemsSource.Union(new MyItem[] { new MyItem($"Item #{m_ItemsSource.Length + 1}") }).ToArray();
                SelectedItem = null;
                SelectedItem = selItem;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedItem)));
            }
        }

        [Tab]
        public class ControlsTab
        {
            [Title("Simple Controls")]
            public SimpleControlsGroup Simple { get; }
            public AdvancedControlsGroup Advanced { get; }

            public ControlsTab() 
            {
                Simple = new SimpleControlsGroup();
                Advanced = new AdvancedControlsGroup();
            }
        }

        [Tab]
        public class BehaviorTab
        {
            public class StateGroup : INotifyPropertyChanged
            {
                public event PropertyChangedEventHandler PropertyChanged;

                public class VisibilityConverter : IDependencyHandler
                {
                    public void UpdateState(IXApplication app, IControl source, IControl[] dependencies, object parameter)
                    {
                        source.Visible = (bool)dependencies.First().GetValue();
                    }
                }

                public class EnableConverter : IDependencyHandler
                {
                    public void UpdateState(IXApplication app, IControl source, IControl[] dependencies, object parameter)
                    {
                        source.Enabled = (bool)dependencies.First().GetValue();
                    }
                }

                public class EnableMetadataConverter : IMetadataDependencyHandler
                {
                    public void UpdateState(IXApplication app, IControl source, IMetadata[] metadata, object parameter)
                    {
                        source.Enabled = (bool)metadata.First().Value;
                    }
                }

                [ControlTag(nameof(EnableNext))]
                [Title("Enable")]
                public bool EnableNext { get; set; }

                [Label("Text Box:")]
                [DependentOn(typeof(EnableConverter), nameof(EnableNext))]
                public string TextBox1 { get; set; }

                [ControlTag(nameof(VisibleNext))]
                [Title("Visible")]
                public bool VisibleNext { get; set; }

                [Label("Number Box:")]
                [DependentOn(typeof(VisibilityConverter), nameof(VisibleNext))]
                public decimal NumberBox1 { get; set; }

                [Title("Toggle Enable")]
                public Action ToggleEnableNextAction { get; }

                [Metadata(nameof(IsEnabledNext))]
                public bool IsEnabledNext 
                {
                    get => m_IsEnabledNext;
                    private set 
                    {
                        m_IsEnabledNext = value;
                        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsEnabledNext)));
                    }
                }

                [ComboBox("A", "B", "C")]
                [Label("Combo Box")]
                [DependentOnMetadata(typeof(EnableMetadataConverter), nameof(IsEnabledNext))]
                public string CombobBox1 { get; set; }

                private bool m_IsEnabledNext;

                public StateGroup()
                {
                    ToggleEnableNextAction = ToggleEnableNext;
                    EnableNext = true;
                    VisibleNext = true;
                    IsEnabledNext = true;
                }

                private void ToggleEnableNext()
                {
                    IsEnabledNext = !IsEnabledNext;
                }
            }

            public StateGroup State { get; }

            public BehaviorTab() 
            {
                State = new StateGroup();
            }
        }

        public ControlsTab Controls { get; }
        public BehaviorTab Behavior { get; }

        [Title("Check")]
        public Action CheckAction { get; }

        public SampleData() 
        {
            Controls = new ControlsTab();
            Behavior = new BehaviorTab();
            CheckAction = OnCheck;
+        }

        private void OnCheck()
        {
            Debugger.Launch();
        }
    }
}
