//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
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
        private readonly string m_DisplayMemberMemberPath;

        internal CustomItemsAttributeDependencyHandler(ICustomItemsProvider itemsProvider, string displayMemberMemberPath)
        {
            m_ItemsProvider = itemsProvider;
            m_DisplayMemberMemberPath = displayMemberMemberPath;
        }

        public void UpdateState(IXApplication app, IControl source, IControl[] dependencies)
        {
            var itemsCtrl = (IItemsControl)source;

            itemsCtrl.Items = m_ItemsProvider.ProvideItems(app, dependencies)
                ?.Select(i => new ItemsControlItem(i, m_DisplayMemberMemberPath)).ToArray();
        }
    }
}
