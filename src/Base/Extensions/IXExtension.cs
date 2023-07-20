//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Base;
using Xarial.XCad.Extensions.Delegates;
using Xarial.XCad.UI;
using Xarial.XCad.UI.Commands;
using Xarial.XCad.UI.PropertyPage;
using Xarial.XCad.UI.PropertyPage.Delegates;
using Xarial.XCad.UI.TaskPane;

namespace Xarial.XCad.Extensions
{
    /// <summary>
    /// Represents the extensibility interface (add-in)
    /// </summary>
    public interface IXExtension
    {
        /// <summary>
        /// Event for <see cref="OnConnect"/>
        /// </summary>
        /// <remarks>If <see cref="OnConnect"/> is overridden this event will not be raised</remarks>
        event ExtensionConnectDelegate Connect;

        /// <summary>
        /// Event for <see cref="OnDisconnect"/>
        /// </summary>
        /// <remarks>If <see cref="OnDisconnect"/> is overridden this event will not be raised</remarks>
        event ExtensionDisconnectDelegate Disconnect;

        /// <summary>
        /// Fired when extension startup is completed and all the components and application fully loaded
        /// </summary>
        event ExtensionStartupCompletedDelegate StartupCompleted;

        /// <summary>
        /// Called when extension is loading
        /// </summary>
        void OnConnect();

        /// <summary>
        /// Called when extension is unloading
        /// </summary>
        void OnDisconnect();

        /// <summary>
        /// Pointer to the main application
        /// </summary>
        IXApplication Application { get; }

        /// <summary>
        /// Returns the instance of the commands manager
        /// </summary>
        IXCommandManager CommandManager { get; }

        /// <summary>
        /// Create native property page to manage parameters
        /// </summary>
        /// <typeparam name="TData">Type defining the data model of the property page</typeparam>
        /// <param name="createDynCtrlHandler">Dynamic control creation handler for properties marked with <see cref="UI.PropertyPage.Attributes.DynamicControlsAttribute"/></param>
        /// <returns>Instance of the property page</returns>
        IXPropertyPage<TData> CreatePage<TData>(CreateDynamicControlsDelegate createDynCtrlHandler = null);

        /// <summary>
        /// Returns the instance of the current logger
        /// </summary>
        IXLogger Logger { get; }

        /// <summary>
        /// Hosts the control in the Feature Manager View
        /// </summary>
        /// <typeparam name="TControl">Type of control to host</typeparam>
        /// <param name="doc">Document where to host control</param>
        /// <returns>Custom panel</returns>
        IXCustomPanel<TControl> CreateFeatureManagerTab<TControl>(Documents.IXDocument doc);

        /// <summary>
        /// Hosts the control in the Document Tab view
        /// </summary>
        /// <typeparam name="TControl">Type of control to host</typeparam>
        /// <param name="doc">Document where to host control</param>
        /// <returns>Custom panel</returns>
        IXCustomPanel<TControl> CreateDocumentTab<TControl>(Documents.IXDocument doc);
        
        /// <summary>
        /// Creates the popup window in the context of current application
        /// </summary>
        /// <typeparam name="TWindow">Window to show</typeparam>
        /// <param name="window">Instance of the window</param>
        /// <returns>Custom popup window</returns>
        IXPopupWindow<TWindow> CreatePopupWindow<TWindow>(TWindow window);
        
        /// <summary>
        /// Hosts the control in the task pane
        /// </summary>
        /// <typeparam name="TControl">Type of control</typeparam>
        /// <param name="spec">Specification of the Task Pane</param>
        /// <returns>Custom panel</returns>
        IXTaskPane<TControl> CreateTaskPane<TControl>(TaskPaneSpec spec);

        /// <summary>
        /// Pre-creates work unit which can be run in the background
        /// </summary>
        /// <returns>Work unit template</returns>
        IXWorkUnit PreCreateWorkUnit();
    }
}