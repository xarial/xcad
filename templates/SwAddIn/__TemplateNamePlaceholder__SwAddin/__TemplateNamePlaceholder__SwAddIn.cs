#if _AddCommandManager_ || _AddPropertyPage_ || _AddCustomFeature_
using __TemplateNamePlaceholder__SwAddin;
using __TemplateNamePlaceholder__SwAddin.Properties;
#endif
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Xarial.XCad.Base;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.Base.Enums;
using Xarial.XCad.Features;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.UI.Commands;
using Xarial.XCad.UI.Commands.Attributes;
using Xarial.XCad.UI.Commands.Enums;
using Xarial.XCad.UI.PropertyPage;
using Xarial.XCad.UI.PropertyPage.Enums;

namespace __TemplateNamePlaceholder__.Sw.AddIn
{
    [ComVisible(true)]
    [Guid("E82CC06F-81FC-4CE6-B88F-5B65023808F9")]
    [Title("__TemplateNamePlaceholder__ SOLIDWORKS Add-In")]
    [Description("SOLIDWORKS add-in created with xCAD.NET")]
    public class __TemplateNamePlaceholder__SwAddIn : SwAddInEx
    {
#if _AddCommandManager_ || _AddPropertyPage_ || _AddCustomFeature_
        [Title("__TemplateNamePlaceholder__")]
        [Description("Commands of __TemplateNamePlaceholder__")]
        private enum Commands_e
        {
#if _AddCommandManager_ || _AddPropertyPage_
            [Icon(typeof(Resources), nameof(Resources.box_icon))]
            [Title("Create Box")]
            [Description("Creates box using standard feature")]
            [CommandItemInfo(WorkspaceTypes_e.Part | WorkspaceTypes_e.InContextPart)]
            CreateBox,

#endif
#if _AddCustomFeature_
            [Icon(typeof(Resources), nameof(Resources.box_icon))]
            [Title("Create Parametric Box")]
            [Description("Creates parametric macro feature")]
            [CommandItemInfo(WorkspaceTypes_e.Part | WorkspaceTypes_e.InContextPart)]
            CreateParametricBox,

#endif
            [Icon(typeof(Resources), nameof(Resources.about_icon))]
            [Title("About...")]
            [Description("Shows About Box")]
            About
        }

#endif
#if _AddPropertyPage_
        private IXPropertyPage<BoxPropertyPage> m_BoxPage;
        private BoxPropertyPage m_BoxData;

#endif
        public override void OnConnect()
        {
#if _AddCommandManager_ || _AddPropertyPage_ || _AddCustomFeature_
            CommandManager.AddCommandGroup<Commands_e>().CommandClick += OnCommandClick;
#if _AddPropertyPage_
            m_BoxPage = CreatePage<BoxPropertyPage>();
            m_BoxData = new BoxPropertyPage();
            m_BoxPage.Closed += OnBoxPageClosed;
#endif
#else
            Application.ShowMessageBox("Hello, __TemplateNamePlaceholder__! xCAD.NET", MessageBoxIcon_e.Info);
#endif
        }

#if _AddCommandManager_ || _AddPropertyPage_ || _AddCustomFeature_

        private void OnCommandClick(Commands_e spec)
        {
            switch (spec)
            {
#if _AddCommandManager_ || _AddPropertyPage_
                case Commands_e.CreateBox:
#if _AddPropertyPage_
                    m_BoxPage.Show(m_BoxData);
#else
                    CreateBox(100, 200, 300);
#endif
                    break;

#endif
#if _AddCustomFeature_
                case Commands_e.CreateParametricBox:
                    break;

#endif
                case Commands_e.About:
                    Application.ShowMessageBox("About __TemplateNamePlaceholder__", MessageBoxIcon_e.Info);
                    break;
            }
        }
#endif
#if _AddPropertyPage_
        private void OnBoxPageClosed(PageCloseReasons_e reason)
        {
            if (reason == PageCloseReasons_e.Okay)
            {
                CreateBox(m_BoxData.Parameters.Width, m_BoxData.Parameters.Height, m_BoxData.Parameters.Length);
            }
        }

#endif

#if _AddCommandManager_ || _AddPropertyPage_

        private void CreateBox(double width, double height, double length) 
        {
            var doc = Application.Documents.Active;

            ISwPart part;

            switch (doc) 
            {
                case ISwPart activePart:
                    part = activePart;
                    break;

                case ISwAssembly assm:
                    var editingDoc = assm.EditingComponent?.ReferencedDocument;
                    if (editingDoc is ISwPart)
                    {
                        part = (ISwPart)editingDoc;
                    }
                    else 
                    {
                        throw new NotSupportedException();
                    }
                    break;

                default:
                    throw new NotSupportedException();
            }

            var sketch = part.Features.PreCreate2DSketch();
            sketch.Commit();
        }
#endif
    }
}
