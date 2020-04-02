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

namespace SwAddInExample
{
    [ComVisible(true)]
    [Guid("3078E7EF-780E-4A70-9359-172D90FAAED2")]
    public class SwAddInSample : SwAddInEx
    {
        [Icon(typeof(Resources), nameof(Resources.xarial))]
        public enum Commands_e 
        {
            [Icon(typeof(Resources), nameof(Resources.xarial))]
            OpenDoc,

            [Icon(typeof(Resources), nameof(Resources.xarial))]
            [CommandItemInfo(WorkspaceTypes_e.Part)]
            ShowPmPageMacroFeature,

            [Icon(typeof(Resources), nameof(Resources.xarial))]
            RecordView,

            CreateBox,

            WatchDimension,

            WatchCustomProperty,

            CreateModelView,

            CreatePopup,

            CreateTaskPane
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

        private IXPropertyPage<PmpData> m_Page;
        private PmpData m_Data;

        public override void OnConnect()
        {
            CommandManager.AddCommandGroup<Commands_e>().CommandClick += OnCommandClick;
            Application.Documents.RegisterHandler<SwDocHandler>();
            m_Page = this.CreatePage<PmpData>();
            m_Page.Closed += OnClosed;
        }

        private void OnClosed(PageCloseReasons_e reason)
        {
            if (reason == PageCloseReasons_e.Okay) 
            {
                var feat = Application.Documents.Active.Features.CreateCustomFeature<SampleMacroFeature, PmpData>(m_Data);
            }
        }

        private SwDimension m_WatchedDim;
        private SwCustomProperty m_WatchedPrp;

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

        private void OnCommandClick(Commands_e spec)
        {
            switch (spec) 
            {
                case Commands_e.OpenDoc:
                    var doc = Application.Documents.Open(new DocumentOpenArgs()
                    {
                        Path = @"C:\Users\artem\OneDrive\Attribution\SwModels\Annotation.sldprt",
                    });
                    break;

                case Commands_e.ShowPmPageMacroFeature:
                    m_Data = new PmpData() { Text = "ABC", Number = 0.1 };
                    m_Page.Show(m_Data);
                    break;

                case Commands_e.RecordView:
                    var view = (Application.Documents.Active as IXDocument3D).ActiveView;

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
                    Application.Documents.Active.Features.CreateCustomFeature<BoxMacroFeatureEditor, BoxData, BoxData>();
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

                case Commands_e.CreatePopup:
                    var winForm = this.CreatePopupWinForm<WinForm>();
                    winForm.Modal = true;
                    winForm.IsActive = true;
                    break;

                case Commands_e.CreateTaskPane:
                    var tp = this.CreateTaskPaneWpf<WpfUserControl, TaskPaneButtons_e>();
                    tp.ButtonClick += OnButtonClick;
                    //this.CreateTaskPaneWinForm<WinUserControl>();
                    //this.CreateTaskPaneWinForm<ComUserControl>();
                    break;
            }
        }

        private void OnButtonClick(TaskPaneButtons_e spec)
        {
        }
    }
}
