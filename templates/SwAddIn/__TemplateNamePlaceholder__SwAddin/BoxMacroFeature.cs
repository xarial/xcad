using __TemplateNamePlaceholder__SwAddin.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.Documents;
using Xarial.XCad.Exceptions;
using Xarial.XCad.Features.CustomFeature;
using Xarial.XCad.Features.CustomFeature.Attributes;
using Xarial.XCad.Features.CustomFeature.Delegates;
using Xarial.XCad.Features.CustomFeature.Enums;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Features.CustomFeature;
using Xarial.XCad.SolidWorks.Geometry;

namespace __TemplateNamePlaceholder__SwAddin
{
    public class UserException : Exception, IUserException
    {
        public UserException(string msg) : base(msg)
        {
        }
    }

    public class BoxMacroFeatureData
    {
        public IXEntity PlaneOrFace { get; set; }
        
        [ParameterDimension(CustomFeatureDimensionType_e.Linear)]
        public double Width { get; set; } = 0.1;

        [ParameterDimension(CustomFeatureDimensionType_e.Linear)]
        public double Height { get; set; } = 0.1;

        [ParameterDimension(CustomFeatureDimensionType_e.Linear)]
        public double Length { get; set; } = 0.1;
    }

    [ComVisible(true)]
    [Guid("4F6D68F7-65C5-42CE-9F7E-30470FE1ED4B")]
    [Icon(typeof(Resources), nameof(Resources.box_icon))]
    [Title("Box")]
    public class BoxMacroFeature : SwMacroFeatureDefinition<BoxMacroFeatureData, BoxPropertyPage>
    {
        public override BoxMacroFeatureData ConvertPageToParams(IXApplication app, IXDocument doc, BoxPropertyPage page, BoxMacroFeatureData cudData)
            => new BoxMacroFeatureData()
            {
                Height = page.Parameters.Height,
                Length = page.Parameters.Length,
                Width = page.Parameters.Width,
                PlaneOrFace = page.Location.PlaneOrFace,
            };

        public override BoxPropertyPage ConvertParamsToPage(IXApplication app, IXDocument doc, BoxMacroFeatureData par)
        {
            var page = new BoxPropertyPage();
            page.Parameters.Height = par.Height;
            page.Parameters.Width = par.Width;
            page.Parameters.Length = par.Length;
            page.Location.PlaneOrFace = par.PlaneOrFace;
            return page;
        }

        public override ISwBody[] CreateGeometry(ISwApplication app, ISwDocument model, BoxMacroFeatureData data,
            out AlignDimensionDelegate<BoxMacroFeatureData> alignDim)
        {
            var face = data.PlaneOrFace;

            Point pt;
            Vector dir;
            Vector refDir;

            if (face is IXPlanarRegion)
            {
                var transform = face.GetRelativeTransform(model);

                var plane = ((IXPlanarRegion)face).Plane;

                pt = plane.Point.Transform(transform);
                dir = plane.Normal.Transform(transform);
                refDir = plane.Reference.Transform(transform);
            }
            else 
            {
                throw new UserException("Select planar face or plane for the location");
            }

            var box = (ISwBody)app.MemoryGeometryBuilder.CreateSolidBox(
                pt, dir, refDir,
                data.Width, data.Length, data.Height).Bodies.First();

            var secondRefDir = refDir.Cross(dir);

            alignDim = (n, d) =>
            {
                switch (n)
                {
                    case nameof(BoxMacroFeatureData.Width):
                        this.AlignLinearDimension(d,
                            pt
                            .Move(refDir * -1, data.Width / 2)
                            .Move(secondRefDir * -1, data.Length / 2),
                            refDir);
                        break;

                    case nameof(BoxMacroFeatureData.Length):
                        this.AlignLinearDimension(d,
                            pt
                            .Move(refDir, data.Width / 2)
                            .Move(secondRefDir * -1, data.Length / 2),
                            secondRefDir);
                        break;

                    case nameof(BoxMacroFeatureData.Height):
                        this.AlignLinearDimension(d,
                            pt
                            .Move(refDir, data.Width / 2)
                            .Move(secondRefDir * -1, data.Length / 2),
                            dir);
                        break;
                }
            };


            return new ISwBody[] { box };
        }
    }
}
