//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

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
        void OnConnect();

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
        /// <returns>Instance of the proeprty page</returns>
        IXPropertyPage<TData> CreatePage<TData>();

        IXCustomPanel<TControl> CreateDocumentTab<TControl>(Documents.IXDocument doc);
        IXCustomPanel<TWindow> CreatePopupWindow<TWindow>();
        IXTaskPane<TControl> CreateTaskPane<TControl>(TaskPaneSpec spec);
    }
}