using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Reflection;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Structures;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.Reflection;

namespace Xarial.XCad.Toolkit.Windows.UI.PropertyPage
{
    public abstract class ItemsControlManager<TVal>
    {
        private class WrappedGenericEqualityComparer : IEqualityComparer
        {
            private object m_Instance;
            private readonly MethodInfo m_EqualsMethod;
            private readonly MethodInfo m_GetHashCodeMethod;

            internal WrappedGenericEqualityComparer(Type eqCompType)
            {
                var eqObjType = eqCompType.GetArgumentsOfGenericType(typeof(IEqualityComparer<>)).First();

                m_Instance = Activator.CreateInstance(eqCompType);

                m_EqualsMethod = eqCompType.GetMethod(nameof(IEqualityComparer<object>.Equals), new Type[] { eqObjType, eqObjType });
                m_GetHashCodeMethod = eqCompType.GetMethod(nameof(IEqualityComparer<object>.GetHashCode), new Type[] { eqObjType });
            }

            public new bool Equals(object x, object y) => (bool)m_EqualsMethod.Invoke(m_Instance, new object[] { x, y });

            public int GetHashCode(object obj) => (int)m_EqualsMethod.Invoke(m_Instance, new object[] { obj });
        }

        private class DefaultEqualityComparer : IEqualityComparer
        {
            public new bool Equals(object x, object y) => object.Equals(x, y);

            public int GetHashCode(object obj) => 0;
        }

        private ItemsControlItem[] m_Items;

        private readonly IMetadata m_SrcMetadata;
        private readonly Type m_SpecificItemType;

        private readonly string m_DispMembPath;
        private readonly IEqualityComparer m_EqualityComparer;

        private object m_CurMetadataValue;

        private readonly IItemsControl m_ItemsCtrl;

        private readonly bool m_IsStatic;

        private readonly IAttributeSet m_Atts;

        public ItemsControlManager(IItemsControl itemsCtrl, IXApplication app, IAttributeSet atts, IMetadata[] metadata)
        {
            m_ItemsCtrl = itemsCtrl;

            m_Atts = atts;

            m_SpecificItemType = atts.ContextType;

            ParseItems(app, atts, metadata, out m_IsStatic, out ItemsControlItem[] staticItems,
                out m_SrcMetadata, out m_DispMembPath, out m_EqualityComparer);

            if (m_IsStatic)
            {
                m_Items = staticItems;
            }
            else
            {
                if (m_SrcMetadata != null)
                {
                    m_SrcMetadata.Changed += OnMetadataChanged;
                    m_CurMetadataValue = m_SrcMetadata.Value;
                    m_Items = LoadItemsFromSource(m_CurMetadataValue);
                }
            }
        }

        public void Init() 
        {
            Items = LoadInitialItems(m_Atts, m_IsStatic, m_Items);
        }

        public ItemsControlItem[] Items
        {
            get => m_Items;
            set
            {
                if (m_Items != null)
                {
                    foreach (var item in m_Items)
                    {
                        item.DisplayNameChanged -= OnItemDisplayNameChanged;
                    }
                }

                m_Items = value;

                LoadItemsIntoControl(value);

                if (m_Items != null)
                {
                    foreach (var item in m_Items)
                    {
                        item.DisplayNameChanged += OnItemDisplayNameChanged;
                    }
                }
            }
        }

        public void Update()
        {
            if (m_SrcMetadata != null)
            {
                var thisMetadataVal = m_SrcMetadata.Value;

                if (m_CurMetadataValue != thisMetadataVal)
                {
                    m_CurMetadataValue = thisMetadataVal;

                    Items = LoadItemsFromSource(m_CurMetadataValue);
                }
            }
        }

        public TVal GetItem(int index)
        {
            if (Items != null)
            {
                if (index >= 0 && index < Items.Length)
                {
                    return (TVal)Items[index].Value;
                }
            }

            return GetDefaultItemValue();
        }

        public TVal GetDefaultItemValue()
        {
            if (m_SpecificItemType.IsValueType)
            {
                return (TVal)Activator.CreateInstance(m_SpecificItemType);
            }
            else
            {
                return default;
            }
        }

        public int GetItemIndex(TVal value)
        {
            int index = -1;

            if (Items != null)
            {
                for (int i = 0; i < Items.Length; i++)
                {
                    if (m_EqualityComparer.Equals(Items[i].Value, value))
                    {
                        index = (short)i;
                        break;
                    }
                }
            }

            return index;
        }

        public bool CompareValues(object firstVal, object secondVal) => m_EqualityComparer.Equals(firstVal, secondVal);

        protected abstract void LoadItemsIntoControl(ItemsControlItem[] newItems);

        protected virtual void SetItemDisplayName(ItemsControlItem item, int index, string newDispName)
        {
        }

        protected virtual ItemsControlItem[] LoadInitialItems(IAttributeSet atts, bool isStatic, ItemsControlItem[] items) => items;

        protected virtual ItemsControlItem[] CreateEnumItems(Type enumType)
        {
            if (!enumType.IsEnum)
            {
                throw new InvalidCastException($"{enumType.FullName} must be an enum");
            }

            var items = new List<ItemsControlItem>();

            foreach (Enum en in Enum.GetValues(enumType))
            {
                var dispName = "";

                en.TryGetAttribute<DisplayNameAttribute>(a => dispName = a.DisplayName);

                if (string.IsNullOrEmpty(dispName))
                {
                    dispName = en.ToString();
                }

                var desc = "";

                en.TryGetAttribute<DescriptionAttribute>(a => desc = a.Description);

                items.Add(new ItemsControlItem(en, dispName, desc));
            }

            return items.ToArray();
        }

        protected bool CompareItems(ItemsControlItem[] oldItems, ItemsControlItem[] newItems)
        {
            if (newItems == null && oldItems == null)
            {
                return true;
            }
            else if (newItems == null || oldItems == null)
            {
                return false;
            }
            else if (newItems.Length != oldItems.Length)
            {
                return false;
            }
            else
            {
                for (int i = 0; i < oldItems.Length; i++)
                {
                    if (!CompareValues(oldItems[i].Value, newItems[i].Value))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        private void OnItemDisplayNameChanged(ItemsControlItem item, string newDispName)
        {
            SetItemDisplayName(item, Array.IndexOf(Items, item), newDispName);
        }

        private void OnMetadataChanged(IMetadata metadata, object value)
        {
            Items = LoadItemsFromSource(value);
        }

        private ItemsControlItem[] LoadItemsFromSource(object value)
        {
            var items = new List<ItemsControlItem>();

            if (value is IEnumerable)
            {
                foreach (var item in value as IEnumerable)
                {
                    items.Add(new ItemsControlItem(item, m_DispMembPath));
                }
            }
            else if (value is null)
            {
                //return empty
            }
            else
            {
                throw new NotSupportedException("Source property must be enumerable");
            }

            return items.ToArray();
        }

        private void ParseItems(IXApplication app, IAttributeSet atts, IMetadata[] metadata,
            out bool isStatic, out ItemsControlItem[] staticItems, out IMetadata itemsSourceMetadata,
            out string dispMembPath, out IEqualityComparer eqComparer)
        {
            if (atts.ContextType.IsEnum)
            {
                staticItems = CreateEnumItems(atts.ContextType);

                isStatic = true;
                itemsSourceMetadata = null;
                dispMembPath = "";
                eqComparer = new DefaultEqualityComparer();
            }
            else
            {
                var customItemsAtt = atts.Get<ItemsSourceControlAttribute>();
                dispMembPath = customItemsAtt.DisplayMemberPath;

                var eqCompType = customItemsAtt.EqualityComparer;

                if (eqCompType != null)
                {
                    if (typeof(IEqualityComparer).IsAssignableFrom(eqCompType))
                    {
                        eqComparer = (IEqualityComparer)Activator.CreateInstance(eqCompType);
                    }
                    if (eqCompType.IsAssignableToGenericType(typeof(IEqualityComparer<>)))
                    {
                        eqComparer = new WrappedGenericEqualityComparer(eqCompType);
                    }
                    else
                    {
                        throw new Exception($"{eqCompType} does not implement {typeof(IEqualityComparer)}");
                    }
                }
                else
                {
                    eqComparer = new DefaultEqualityComparer();
                }

                if (customItemsAtt.StaticItems?.Any() == true)
                {
                    staticItems = customItemsAtt
                        .StaticItems.Select(i => new ItemsControlItem(i, customItemsAtt.DisplayMemberPath)).ToArray();

                    isStatic = true;
                    itemsSourceMetadata = null;
                }
                else if (customItemsAtt.CustomItemsProvider != null)
                {
                    itemsSourceMetadata = null;

                    if (customItemsAtt.Dependencies?.Any() != true)
                    {
                        var provider = customItemsAtt.CustomItemsProvider;
                        staticItems = provider.ProvideItems(app, m_ItemsCtrl, Array.Empty<IControl>(), customItemsAtt.Parameter)
                            .Select(i => new ItemsControlItem(i, customItemsAtt.DisplayMemberPath)).ToArray();
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
