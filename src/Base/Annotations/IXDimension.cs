//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
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
    public interface IXDimension : IXSelObject, IXTransaction
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
        /// Gets the value of the dimension in the system units in the specified configuration or default
        /// </summary>
        /// <param name="confName">Name of the configuration</param>
        /// <returns>Dimension value</returns>
        double GetValue(string confName = "");

        /// <summary>
        /// Sets the value of the dimension in the specified configuration
        /// </summary>
        /// <param name="val">Value to set in the system units</param>
        /// <param name="confName">Name of the configuration or default</param>
        void SetValue(double val, string confName = "");
    }
}