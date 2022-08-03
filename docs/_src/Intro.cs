using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Enums;
using Xarial.XCad.Features;
using Xarial.XCad.Features.CustomFeature.Delegates;
using Xarial.XCad.Features.CustomFeature.Structures;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Enums;
using Xarial.XCad.SolidWorks.Features.CustomFeature;
using Xarial.XCad.SolidWorks.Geometry;
using Xarial.XCad.SolidWorks.UI.PropertyPage;
using Xarial.XCad.SwDocumentManager;
using Xarial.XCad.UI.Commands;
using Xarial.XCad.UI.PropertyPage;
using Xarial.XCad.UI.PropertyPage.Enums;

namespace Xarial.XCad.Documentation
{
    [ComVisible(true)]
    public class XCadAddIn : SwAddInEx
    {
        public enum Commands_e
        {
            Command1,
            Command2
        }

        public override void OnConnect()
        {
            this.CommandManager.AddCommandGroup<Commands_e>().CommandClick += OnCommandsButtonClick;
        }

        private void OnCommandsButtonClick(Commands_e cmd)
        {
            //TODO: handle the button click
        }
    }

    [ComVisible(true)]
    public class IntroPmpPageAddIn : SwAddInEx
    {
        [ComVisible(true)]
        public class MyPMPageData : SwPropertyManagerPageHandler
        {
            public string Text { get; set; }
            public int Number { get; set; }
            public IXComponent Component { get; set; }
        }

        private enum Commands_e
        {
            ShowPmpPage
        }

        private IXPropertyPage<MyPMPageData> m_Page;
        private MyPMPageData m_Data = new MyPMPageData();

        public override void OnConnect()
        {
            m_Page = this.CreatePage<MyPMPageData>();
            m_Page.Closed += OnPageClosed;
            this.CommandManager.AddCommandGroup<Commands_e>().CommandClick += ShowPmpPage;
        }

        private void ShowPmpPage(Commands_e cmd)
        {
            m_Page.Show(m_Data);
        }

        private void OnPageClosed(PageCloseReasons_e reason)
        {
            Debug.Print($"Text: {m_Data.Text}");
            Debug.Print($"Number: {m_Data.Number}");
            Debug.Print($"Selection component name: {m_Data.Component.Name}");
        }
    }

    [ComVisible(true)]
    public class IntroMacroFeatureAddIn : SwAddInEx 
    {
        [ComVisible(true)]
        public class BoxData : SwPropertyManagerPageHandler
        {
            public double Width { get; set; }
            public double Length { get; set; }
            public double Height { get; set; }
        }

        [ComVisible(true)]
        public class BoxMacroFeature : SwMacroFeatureDefinition<BoxData, BoxData>
        {
            public override ISwBody[] CreateGeometry(ISwApplication app, ISwDocument model, BoxData data)
            {
                var body = (ISwBody)app.MemoryGeometryBuilder.CreateSolidBox(new Point(0, 0, 0),
                    new Vector(1, 0, 0), new Vector(0, 1, 0),
                    data.Width, data.Length, data.Height).Bodies.First();

                return new ISwBody[] { body };
            }

        }

        public enum Commands_e
        {
            InsertMacroFeature,
        }

        public override void OnConnect()
        {
            this.CommandManager.AddCommandGroup<Commands_e>().CommandClick += OnCommandsButtonClick;
        }

        private void OnCommandsButtonClick(Commands_e cmd)
        {
            switch (cmd) 
            {
                case Commands_e.InsertMacroFeature:
                    Application.Documents.Active.Features.CreateCustomFeature<BoxMacroFeature, BoxData, BoxData>();
                    break;
            }
        }
    }

    public class InitStandAlone 
    {
        static void Main(string[] args)
        {
            var assmFilePath = @"C:\sample-assembly.sldasm";

            //print assembly components using SOLIDWORKS API
            var swApp = SwApplicationFactory.Create(SwVersion_e.Sw2022, ApplicationState_e.Silent);
            PrintAssemblyComponents(swApp, assmFilePath);

            //print assembly components using SOLIDWORKS Document Manager API
            var swDmApp = SwDmApplicationFactory.Create("[Document Manager Lincese Key]");
            PrintAssemblyComponents(swDmApp, assmFilePath);
        }

        //CAD-agnostic function to open assembly, print all components and close assembly
        private static void PrintAssemblyComponents(IXApplication app, string filePath) 
        {
            using (var assm = app.Documents.Open(filePath, DocumentState_e.ReadOnly))
            {
                IterateComponentsRecursively(((IXAssembly)assm).Configurations.Active.Components, 0);
            }
        }

        private static void IterateComponentsRecursively(IXComponentRepository compsRepo, int level) 
        {
            foreach (var comp in compsRepo)
            {
                Console.WriteLine(Enumerable.Repeat("  ", level) + comp.Name);

                IterateComponentsRecursively(comp.Children, level + 1);
            }
        }
    }
}
