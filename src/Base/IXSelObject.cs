//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Base;
using Xarial.XCad.Base.Enums;

namespace Xarial.XCad
{
    /// <summary>
    /// Represents objects which can be selected by the user
    /// </summary>
    public interface IXSelObject : IXObject, IXTransaction
    {
        /// <summary>
        /// Identifies if this object is currently selected
        /// </summary>
        bool IsSelected { get; }

        /// <summary>
        /// Selects object in the document
        /// </summary>
        /// <param name="append">True to add selection to the current list, false to clear existing selection</param>
        void Select(bool append);

        /// <summary>
        /// Type of this selection object
        /// </summary>
        SelectType_e SelectionType { get; }
    }
}