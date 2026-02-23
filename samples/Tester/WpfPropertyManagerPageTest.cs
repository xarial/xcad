using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using Tester.Properties;
using Tester.UI;
using Xarial.XCad;
using Xarial.XCad.Base;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.Services;
using Xarial.XCad.Toolkit;
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.Toolkit.Windows.Services;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Enums;
using Xarial.XCad.UI.PropertyPage.Services;
using Xarial.XCad.UI.PropertyPage.Structures;
using Xarial.XCad.Utils.Diagnostics;

namespace Tester
{
    [Icon(typeof(Resources), nameof(Resources.sample_icon))]
    [Title("Sample Page")]
    [Description("Sample Page Data with multiple groups and controls")]
    [Help("https://xcad.net/", "https://xcad.xarial.com/changelog/")]
    public class PageData 
    {
        public enum EnumVals1_e
        {
            Val1,
            Val2,
            Val3
        }

        public class AllControlsGroup 
        {
            public string Text1 { get; set; }

            public EnumVals1_e Option1 { get; set; }

            public bool CheckBox { get; set; }

            [ControlOptions(align: ControlLeftAlign_e.Indent)]
            public Action Button1 { get; }

            [CustomControl(typeof(CustomControl))]
            [ControlOptions(height: 100)]
            public string CustomControl { get; set; }

            [Label("Second Text:")]
            public string Text2 { get; set; }

            [TextBlock]
            public string TextBlock1 { get; }

            public AllControlsGroup() 
            {
                CustomControl = "Custom Control Text";
                TextBlock1 = "Sample Text Block";

                Button1 = OnButton1Click;
            }

            private void OnButton1Click() 
            {
                MessageBox.Show("Button1 Click");
            }
        }

        public class ReactiveGroup : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            public class OptionItem1 
            {
                public string Name { get; }

                public OptionItem1(string name) 
                {
                    Name = name;
                }
            }

            public string Text1 { get; set; }

            [ControlOptions(align: ControlLeftAlign_e.Indent)]
            public bool Check1 { get; set; }

            [ControlOptions(align: ControlLeftAlign_e.DoubleIndent)]
            public EnumVals1_e Option1 { get; set; }

            [Metadata(nameof(Options))]
            public OptionItem1[] Options { get; }

            [Icon(typeof(Resources), nameof(Resources.sample_icon))]
            [ComboBox(ItemsSource = nameof(Options), DisplayMemberPath = nameof(OptionItem1.Name))]
            public OptionItem1 Option2 { get; set; }

            public Action Update { get; }

            public ReactiveGroup() 
            {
                Options = new OptionItem1[]
                {
                    new OptionItem1("A"),
                    new OptionItem1("B"),
                    new OptionItem1("C")
                };

                Text1 = "Text1";
                Check1 = true;
                Option1 = EnumVals1_e.Val2;
                Option2 = Options[1];

                Update = OnUpdate;
            }

            private void OnUpdate() 
            {
                Text1 = Guid.NewGuid().ToString();
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text1)));

                Check1 = !Check1;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Check1)));

                Option1 = Option1 == Enum.GetValues(typeof(EnumVals1_e)).Cast<EnumVals1_e>().Last() ? EnumVals1_e.Val1 : Option1 + 1;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Option1)));

                Option2 = Array.IndexOf(Options, Option2) == Options.Length - 1 ? Options[0] : Options[Array.IndexOf(Options, Option2) + 1];
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Option2)));
            }
        }

        public class DependencyGroup : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            public class EnabledDepHandler : IDependencyHandler
            {
                public void UpdateState(IXApplication app, IControl source, IControl[] dependencies, object parameter)
                {
                    source.Enabled = (bool)dependencies.First().GetValue();
                }
            }

            public class VisibleDepHandler : IDependencyHandler
            {
                public void UpdateState(IXApplication app, IControl source, IControl[] dependencies, object parameter)
                {
                    source.Visible = (bool)dependencies.First().GetValue();
                }
            }

            public class EnabledVisibleMetadataDepHandler : IMetadataDependencyHandler
            {
                public void UpdateState(IXApplication app, IControl source, IMetadata[] metadata, object parameter)
                {
                    var val = (bool)metadata.First().Value;

                    if (bool.Equals(parameter, true))
                    {
                        source.Visible = val;
                    }
                    else if (bool.Equals(parameter, false))
                    {
                        source.Enabled = val;
                    }
                    else 
                    {
                        throw new NotSupportedException();
                    }
                }
            }


            [DependentOn(typeof(EnabledDepHandler), nameof(Enable))]
            public string Text1 { get; set; }

            [ControlTag(nameof(Enable))]
            [ControlOptions(align: ControlLeftAlign_e.Indent)]
            public bool Enable { get; set; }

            [DependentOn(typeof(VisibleDepHandler), nameof(Visible))]
            public string Text2 { get; set; }

            [ControlTag(nameof(Visible))]
            [ControlOptions(align: ControlLeftAlign_e.Indent)]
            public bool Visible { get; set; }

            [Metadata(nameof(VisibleEnabledFlag))]
            public bool VisibleEnabledFlag { get; set; }

            [DependentOnMetadata(typeof(EnabledVisibleMetadataDepHandler), nameof(VisibleEnabledFlag), Parameter = true)]
            public string Text3 { get; set; }

            [DependentOnMetadata(typeof(EnabledVisibleMetadataDepHandler), nameof(VisibleEnabledFlag), Parameter = false)]
            public string Text4 { get; set; }

            public Action UpdateMetadata { get; }

            public DependencyGroup() 
            {
                Text1 = "Text1";
                Enable = true;

                Text2 = "Text2";
                Visible = true;

                Text3 = "Text3";
                Text4 = "Text4";

                VisibleEnabledFlag = true;

                UpdateMetadata = OnUpdateMetadata;
            }

            private void OnUpdateMetadata() 
            {
                VisibleEnabledFlag = !VisibleEnabledFlag;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VisibleEnabledFlag)));
            }
        }

        public AllControlsGroup AllControls { get; }
        public ReactiveGroup Reactive { get; }
        public DependencyGroup Dependency { get; }

        public PageData()
        {
            AllControls = new AllControlsGroup();
            Reactive = new ReactiveGroup();
            Dependency = new DependencyGroup();
        }
    }

    public class SimplePageData 
    {
        public class Group 
        {
            [BitmapButton(typeof(Resources), nameof(Resources.sample_icon))]
            [ControlOptions(width: 50, height: 50, top: 0, left: 0)]
            public bool CheckButton1 { get; set; }

            [BitmapButton(typeof(Resources), nameof(Resources.sample_icon1))]
            [ControlOptions(width: 50, height: 50, top: 0, left: 60)]
            public bool CheckButton2 { get; set; }

            [ControlOptions(top: 0, left: 80)]
            public string Text { get; set; }
        }

        public Group Group1 { get; }

        public SimplePageData() 
        {
            Group1 = new Group();
        }
    }

    public static class WpfPropertyManagerPageTest
    {
        private static PageData m_Data;

        public static void TestPageBuilderAll()
        {
            var app = new TestApplication();

            var svcProv = CreateServiceProvider(app);

            var page = new TestWpfPropertyManagerPage<PageData>(app, svcProv);

            page.Closing += OnPageClosing;
            page.Closed += OnPageClosed;

            m_Data = new PageData();

            page.Show(m_Data);
        }

        public static void TestPageBuilderSimple()
        {
            var app = new TestApplication();

            var svcProv = CreateServiceProvider(app);

            var page = new TestWpfPropertyManagerPage<SimplePageData>(app, svcProv);

            var data = new SimplePageData();

            page.Show(data);
        }

        private static IServiceProvider CreateServiceProvider(TestApplication app)
        {
            var svcColl = new ServiceCollection();

            svcColl.Add<IXLogger>(() => new TraceLogger("Test"), ServiceLifetimeScope_e.Singleton, false);
            svcColl.Add<IIconsCreator, BaseIconsCreator>(ServiceLifetimeScope_e.Singleton, false);
            svcColl.Add<IHelpLinkHandler>(() => new HelpLinkHandler(app), ServiceLifetimeScope_e.Singleton, false);
            svcColl.Add<ITooltipLinkLinkHandler>(() => new TooltipLinkLinkHandler(app), ServiceLifetimeScope_e.Singleton, false);

            var svcProv = svcColl.CreateProvider();
            return svcProv;
        }

        private static void OnPageClosing(PageCloseReasons_e reason, PageClosingArg arg)
        {
        }

        private static void OnPageClosed(PageCloseReasons_e reason)
        {
        }
    }
}
