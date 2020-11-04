//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Diagnostics;
using System.Drawing;
using Xarial.XCad.Base.Enums;
using Xarial.XCad.Delegates;
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
        /// Returns the rectangle of the application window
        /// </summary>
        Rectangle WindowRectangle { get; }

        /// <summary>
        /// Notifies when host application is loaded
        /// </summary>
        event ApplicationLoadedDelegate Loaded;

        /// <summary>
        /// Window handle of the application main window
        /// </summary>
        IntPtr WindowHandle { get; }

        /// <summary>
        /// Application process
        /// </summary>
        Process Process { get; }

        /// <summary>
        /// Accesses the documents repository
        /// </summary>
        IXDocumentRepository Documents { get; }

        /// <summary>
        /// Accesses memory geometry builder to build primitive wires, surface and solids
        /// </summary>
        /// <remarks>Usually used in the <see cref="Features.CustomFeature.IXCustomFeatureDefinition"/></remarks>
        IXGeometryBuilder MemoryGeometryBuilder { get; }
        
        /// <summary>
        /// Displays the message box
        /// </summary>
        /// <param name="msg">Message to display</param>
        /// <param name="icon">Message box icon</param>
        /// <param name="buttons">Message box buttons</param>
        /// <returns>Button clicked by the user</returns>
        MessageBoxResult_e ShowMessageBox(string msg, MessageBoxIcon_e icon = MessageBoxIcon_e.Info, MessageBoxButtons_e buttons = MessageBoxButtons_e.Ok);

        /// <summary>
        /// Create instance of the macro
        /// </summary>
        /// <param name="path">Full path to the macro</param>
        /// <returns>Instance of the macro</returns>
        IXMacro OpenMacro(string path);

        /// <summary>
        /// Close current instance of the application
        /// </summary>
        void Close();
    }
}