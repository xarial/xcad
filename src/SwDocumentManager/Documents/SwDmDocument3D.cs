//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.swdocumentmgr;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Xarial.XCad.Documents;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.UI;

namespace Xarial.XCad.SwDocumentManager.Documents
{
    public interface ISwDmDocument3D : ISwDmDocument, IXDocument3D
    {
        new ISwDmConfigurationCollection Configurations { get; }
    }

    public interface ISwDmVirtualDocument3D : ISwDmDocument3D 
    {
    }

    internal abstract class SwDmDocument3D : SwDmDocument, ISwDmDocument3D
    {
        #region Not Supported
        public new IXModelView3DRepository ModelViews => throw new NotSupportedException();
        TSelObject IXObjectContainer.ConvertObject<TSelObject>(TSelObject obj) => throw new NotSupportedException();
        IXDocument3DSaveOperation IXDocument3D.PreCreateSaveAsOperation(string filePath) => throw new NotSupportedException();
        public IXDocumentEvaluation Evaluation => throw new NotSupportedException();
        public IXDocumentGraphics Graphics => throw new NotSupportedException();
        public Color? Color { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public IXDisplayState DisplayState => throw new NotImplementedException();
        #endregion

        IXConfigurationRepository IXDocument3D.Configurations => Configurations;

        public abstract ISwDmConfigurationCollection Configurations { get; }

        public SwDmDocument3D(SwDmApplication dmApp, ISwDMDocument doc, bool isCreated,
            Action<ISwDmDocument> createHandler, Action<ISwDmDocument> closeHandler,
            bool? isReadOnly)
            : base(dmApp, doc, isCreated, createHandler, closeHandler, isReadOnly)
        {
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
