//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Linq;
using Xarial.XCad.SolidWorks.Enums;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Controls;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.Reflection;

namespace Xarial.XCad.SolidWorks.UI.PropertyPage.Toolkit.Constructors
{
    internal class PropertyManagerPageOptionBoxConstructor
        : PropertyManagerPageBaseControlConstructor<PropertyManagerPageOptionBoxControl, PropertyManagerPageOptionBox>, IOptionBoxConstructor
    {
        private delegate IPropertyManagerPageOption ControlCreatorDelegate(int id, short controlType, string caption, short leftAlign, int options, string tip);

        public PropertyManagerPageOptionBoxConstructor(ISldWorks app, IIconsCreator iconsConv)
            : base(app, swPropertyManagerPageControlType_e.swControlType_Option, iconsConv)
        {
        }

        protected override PropertyManagerPageOptionBoxControl Create(PropertyManagerPagePage page, IAttributeSet atts, IMetadata[] metadata, ref int numberOfUsedIds)
        {
            numberOfUsedIds = EnumExtension.GetEnumFields(atts.ContextType).Count;
            return base.Create(page, atts, metadata, ref numberOfUsedIds);
        }

        protected override PropertyManagerPageOptionBoxControl Create(PropertyManagerPageGroupBase group, IAttributeSet atts, IMetadata[] metadata, ref int numberOfUsedIds)
        {
            numberOfUsedIds = EnumExtension.GetEnumFields(atts.ContextType).Count;
            return base.Create(group, atts, metadata, ref numberOfUsedIds);
        }

        protected override PropertyManagerPageOptionBoxControl CreateControl(
            PropertyManagerPageOptionBox swCtrl, IAttributeSet atts, IMetadata[] metadata, 
            SwPropertyManagerPageHandler handler, short height, IPropertyManagerPageLabel label)
        {
            var options = EnumExtension.GetEnumFields(atts.ContextType);

            if (atts.Has<OptionBoxOptionsAttribute>())
            {
                var style = atts.Get<OptionBoxOptionsAttribute>();

                if (style.Style != 0)
                {
                    swCtrl.Style = (int)style.Style;
                }
            }

            return new PropertyManagerPageOptionBoxControl(atts.Id, atts.Tag, swCtrl, options.Keys.ToList().AsReadOnly(), handler, label, metadata);
        }

        protected override PropertyManagerPageOptionBox CreateSwControl(object host, ControlOptionsAttribute opts, IAttributeSet atts)
            => CreateOptionBoxControl(opts, atts,
                (int id, short controlType, string caption, short leftAlign, int options, string tip) =>
                {
                    var legacy = !m_App.IsVersionNewerOrEqual(SwVersion_e.Sw2014, 1);

                    switch (host)
                    {
                        case IPropertyManagerPage2 page:
                            if (!legacy)
                            {
                                return page.AddControl2(id, controlType, caption, leftAlign, options, tip) as IPropertyManagerPageOption;
                            }
                            else
                            {
                                return page.AddControl(id, controlType, caption, leftAlign, options, tip) as IPropertyManagerPageOption;
                            }

                        case IPropertyManagerPageTab tab:
                            if (!legacy)
                            {
                                return tab.AddControl2(id, controlType, caption, leftAlign, options, tip) as IPropertyManagerPageOption;
                            }
                            else
                            {
                                return tab.AddControl(id, controlType, caption, leftAlign, options, tip) as IPropertyManagerPageOption;
                            }

                        case IPropertyManagerPageGroup group:
                            if (!legacy)
                            {
                                return group.AddControl2(id, controlType, caption, leftAlign, options, tip) as IPropertyManagerPageOption;
                            }
                            else
                            {
                                return group.AddControl(id, controlType, caption, leftAlign, options, tip) as IPropertyManagerPageOption;
                            }

                        default:
                            throw new NotSupportedException("Host is not supported");
                    }
                });
        
        private PropertyManagerPageOptionBox CreateOptionBoxControl(ControlOptionsAttribute opts, IAttributeSet atts,
            ControlCreatorDelegate creator)
        {
            var options = EnumExtension.GetEnumFields(atts.ContextType);

            var ctrls = new IPropertyManagerPageOption[options.Count];

            for (int i = 0; i < options.Count; i++)
            {
                var name = options.ElementAt(i).Value;
                ctrls[i] = creator.Invoke(atts.Id + i, (short)swPropertyManagerPageControlType_e.swControlType_Option, name,
                    (short)opts.Align, (short)opts.Options, atts.Description);
            }

            return new PropertyManagerPageOptionBox(ctrls);
        }
    }
}