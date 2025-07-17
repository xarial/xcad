//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

namespace Xarial.XCad.Utils.PageBuilder.Base
{
    /// <summary>
    /// Represents the page container
    /// </summary>
    public interface IPage : IGroup
    {
        /// <summary>
        /// Page binding
        /// </summary>
        IBindingManager Binding { get; }
    }
}