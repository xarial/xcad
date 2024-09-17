using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml.Serialization;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.Documentation.Properties;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.UI.Commands.Attributes;
using Xarial.XCad.UI.Commands.Enums;
using Xarial.XCad.UI.Commands;
using Xarial.XCad.Extensions;
using Xarial.XCad.SolidWorks.Documents.Services;
using Xarial.XCad.Documents.Services;
using Xarial.XCad;
using Xarial.XCad.Documents;
using Xarial.XCad.SolidWorks.Documents;
using SolidWorks.Interop.sldworks;
using Xarial.XCad.Data.Enums;
using Xarial.XCad.Documents.Extensions;

namespace Xarial.XCad.Documentation
{
    #region Register
    [ComVisible(true), Guid("736EEACF-B294-40F6-8541-CFC8E7C5AA61")]
    public class SampleAddIn : SwAddInEx
    {
        #endregion Register
        #region TaskPane
        public class TaskPaneControl : UserControl
        {
        }
        #endregion TaskPane
        #region CommandGroup
        [Title(typeof(Resources), nameof(Resources.ToolbarTitle)), Description("Toolbar with commands")]
        [Icon(typeof(Resources), nameof(Resources.commands))]
        public enum Commands_e
        {
            [Title("Command 1"), Description("Sample command 1")]
            [Icon(typeof(Resources), nameof(Resources.command1))]
            [CommandItemInfo(true, true, WorkspaceTypes_e.Assembly, true, RibbonTabTextDisplay_e.TextBelow)]
            Command1,
            Command2
        }
        #endregion CommandGroup
        #region TaskPane
        public enum TaskPaneCommands_e
        {
            Command1
        }
        #endregion TaskPane
        #region DocHandler
        public class MyDocumentHandler : SwDocumentHandler
        {
            protected override void AttachAssemblyEvents(AssemblyDoc assm)
            {
                assm.FileSaveNotify += OnFileSave;
                assm.RegenNotify += OnRegen;
            }

            protected override void AttachPartEvents(PartDoc part)
            {
                part.FileSaveNotify += OnFileSave;
                part.RegenNotify += OnRegen;
            }

            protected override void AttachDrawingEvents(DrawingDoc draw)
            {
                draw.FileSaveNotify += OnFileSave;
                draw.RegenNotify += OnRegen;
            }

            private int OnFileSave(string FileName)
            {
                //handle saving
                return S_OK;
            }

            private int OnRegen()
            {
                //handle rebuild
                return S_OK;
            }

            protected override void DetachAssemblyEvents(AssemblyDoc assm)
            {
                assm.FileSaveNotify -= OnFileSave;
                assm.RegenNotify -= OnRegen;
            }

            protected override void DetachPartEvents(PartDoc part)
            {
                part.FileSaveNotify -= OnFileSave;
                part.RegenNotify -= OnRegen;
            }

            protected override void DetachDrawingEvents(DrawingDoc draw)
            {
                draw.FileSaveNotify -= OnFileSave;
                draw.RegenNotify -= OnRegen;
            }
        }
        #endregion DocHandler
        public override void OnConnect()
        {
            #region CommandGroup
            this.CommandManager.AddCommandGroup`< Commands_e >`().CommandClick += OnButtonClick;
            #endregion CommandGroup
            #region DocHandler
            Application.Documents.RegisterHandler<MyDocumentHandler>();
            #endregion DocHandler
            #region TaskPane
            var taskPane = this.CreateTaskPane<TaskPaneControl, TaskPaneCommands_e>();
            taskPane.ButtonClick += OnTaskPaneCommandClick;
            TaskPaneControl ctrl = taskPane.Control;
            #endregion TaskPane
            #region 3rdParty
            Application.Documents.Active.StreamWriteAvailable += OnWriteToStream;
            #endregion 3rdParty
        }

        #region CommandGroup
        private void OnButtonClick(Commands_e cmd)
        {
            //handle commands
        }
        #endregion CommandGroup
        #region 3rdParty
        private void OnWriteToStream(IXDocument doc)
        {
            const string STREAM_NAME = "CodeStackStream";

            using (var str = doc.OpenStream(STREAM_NAME, AccessType_e.Write))
            {
                var xmlSer = new XmlSerializer(typeof(string[]));

                xmlSer.Serialize(str, new string[] { "A", "B" });
            }
        }
        #endregion 3rdParty
        #region TaskPane
        private void OnTaskPaneCommandClick(TaskPaneCommands_e cmd)
        {
            switch (cmd)
            {
                case TaskPaneCommands_e.Command1:
                    //handle command
                    break;
            }
        }
        #endregion TaskPane
    }
}
