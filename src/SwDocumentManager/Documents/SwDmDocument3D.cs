using SolidWorks.Interop.swdocumentmgr;
using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Documents;
using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.SwDocumentManager.Documents
{
    public interface ISwDmDocument3D : ISwDmDocument, IXDocument3D
    {
        new ISwDmConfigurationCollection Configurations { get; }
    }

    internal class SwDmDocument3D : SwDmDocument, ISwDmDocument3D
    {
        #region Not Supported

        public IXModelViewRepository ModelViews => throw new NotSupportedException();
        public Box3D CalculateBoundingBox() => throw new NotSupportedException();
        TSelObject IXObjectContainer.ConvertObject<TSelObject>(TSelObject obj) => throw new NotSupportedException();

        #endregion

        IXConfigurationRepository IXDocument3D.Configurations => Configurations;

        public ISwDmConfigurationCollection Configurations => m_Configurations.Value;

        private readonly Lazy<ISwDmConfigurationCollection> m_Configurations;

        public SwDmDocument3D(ISwDmApplication dmApp, ISwDMDocument doc, bool isCreated,
            Action<ISwDmDocument> createHandler, Action<ISwDmDocument> closeHandler)
            : base(dmApp, doc, isCreated, createHandler, closeHandler)
        {
            m_Configurations = new Lazy<ISwDmConfigurationCollection>(() => new SwDmConfigurationCollection(this));
        }
    }
}
