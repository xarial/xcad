//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.ComponentModel;
using System.Linq;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.Toolkit.Services;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.PageElements;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls
{
    /// <summary>
    /// Represents the group box control
    /// </summary>
    public interface IPropertyManagerPageGroupEx
    {
        /// <summary>
        /// Pointer to the underlying group box
        /// </summary>
        IPropertyManagerPageGroup Group { get; }
    }

    internal class PropertyManagerPageGroupControl : PropertyManagerPageGroupBase<IPropertyManagerPageGroup>, IPropertyManagerPageGroupEx
    {
        protected override event ControlValueChangedDelegate<object> ValueChanged;

        /// <summary>
        /// Not supported
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Enabled
        {
            get => true;
            set
            {
            }
        }

        /// <inheritdoc/>
        public override bool Visible
        {
            get => Group.Visible;
            set => Group.Visible = value;
        }

        public IPropertyManagerPageGroup Group => m_SpecificGroup;

        private IMetadata m_ToggleMetadata;

        private bool m_IsCheckable;
        private bool m_Collapse;

        internal PropertyManagerPageGroupControl(SwApplication app, IGroup parentGroup, IAttributeSet atts, IMetadata[] metadata, IIconsCreator iconsConv, ref int numberOfUsedIds)
            : base(app, parentGroup, atts, metadata, iconsConv, ref numberOfUsedIds)
        {
            Handler.GroupChecked += OnGroupChecked;

            if (m_ToggleMetadata != null)
            {
                m_ToggleMetadata.Changed += OnToggleChanged;
            }

            Handler.Opened += OnPageOpened;
        }

        protected override IPropertyManagerPageGroup Create(IGroup host, IAttributeSet atts, IMetadata[] metadata)
        {
            var opts = GetGroupOptions(atts, metadata, out m_ToggleMetadata);

            m_IsCheckable = opts.HasFlag(swAddGroupBoxOptions_e.swGroupBoxOptions_Checkbox);
            m_Collapse = !opts.HasFlag(swAddGroupBoxOptions_e.swGroupBoxOptions_Expanded);

            return ParentPage.AddGroupBox(host, atts.Id, atts.Name, opts);
        }

        private void OnPageOpened()
        {
            if (m_IsCheckable) 
            {
                Group.Checked = (bool)m_ToggleMetadata.Value;

                if (m_Collapse)
                {
                    Group.Expanded = false;
                }
                else 
                {
                    Group.Expanded = Group.Checked;
                }
            }
        }

        private void OnGroupChecked(int id, bool val)
        {
            if (id == Id) 
            {
                m_ToggleMetadata.Value = val;
                ValueChanged?.Invoke(this, val);
            }
        }

        private void OnToggleChanged(IMetadata data, object value)
        {
            Group.Checked = (bool)value;
        }

        private swAddGroupBoxOptions_e GetGroupOptions(IAttributeSet atts, IMetadata[] metadata, out IMetadata toggleMetadata)
        {
            GroupBoxOptions_e opts = 0;

            if (atts.Has<IGroupBoxOptionsAttribute>())
            {
                opts = atts.Get<IGroupBoxOptionsAttribute>().Options;
            }

            var swOpts = swAddGroupBoxOptions_e.swGroupBoxOptions_Visible;

            if (!opts.HasFlag(GroupBoxOptions_e.Collapsed))
            {
                swOpts |= swAddGroupBoxOptions_e.swGroupBoxOptions_Expanded;
            }

            if (atts.Has<ICheckableGroupBoxAttribute>())
            {
                var checkAtt = atts.Get<ICheckableGroupBoxAttribute>();

                swOpts |= swAddGroupBoxOptions_e.swGroupBoxOptions_Checkbox;
                toggleMetadata = metadata?.FirstOrDefault(m => object.Equals(m.Tag, checkAtt.ToggleMetadataTag));

                if (toggleMetadata == null)
                {
                    throw new NullReferenceException($"Failed to find the metadata to drive group toggle: '{checkAtt.ToggleMetadataTag}'");
                }
            }
            else
            {
                toggleMetadata = null;
            }

            return swOpts;
        }
    }
}