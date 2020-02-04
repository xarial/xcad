//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.Core;

namespace Xarial.XCad.Utils.PageBuilder.Binders
{
    public class TypeDataBinder : IDataModelBinder
    {
        public void Bind<TDataModel>(TDataModel model, CreateBindingPageDelegate pageCreator,
            CreateBindingControlDelegate ctrlCreator,
            out IEnumerable<IBinding> bindings, out IRawDependencyGroup dependencies)
        {
            var type = model.GetType();

            var bindingsList = new List<IBinding>();
            bindings = bindingsList;

            var pageAttSet = GetAttributeSet(type, -1);

            OnGetPageAttributeSet(type, ref pageAttSet);

            var page = pageCreator.Invoke(pageAttSet);

            var firstCtrlId = 0;

            dependencies = new RawDependencyGroup();

            TraverseType(model.GetType(), model, new List<PropertyInfo>(),
                ctrlCreator, page, bindingsList, dependencies, ref firstCtrlId);

            OnBeforeControlsDataLoad(bindings);

            LoadControlsData(bindings);
        }

        protected virtual void OnBeforeControlsDataLoad(IEnumerable<IBinding> bindings)
        {
        }

        protected virtual void OnGetPageAttributeSet(Type pageType, ref IAttributeSet attSet)
        {
        }

        private IAttributeSet CreateAttributeSet(int ctrlId, string ctrlName,
            string desc, Type boundType, IAttribute[] atts, object tag, MemberInfo boundMemberInfo = null)
        {
            var attsSet = new AttributeSet(ctrlId, ctrlName, desc, boundType, tag, boundMemberInfo);

            if (atts?.Any() == true)
            {
                foreach (var att in atts)
                {
                    attsSet.Add(att);
                }
            }

            return attsSet;
        }

        private IAttributeSet GetAttributeSet(PropertyInfo prp, int ctrlId)
        {
            string name;
            string desc;
            object tag;

            var type = prp.PropertyType;

            var typeAtts = ParseAttributes(type.GetCustomAttributes(true), out name, out desc, out tag);

            var prpAtts = ParseAttributes(prp.GetCustomAttributes(true), out name, out desc, out tag);

            if (string.IsNullOrEmpty(name))
            {
                name = prp.Name;
            }

            return CreateAttributeSet(ctrlId, name, desc, type, prpAtts.Union(typeAtts).ToArray(), tag, prp);
        }

        private IAttributeSet GetAttributeSet(Type type, int ctrlId)
        {
            string name;
            string desc;
            object tag;

            var typeAtts = ParseAttributes(type.GetCustomAttributes(true), out name, out desc, out tag);

            if (string.IsNullOrEmpty(name))
            {
                name = type.Name;
            }

            return CreateAttributeSet(ctrlId, name, desc, type, typeAtts.ToArray(), tag);
        }

        private void LoadControlsData(IEnumerable<IBinding> bindings)
        {
            foreach (var binding in bindings)
            {
                binding.UpdateControl();
            }
        }

        private IEnumerable<IAttribute> ParseAttributes(
            object[] customAtts, out string name, out string desc, out object tag)
        {
            name = customAtts?.OfType<DisplayNameAttribute>()?.FirstOrDefault()?.DisplayName;
            desc = customAtts?.OfType<DescriptionAttribute>()?.FirstOrDefault()?.Description;
            tag = customAtts?.OfType<ControlTagAttribute>()?.FirstOrDefault()?.Tag;

            if (customAtts == null)
            {
                return Enumerable.Empty<IAttribute>();
            }
            else
            {
                return customAtts.OfType<IAttribute>();
            }
        }

        private void TraverseType<TDataModel>(Type type, TDataModel model, List<PropertyInfo> parents,
                    CreateBindingControlDelegate ctrlCreator,
            IGroup parentCtrl, List<IBinding> bindings, IRawDependencyGroup dependencies, ref int nextCtrlId)
        {
            foreach (var prp in type.GetProperties())
            {
                var prpType = prp.PropertyType;

                var atts = GetAttributeSet(prp, nextCtrlId);

                if (!atts.Has<IIgnoreBindingAttribute>())
                {
                    int idRange;
                    var ctrl = ctrlCreator.Invoke(prpType, atts, parentCtrl, out idRange);
                    nextCtrlId += idRange;

                    var binding = new PropertyInfoBinding<TDataModel>(model, ctrl, prp, parents);
                    bindings.Add(binding);

                    if (atts.Has<IControlTagAttribute>())
                    {
                        var tag = atts.Get<IControlTagAttribute>().Tag;
                        dependencies.RegisterBindingTag(binding, tag);
                    }

                    if (atts.Has<IDependentOnAttribute>())
                    {
                        var depAtt = atts.Get<IDependentOnAttribute>();
                        dependencies.RegisterDependency(binding,
                            depAtt.Dependencies, depAtt.DependencyHandler);
                    }

                    var isGroup = ctrl is IGroup;

                    if (isGroup)
                    {
                        var grpParents = new List<PropertyInfo>(parents);
                        grpParents.Add(prp);
                        TraverseType(prpType, model, grpParents, ctrlCreator,
                            ctrl as IGroup, bindings, dependencies, ref nextCtrlId);
                    }
                }
            }
        }
    }
}