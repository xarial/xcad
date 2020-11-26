//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Base;
using Xarial.XCad.Extensions.Delegates;
using Xarial.XCad.UI;
using Xarial.XCad.UI.Commands;
using Xarial.XCad.UI.PropertyPage;
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
        event ExtensionConnectDelegate Connect;

        /// <summary>
        /// Event for <see cref="OnDisconnect"/>
        /// </summary>
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

        IXCommandManager CommandManager { get; }

        /// <summary>
        /// Create native property page to manage parameters
        /// </summary>
        /// <typeparam name="TData">Type defining the data model of the property page</typeparam>
        /// <returns>Instance of the property page</returns>
        IXPropertyPage<TData> CreatePage<TData>();

        IXLogger Logger { get; }

        IXCustomPanel<TControl> CreateFeatureManagerTab<TControl>(Documents.IXDocument doc);
        IXCustomPanel<TControl> CreateDocumentTab<TControl>(Documents.IXDocument doc);
        IXPopupWindow<TWindow> CreatePopupWindow<TWindow>();
        IXTaskPane<TControl> CreateTaskPane<TControl>(TaskPaneSpec spec);
    }
}