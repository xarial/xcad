//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Base;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.UI;

namespace Xarial.XCad.Documents
{
    /// <summary>
    /// Represents 3D document (assembly or part)
    /// </summary>
    public interface IXDocument3D : IXDocument, IXObjectContainer
    {
        /// <summary>
        /// Access to the document's evaluation features
        /// </summary>
        IXDocumentEvaluation Evaluation { get; }

        /// <summary>
        /// Access the document's graphics features
        /// </summary>
        IXDocumentGraphics Graphics { get; }

        /// <summary>
        /// Returns 3D views collection
        /// </summary>
        new IXModelView3DRepository ModelViews { get; }

        /// <summary>
        /// Returns configurations of this document
        /// </summary>
        IXConfigurationRepository Configurations { get; }

        /// <summary>
        /// <see cref="IXDocument3D"/> specific save as operation
        /// </summary>
        new IXDocument3DSaveOperation PreCreateSaveAsOperation(string filePath);
    }
}