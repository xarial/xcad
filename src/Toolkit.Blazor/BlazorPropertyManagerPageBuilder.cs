//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using Xarial.XCad.Base;
using Xarial.XCad.Toolkit.Blazor.Constructors;
using Xarial.XCad.Toolkit.Blazor.Controls;
using Xarial.XCad.Utils.PageBuilder;
using Xarial.XCad.Utils.PageBuilder.Binders;

namespace Xarial.XCad.Toolkit.Blazor
{
    /// <summary>
    /// Wires the shared reflection/binding engine (<see cref="PageBuilderBase{TPage, TGroup, TControl}"/>)
    /// to the Blazor control set
    /// </summary>
    internal class BlazorPropertyManagerPageBuilder
        : PageBuilderBase<BlazorPropertyManagerPagePage, BlazorPropertyManagerPageGroup, IBlazorPropertyManagerPageControl>
    {
        public BlazorPropertyManagerPageBuilder(IXApplication app, IServiceProvider svcProv, IXLogger logger)
            : this(app, new TypeDataBinder(svcProv, logger))
        {
        }

        private BlazorPropertyManagerPageBuilder(IXApplication app, TypeDataBinder dataBinder)
            : base(app, dataBinder,
                  new BlazorPropertyManagerPageConstructor(),
                  new BlazorPropertyManagerPageGroupConstructor(),
                  new BlazorPropertyManagerPageTextBoxConstructor(app),
                  new BlazorPropertyManagerPageNumberBoxConstructor(app),
                  new BlazorPropertyManagerPageCheckBoxConstructor(app),
                  new BlazorPropertyManagerPageEnumComboBoxConstructor(app),
                  new BlazorPropertyManagerPageCustomItemsComboBoxConstructor(app),
                  new BlazorPropertyManagerPageButtonConstructor(app),
                  new BlazorPropertyManagerPageTabConstructor(),
                  new BlazorPropertyManagerPageCustomControlConstructor(app))
        {
        }
    }
}
