//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Delegates;

namespace Xarial.XCad.Utils.PageBuilder.Base
{
    public delegate IControl CreateBindingControlDelegate(Type dataType, IAttributeSet atts,
        IGroup parent, IMetadata metadata, out int idRange);

    public delegate IPage CreateBindingPageDelegate(IAttributeSet atts);

    public interface IDataModelBinder
    {
        void Bind<TDataModel>(CreateBindingPageDelegate pageCreator,
            CreateBindingControlDelegate ctrlCreator, CreateDynamicControlsDelegate dynCtrlDescCreator,
            out IEnumerable<IBinding> bindings, out IRawDependencyGroup dependencies);
    }
}