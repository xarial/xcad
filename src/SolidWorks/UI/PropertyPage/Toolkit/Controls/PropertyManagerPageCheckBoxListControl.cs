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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using Xarial.XCad.Reflection;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Enums;
using Xarial.XCad.UI.PropertyPage.Structures;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.PageElements;
using Xarial.XCad.Utils.Reflection;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls
{
    /// <summary>
    /// Wrapper class around the group of <see href="http://help.solidworks.com/2016/english/api/sldworksapi/solidworks.interop.sldworks~solidworks.interop.sldworks.ipropertymanagerpagecheckbox.html">IPropertyManagerPageCheckbox</see> controls
    /// </summary>
    /// <remarks>All set properties will be applied to all controls in the group, while get will return the value of first control</remarks>
    public class PropertyManagerPageCheckBoxList : PropertyManagerPageControlList<IPropertyManagerPageCheckbox>, IPropertyManagerPageCheckbox
    {
        private IPropertyManagerPageCheckbox[] m_Controls;

        private readonly Func<ItemsControlItem, IPropertyManagerPageCheckbox> m_CheckBoxCreator;

        private bool m_IsCreated;

        private Action m_ControlsAttributesAssinger;

        public PropertyManagerPageCheckBoxList(Func<ItemsControlItem, IPropertyManagerPageCheckbox> checkBoxCreator) : base()
        {
            m_CheckBoxCreator = checkBoxCreator;
            m_IsCreated = false;
        }

        internal void CreateControls(ItemsControlItem[] items) 
        {
            if (!m_IsCreated)
            {
                m_IsCreated = true;
                m_Controls = items.Select(i => m_CheckBoxCreator.Invoke(i)).ToArray();
                m_ControlsAttributesAssinger.Invoke();
            }
            else 
            {
                throw new NotSupportedException("Controls cannot be recreated");
            }
        }

        internal void DelayAssignControlAttributes(Action assigner)
        {
            m_ControlsAttributesAssinger = assigner;
        }

        public override IPropertyManagerPageCheckbox[] Controls => m_Controls;

        public bool Checked
        {
            get => Controls.First().Checked;
            set => ForEach(c => c.Checked = value);
        }

        public string Caption
        {
            get => Controls.First().Caption;
            set => ForEach(c => c.Caption = value);
        }

        public int State
        {
            get => Controls.First().State;
            set => ForEach(c => c.State = value);
        }
    }

    internal class PropertyManagerPageCheckBoxListControl : PropertyManagerPageItemsSourceControl<object, PropertyManagerPageCheckBoxList>
    {
        private enum EnumItemType_e
        {
            Default,
            Combined,
            None
        }

        private class FlagEnumItem : ItemsControlItem
        {
            internal new Enum Value => (Enum)base.Value;
            
            internal Enum[] AffectedFlags { get; }
            internal EnumItemType_e Type { get; }
            
            internal FlagEnumItem(Enum value, string dispName, string description, Enum[] affectedFlags) : base(value, dispName, description)
            {
                AffectedFlags = affectedFlags;

                if (AffectedFlags.Length > 1)
                {
                    Type = EnumItemType_e.Combined;
                }
                else if (AffectedFlags.Length == 0)
                {
                    Type = EnumItemType_e.None;
                }
                else
                {
                    Type = EnumItemType_e.Default;
                }
            }
        }

        private delegate PropertyManagerPageCheckBoxList ControlCreatorDelegate(int id, short controlType, string caption, short leftAlign, int options, string tip);

        protected override event ControlValueChangedDelegate<object> ValueChanged;

        private IReadOnlyList<Enum> m_HiddenFlags;

        private Type m_TargetType;
        private object m_Value;
        private bool m_IsSettingValues;

        private ItemsControlItem[] m_InitialItemsCopy;

        public PropertyManagerPageCheckBoxListControl(SwApplication app, IGroup parentGroup, IIconsCreator iconConv,
            IAttributeSet atts, IMetadata[] metadata, ref int numberOfUsedIds)
            : base(app, parentGroup, iconConv, atts, metadata, swPropertyManagerPageControlType_e.swControlType_Checkbox, ref numberOfUsedIds)
        {
            m_Handler.CheckChanged += OnCheckChanged;
            numberOfUsedIds = Items.Length;
        }

        protected override ItemsControlItem[] CreateEnumItems(Type enumType)
        {
            if (enumType.IsEnum
                && enumType.GetCustomAttribute<FlagsAttribute>() != null)
            {
                var flags = GetEnumFlags(enumType);

                var items = GetEnumValueOrderAsDefined(enumType);

                var itemsList = new List<FlagEnumItem>();
                var hiddenFlagsList = new List<Enum>();

                foreach (Enum item in items)
                {
                    var visible = true;
                    Reflection.EnumExtension.TryGetAttribute<BrowsableAttribute>(item, a => visible = a.Browsable);

                    if (visible)
                    {
                        var affectedFlags = flags.Where(item.HasFlag).ToArray();

                        var name = "";

                        item.TryGetAttribute<DisplayNameAttribute>(a => name = a.DisplayName);

                        if (string.IsNullOrEmpty(name))
                        {
                            name = item.ToString();
                        }

                        var desc = "";

                        item.TryGetAttribute<DescriptionAttribute>(a => desc = a.Description);

                        itemsList.Add(new FlagEnumItem(item, name, desc, affectedFlags));
                    }
                    else
                    {
                        hiddenFlagsList.Add(item);
                    }
                }

                m_HiddenFlags = hiddenFlagsList;
                return itemsList.ToArray();
            }
            else 
            {
                throw new NotSupportedException("Only flag enums are supported");
            }
        }

        protected override void InitData(IControlOptionsAttribute opts, IAttributeSet atts)
        {
            m_TargetType = atts.ContextType;

            if (!(m_TargetType.IsEnum && m_TargetType.GetCustomAttribute<FlagsAttribute>() != null)
                && !typeof(IList).IsAssignableFrom(m_TargetType))
            {
                throw new NotSupportedException($"Context type '{atts.ContextType.FullName}' must be either flag enum or list");
            }
        }

        protected override PropertyManagerPageCheckBoxList Create(IGroup host, int id, string name, ControlLeftAlign_e align,
            AddControlOptions_e options, string description, swPropertyManagerPageControlType_e type)
            => new PropertyManagerPageCheckBoxList(i => CreateSwControl<IPropertyManagerPageCheckbox>(host, id + Array.IndexOf(Items, i), i.DisplayName,
                align, options, string.IsNullOrEmpty(i.Description) ? description : i.Description, type));

        protected override void AssignControlAttributes(PropertyManagerPageCheckBoxList ctrl, IControlOptionsAttribute opts, IAttributeSet atts)
        {
            ctrl.DelayAssignControlAttributes(() => base.AssignControlAttributes(ctrl, opts, atts));
        }

        protected override void SetOptions(PropertyManagerPageCheckBoxList ctrl, IControlOptionsAttribute opts, IAttributeSet atts)
        {
            if (atts.Has<CheckBoxListOptionsAttribute>())
            {
                var style = atts.Get<CheckBoxListOptionsAttribute>();

                for (int i = 0; i < Items.Length; i++)
                {
                    var item = Items[i];

                    if (item is FlagEnumItem)
                    {
                        var checkBox = ctrl.Controls[i];

                        KnownColor color;

                        switch (((FlagEnumItem)item).Type)
                        {
                            case EnumItemType_e.None:
                                color = style.NoneItemColor;
                                break;

                            case EnumItemType_e.Combined:
                                color = style.CombinedItemColor;
                                break;

                            case EnumItemType_e.Default:
                                color = 0;
                                break;

                            default:
                                throw new NotSupportedException();
                        }

                        if (color != 0)
                        {
                            ((IPropertyManagerPageControl)checkBox).TextColor = ConvertColor(color);
                        }
                    }
                }
            }
        }

        private Enum[] GetEnumFlags(Type enumType)
        {
            if (!enumType.IsEnum)
            {
                throw new Exception("Only flag enums are supported");
            }

            var flags = new List<Enum>();

            var flag = 0x1;

            foreach (Enum value in Enum.GetValues(enumType))
            {
                var bits = Convert.ToInt32(value);

                if (bits != 0)
                {
                    while (flag < bits)
                    {
                        flag <<= 1;
                    }
                    if (flag == bits)
                    {
                        flags.Add(value);
                    }
                }
            }

            return flags.ToArray();
        }

        private int GetIndex(int id) => id - Id;

        private void OnCheckChanged(int id, bool value)
        {
            if (!m_IsSettingValues)
            {
                if (id >= Id && id < (Id + Items.Length))
                {
                    var checkedItem = Items[GetIndex(id)];

                    if (checkedItem is FlagEnumItem)
                    {
                        var checkedEnumItem = (FlagEnumItem)checkedItem;

                        if (checkedEnumItem.Type == EnumItemType_e.None)
                        {
                            m_Value = (Enum)Enum.ToObject(checkedItem.Value.GetType(), 0);
                        }
                        else
                        {
                            int val = Convert.ToInt32(m_Value);

                            if (value)
                            {
                                foreach (var flag in checkedEnumItem.AffectedFlags)
                                {
                                    if (!((Enum)m_Value).HasFlag(flag))
                                    {
                                        val += Convert.ToInt32(flag);
                                    }
                                }
                            }
                            else
                            {
                                foreach (var flag in checkedEnumItem.AffectedFlags)
                                {
                                    if (((Enum)m_Value).HasFlag(flag))
                                    {
                                        val -= Convert.ToInt32(flag);
                                    }
                                }
                            }

                            var enumVal = (Enum)Enum.ToObject(checkedItem.Value.GetType(), val);
                            enumVal = RemoveDanglingHiddentEnumValues(enumVal);

                            m_Value = enumVal;
                        }

                        //enum value migth have groups which affect other enums, need to resolve other checkboxes as well
                        SetSpecificValue(m_Value);
                    }
                    else 
                    {
                        var list = (IList)m_Value;

                        if (list == null) 
                        {
                            list = (IList)Activator.CreateInstance(m_TargetType);
                        }

                        if (value)
                        {
                            list.Add(checkedItem.Value);
                        }
                        else 
                        {
                            var index = FindIndex(list, checkedItem.Value);
                            
                            if (index != -1)
                            {
                                list.RemoveAt(index);
                            }
                            else 
                            {
                                System.Diagnostics.Debug.Assert(false, "Item is not found");
                            }
                        }

                        m_Value = list;
                    }

                    ValueChanged?.Invoke(this, m_Value);
                }
            }
        }

        protected override object GetSpecificValue() => ComposeValueFromCheckBoxes();

        private object ComposeValueFromCheckBoxes() 
        {
            if (m_TargetType.IsEnum && m_TargetType.GetCustomAttribute<FlagsAttribute>() != null)
            {
                int val = 0;

                for (int i = 0; i < Items.Length; i++)
                {
                    var item = Items[i];
                    var checkBox = SwSpecificControl.Controls[i];

                    if (checkBox.Checked) 
                    {
                        val += Convert.ToInt32(item.Value);
                    }
                }

                m_Value = val;
            }
            else if (typeof(IList).IsAssignableFrom(m_TargetType))
            {
                var list = (IList)m_Value;

                if (list == null)
                {
                    list = (IList)Activator.CreateInstance(m_TargetType);
                }

                for (int i = 0; i < Items.Length; i++)
                {
                    var item = Items[i];
                    var checkBox = SwSpecificControl.Controls[i];

                    if (checkBox.Checked)
                    {
                        list.Add(item.Value);
                    }
                }

                m_Value = list;
            }
            else 
            {
                throw new NotSupportedException();

            }

            return m_Value;
        }

        protected override void SetSpecificValue(object value)
        {
            m_Value = value;

            try
            {
                m_IsSettingValues = true;

                for (int i = 0; i < Items.Length; i++) 
                {
                    var item = Items[i];
                    var checkBox = SwSpecificControl.Controls[i];

                    if (item is FlagEnumItem)
                    {
                        var enumItem = (FlagEnumItem)item;

                        if (enumItem.Type == EnumItemType_e.None)
                        {
                            checkBox.Checked = IsNone((Enum)value);
                        }
                        else
                        {
                            checkBox.Checked = ((Enum)value).HasFlag(enumItem.Value);
                        }
                    }
                    else 
                    {
                        var list = (IList)m_Value;
                        
                        checkBox.Checked = FindIndex(list, item.Value) != -1;
                    }
                }
            }
            finally
            {
                m_IsSettingValues = false;
            }
        }

        private int FindIndex(IList list, object val) 
        {
            if (list != null)
            {
                for(int i = 0; i < list.Count; i++)
                {
                    var elem = list[i];

                    if (m_EqualityComparer.Equals(elem, val))
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        private bool IsNone(Enum val) => Convert.ToInt32(val) == 0;

        private Enum RemoveDanglingHiddentEnumValues(Enum enumVal)
        {
            foreach (var hiddenItem in m_HiddenFlags)
            {
                var hiddenItemsGroup = Items.Cast<FlagEnumItem>().Where(i => i.Value.HasFlag(hiddenItem));

                if (enumVal.HasFlag(hiddenItem) && !hiddenItemsGroup.Any(g => enumVal.HasFlag(g.Value)))
                {
                    var val = Convert.ToInt32(enumVal);
                    val -= Convert.ToInt32(hiddenItem);
                    enumVal = (Enum)Enum.ToObject(m_Value.GetType(), val);
                }
            }

            return enumVal;
        }

        private Array GetEnumValueOrderAsDefined(Type enumType)
        {
            var fields = enumType.GetFields(BindingFlags.Static | BindingFlags.Public);
            return Array.ConvertAll(fields, x => (Enum)x.GetValue(null));
        }

        protected override ItemsControlItem[] LoadInitialItems(IAttributeSet atts, bool isStatic, ItemsControlItem[] items)
        {
            m_InitialItemsCopy = items?.ToArray();

            SwSpecificControl.CreateControls(items);
            return items;
        }

        protected override void LoadItemsIntoControl(ItemsControlItem[] newItems)
        {
            if (Items != newItems || !CompareToInitialItems(newItems))
            {
                throw new Exception("Cannot create the control for changed items. CheckBoxList control does not allow dynamic changing of the items. For the dynamic items use the static items source property and initiate it with items");
            }
        }

        private bool CompareToInitialItems(ItemsControlItem[] newItems) 
        {
            if (newItems == null && m_InitialItemsCopy == null)
            {
                return true;
            }
            else if (newItems == null || m_InitialItemsCopy == null)
            {
                return false;
            }
            else if (newItems.Length != m_InitialItemsCopy.Length)
            {
                return false;
            }
            else 
            {
                for (int i = 0; i < m_InitialItemsCopy.Length; i++) 
                {
                    if (!m_EqualityComparer.Equals(m_InitialItemsCopy[i].Value, newItems[i].Value)) 
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        protected override void SetItemDisplayName(ItemsControlItem item, int index, string newDispName)
        {
            if (index != -1 && SwSpecificControl.Controls.Length > index)
            {
                SwSpecificControl.Controls[index].Caption = newDispName;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_Handler.CheckChanged -= OnCheckChanged;
            }
        }
    }
}