//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls
{
    /// <summary>
    /// Represents the base interface for elements in property manager page (e.g. controls, groups, tabs)
    /// </summary>
    public interface IPropertyManagerPageElementEx
    {
        /// <summary>
        /// Enable state of this control
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Visibility state of the control
        /// </summary>
        bool Visible { get; set; }
    }

    /// <summary>
    /// List of extension methods of property manager page element
    /// </summary>
    public static class PropertyManagerPageElement
    {
        /// <summary>
        /// Gets the value from the property manager page element
        /// </summary>
        /// <typeparam name="T">Type of the control value</typeparam>
        /// <param name="elem">Property Manager Page element</param>
        /// <returns>Value extracted from the control</returns>
        /// <exception cref="InvalidCastException"/>
        /// <exception cref="NotSupportedException"/>
        public static T GetValue<T>(this IPropertyManagerPageElementEx elem)
        {
            if (elem is IPropertyManagerPageControlEx)
            {
                return (T)(elem as IPropertyManagerPageControlEx).GetValue();
            }
            else
            {
                throw new NotSupportedException($"Currently only {typeof(IPropertyManagerPageControlEx).FullName} has value");
            }
        }

        /// <summary>
        /// Gets the underlying SOLIDWORKS Property Manager Page control
        /// </summary>
        /// <typeparam name="T">Type of control to return</typeparam>
        /// <param name="elem">Property Manager Page element</param>
        /// <returns>Underlying control in the Property Manager Page</returns>
        /// <exception cref="InvalidCastException"/>
        /// <exception cref="NotSupportedException"/>
        /// <remarks>For controls specify <see href="http://help.solidworks.com/2017/English/api/sldworksapi/SolidWorks.Interop.sldworks~SolidWorks.Interop.sldworks.IPropertyManagerPageControl.html">IPropertyManagerPageControl</see> as a generic parameter to return base interface for control.
        /// You can also specify specific interfaces (e.g.  <see href="https://help.solidworks.com/2017/English/api/sldworksapi/SolidWorks.Interop.sldworks~SolidWorks.Interop.sldworks.IPropertyManagerPageNumberbox.html">IPropertyManagerPageNumberbox</see> or <see href="https://help.solidworks.com/2017/English/api/sldworksapi/SolidWorks.Interop.sldworks~SolidWorks.Interop.sldworks.IPropertyManagerPageTextbox.html">IPropertyManagerPageTextbox</see>.
        /// For groups specify <see href="http://help.solidworks.com/2017/English/api/sldworksapi/SolidWorks.Interop.sldworks~SolidWorks.Interop.sldworks.IPropertyManagerPageGroup.html">IPropertyManagerPageGroup</see>.
        /// For tabs specify <see href="http://help.solidworks.com/2017/English/api/sldworksapi/SolidWorks.Interop.sldworks~SolidWorks.Interop.sldworks.IPropertyManagerPageTab.html">IPropertyManagerPageTab</see>
        /// </remarks>
        public static T GetSwControl<T>(this IPropertyManagerPageElementEx elem)
        {
            if (elem is IPropertyManagerPageControlEx)
            {
                return (T)(elem as IPropertyManagerPageControlEx).SwControl;
            }
            else if (elem is IPropertyManagerPageGroupEx)
            {
                return (T)(elem as IPropertyManagerPageGroupEx).Group;
            }
            else if (elem is PropertyManagerPageTabControl)
            {
                return (T)(elem as PropertyManagerPageTabControl).Tab;
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }
}