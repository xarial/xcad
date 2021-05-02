//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System.ComponentModel;
using Xarial.XCad.UI.PropertyPage.Base;

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

    internal class PropertyManagerPageGroupControl : PropertyManagerPageGroupBase, IPropertyManagerPageGroupEx
    {
        public IPropertyManagerPageGroup Group { get; private set; }

        /// <summary>
        /// Not supported
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Enabled
        {
            get
            {
                return true;
            }
            set
            {
            }
        }

        /// <inheritdoc/>
        public override bool Visible
        {
            get
            {
                return Group.Visible;
            }
            set
            {
                Group.Visible = value;
            }
        }

        private IMetadata m_ToggleMetadata;

        private readonly bool m_IsCheckable;
        private readonly bool m_Collapse;

        internal PropertyManagerPageGroupControl(int id, object tag, SwPropertyManagerPageHandler handler,
            IPropertyManagerPageGroup group,
            ISldWorks app, PropertyManagerPagePage parentPage, IMetadata metadata, bool isCheckable, bool collapse)
            : base(id, tag, handler, app, parentPage)
        {
            Group = group;
            m_ToggleMetadata = metadata;

            m_IsCheckable = isCheckable;
            m_Collapse = collapse;
            
            Handler.GroupChecked += OnGroupChecked;
            
            if (m_ToggleMetadata != null) 
            {
                m_ToggleMetadata.Changed += OnToggleChanged;
            }

            Handler.Opened += OnPageOpened;
        }

        private void OnPageOpened()
        {
            if (m_IsCheckable) 
            {
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
            }
        }

        private void OnToggleChanged(IMetadata data, object value)
        {
            Group.Checked = (bool)value;
        }
    }
}