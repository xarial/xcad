//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

namespace Xarial.XCad
{
    /// <summary>
    /// Represents objects which can be selected by the user
    /// </summary>
    public interface IXSelObject : IXObject
    {
        /// <summary>
        /// Selects object in the document
        /// </summary>
        /// <param name="append">True to add selection to the current list, false to clear existing selection</param>
        void Select(bool append);
    }
}