//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Annotations.Delegates;
using Xarial.XCad.Base;

namespace Xarial.XCad.Annotations
{
    /// <summary>
    /// Annotation which drives the dimension parameter
    /// </summary>
    public interface IXDimension : IXAnnotation, IXTransaction
    {
        /// <summary>
        /// Fired when the value of this dimension is changed
        /// </summary>
        event DimensionValueChangedDelegate ValueChanged;

        /// <summary>
        /// Name of the dimension
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Dimension value in the system units
        /// </summary>
        double Value { get; set; }
    }
}