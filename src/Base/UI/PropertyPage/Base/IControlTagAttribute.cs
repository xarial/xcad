//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

namespace Xarial.XCad.UI.PropertyPage.Base
{
    public interface IControlTagAttribute : IAttribute
    {
        /// <summary>
        /// Tag associated with the control
        /// </summary>
        object Tag { get; }
    }
}