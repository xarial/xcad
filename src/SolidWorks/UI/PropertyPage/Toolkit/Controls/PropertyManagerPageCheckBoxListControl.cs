//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using Xarial.XCad.Reflection;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Enums;
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
        public PropertyManagerPageCheckBoxList(IPropertyManagerPageCheckbox[] ctrls) : base(ctrls)
        {
        }

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

    internal class PropertyManagerPageCheckBoxListControl : PropertyManagerPageBaseControl<Enum, PropertyManagerPageCheckBoxList>
    {
        private enum EnumItemType_e
        {
            Default,
            Combined,
            None
        }

        private class FlagEnumItem 
        {
            internal Enum Value { get; }
            internal string Name { get; }
            internal string Description { get; }
            internal Enum[] AffectedFlags { get; }
            internal EnumItemType_e Type { get; }
            internal FlagEnumItem(Enum value, string name, string description, Enum[] affectedFlags)
            {
                Value = value;
                Name = name;
                Description = description;
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

        protected override event ControlValueChangedDelegate<Enum> ValueChanged;

        private IReadOnlyList<FlagEnumItem> m_Items;
        private IReadOnlyList<Enum> m_HiddenFlags;

        private Enum m_Value;
        private bool m_IsSettingValues;

        public PropertyManagerPageCheckBoxListControl(SwApplication app, IGroup parentGroup, IIconsCreator iconConv,
            IAttributeSet atts, IMetadata[] metadata, ref int numberOfUsedIds)
            : base(app, parentGroup, iconConv, atts, metadata, swPropertyManagerPageControlType_e.swControlType_Checkbox, ref numberOfUsedIds)
        {
            m_Handler.CheckChanged += OnCheckChanged;
            numberOfUsedIds = m_Items.Count;
        }

        protected override void InitData(IControlOptionsAttribute opts, IAttributeSet atts)
        {
            var flags = XCad.Utils.Reflection.EnumExtension.GetEnumFlags(atts.ContextType);

            var items = GetEnumValueOrderAsDefined(atts.ContextType);

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

            m_Items = itemsList;
            m_HiddenFlags = hiddenFlagsList;
        }

        protected override PropertyManagerPageCheckBoxList Create(IGroup host, int id, string name, ControlLeftAlign_e align,
            AddControlOptions_e options, string description, swPropertyManagerPageControlType_e type)
        {
            var ctrls = new IPropertyManagerPageCheckbox[m_Items.Count];

            for (int i = 0; i < m_Items.Count; i++)
            {
                var item = m_Items[i];

                ctrls[i] = base.CreateSwControl<IPropertyManagerPageCheckbox>(host, id + i, item.Name,
                    align, options, string.IsNullOrEmpty(item.Description) ? description : item.Description,
                    type);
            }

            return new PropertyManagerPageCheckBoxList(ctrls);
        }

        protected override void SetOptions(PropertyManagerPageCheckBoxList ctrl, IControlOptionsAttribute opts, IAttributeSet atts)
        {
            if (atts.Has<CheckBoxListOptionsAttribute>())
            {
                var style = atts.Get<CheckBoxListOptionsAttribute>();

                for (int i = 0; i < m_Items.Count; i++)
                {
                    var item = m_Items[i];

                    var checkBox = ctrl.Controls[i];

                    KnownColor color;

                    switch (item.Type)
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

        private int GetIndex(int id) => id - Id;

        private void OnCheckChanged(int id, bool value)
        {
            if (!m_IsSettingValues)
            {
                if (id >= Id && id < (Id + m_Items.Count))
                {
                    var checkedItem = m_Items[GetIndex(id)];

                    if (checkedItem.Type == EnumItemType_e.None)
                    {
                        m_Value = (Enum)Enum.ToObject(checkedItem.Value.GetType(), 0);
                    }
                    else
                    {
                        int val = Convert.ToInt32(m_Value);

                        if (value)
                        {
                            foreach (var flag in checkedItem.AffectedFlags)
                            {
                                if (!m_Value.HasFlag(flag))
                                {
                                    val += Convert.ToInt32(flag);
                                }
                            }
                        }
                        else
                        {
                            foreach (var flag in checkedItem.AffectedFlags)
                            {
                                if (m_Value.HasFlag(flag))
                                {
                                    val -= Convert.ToInt32(flag);
                                }
                            }
                        }

                        var enumVal = (Enum)Enum.ToObject(checkedItem.Value.GetType(), val);
                        enumVal = RemoveDanglingHiddentEnumValues(enumVal);

                        m_Value = enumVal;
                    }

                    SetSpecificValue(m_Value);

                    ValueChanged?.Invoke(this, m_Value);
                }
            }
        }

        protected override Enum GetSpecificValue() => m_Value;

        protected override void SetSpecificValue(Enum value)
        {
            m_Value = value;

            try
            {
                m_IsSettingValues = true;

                for (int i = 0; i < m_Items.Count; i++) 
                {
                    var item = m_Items[i];
                    var checkBox = SwSpecificControl.Controls[i];
                    
                    if (item.Type == EnumItemType_e.None)
                    {
                        checkBox.Checked = IsNone(value);
                    }
                    else
                    {
                        checkBox.Checked = value.HasFlag(item.Value);
                    }
                }
            }
            finally
            {
                m_IsSettingValues = false;
            }
        }

        private bool IsNone(Enum val) => Convert.ToInt32(val) == 0;

        private Enum RemoveDanglingHiddentEnumValues(Enum enumVal)
        {
            foreach (var hiddenItem in m_HiddenFlags)
            {
                var hiddenItemsGroup = m_Items.Where(i => i.Value.HasFlag(hiddenItem));

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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_Handler.CheckChanged -= OnCheckChanged;
            }
        }
    }
}