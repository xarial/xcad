//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
    /// Wrapper class around the group of <see href="http://help.solidworks.com/2016/english/api/sldworksapi/solidworks.interop.sldworks~solidworks.interop.sldworks.ipropertymanagerpageoption.html">IPropertyManagerPageOption </see> controls
    /// </summary>
    /// <remarks>All set properties will be applied to all controls in the group, while get will return the value of first control</remarks>
    public class PropertyManagerPageOptionBox : PropertyManagerPageControlList<IPropertyManagerPageOption>, IPropertyManagerPageOption
    {
        private IPropertyManagerPageOption[] m_Controls;

        private readonly Func<ItemsControlItem, IPropertyManagerPageOption> m_OptionBoxCreator;

        private bool m_IsCreated;

        private Action m_ControlsAttributesAssinger;

        public PropertyManagerPageOptionBox(Func<ItemsControlItem, IPropertyManagerPageOption> optionBoxCreator) : base()
        {
            m_OptionBoxCreator = optionBoxCreator;
            m_IsCreated = false;
        }

        internal void CreateControls(ItemsControlItem[] items)
        {
            if (!m_IsCreated)
            {
                m_IsCreated = true;
                m_Controls = items.Select(i => m_OptionBoxCreator.Invoke(i)).ToArray();
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

        public override IPropertyManagerPageOption[] Controls => m_Controls;

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

        public int Style
        {
            get => Controls.First().Style; 
            set => ForEach(c => c.Style = value);
        }
    }

    internal class PropertyManagerPageOptionBoxControl : PropertyManagerPageItemsSourceControl<object, PropertyManagerPageOptionBox>
    {
        private delegate IPropertyManagerPageOption ControlCreatorDelegate(int id, short controlType, string caption, short leftAlign, int options, string tip);

        protected override event ControlValueChangedDelegate<object> ValueChanged;

        public PropertyManagerPageOptionBoxControl(SwApplication app, IGroup parentGroup, IIconsCreator iconConv,
            IAttributeSet atts, IMetadata[] metadata, ref int numberOfUsedIds)
            : base(app, parentGroup, iconConv, atts, metadata, swPropertyManagerPageControlType_e.swControlType_Option, ref numberOfUsedIds)
        {
            m_Handler.OptionChecked += OnOptionChecked;
            numberOfUsedIds = Items.Length;
        }

        protected override PropertyManagerPageOptionBox Create(IGroup host, int id, string name, ControlLeftAlign_e align,
            AddControlOptions_e options, string description, swPropertyManagerPageControlType_e type)
            => new PropertyManagerPageOptionBox(i =>
            {
                var index = Array.IndexOf(Items, i);

                var optBox = CreateSwControl<IPropertyManagerPageOption>(host, id + index, i.DisplayName, 
                    align, options, string.IsNullOrEmpty(i.Description) ? description : i.Description, type);

                if (index == 0)
                {
                    optBox.Style = (int)swPropMgrPageOptionStyle_e.swPropMgrPageOptionStyle_FirstInGroup;
                }

                return optBox;
            });

        protected override void AssignControlAttributes(PropertyManagerPageOptionBox ctrl, IControlOptionsAttribute opts, IAttributeSet atts)
        {
            ctrl.DelayAssignControlAttributes(() => base.AssignControlAttributes(ctrl, opts, atts));
        }

        protected override void SetOptions(PropertyManagerPageOptionBox ctrl, IControlOptionsAttribute opts, IAttributeSet atts)
        {
            if (atts.Has<OptionBoxOptionsAttribute>())
            {
                var style = atts.Get<OptionBoxOptionsAttribute>();
            }
        }

        private int GetIndex(int id) => id - Id;

        private void OnOptionChecked(int id)
        {
            if (id >= Id && id < (Id + Items.Length))
            {
                ValueChanged?.Invoke(this, Items[GetIndex(id)].Value);
            }
        }

        protected override object GetSpecificValue()
        {
            for (int i = 0; i < SwSpecificControl.Controls.Length; i++)
            {
                if (SwSpecificControl.Controls[i].Checked)
                {
                    return Items[i].Value;
                }
            }

            //TODO: check how this condition works
            return null;
        }

        protected override void SetSpecificValue(object value)
        {
            var index = -1;

            for (int i = 0; i < Items.Length; i++) 
            {
                var item = Items[i];

                if (object.Equals(item.Value, value)) 
                {
                    index = i;
                    break;
                }
            }

            if (index != -1)
            {
                for (int i = 0; i < SwSpecificControl.Controls.Length; i++)
                {
                    SwSpecificControl.Controls[i].Checked = i == index;
                }
            }
            else 
            {
                throw new Exception("Value is not in the source");
            }
        }

        protected override void LoadItemsIntoControl(ItemsControlItem[] newItems)
        {
            SwSpecificControl.CreateControls(newItems);
        }

        protected override void SetStaticItems(IAttributeSet atts, bool isStatic, ItemsControlItem[] staticItems)
        {
            if (isStatic)
            {
                Items = staticItems;
            }
            else
            {
                throw new Exception("Dynamic items are not supported");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_Handler.OptionChecked -= OnOptionChecked;
            }
        }
    }
}