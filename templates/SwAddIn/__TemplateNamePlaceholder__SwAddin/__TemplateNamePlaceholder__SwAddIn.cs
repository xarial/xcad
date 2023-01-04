#if _AddCommandManager_ || _AddPropertyPage_ || _AddCustomFeature_
using __TemplateNamePlaceholder__SwAddin;
using __TemplateNamePlaceholder__SwAddin.Properties;
using SolidWorks.Interop.swconst;
#endif
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using Xarial.XCad.Base;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.Base.Enums;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.UI.Commands;
using Xarial.XCad.UI.Commands.Attributes;
using Xarial.XCad.UI.Commands.Enums;
using Xarial.XCad.UI.PropertyPage;
using Xarial.XCad.UI.PropertyPage.Enums;
using Xarial.XCad.UI.PropertyPage.Structures;

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
            [CommandItemInfo(true, true, WorkspaceTypes_e.Part | WorkspaceTypes_e.InContextPart, true)]
            CreateBox,

#endif
#if _AddCustomFeature_
            [Icon(typeof(Resources), nameof(Resources.parametric_box_icon))]
            [Title("Create Parametric Box")]
            [Description("Creates parametric macro feature")]
            [CommandItemInfo(true, true, WorkspaceTypes_e.Part | WorkspaceTypes_e.InContextPart, true)]
            CreateParametricBox,

#endif
            [Icon(typeof(Resources), nameof(Resources.about_icon))]
            [Title("About...")]
            [Description("Shows About Box")]
            [CommandItemInfo(true, false, WorkspaceTypes_e.All)]
            About
        }

#endif
#if _AddPropertyPage_
        private IXPropertyPage<BoxPropertyPage> m_BoxPage;
        private BoxPropertyPage m_BoxData;

#endif
        public override void OnConnect()
        {
            System.Diagnostics.Debugger.Launch();
#if _AddCommandManager_ || _AddPropertyPage_ || _AddCustomFeature_
            CommandManager.AddCommandGroup<Commands_e>().CommandClick += OnCommandClick;
#if _AddPropertyPage_
            m_BoxPage = CreatePage<BoxPropertyPage>();
            m_BoxData = new BoxPropertyPage();
            m_BoxPage.Closing += OnBoxPageClosing;
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
                    var frontPlane = Application.Documents.Active.Features.OfType<IXPlane>().First();
                    CreateBox(frontPlane, 100, 200, 300);
#endif
                    break;

#endif
#if _AddCustomFeature_
                case Commands_e.CreateParametricBox:
                    Application.Documents.Active.Features.CreateCustomFeature<BoxMacroFeature, BoxMacroFeatureData, BoxPropertyPage>();
                    break;

#endif
                case Commands_e.About:
                    Application.ShowMessageBox("About __TemplateNamePlaceholder__", MessageBoxIcon_e.Info);
                    break;
            }
        }
#endif
#if _AddPropertyPage_
        private void OnBoxPageClosing(PageCloseReasons_e reason, PageClosingArg arg)
        {
            if (reason == PageCloseReasons_e.Okay)
            {
                if (!(m_BoxData.Location.PlaneOrFace is IXPlanarRegion))
                {
                    arg.Cancel = true;
                    arg.ErrorMessage = "Specify plane or face to place box on";
                }
            }
        }

        private void OnBoxPageClosed(PageCloseReasons_e reason)
        {
            if (reason == PageCloseReasons_e.Okay)
            {
                CreateBox((IXPlanarRegion)m_BoxData.Location.PlaneOrFace,
                    m_BoxData.Parameters.Width, m_BoxData.Parameters.Height, m_BoxData.Parameters.Length);
            }
        }
#endif
#if _AddCommandManager_ || _AddPropertyPage_

        private void CreateBox(IXPlanarRegion refEnt, double width, double height, double length) 
        {
            var doc = Application.Documents.Active;

            using (doc.ModelViews.Active.Freeze(true))
            {
                var sketch = doc.Features.PreCreate2DSketch();
                sketch.ReferenceEntity = refEnt;
                var rect = sketch.Entities.PreCreateRectangle(new Point(0, 0, 0), width, length, new Vector(1, 0, 0), new Vector(0, 1, 0));
                sketch.Entities.AddRange(rect);
                sketch.Commit();

                sketch.Select(false);

                var extrFeat = doc.Model.FeatureManager.FeatureExtrusion3(true, false, false,
                    (int)swEndConditions_e.swEndCondBlind, (int)swEndConditions_e.swEndCondBlind, height, 0, false, false, false,
                    false, 0, 0, false, false, false, false, true, true, true,
                    (int)swStartConditions_e.swStartSketchPlane, 0, false);

                if (extrFeat == null)
                {
                    throw new Exception("Failed to create extrude feature");
                }
            }
        }
#endif
    }
}
