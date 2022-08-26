//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.SolidWorks.Documents;
using SolidWorks.Interop.sldworks;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry;
using SolidWorks.Interop.swconst;
using Xarial.XCad.SolidWorks.Geometry;

namespace Xarial.XCad.SolidWorks.Features
{
    public interface ISwFlatPattern : IXFlatPattern, ISwFeature
    {
    }

    internal class SwFlatPattern : SwFeature, ISwFlatPattern
    {
        internal SwFlatPattern(IFeature feat, SwDocument doc, SwApplication app, bool created) : base(feat, doc, app, created)
        {
        }

        public bool IsFlattened 
        {
            get 
            {
                var suppStates = (bool[])Feature.IsSuppressed2((int)swInConfigurationOpts_e.swThisConfiguration, null);

                return !suppStates[0];
            }
            set 
            {
                if (Feature.Select2(false, -1))
                {
                    if (!Feature.SetSuppression2(value
                        ? (int)swFeatureSuppressionAction_e.swUnSuppressFeature
                        : (int)swFeatureSuppressionAction_e.swSuppressFeature,
                        (int)swInConfigurationOpts_e.swThisConfiguration, null))
                    {
                        throw new Exception("Failed to change the flattened state of the feature");
                    }
                }
                else 
                {
                    throw new Exception("Failed to select feature");
                }
            }
        }

        public IXEntity FixedEntity 
        {
            get 
            {
                var featData = (FlatPatternFeatureData)Feature.GetDefinition();

                return OwnerDocument.CreateObjectFromDispatch<ISwEntity>(featData.FixedFace2);
            }
        }
    }
}