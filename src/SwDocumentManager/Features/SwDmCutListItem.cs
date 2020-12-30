//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
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
using Xarial.XCad.Features;
using Xarial.XCad.Geometry;
using Xarial.XCad.SwDocumentManager.Data;
using Xarial.XCad.SwDocumentManager.Geometry;

namespace Xarial.XCad.SwDocumentManager.Features
{
    public interface ISwDmCutListItem : ISwDmObject, IXCutListItem
    {
        new ISwDmCustomPropertiesCollection Properties { get; }
        ISwDMCutListItem2 CutListItem { get; }
    }

    internal class SwDmCutListItem : SwDmObject, ISwDmCutListItem
    {
        #region Not Supported
        
        public IXDimensionRepository Dimensions => throw new NotSupportedException();

        public Color? Color
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public void Commit(CancellationToken cancellationToken) => throw new NotSupportedException();
        public void Select(bool append) => throw new NotSupportedException();

        #endregion

        IXPropertyRepository IPropertiesOwner.Properties => Properties;

        public ISwDMCutListItem2 CutListItem { get; }

        private readonly Lazy<ISwDmCustomPropertiesCollection> m_Properties;

        internal SwDmCutListItem(ISwDMCutListItem2 cutListItem) : base(cutListItem)
        {
            CutListItem = cutListItem;
            m_Properties = new Lazy<ISwDmCustomPropertiesCollection>(
                () => new SwDmCutListCustomPropertiesCollection(this));
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

        public bool IsCommitted => true;
                
        public ISwDmCustomPropertiesCollection Properties => m_Properties.Value;
    }
}
