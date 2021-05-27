//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.swdocumentmgr;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Xarial.XCad.Annotations;
using Xarial.XCad.Data;
using Xarial.XCad.Enums;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry;
using Xarial.XCad.SwDocumentManager.Data;
using Xarial.XCad.SwDocumentManager.Documents;
using Xarial.XCad.SwDocumentManager.Geometry;

namespace Xarial.XCad.SwDocumentManager.Features
{
    public interface ISwDmCutListItem : ISwDmObject, IXCutListItem
    {
        new ISwDmCustomPropertiesCollection Properties { get; }
        ISwDMCutListItem2 CutListItem { get; }
    }

    internal class SwDmCutListItem : SwDmSelObject, ISwDmCutListItem
    {
        #region Not Supported
        
        public IXDimensionRepository Dimensions => throw new NotSupportedException();

        public Color? Color
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        #endregion

        IXPropertyRepository IPropertiesOwner.Properties => Properties;

        public ISwDMCutListItem2 CutListItem { get; }

        private readonly Lazy<ISwDmCustomPropertiesCollection> m_Properties;
        private readonly SwDmDocument3D m_Doc;
        private readonly SwDmConfiguration m_Conf;

        internal SwDmCutListItem(ISwDMCutListItem2 cutListItem, SwDmDocument3D doc) : base(cutListItem)
        {
            CutListItem = cutListItem;
            m_Doc = doc;
            
            m_Properties = new Lazy<ISwDmCustomPropertiesCollection>(
                () => new SwDmCutListCustomPropertiesCollection(this, m_Doc, m_Conf));
        }

        internal SwDmCutListItem(ISwDMCutListItem2 cutListItem, SwDmDocument3D doc, SwDmConfiguration conf) : this(cutListItem, doc)
        {
            m_Conf = conf;
        }

        public IXSolidBody[] Bodies 
        {
            get 
            {
                var bodies = new IXSolidBody[CutListItem.Quantity];

                for (int i = 0; i < bodies.Length; i++) 
                {
                    bodies[i] = new SwDmSolidBody();
                }

                return bodies;
            }
        }

        public string Name 
        {
            get => CutListItem.Name; 
            set => CutListItem.Name = value; 
        }

        public ISwDmCustomPropertiesCollection Properties => m_Properties.Value;

        public CutListState_e State 
        {
            get 
            {
                if (m_Doc.SwDmApp.IsVersionNewerOrEqual(SwDmVersion_e.Sw2021))
                {
                    var cutListStatus = (CutListItem as ISwDMCutListItem4).ExcludeFromCutlist;

                    if (cutListStatus == swDMCutListExclusionStatus_e.swDMCutListStatus_Excluded)
                    {
                        return CutListState_e.ExcludeFromBom;
                    }
                    else if (cutListStatus == swDMCutListExclusionStatus_e.swDMCutListStatus_Included)
                    {
                        return 0;
                    }
                    else
                    {
                        throw new Exception("Failed to extract the BOM status. Save document in SW 2021 or newer");
                    }
                }
                else 
                {
                    throw new NotSupportedException("This API is available in SW 2021 or newer");
                }
            }
        }
    }
}
