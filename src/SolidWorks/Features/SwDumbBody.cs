//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Geometry;

namespace Xarial.XCad.SolidWorks.Features
{
    public interface ISwDumbBody : IXDumbBody, ISwFeature
    {
    }

    internal class SwDumbBody : SwFeature, ISwDumbBody
    {
        internal SwDumbBody(IFeature feat, SwDocument doc, SwApplication app, bool created) : base(feat, doc, app, created)
        {
        }

        public IXBody Body 
        {
            get 
            {
                if (IsCommitted)
                {
                    var face = (IFace2)((object[])Feature.GetFaces()).First();
                    var body = face.IGetBody();
                    return OwnerDocument.CreateObjectFromDispatch<ISwBody>(body);
                }
                else 
                {
                    return m_Creator.CachedProperties.Get<IXBody>();
                }
            }
            set 
            {
                if (value == null) 
                {
                    throw new ArgumentNullException("Body cannot be null");
                }

                if (!(value is ISwTempBody)) 
                {
                    throw new InvalidCastException("Only temp bodies can be set to the feature");
                }

                if (IsCommitted)
                {
                    if (!Feature.SetBody2(((ISwBody)value).Body, true)) 
                    {
                        throw new Exception("Failed to chnage the feature body");
                    }
                }
                else 
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        protected override IFeature InsertFeature(CancellationToken cancellationToken)
        {
            IPartDoc part = null;

            if (OwnerDocument is ISwPart)
            {
                part = ((ISwPart)OwnerDocument).Part;
            }
            else if (OwnerDocument is ISwAssembly) 
            {
                var editComp = ((ISwAssembly)OwnerDocument).EditingComponent;

                if (editComp != null) 
                {
                    if (editComp.ReferencedDocument is ISwPart) 
                    {
                        part = ((ISwPart)editComp.ReferencedDocument).Part;
                    }
                }
            }

            if (part != null)
            {
                var feat = (IFeature)part.CreateFeatureFromBody3(((ISwBody)Body).Body, false, (int)swCreateFeatureBodyOpts_e.swCreateFeatureBodySimplify);

                if (feat != null)
                {
                    return feat;
                }
                else 
                {
                    throw new Exception("Failed to create feature from body");
                }
            }
            else 
            {
                throw new Exception("This feature can only be inserted into a part");
            }
        }
    }
}
