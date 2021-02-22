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

namespace SwAddInExample
{
    public enum Opts
    {
        Opt1,
        Opt2,
        Opt3
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

    [ComVisible(true)]
    public class PmpData : SwPropertyManagerPageHandler, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [CustomControl(typeof(WpfUserControl))]
        //[CustomControl(typeof(WinUserControl))]
        [ControlOptions(height: 200)]
        public CustomControlDataContext CustomControl { get; set; } = new CustomControlDataContext();

        public List<ISwComponent> Components { get; set; }

        public ISwBody Body { get; set; }

        public ISwCircularEdge CircEdge { get; set; }

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

        [ComboBoxItemsSource(1, 2, 3, 4, 5)]
        public int StaticComboBox { get; set; }

        [Metadata("_SRC_")]
        public string[] Source => new string[] { "X", "Y", "Z" };

        [ComboBoxItemsSource(Source = "_SRC_")]
        public string ItemsSourceComboBox { get; set; }

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
        [ComboBoxItemsSource(typeof(MyCustomItemsProvider))]
        [ComboBoxOptions(selectDefaultValue: true)]
        public MyItem Option2 { get; set; }

        [ParameterDimension(CustomFeatureDimensionType_e.Angular)]
        [ExcludeControl]
        public double Angle { get; set; } = Math.PI / 9;

        public PmpMacroFeatData() 
        {
            Option2 = MyItem.All.Last();
        }
    }
}
