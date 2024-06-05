﻿//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using Xarial.XCad.Base;
using Xarial.XCad.Base.Enums;
using Xarial.XCad.Delegates;
using Xarial.XCad.Documents;
using Xarial.XCad.Enums;
using Xarial.XCad.Geometry;

namespace Xarial.XCad
{
    /// <summary>
    /// Application level options
    /// </summary>
    public interface IXApplicationOptions : IXOptions 
    {
        /// <summary>
        /// Application level options for drawings
        /// </summary>
        IXDrawingsApplicationOptions Drawings { get; }
    }

    /// <summary>
    /// Application level options for drawings
    /// </summary>
    public interface IXDrawingsApplicationOptions
    {
        /// <summary>
        /// Specifies whether new views are scaled to fit drawing sheet
        /// </summary>
        bool AutomaticallyScaleNewDrawingViews { get; set; }
    }

    /// <summary>
    /// Top level object in the class hierarchy
    /// </summary>
    public interface IXApplication : IXTransaction, IDisposable
    {
        /// <summary>
        /// Fires when application is starting
        /// </summary>
        event ApplicationStartingDelegate Starting;

        /// <summary>
        /// Fires when no activity detected in the application
        /// </summary>
        event ApplicationIdleDelegate Idle;

        /// <summary>
        /// Version of the application
        /// </summary>
        IXVersion Version { get; set; }

        /// <summary>
        /// State of the application
        /// </summary>
        ApplicationState_e State { get; set; }

        /// <summary>
        /// Checks if this application is alive
        /// </summary>
        bool IsAlive { get; }

        /// <summary>
        /// Returns the rectangle of the application window
        /// </summary>
        Rectangle WindowRectangle { get; }
        
        /// <summary>
        /// Window handle of the application main window
        /// </summary>
        IntPtr WindowHandle { get; }

        /// <summary>
        /// Application process
        /// </summary>
        Process Process { get; }

        /// <summary>
        /// Application level options
        /// </summary>
        IXApplicationOptions Options { get; }

        /// <summary>
        /// Accesses the documents repository
        /// </summary>
        IXDocumentRepository Documents { get; }

        /// <summary>
        /// Accesses memory geometry builder to build primitive wires, surface and solids
        /// </summary>
        /// <remarks>Usually used in the <see cref="Features.CustomFeature.IXCustomFeatureDefinition"/></remarks>
        IXMemoryGeometryBuilder MemoryGeometryBuilder { get; }
        
        /// <summary>
        /// Displays the message box
        /// </summary>
        /// <param name="msg">Message to display</param>
        /// <param name="icon">Message box icon</param>
        /// <param name="buttons">Message box buttons</param>
        /// <returns>Button clicked by the user</returns>
        MessageBoxResult_e ShowMessageBox(string msg, MessageBoxIcon_e icon = MessageBoxIcon_e.Info, MessageBoxButtons_e buttons = MessageBoxButtons_e.Ok);

        /// <summary>
        /// Displays the modeless tooltip
        /// </summary>
        /// <param name="spec">Specification of the tooltip</param>
        void ShowTooltip(ITooltipSpec spec);

        /// <summary>
        /// Create instance of the macro
        /// </summary>
        /// <param name="path">Full path to the macro</param>
        /// <returns>Instance of the macro</returns>
        IXMacro OpenMacro(string path);

        /// <summary>
        /// Initiates the displaying of progress in the application
        /// </summary>
        /// <returns>Pointer to progress manager</returns>
        IXProgress CreateProgress();

        /// <summary>
        /// Creates an object tracker to track objects across operations
        /// </summary>
        /// <param name="name">Name of the tracker</param>
        /// <returns>Tracker</returns>
        IXObjectTracker CreateObjectTracker(string name);

        /// <summary>
        /// Material databases
        /// </summary>
        IXMaterialsDatabaseRepository MaterialDatabases { get; }

        /// <summary>
        /// Close current instance of the application
        /// </summary>
        void Close();
    }
}