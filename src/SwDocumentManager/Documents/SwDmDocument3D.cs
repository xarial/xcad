//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.swdocumentmgr;
using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Documents;
using Xarial.XCad.Geometry;
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
        public IXBoundingBox PreCreateBoundingBox() => throw new NotSupportedException();
        TSelObject IXObjectContainer.ConvertObject<TSelObject>(TSelObject obj) => throw new NotSupportedException();
        public IXMassProperty PreCreateMassProperty() => throw new NotSupportedException();

        #endregion

        IXConfigurationRepository IXDocument3D.Configurations => Configurations;

        public ISwDmConfigurationCollection Configurations => m_Configurations.Value;

        private readonly Lazy<ISwDmConfigurationCollection> m_Configurations;

        public SwDmDocument3D(ISwDmApplication dmApp, ISwDMDocument doc, bool isCreated,
            Action<ISwDmDocument> createHandler, Action<ISwDmDocument> closeHandler,
            bool? isReadOnly)
            : base(dmApp, doc, isCreated, createHandler, closeHandler, isReadOnly)
        {
            m_Configurations = new Lazy<ISwDmConfigurationCollection>(() => new SwDmConfigurationCollection(this));
        }
    }

    internal class SwDmVirtualDocumentHelper
    {
        internal static string GetTitle(string fileName)
        {
            const string PREFIX = "_temp_";

            if (fileName.StartsWith(PREFIX, StringComparison.CurrentCultureIgnoreCase))
            {
                return fileName.Substring(PREFIX.Length);
            }
            else 
            {
                return fileName;
            }
        }
    }
}
