//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.UI.PropertyPage.Base;

namespace Xarial.XCad.Utils.PageBuilder.Base.Attributes
{
    ///<summary>
    ///This attribute indicates that the constructor must be used as a default control constructor for the nominated type
    ///</summary>
    /// <remarks>
    /// Must be applied to <see cref="IPageElementConstructor{TGroup, TPage}"/>
    /// or <see cref="IPageConstructor{TPage}"/> only
    /// </summary>
    public interface IDefaultTypeAttribute : IAttribute
    {
        /// <summary>
        /// Specifies the type of the data which this constructor creates control for
        /// </summary>
        Type Type { get; }
    }
}