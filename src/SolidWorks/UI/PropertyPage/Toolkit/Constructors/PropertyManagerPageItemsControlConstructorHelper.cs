//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

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
        internal bool TryGetStaticItems(IXApplication app, IAttributeSet atts, out ItemsControlItem[] staticItems)
        {
            if (atts.ContextType.IsEnum)
            {
                var items = EnumExtension.GetEnumFields(atts.ContextType);
                staticItems = items.Select(i => new ItemsControlItem()
                {
                    DisplayName = i.Value,
                    Value = i.Key
                }).ToArray();

                return true;
            }
            else 
            {
                var customItemsAtt = atts.Get<ItemsSourceControlAttribute>();

                if (customItemsAtt.StaticItems?.Any() == true)
                {
                    staticItems = customItemsAtt
                        .StaticItems.Select(i => new ItemsControlItem(i)).ToArray();

                    return true;
                }
                else if (customItemsAtt.CustomItemsProvider != null)
                {
                    if (customItemsAtt.Dependencies?.Any() != true) 
                    {
                        var provider = customItemsAtt.CustomItemsProvider;

                        staticItems = provider.ProvideItems(app, new IControl[0]).Select(i => new ItemsControlItem(i)).ToArray();
                        return true;
                    }
                }
            }

            staticItems = null;
            return false;
        }
    }
}