//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Graphics;
using Xarial.XCad.UI;

namespace Xarial.XCad.Documents
{
    /// <summary>
    /// Provides graphics features to the see <see cref="IXDocument3D"/>
    /// </summary>
    public interface IXDocumentGraphics
    {
        /// <summary>
        /// Pre-creates callout instance
        /// </summary>
        /// <returns>Instance of the callout</returns>
        IXCallout PreCreateCallout();

        /// <summary>
        /// Pre-creates triad manipulator
        /// </summary>
        /// <returns>Instance of triad manipulator</returns>
        IXTriad PreCreateTriad();

        IXDragArrow PreCreateDragArrow();
    }
}
