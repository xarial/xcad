//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad;
using Xarial.XCad.Documents;
using Xarial.XCad.Features.CustomFeature;
using Xarial.XCad.Features.CustomFeature.Attributes;
using Xarial.XCad.Features.CustomFeature.Delegates;
using Xarial.XCad.Features.CustomFeature.Enums;
using Xarial.XCad.Features.CustomFeature.Services;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Features.CustomFeature;
using Xarial.XCad.SolidWorks.Geometry;
using Xarial.XCad.SolidWorks.UI.PropertyPage;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Enums;
using Xarial.XCad.UI.PropertyPage.Services;
using Xarial.XCad.UI.PropertyPage.Structures;

namespace SwAddInExample
{
    [PageOptions(PageOptions_e.OkayButton | PageOptions_e.PushpinButton | PageOptions_e.CancelButton | PageOptions_e.LockedPage)]
    public class BoxPage
    {
        public BoxParametersPage Parameters { get; set; }

        public BoxPage(string dummy) 
        {
            Parameters = new BoxParametersPage();
        }
    }

    public class BoxParametersPage : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public class SampleSelectionFilter : ISelectionCustomFilter
        {
            public void Filter(IControl selBox, IXSelObject selection, SelectionCustomFilterArguments args)
            {
                var val = selBox.GetValue();
            }
        }
        
        public IXFace BaseFace { get; set; }

        [SelectionBoxOptions(typeof(SampleSelectionFilter))]
        public List<IXFace> TestFaces { get; set; }

        [NumberBoxOptions(units: NumberBoxUnitType_e.Length, 0, 1000, 0.01, false, 0.1, 0.001)]
        public double Width { get; set; }

        [NumberBoxOptions(units: NumberBoxUnitType_e.Length, 0, 1000, 0.01, false, 0.1, 0.001)]
        public double Height { get; set; }

        [NumberBoxOptions(units: NumberBoxUnitType_e.Length, 0, 1000, 0.01, false, 0.1, 0.001)]
        public double Length { get; set; }

        internal void Reset()
        {
            Width = 0.01;
            Height = 0.02;
            Length = 0.03;
            TestFaces = null;
            BaseFace = null;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BaseFace)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TestFaces)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Width)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Height)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Length)));
        }
    }

    public class BoxMacroFeatureData
    {   
        public IXFace BaseFace { get; set; }

        //[ParameterExclude]
        public List<IXFace> TestFaces { get; set; }

        public double Width { get; set; } = 0.1;
        public double Height { get; set; } = 0.2;

        [ParameterDimension(CustomFeatureDimensionType_e.Linear)]
        public double Length { get; set; } = 0.3;
    }

    [ComVisible(true)]
    public class BoxMacroFeatureEditor : SwMacroFeatureDefinition<BoxMacroFeatureData, BoxPage>
    {
        public BoxMacroFeatureEditor()
        {
            this.PostRebuild += OnPostRebuild;
        }

        public override BoxMacroFeatureData ConvertPageToParams(IXApplication app, IXDocument doc, BoxPage page, BoxMacroFeatureData cudData)
            => new BoxMacroFeatureData()
            {
                Height = page.Parameters.Height,
                Length = page.Parameters.Length,
                Width = page.Parameters.Width,
                BaseFace = page.Parameters.BaseFace,
                TestFaces = page.Parameters.TestFaces
            };

        public override BoxPage ConvertParamsToPage(IXApplication app, IXDocument doc, BoxMacroFeatureData par)
            => new BoxPage("")
            {
                Parameters = new BoxParametersPage()
                {
                    Height = par.Height,
                    Length = par.Length,
                    Width = par.Width,
                    BaseFace = par.BaseFace,
                    TestFaces = par.TestFaces
                }
            };

        public override ISwBody[] CreateGeometry(ISwApplication app, ISwDocument model,
            BoxMacroFeatureData data, out AlignDimensionDelegate<BoxMacroFeatureData> alignDim)
        {
            var face = data.BaseFace;

            Xarial.XCad.Geometry.Structures.Point pt;
            Vector dir;
            Vector refDir;

            if (face is IFaultObject)
            {
                throw new UserException("Base face is a fault entity");
            }
            else if (face is IXPlanarFace)
            {
                var plane = ((IXPlanarFace)face).Plane;
                pt = face.FindClosestPoint(plane.Point);
                dir = plane.Normal * (face.Sense ? 1 : -1);
                refDir = plane.Reference;
            }
            else if (face == null)
            {
                pt = new Xarial.XCad.Geometry.Structures.Point(0, 0, 0);
                dir = new Vector(0, 0, 1);
                refDir = new Vector(1, 0, 0);
            }
            else 
            {
                throw new NotSupportedException();
            }

            var box = (ISwBody)app.MemoryGeometryBuilder.CreateSolidBox(
                pt, dir, refDir,
                data.Width, data.Height, data.Length).Bodies.First();

            alignDim = new AlignDimensionDelegate<BoxMacroFeatureData>((p, d) => 
            {
                if (string.Equals(p, nameof(BoxMacroFeatureData.Length))) 
                {
                    this.AlignLinearDimension(d, pt, dir);
                }
            });

            return new ISwBody[] { box };
        }

        protected override BoxMacroFeatureData HandleEditingException(IXCustomFeature<BoxMacroFeatureData> feat, Exception ex)
        {
            return new BoxMacroFeatureData();
        }

        public override ISwBody[] CreatePreviewGeometry(ISwApplication app, ISwDocument model, BoxMacroFeatureData data, BoxPage page,
            out ShouldHidePreviewEditBodyDelegate<BoxMacroFeatureData, BoxPage> shouldHidePreviewEdit, out AssignPreviewBodyColorDelegate assignPreviewColor)
        {
            shouldHidePreviewEdit = null;
            assignPreviewColor = AssignPreviewBodyColor;
            return CreateGeometry(app, model, data, out _);
        }

        private void AssignPreviewBodyColor(IXBody body, out Color color)
            => color = Color.FromArgb(100, Color.Green);

        private void OnPostRebuild(ISwApplication app, ISwDocument model, ISwMacroFeature<BoxMacroFeatureData> feature, BoxMacroFeatureData parameters)
        { 
        }

        public override void OnFeatureInserted(IXApplication app, IXDocument doc, IXCustomFeature<BoxMacroFeatureData> feat, BoxMacroFeatureData data, BoxPage page)
        {
            page.Parameters.Reset();
            doc.Selections.Clear();
        }
    }
}