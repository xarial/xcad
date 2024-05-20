//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

namespace Xarial.XCad.UI.PropertyPage.Base
{
    /// <summary>
    /// Indicates that this binding has a static metadata
    /// </summary>
    public interface IStaticMetadataAttribute : IAttribute 
    {
        /// <summary>
        /// Static metadata name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Static value of the metadata
        /// </summary>
        object StaticValue { get; }
    }
}
