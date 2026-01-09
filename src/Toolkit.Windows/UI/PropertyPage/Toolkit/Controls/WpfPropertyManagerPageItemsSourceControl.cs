//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Structures;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.PageElements;

namespace Xarial.XCad.Toolkit.Windows.UI.PropertyPage.Toolkit.Controls
{
    internal abstract class WpfPropertyManagerPageItemsSourceControl<TVal> : WpfPropertyManagerPageControl<TVal>, IItemsControl
    {
        protected override event ControlValueChangedDelegate<TVal> ValueChanged;

        public ItemsControlItem[] Items
        {
            get => ItemsControlManager.Items;
            set
            {
                ItemsControlManager.Items = value;
                NotifyPropertyChanged();
            }
        }

        protected ItemsControlManager<TVal> ItemsControlManager { get; }

        public WpfPropertyManagerPageItemsSourceControl(IXApplication app, IGroup parentGroup, IAttributeSet atts, IMetadata[] metadata, IIconsCreator iconConv)
            : base(parentGroup, atts, metadata, iconConv)
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