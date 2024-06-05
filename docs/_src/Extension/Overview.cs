﻿using System;
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
using Xarial.XCad;
using Xarial.XCad.Documents;
using SolidWorks.Interop.sldworks;
using Xarial.XCad.Documents.Extensions;

namespace Xarial.XCad.Documentation
{
    //--- Register
    [ComVisible(true), Guid("736EEACF-B294-40F6-8541-CFC8E7C5AA61")]
    public class SampleAddIn : SwAddInEx
    {
        //---
        //--- TaskPane
        public class TaskPaneControl : UserControl
        {
        }
        //---
        //--- CommandGroup
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
        //---
        //--- TaskPane
        public enum TaskPaneCommands_e
        {
            Command1
        }
        //---
        //--- DocHandler
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
        //---
        public override void OnConnect()
        {
            //--- CommandGroup
            this.CommandManager.AddCommandGroup<Commands_e>().CommandClick += OnButtonClick;
            //---
            //--- DocHandler
            Application.Documents.RegisterHandler<MyDocumentHandler>();
            //---
            //--- TaskPane
            var taskPane = this.CreateTaskPane<TaskPaneControl, TaskPaneCommands_e>();
            taskPane.ButtonClick += OnTaskPaneCommandClick;
            TaskPaneControl ctrl = taskPane.Control;
            //---
            //--- 3rdParty
            Application.Documents.Active.StreamWriteAvailable += OnWriteToStream;
            //---
        }
        
        //--- CommandGroup
        private void OnButtonClick(Commands_e cmd)
        {
            //handle commands
        }
        //---
        //--- 3rdParty
        private void OnWriteToStream(IXDocument doc)
        {
            const string STREAM_NAME = "CodeStackStream";

            using (var str = doc.OpenStream(STREAM_NAME, true))
            {
                var xmlSer = new XmlSerializer(typeof(string[]));

                xmlSer.Serialize(str, new string[] { "A", "B" });
            }
        }
        //---
        //--- TaskPane
        private void OnTaskPaneCommandClick(TaskPaneCommands_e cmd)
        {
            switch (cmd)
            {
                case TaskPaneCommands_e.Command1:
                    //handle command
                    break;
            }
        }
        //---
    }
}
