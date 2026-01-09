//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using Xarial.XCad.UI.PropertyPage;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Delegates;

namespace Xarial.XCad.Utils.PageBuilder.Base
{
    /// <summary>
    /// Delegate for creating control via <see cref="IDataModelBinder.Bind{TDataModel}(CreateBindingPageDelegate, CreateBindingControlDelegate, CreateDynamicControlsDelegate, IContextProvider, out IReadOnlyList{IBinding}, out IRawDependencyGroup, out IMetadata[])"/>
    /// </summary>
    /// <param name="dataType">Control data type</param>
    /// <param name="atts">Attributes</param>
    /// <param name="parent">Parent group</param>
    /// <param name="metadata">Metadata</param>
    /// <param name="numberOfUsedIds">Number of IDs reserved by control (e.g. nested controls)</param>
    /// <returns>Instance of the control</returns>
    public delegate IControl CreateBindingControlDelegate(Type dataType, IAttributeSet atts,
        IGroup parent, IMetadata[] metadata, out int numberOfUsedIds);

    /// <summary>
    /// Delegate for creating page via <see cref="IDataModelBinder.Bind{TDataModel}(CreateBindingPageDelegate, CreateBindingControlDelegate, CreateDynamicControlsDelegate, IContextProvider, out IReadOnlyList{IBinding}, out IRawDependencyGroup, out IMetadata[])"/>
    /// </summary>
    /// <param name="atts">Attributes</param>
    /// <returns>Instance of the page</returns>
    public delegate IPage CreateBindingPageDelegate(IAttributeSet atts);

    /// <summary>
    /// Utility for data binding
    /// </summary>
    public interface IDataModelBinder
    {
        /// <summary>
        /// Binds data model to control
        /// </summary>
        /// <typeparam name="TDataModel">Data model type</typeparam>
        /// <param name="prpPage">Property page</param>
        /// <param name="pageCreator">Page factory</param>
        /// <param name="ctrlCreator">Control factory</param>
        /// <param name="modelSetter">Context provider for the model</param>
        /// <param name="bindings">Bindings</param>
        /// <param name="dependencies">Dependencies</param>
        /// <param name="metadata">Metadata</param>
        void Bind<TDataModel>(IXPropertyPage<TDataModel> prpPage, CreateBindingPageDelegate pageCreator,
            CreateBindingControlDelegate ctrlCreator, IContextProvider modelSetter,
            out IReadOnlyList<IBinding> bindings, out IRawDependencyGroup dependencies, out IMetadata[] metadata);
    }
}