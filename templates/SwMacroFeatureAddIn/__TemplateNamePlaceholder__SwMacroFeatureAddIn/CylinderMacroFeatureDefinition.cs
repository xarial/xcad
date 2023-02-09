using __TemplateNamePlaceholder__SwMacroFeatureAddIn.Properties;
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
using Xarial.XCad.Features.CustomFeature.Structures;
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
    [Guid("4EDF005F-D48D-41C5-A1F7-B9D103043734")]
    [Icon(typeof(Resources), nameof(Resources.cylinder_icon))]
    [Title("Cylinder")]//TitleAttribute allows to specify the default (base) name of the feature in the feature manager tree
#if _AddEditor_
    public class CylinderMacroFeatureDefinition : SwMacroFeatureDefinition<CylinderMacroFeatureData, CylinderPropertyPage>
#else
    public class CylinderMacroFeatureDefinition : SwMacroFeatureDefinition<CylinderMacroFeatureData>
#endif
    {
#if _AddEditor_
        //converting data model from the page to feature data
        //in some cases page and feature data can be of the same class and the conversion is not required
        //this method will be called when user changes the parameters in the property manager page
        public override CylinderMacroFeatureData ConvertPageToParams(IXApplication app, IXDocument doc, CylinderPropertyPage page, CylinderMacroFeatureData cudData)
            => new CylinderMacroFeatureData()
            {
                Height = page.Parameters.Height,
                Radius = page.Parameters.Radius,
                PlaneOrFace = page.Location.PlaneOrFace,
                Reverse = page.Location.Reverse,
#if _SupportsEditBodies_
                BooleanOptions = page.Location.BooleanOptions,
                EditBody = page.Location.BooleanOptions == BooleanOptions_e.Extrude ? null : page.Location.PlaneOrFace?.Body
#endif
            };

        //converting feature data to the property page
        //this method will be called when existing feature definiton is edited
        public override CylinderPropertyPage ConvertParamsToPage(IXApplication app, IXDocument doc, CylinderMacroFeatureData par)
        {
            var page = new CylinderPropertyPage();
            page.Parameters.Height = par.Height;
            page.Parameters.Radius = par.Radius;
            page.Location.Reverse = par.Reverse; page.Location.PlaneOrFace = par.PlaneOrFace;
#if _SupportsEditBodies_
            page.Location.BooleanOptions = par.BooleanOptions;
#endif
            return page;
        }

#endif
#if !_AddEditor_
        public override CustomFeatureRebuildResult OnRebuild(ISwApplication app, ISwDocument doc,
            ISwMacroFeature<CylinderMacroFeatureData> feature, out AlignDimensionDelegate<CylinderMacroFeatureData> alignDim)
            => CustomFeatureRebuildResult.FromBodies(CreateGeometry(app, doc, feature, out alignDim));
#endif
#if _AddEditor_
        //this method is called when feature is being inserted and user changes the parameters of the property page (preview purposes)
        //this method will also be called when macro feature is regenerated to create a macro feature body
        //in most cases the procedure of creating the preview body and the generated body is the same
        //but it is also possible to provide custom preview geometry by overriding the CreatePreviewGeometry method
        public override ISwBody[] CreateGeometry(ISwApplication app, ISwDocument doc, ISwMacroFeature<CylinderMacroFeatureData> feat,
            out AlignDimensionDelegate<CylinderMacroFeatureData> alignDim)
#else
        public ISwBody[] CreateGeometry(ISwApplication app, ISwDocument doc, ISwMacroFeature<CylinderMacroFeatureData> feat,
            out AlignDimensionDelegate<CylinderMacroFeatureData> alignDim)
#endif
        {
            var data = feat.Parameters;

            var face = data.PlaneOrFace;

            Point pt;
            Vector dir;

            if (face is IXPlanarRegion)
            {
                var plane = ((IXPlanarRegion)face).Plane;

                pt = plane.Point;
                dir = plane.Normal;
            }
            else //it is only possible to create geometry if planar face or plane is selected
            {
                //it is required to throw the exception which implements the IUserException
                //so this error is displayed to the user
                throw new UserException("Select planar face or plane for the location");
            }

            if (data.Reverse) 
            {
                dir *= -1;
            }
#if _SupportsInContext_

            //find the transformation of the entity (if used in the context of the assembly) and transform coordinate and direction
            var entToTargTransform = feat.GetEntityToTargetTransformation(face);

            pt *= entToTargTransform;
            dir *= entToTargTransform;

#endif
            ISwBody[] result;

            //creating a temp body of the cylinder by providing the center point, direction vectors and size
            var cylinder = (ISwTempBody)app.MemoryGeometryBuilder.CreateSolidCylinder(
                pt, dir, data.Radius, data.Height).Bodies.First();
#if _SupportsEditBodies_

            switch (data.BooleanOptions) 
            {
                case BooleanOptions_e.Extrude:
                    //do nothing - return body as is
                    result = new ISwBody[] { cylinder };
                    break;

                case BooleanOptions_e.MergeExtrudeResults:
                    if (data.EditBody == null) 
                    {
                        throw new UserException("Edit body is not specified");
                    }
                    result = new ISwBody[] { ((ISwMacroFeatureEditBody)data.EditBody).Add(cylinder) };
                    break;

                case BooleanOptions_e.Cut:
                    if (data.EditBody == null)
                    {
                        throw new UserException("Edit body is not specified");
                    }
                    result = ((ISwMacroFeatureEditBody)data.EditBody).Substract(cylinder);
                    break;

                default:
                    throw new NotSupportedException();
            }

#else
            result = new ISwBody[] { cylinder };
#endif
#if _AddDimensions_

            //aligning dimensions. Refer https://xcad.xarial.com/custom-features/data/dimensions/ for more information
            alignDim = (n, d) =>
            {
                switch (n)
                {
                    case nameof(CylinderMacroFeatureData.Height):
                        this.AlignLinearDimension(d, pt, dir);
                        break;

                    case nameof(CylinderMacroFeatureData.Radius):
                        this.AlignRadialDimension(d, pt, dir);
                        break;
                }
            };
#endif
            
            return result;
        }
    }
}
