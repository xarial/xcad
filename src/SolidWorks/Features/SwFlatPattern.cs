//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
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
using Xarial.XCad.Documents;

namespace Xarial.XCad.SolidWorks.Features
{
    /// <summary>
    /// SOLIDWORKS-specific flat pattern feature
    /// </summary>
    public interface ISwFlatPattern : IXFlatPattern, ISwFeature
    {
    }

    internal class SwFlatPattern : SwFeature, ISwFlatPattern
    {
        internal const string TypeName = "FlatPattern";

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
                if (!Feature.SetSuppression2(value
                    ? (int)swFeatureSuppressionAction_e.swUnSuppressFeature
                    : (int)swFeatureSuppressionAction_e.swSuppressFeature,
                    (int)swInConfigurationOpts_e.swThisConfiguration, null))
                {
                    throw new Exception("Failed to change the flattened state of the feature");
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

        public override ISwBody Body => (ISwBody)FixedEntity.Body;

        public IFlatPatternSaveOperation PreCreateSaveAsOperation(string filePath)
        {
            if (OwnerDocument is SwPart)
            {
                return new SwFlatPatternSaveOperation((SwPart)OwnerDocument, this, filePath);
            }
            else 
            {
                throw new NotSupportedException($"Operation can be created for features in the part context only");
            }
        }
    }
}