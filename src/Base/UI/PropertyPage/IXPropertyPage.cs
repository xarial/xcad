//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.UI.PropertyPage.Delegates;

namespace Xarial.XCad.UI.PropertyPage
{
    /// <summary>
    /// Represents native proeprty page to manage entity parameters
    /// </summary>
    /// <typeparam name="TDataModel"></typeparam>
    public interface IXPropertyPage<TDataModel>
    {
        /// <summary>
        /// Fired when the data is changed (i.e. text box changed, combobox selection changed etc.)
        /// </summary>
        event PageDataChangedDelegate DataChanged;

        /// <summary>
        /// Fired when property page is about to be closed. Use the argument to provide additional instructions
        /// </summary>
        event PageClosingDelegate Closing;

        /// <summary>
        /// Fired when property manager page is closed
        /// </summary>
        event PageClosedDelegate Closed;

        /// <summary>
        /// Keystroke handler if page created with option <see cref="Enums.PageOptions_e.HandleKeystrokes"/>
        /// </summary>
        event KeystrokeHookDelegate KeystrokeHook;

        /// <summary>
        /// Raised when preview button is clicked
        /// </summary>
        event PagePreviewDelegate Preview;

        /// <summary>
        /// Raised when undo or redo action is clicked
        /// </summary>
        event PageUndoDelegate Undo;

        /// <summary>
        /// Raised when navigation button is clicked
        /// </summary>
        event PageNavigationDelegate Navigate;

        /// <summary>
        /// Checks if page is pinned
        /// </summary>
        /// <remarks>Set the <see cref="Enums.PageButtons_e.Pushpin"/> button in the <see cref="Attributes.PageButtonsAttribute"/></remarks>
        bool IsPinned { get; set; }

        /// <summary>
        /// Data model of the current page
        /// </summary>
        TDataModel Model { get; }

        /// <summary>
        /// Opens the property page with the specified data model
        /// </summary>
        /// <param name="model">Pointer to an instance of the bound data model</param>
        void Show(TDataModel model);

        /// <summary>
        /// Closes the current page
        /// </summary>
        /// <param name="cancel">Cancel the current page or OK</param>
        void Close(bool cancel);

        /// <summary>
        /// Temporarily closes the current page and restores once the suppressor is disposed
        /// </summary>
        /// <returns>Suppressor</returns>
        /// <remarks>This can be useful if some of the operations cannot be completed while proeprty page is open.
        /// This will closes the page without the notification and restores its</remarks>
        IDisposable Suppress();
    }
}