//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SwAddInExample.Properties;
using SolidWorks.Interop.swconst;
using System.Numerics;
using SolidWorks.Interop.sldworks;
using Xarial.XCad.UI.Commands.Attributes;
using Xarial.XCad.UI.Commands.Enums;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.UI.PropertyPage;
using Xarial.XCad.UI.Commands;
using Xarial.XCad.UI.PropertyPage.Enums;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Documents.Structures;
using Xarial.XCad.Documents;
using Xarial.XCad.Base;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Annotations;
using Xarial.XCad.SolidWorks.Data;
using Xarial.XCad.UI.TaskPane.Attributes;
using Xarial.XCad.SolidWorks.UI;
using Xarial.XCad.SolidWorks.UI.PropertyPage;
using Xarial.XCad.UI.Commands.Structures;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI;
using System.Collections.Generic;
using Xarial.XCad.Reflection;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.Extensions;
using Xarial.XCad.Enums;
using System.Drawing;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.SolidWorks.Features;
using System.Diagnostics;

namespace SwAddInExample
{
    [ComVisible(true)]
    [Guid("3078E7EF-780E-4A70-9359-172D90FAAED2")]
    public class SwAddInSample : SwAddInEx
    {
        public class DictionaryControl : IControlDescriptor
        {
            public string DisplayName { get; set; }
            public string Description { get; set; }
            public string Name { get; set; }
            public IXImage Icon { get; set; }
            public Type DataType { get; set; }
            public Xarial.XCad.UI.PropertyPage.Base.IAttribute[] Attributes { get; set; }

            public object GetValue(object context)
            {
                var dict = context as Dictionary<string, object>;
                
                if (!dict.TryGetValue(Name, out object val)) 
                {
                    val = Activator.CreateInstance(DataType);
                }

                return val;
            }

            public void SetValue(object context, object value)
            {
                var dict = context as Dictionary<string, object>;
                dict[Name] = value;
            }
        }

        [Icon(typeof(Resources), nameof(Resources.xarial))]
        public enum Commands_e 
        {
            [Icon(typeof(Resources), nameof(Resources.xarial))]
            OpenDoc,

            ShowPmPage,

            ShowToggleGroupPage,

            [Icon(typeof(Resources), nameof(Resources.xarial))]
            [CommandItemInfo(WorkspaceTypes_e.Part)]
            ShowPmPageMacroFeature,

            [Icon(typeof(Resources), nameof(Resources.xarial))]
            RecordView,

            [Icon(typeof(Resources), nameof(Resources.horizontal))]
            CreateBox,

            [Icon(typeof(Resources), nameof(Resources.vertical))]
            WatchDimension,

            WatchCustomProperty,

            CreateModelView,

            CreateFeatMgrView,

            CreatePopup,

            CreateTaskPane,

            HandleSelection,

            ShowTooltip,

            ShowPmpComboBox,

            GetMassPrps
        }

        [Icon(typeof(Resources), nameof(Resources.xarial))]
        public class MyTooltipSpec : ITooltipSpec
        {
            public string Title { get; }
            public string Message { get; }
            public System.Drawing.Point Position { get; }
            public TooltipArrowPosition_e ArrowPosition { get; }

            internal MyTooltipSpec(string title, string msg, System.Drawing.Point pt, TooltipArrowPosition_e arrPos)
            {
                Title = title;
                Message = msg;
                Position = pt;
                ArrowPosition = arrPos;
            }
        }

        [Title("Sample Context Menu")]
        public enum ContextMenuCommands_e 
        {
            Command1,

            Command2
        }

        [Icon(typeof(Resources), nameof(Resources.xarial))]
        [Title("Sample Task Pane")]
        public enum TaskPaneButtons_e 
        {
            [Icon(typeof(Resources), nameof(Resources.xarial))]
            Button1,

            [Title("Second Button")]
            Button2,

            [TaskPaneStandardIcon(Xarial.XCad.UI.TaskPane.Enums.TaskPaneStandardIcons_e.Options)]
            Button3
        }

        private IXPropertyPage<PmpMacroFeatData> m_MacroFeatPage;
        private PmpMacroFeatData m_MacroFeatPmpData;
        private PmpComboBoxData m_PmpComboBoxData;

        private ISwPropertyManagerPage<PmpData> m_Page;
        private ISwPropertyManagerPage<ToggleGroupPmpData> m_ToggleGroupPage;
        private ISwPropertyManagerPage<PmpComboBoxData> m_ComboBoxPage;
        private ToggleGroupPmpData m_TogglePageData;

        private PmpData m_Data;

        [CommandGroupInfo(1)]
        public enum Commands1_e 
        {
            Cmd1,
            //Cmd2,
            //Cmd5
        }

        [CommandGroupInfo(2)]
        [CommandGroupParent(typeof(Commands1_e))]
        public enum Commands2_e
        {
            Cmd3,
            Cmd4,
            Cmd7,
            Cmd8
        }

        public override void OnConnect()
        {
            //CommandManager.AddCommandGroup<Commands1_e>();
            //CommandManager.AddCommandGroup<Commands2_e>();
            //return;

            try
            {
                CommandManager.AddCommandGroup(new CommandGroupSpec(99)
                {
                    Title = "Group 1",
                    Commands = new CommandSpec[]
                    {
                    new CommandSpec(1)
                    {
                        Title = "Cmd1",
                        HasMenu = true,
                        HasToolbar = true,
                        HasRibbon = true,
                        RibbonTextStyle = RibbonTabTextDisplay_e.TextBelow,
                        SupportedWorkspace = WorkspaceTypes_e.All
                    },
                    new CommandSpec(4)
                    {
                        Title = "Cmd2",
                        HasMenu = true,
                        HasToolbar = true,
                        HasRibbon = true,
                        RibbonTextStyle = RibbonTabTextDisplay_e.TextBelow,
                        SupportedWorkspace = WorkspaceTypes_e.All
                    },
                    new CommandSpec(5)
                    {
                        Title = "Cmd3",
                        HasMenu = true,
                        HasToolbar = true,
                        HasRibbon = true,
                        RibbonTextStyle = RibbonTabTextDisplay_e.TextBelow,
                        SupportedWorkspace = WorkspaceTypes_e.All
                    }
                    }
                });

                CommandManager.AddCommandGroup<Commands_e>().CommandClick += OnCommandClick;
                CommandManager.AddContextMenu<ContextMenuCommands_e>(Xarial.XCad.Base.Enums.SelectType_e.Faces).CommandClick += OnContextMenuCommandClick;

                Application.Documents.RegisterHandler<SwDocHandler>();

                Application.Documents.DocumentActivated += OnDocumentActivated;

                m_Page = this.CreatePage<PmpData>(OnCreateDynamicControls);
                m_Page.Closed += OnPage1Closed;

                m_ToggleGroupPage = this.CreatePage<ToggleGroupPmpData>();
                m_ToggleGroupPage.Closed += OnToggleGroupPageClosed;

                m_MacroFeatPage = this.CreatePage<PmpMacroFeatData>();
                m_MacroFeatPage.Closed += OnClosed;

                m_ComboBoxPage = this.CreatePage<PmpComboBoxData>();
                m_ComboBoxPage.Closed += OnComboBoxPageClosed;
            }
            catch 
            {
                Debug.Assert(false);
            }
        }

        private void OnComboBoxPageClosed(PageCloseReasons_e reason)
        {
            var x = m_PmpComboBoxData;
        }

        private void OnDocumentActivated(IXDocument doc)
        {
        }

        private void OnToggleGroupPageClosed(PageCloseReasons_e reason)
        {
        }

        private IControlDescriptor[] OnCreateDynamicControls(object tag)
        {
            return new IControlDescriptor[]
            {
                new DictionaryControl()
                {
                    DataType = typeof(string),
                    Name = "A",
                    Attributes = new Xarial.XCad.UI.PropertyPage.Base.IAttribute[]
                    {
                        new ControlOptionsAttribute(backgroundColor: System.Drawing.KnownColor.Yellow)
                    }
                },
                new DictionaryControl()
                {
                    DataType = typeof(ContextMenuCommands_e),
                    Name = "B"
                },
                new DictionaryControl()
                {
                    DataType = typeof(int),
                    Name = "C",
                    Icon = ResourceHelper.GetResource<IXImage>(typeof(Resources), nameof(Resources.xarial)),
                    Description = ""
                }
            };
        }

        private void OnPage1Closed(PageCloseReasons_e reason)
        {
        }

        private void OnContextMenuCommandClick(ContextMenuCommands_e spec)
        {
        }

        private void OnClosed(PageCloseReasons_e reason)
        {
            if (reason == PageCloseReasons_e.Okay) 
            {
                //var feat = Application.Documents.Active.Features.CreateCustomFeature<SimpleMacroFeature>();
                var feat = Application.Documents.Active.Features.CreateCustomFeature<SampleMacroFeature, PmpMacroFeatData>(m_MacroFeatPmpData);
            }
        }

        private ISwDimension m_WatchedDim;
        private ISwCustomProperty m_WatchedPrp;

        private void WatchDimension() 
        {
            if (m_WatchedDim == null)
            {
                m_WatchedDim = Application.Documents.Active.Dimensions["D1@Sketch1"];
                m_WatchedDim.ValueChanged += OnDimValueChanged;
            }
            else 
            {
                m_WatchedDim.ValueChanged -= OnDimValueChanged;
                m_WatchedDim = null;
            }
        }

        private void OnDimValueChanged(Xarial.XCad.Annotations.IXDimension dim, double newVal)
        {
        }

        private void WatchCustomProperty() 
        {
            m_WatchedPrp = Application.Documents.Active.Properties["Test"];
            m_WatchedPrp.ValueChanged += OnPropertyValueChanged;
        }

        private void OnPropertyValueChanged(Xarial.XCad.Data.IXProperty prp, object newValue)
        {
        }

        private TransformMatrix m_ViewTransform;
        private ISwPopupWindow<WpfWindow> m_Window;
        private IXCustomPanel<WpfUserControl> m_FeatMgrTab;

        private void OnCommandClick(Commands_e spec)
        {
            try
            {
                switch (spec)
                {
                    case Commands_e.OpenDoc:

                        var cutLists = (Application.Documents.Active as ISwPart).Configurations.Active.CutLists.ToArray();

                        //(Application.Documents.Active.Model as AssemblyDoc).FileDropPreNotify += SwAddInSample_FileDropPreNotify;
                        var doc = Application.Documents.PreCreate<IXDocument>();
                        doc.Path = @"C:\Users\artem\OneDrive\xCAD\TestData\Assembly2\TopAssem.SLDASM";
                        doc.State = DocumentState_e.Rapid;
                        doc.Commit();
                        break;

                    case Commands_e.ShowPmPage:
                        m_Data = new PmpData();
                        m_Data.ItemsSourceComboBox = "Y";
                        m_Page.Show(m_Data);
                        m_Page.DataChanged += OnPageDataChanged;
                        break;

                    case Commands_e.ShowToggleGroupPage:
                        m_ToggleGroupPage.Show(m_TogglePageData ?? (m_TogglePageData = new ToggleGroupPmpData()));
                        break;

                    case Commands_e.ShowPmPageMacroFeature:
                        m_MacroFeatPmpData = new PmpMacroFeatData() { Text = "ABC", Number = 0.1 };
                        m_MacroFeatPage.Show(m_MacroFeatPmpData);
                        break;

                    case Commands_e.RecordView:
                        var view = (Application.Documents.Active as IXDocument3D).ModelViews.Active;

                        if (m_ViewTransform == null)
                        {
                            m_ViewTransform = view.Transform;
                            Application.Sw.SendMsgToUser("Recorded");
                        }
                        else
                        {
                            view.Transform = m_ViewTransform;
                            view.Update();
                            m_ViewTransform = null;
                            Application.Sw.SendMsgToUser("Restored");
                        }
                        break;

                    case Commands_e.CreateBox:
                        Application.Documents.Active.Features.CreateCustomFeature<BoxMacroFeatureEditor, BoxMacroFeatureData, BoxData>();
                        break;

                    case Commands_e.WatchDimension:
                        WatchDimension();
                        break;

                    case Commands_e.WatchCustomProperty:
                        WatchCustomProperty();
                        break;

                    case Commands_e.CreateModelView:
                        this.CreateDocumentTabWpf<WpfUserControl>(Application.Documents.Active);
                        //this.CreateDocumentTabWinForm<WinUserControl>(Application.Documents.Active);
                        //this.CreateDocumentTabWinForm<ComUserControl>(Application.Documents.Active);
                        break;

                    case Commands_e.CreateFeatMgrView:
                        m_FeatMgrTab = this.CreateFeatureManagerTab<WpfUserControl>(Application.Documents.Active);
                        m_FeatMgrTab.Activated += OnFeatureManagerTabActivated;

                        foreach (var comp in Application.Documents.Active.Selections.OfType<IXComponent>())
                        {
                            this.CreateFeatureManagerTab<WpfUserControl>((ISwDocument)comp.ReferencedDocument);
                        }

                        //this.CreateDocumentTabWinForm<WinUserControl>(Application.Documents.Active);
                        //this.CreateDocumentTabWinForm<ComUserControl>(Application.Documents.Active);
                        break;

                    case Commands_e.CreatePopup:
                        //var winForm = this.CreatePopupWinForm<WinForm>();
                        //winForm.Show(true);
                        m_Window?.Close();
                        m_Window = this.CreatePopupWpfWindow<WpfWindow>();
                        m_Window.Closed += OnWindowClosed;
                        m_Window.Show();
                        break;

                    case Commands_e.CreateTaskPane:
                        var tp = this.CreateTaskPaneWpf<WpfUserControl, TaskPaneButtons_e>();
                        tp.ButtonClick += OnButtonClick;
                        //this.CreateTaskPaneWinForm<WinUserControl>();
                        //this.CreateTaskPaneWinForm<ComUserControl>();
                        break;

                    case Commands_e.HandleSelection:
                        Application.Documents.Active.Selections.NewSelection += OnNewSelection;
                        Application.Documents.Active.Selections.ClearSelection += OnClearSelection;
                        break;

                    case Commands_e.ShowTooltip:
                        var modelView = (Application.Documents.Active as IXDocument3D).ModelViews.Active;
                        var pt = new System.Drawing.Point(modelView.ScreenRect.Left, modelView.ScreenRect.Top);
                        Application.ShowTooltip(new MyTooltipSpec("xCAD", "Test Message", pt, TooltipArrowPosition_e.LeftTop));
                        break;

                    case Commands_e.ShowPmpComboBox:
                        m_PmpComboBoxData = new PmpComboBoxData();
                        m_ComboBoxPage.Show(m_PmpComboBoxData);
                        break;

                    case Commands_e.GetMassPrps:

                        var visOnly = true;
                        var relToCoordSys = "Coordinate System1";
                        var userUnits = true;

                        var massPrps = ((ISwAssembly)Application.Documents.Active).PreCreateMassProperty();
                        massPrps.Scope = Application.Documents.Active.Selections.OfType<IXComponent>().ToArray();
                        massPrps.VisibleOnly = visOnly;
                        massPrps.UserUnits = userUnits;
                        if (!string.IsNullOrEmpty(relToCoordSys))
                        {
                            massPrps.RelativeTo = ((ISwCoordinateSystem)Application.Documents.Active.Features[relToCoordSys]).Transform;
                        }
                        massPrps.Commit();
                        var cog = massPrps.CenterOfGravity;
                        var dens = massPrps.Density;
                        var mass = massPrps.Mass;
                        var moi = massPrps.MomentOfInertia;
                        var paoi = massPrps.PrincipalAxesOfInertia;
                        var pmoi = massPrps.PrincipalMomentOfInertia;
                        var surfArea = massPrps.SurfaceArea;
                        var volume = massPrps.Volume;
                        break;
                }
            }
            catch 
            {
                Debug.Assert(false);
            }
        }

        private int SwAddInSample_FileDropPreNotify(string FileName)
        {
            var res = 0;
            var fileName = @"D:\Temp\Part1.SLDPRT";
            var x = (Application.Documents.Active.Model as IAssemblyDoc).SetDroppedFileName(fileName);

            return res;
        }

        private void OnFeatureManagerTabActivated(IXCustomPanel<WpfUserControl> sender)
        {
        }

        public override void OnConfigureServices(IXServiceCollection collection)
        {
            collection.AddOrReplace<IMemoryGeometryBuilderDocumentProvider>(
                () => new LazyNewDocumentGeometryBuilderDocumentProvider(Application));
        }

        private void OnPageDataChanged()
        {
        }

        private void OnNewSelection(IXDocument doc, Xarial.XCad.IXSelObject selObject)
        {
        }

        private void OnClearSelection(IXDocument doc)
        {
        }

        private void OnWindowClosed(Xarial.XCad.UI.IXPopupWindow<WpfWindow> sender)
        {
        }

        private void OnButtonClick(TaskPaneButtons_e spec)
        {
        }
    }
}
