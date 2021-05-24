//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.PageElements;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls
{
    /// <summary>
    /// Base wrapper around native SOLIDWORKS Property Manager Page controls (i.e. TextBox, SelectionBox, NumberBox etc.)
    /// </summary>
    public interface IPropertyManagerPageControlEx : IControl, IPropertyManagerPageElementEx
    {
        /// <summary>
        /// Pointer to the native SOLIDWORKS control of type <see href="http://help.solidworks.com/2012/english/api/sldworksapi/solidworks.interop.sldworks~solidworks.interop.sldworks.ipropertymanagerpagecontrol.html"/>
        /// </summary>
        IPropertyManagerPageControl SwControl { get; }
    }

    internal abstract class PropertyManagerPageBaseControl<TVal, TSwControl>
        : Control<TVal>, IPropertyManagerPageControlEx
        where TSwControl : class
    {
        protected SwPropertyManagerPageHandler m_Handler;

        protected PropertyManagerPageBaseControl(TSwControl ctrl, int id, object tag, SwPropertyManagerPageHandler handler)
            : base(id, tag)
        {
            SwSpecificControl = ctrl;
            m_Handler = handler;
        }

        protected TSwControl SwSpecificControl { get; private set; }

        public override bool Enabled
        {
            get
            {
                return SwControl.Enabled;
            }
            set
            {
                SwControl.Enabled = value;
            }
        }

        public override bool Visible
        {
            get
            {
                return SwControl.Visible;
            }
            set
            {
                SwControl.Visible = value;
            }
        }

        public IPropertyManagerPageControl SwControl
        {
            get
            {
                if (SwSpecificControl is IPropertyManagerPageControl)
                {
                    return SwSpecificControl as IPropertyManagerPageControl;
                }
                else
                {
                    throw new InvalidCastException(
                        $"Failed to cast {typeof(TSwControl).FullName} to {typeof(IPropertyManagerPageControl).FullName}");
                }
            }
        }
    }
}