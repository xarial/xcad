//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Base.Enums;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Structures;

namespace Xarial.XCad.UI.PropertyPage.Services
{
    /// <summary>
    /// Enables custom logic for filtering the selection
    /// </summary>
    /// <remarks>Assigned via <see cref="Attributes.SelectionBoxOptionsAttribute.CustomFilter"/></remarks>
    public interface ISelectionCustomFilter
    {
        /// <summary>
        /// Called when entity is about to be selected
        /// </summary>
        /// <param name="selBox">Sender selection box</param>
        /// <param name="selection">Selection object</param>
        /// <param name="args">Filtering arguments</param>
        /// <returns></returns>
        void Filter(IControl selBox, IXSelObject selection, SelectionCustomFilterArguments args);
    }
}