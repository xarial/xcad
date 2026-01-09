//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Constructors;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Structures;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.PageElements;
using Xarial.XCad.Utils.Reflection;
using System.ComponentModel;
using Xarial.XCad.Reflection;
using System.Diagnostics.CodeAnalysis;
using Xarial.XCad.Services;
using Xarial.XCad.Documents;
using System.Runtime.InteropServices;
using System.Reflection;
using Xarial.XCad.Toolkit.Windows.UI.PropertyPage;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls
{
    internal abstract class PropertyManagerPageItemsSourceControl<TVal, TSwCtrl> : PropertyManagerPageBaseControl<TVal, TSwCtrl>, IItemsControl
        where TSwCtrl : class
    {
        protected override event ControlValueChangedDelegate<TVal> ValueChanged;

        public ItemsControlItem[] Items
        {
            get => ItemsControlManager.Items;
            set => ItemsControlManager.Items = value;
        }

        protected ItemsControlManager<TVal> ItemsControlManager { get; }

        public PropertyManagerPageItemsSourceControl(SwApplication app, IGroup parentGroup, IIconsCreator iconConv,
            IAttributeSet atts, IMetadata[] metadata, swPropertyManagerPageControlType_e type, ref int numberOfUsedIds)
            : base(app, parentGroup, iconConv, atts, metadata, type, ref numberOfUsedIds)
        {
            ItemsControlManager = CreateItemsControlManager(app, atts, metadata);
            ItemsControlManager.Init();
        }

        public override void Update()
        {
            base.Update();

            ItemsControlManager.Update();
        }

        protected abstract ItemsControlManager<TVal> CreateItemsControlManager(SwApplication app, IAttributeSet atts, IMetadata[] metadata);
    }
}