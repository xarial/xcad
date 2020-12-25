using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xarial.XCad.Data;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Geometry;

namespace Xarial.XCad.SolidWorks.Features
{
    public interface ISwCutListItem : IXCutListItem, ISwFeature 
    {
        IBodyFolder CutListBodyFolder { get; }
    }

    internal class SwCutListItem : SwFeature, ISwCutListItem
    {
        internal SwCutListItem(ISwDocument doc, IFeature feat, bool created) : base(doc, feat, created)
        {
            if (feat.GetTypeName2() != "CutListFolder") 
            {
                throw new InvalidCastException("Specified feature is not a cut-list feature");
            }

            CutListBodyFolder = (IBodyFolder)feat.GetSpecificFeature2();
        }

        public IBodyFolder CutListBodyFolder { get; }

        public IXSolidBody[] Bodies 
        {
            get
            {
                var bodies = CutListBodyFolder.GetBodies() as object[];

                if (bodies != null)
                {
                    return bodies.Select(b => SwObjectFactory.FromDispatch<ISwSolidBody>(b, m_Doc)).ToArray();
                }
                else 
                {
                    return new IXSolidBody[0];
                }
            }
        }

        public IXPropertyRepository Properties => throw new NotImplementedException();
    }
}
