//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Structures;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.Reflection;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Constructors
{
    internal class PropertyManagerPageItemsControlConstructorHelper
    {
        internal void ParseItems(IXApplication app, IAttributeSet atts, IMetadata[] metadata,
            out bool isStatic, out ItemsControlItem[] staticItems, out IMetadata itemsSourceMetadata)
        {
            if (atts.ContextType.IsEnum)
            {
                var items = EnumExtension.GetEnumFields(atts.ContextType);
                staticItems = items.Select(i => new ItemsControlItem()
                {
                    DisplayName = i.Value,
                    Value = i.Key
                }).ToArray();

                isStatic = true;
                itemsSourceMetadata = null;
            }
            else 
            {
                var customItemsAtt = atts.Get<ItemsSourceControlAttribute>();

                if (customItemsAtt.StaticItems?.Any() == true)
                {
                    staticItems = customItemsAtt
                        .StaticItems.Select(i => new ItemsControlItem(i)).ToArray();

                    isStatic = true;
                    itemsSourceMetadata = null;
                }
                else if (customItemsAtt.CustomItemsProvider != null)
                {
                    itemsSourceMetadata = null;

                    if (customItemsAtt.Dependencies?.Any() != true)
                    {
                        var provider = customItemsAtt.CustomItemsProvider;
                        staticItems = provider.ProvideItems(app, new IControl[0]).Select(i => new ItemsControlItem(i)).ToArray();
                        isStatic = true;
                    }
                    else
                    {
                        isStatic = false;
                        staticItems = null;
                    }
                }
                else if (customItemsAtt.ItemsSource != null)
                {
                    isStatic = false;
                    staticItems = null;
                    itemsSourceMetadata = metadata?.FirstOrDefault(m => object.Equals(m.Tag, customItemsAtt.ItemsSource));

                    if (itemsSourceMetadata == null)
                    {
                        throw new NullReferenceException($"Failed to find the items source metadata property: {customItemsAtt.ItemsSource}");
                    }
                }
                else 
                {
                    throw new NotSupportedException("Items source is not specified");
                }
            }
        }
    }
}