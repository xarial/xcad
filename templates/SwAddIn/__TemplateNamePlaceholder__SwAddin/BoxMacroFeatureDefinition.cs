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
using Xarial.XCad.Features.CustomFeature.Delegates;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Features.CustomFeature;
using Xarial.XCad.SolidWorks.Geometry;

namespace __TemplateNamePlaceholder__.Sw.AddIn
{
    public class UserException : Exception, IUserException
    {
        public UserException(string msg) : base(msg)
        {
        }
    }

    [ComVisible(true)]
    [Guid("4F6D68F7-65C5-42CE-9F7E-30470FE1ED4B")]
    [Icon(typeof(Resources), nameof(Resources.box_icon))]
    [Title("Box")]//TitleAttribute allows to specify the default (base) name of the feature in the feature manager tree
    public class BoxMacroFeatureDefinition : SwMacroFeatureDefinition<BoxMacroFeatureData, BoxPropertyPage>
    {
        //converting data model from the page to feature data
        //in some cases page and feature data can be of the same class and the conversion is not required
        //this method will be called when user changes the parameters in the property manager page
        public override BoxMacroFeatureData ConvertPageToParams(IXApplication app, IXDocument doc, BoxPropertyPage page, BoxMacroFeatureData cudData)
            => new BoxMacroFeatureData()
            {
                Height = page.Parameters.Height,
                Length = page.Parameters.Length,
                Width = page.Parameters.Width,
                PlaneOrFace = page.Location.PlaneOrFace,
            };

        //converting feature data to the property page
        //this method will be called when existing feature definiton is edited
        public override BoxPropertyPage ConvertParamsToPage(IXApplication app, IXDocument doc, BoxMacroFeatureData par)
        {
            var page = new BoxPropertyPage();
            page.Parameters.Height = par.Height;
            page.Parameters.Width = par.Width;
            page.Parameters.Length = par.Length;
            page.Location.PlaneOrFace = par.PlaneOrFace;
            return page;
        }
        
        //this method is called when feature is being inserted and user changes the parameters of the property page (preview purposes)
        //this method will also be called when macro feature is regenerated to create a macro feature body
        //in most cases the procedure of creating the preview body and the generated body is the same
        //but it is also possible to provide custom preview geometry by overriding the CreatePreviewGeometry method
        public override ISwBody[] CreateGeometry(ISwApplication app, ISwDocument model, BoxMacroFeatureData data,
            out AlignDimensionDelegate<BoxMacroFeatureData> alignDim)
        {
            var face = data.PlaneOrFace;

            Point pt;
            Vector dir;
            Vector refDir;

            if (face is IXPlanarRegion)
            {
                var plane = ((IXPlanarRegion)face).Plane;

                pt = plane.Point;
                dir = plane.Normal;
                refDir = plane.Reference;
            }
            else //it is only possible to create geometry if planar face or plane is selected
            {
                //it is required to throw the exception which implements the IUserException
                //so this error is displayed to the user
                throw new UserException("Select planar face or plane for the location");
            }

            //creating a temp body of the box by providing the center point, direction vectors and size
            var box = (ISwBody)app.MemoryGeometryBuilder.CreateSolidBox(
                pt, dir, refDir,
                data.Width, data.Length, data.Height).Bodies.First();

            var secondRefDir = refDir.Cross(dir);

            //aligning dimensions. For linear dimensions it is required to specify the origin point and the direction
            //see https://xcad.xarial.com/custom-features/data/dimensions/
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
