//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
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
        internal ItemsControlItem[] GetItems(IXApplication app, IAttributeSet atts)
        {
            if (atts.ContextType.IsEnum)
            {
                var items = EnumExtension.GetEnumFields(atts.ContextType);
                return items.Select(i => new ItemsControlItem()
                {
                    DisplayName = i.Value,
                    Value = i.Key
                }).ToArray();
            }
            else 
            {
                var customItemsAtt = atts.Get<ItemsSourceControlAttribute>();

                List<object> items = null;

                if (customItemsAtt.StaticItems != null)
                {
                    items = customItemsAtt.StaticItems.ToList();
                }
                else if (customItemsAtt.CustomItemsProvider != null) 
                {
                    var provider = customItemsAtt.CustomItemsProvider;

                    var depsCount = customItemsAtt.Dependencies?.Length;

                    //TODO: dependency controls cannot be loaded at this stage as binding is not yet loaded - need to sort this out
                    //Not very critical at this stage as provide items wil be called as part ResolveState for dependent controls
                    //For now just add a note in the documentation for this behavior
                    items = provider.ProvideItems(app, new IControl[depsCount.Value])?.ToList();
                }
                else if (customItemsAtt.ItemsSource != null)
                {
                    //this will be resolved dynamically
                    items = new List<object>();
                }
                
                if (items == null)
                {
                    items = new List<object>();
                }

                return items.Select(i => new ItemsControlItem(i)).ToArray();
            }
        }
    }
}