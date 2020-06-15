//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Base.Enums;
using Xarial.XCad.Documents;
using Xarial.XCad.Geometry;

namespace Xarial.XCad
{
    /// <summary>
    /// Top level object in the class hierarchy
    /// </summary>
    public interface IXApplication
    {
        /// <summary>
        /// Accesses the documents repository
        /// </summary>
        IXDocumentCollection Documents { get; }

        /// <summary>
        /// Accesses geometry builder to build primitive geometry
        /// </summary>
        /// <remarks>Usually used in the <see cref="Features.CustomFeature.IXCustomFeatureDefinition"/></remarks>
        IXGeometryBuilder GeometryBuilder { get; }

        /// <summary>
        /// Displays the message box
        /// </summary>
        /// <param name="msg">Message to display</param>
        /// <param name="icon">Message box icon</param>
        /// <param name="buttons">Message box buttons</param>
        /// <returns>Button clicked by the user</returns>
        MessageBoxResult_e ShowMessageBox(string msg, MessageBoxIcon_e icon = MessageBoxIcon_e.Info, MessageBoxButtons_e buttons = MessageBoxButtons_e.Ok);

        IXMacro OpenMacro(string path);

        void Close();
    }
}