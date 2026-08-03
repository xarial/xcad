//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Toolkit.PageBuilder;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Structures;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.PageElements;

namespace Xarial.XCad.Toolkit.Blazor.Controls
{
    public abstract class BlazorPropertyManagerPageItemsSourceControl<TVal> : BlazorPropertyManagerPageControl<TVal>, IItemsControl
    {
        protected override event ControlValueChangedDelegate<TVal> ValueChanged;

        public ItemsControlItem[] Items
        {
            get => ItemsControlManager.Items;
            set
            {
                ItemsControlManager.Items = value;
                NotifyInvalidated();
            }
        }

        protected ItemsControlManager<TVal> ItemsControlManager { get; }

        protected BlazorPropertyManagerPageItemsSourceControl(IXApplication app, IGroup parentGroup, IAttributeSet atts, IMetadata[] metadata)
            : base(parentGroup, atts, metadata)
        {
            ItemsControlManager = CreateItemsControlManager(app, atts, metadata);
            ItemsControlManager.Init();
        }

        public override void Update()
        {
            base.Update();
            ItemsControlManager.Update();
        }

        protected abstract ItemsControlManager<TVal> CreateItemsControlManager(IXApplication app, IAttributeSet atts, IMetadata[] metadata);
    }
}
