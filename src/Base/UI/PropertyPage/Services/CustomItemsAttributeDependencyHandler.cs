//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Structures;

namespace Xarial.XCad.UI.PropertyPage.Services
{
    internal class CustomItemsAttributeDependencyHandler : IDependencyHandler
    {
        private readonly ICustomItemsProvider m_ItemsProvider;
        private readonly Func<string> m_DisplayMemberMemberPathFunc;

        internal CustomItemsAttributeDependencyHandler(ICustomItemsProvider itemsProvider, Func<string> displayMemberMemberPathFunc)
        {
            m_ItemsProvider = itemsProvider;
            m_DisplayMemberMemberPathFunc = displayMemberMemberPathFunc;
        }

        public void UpdateState(IXApplication app, IControl source, IControl[] dependencies, object parameter)
        {
            var itemsCtrl = (IItemsControl)source;

            var dispMembPath = m_DisplayMemberMemberPathFunc.Invoke();

            itemsCtrl.Items = m_ItemsProvider.ProvideItems(app, source, dependencies, parameter)
                ?.Select(i => new ItemsControlItem(i, dispMembPath)).ToArray();
        }
    }
}
