//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Xarial.XCad.Toolkit.PageBuilder.Binders;
using Xarial.XCad.Toolkit.PageBuilder.Exceptions;
using Xarial.XCad.UI.Exceptions;
using Xarial.XCad.UI.PropertyPage.Attributes;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Delegates;
using Xarial.XCad.Utils.PageBuilder.Base;
using Xarial.XCad.Utils.PageBuilder.Core;

namespace Xarial.XCad.Utils.PageBuilder.Binders
{
    public class TypeDataBinder : IDataModelBinder
    {
        public void Bind<TDataModel>(CreateBindingPageDelegate pageCreator,
            CreateBindingControlDelegate ctrlCreator, CreateDynamicControlsDelegate dynCtrlDescCreator,
            out IEnumerable<IBinding> bindings, out IRawDependencyGroup dependencies)
        {
            var type = typeof(TDataModel);

            var bindingsList = new List<IBinding>();
            bindings = bindingsList;

            var pageAttSet = GetAttributeSet(type, -1);

            OnGetPageAttributeSet(type, ref pageAttSet);

            var page = pageCreator.Invoke(pageAttSet);

            var firstCtrlId = 0;

            dependencies = new RawDependencyGroup();

            TraverseType<TDataModel>(type, new List<IControlDescriptor>(),
                ctrlCreator, dynCtrlDescCreator, page, bindingsList, dependencies, ref firstCtrlId);

            OnBeforeControlsDataLoad(bindings);
        }

        protected virtual void OnBeforeControlsDataLoad(IEnumerable<IBinding> bindings)
        {
        }

        protected virtual void OnGetPageAttributeSet(Type pageType, ref IAttributeSet attSet)
        {
        }

        private IAttributeSet CreateAttributeSet(int ctrlId, string ctrlName,
            string desc, Type boundType, IAttribute[] atts, object tag, IControlDescriptor ctrlDescriptor = null)
        {
            var attsSet = new AttributeSet(ctrlId, ctrlName, desc, boundType, tag, ctrlDescriptor);

            if (atts?.Any() == true)
            {
                foreach (var att in atts)
                {
                    attsSet.Add(att);
                }
            }

            return attsSet;
        }

        private IAttributeSet GetAttributeSet(IControlDescriptor prp, int ctrlId)
        {
            string name;
            string desc;
            object tag;

            var type = prp.DataType;

            var typeAtts = (type.GetCustomAttributes(true) ?? new object[0]).OfType<IAttribute>();

            var prpAtts = prp.Attributes ?? new IAttribute[0];

            name = prp.DisplayName;
            desc = prp.Description;
            tag = prpAtts.OfType<IControlTagAttribute>().FirstOrDefault()?.Tag;

            if (string.IsNullOrEmpty(name))
            {
                name = prp.Name;
            }

            return CreateAttributeSet(ctrlId, name, desc, type, prpAtts.Union(typeAtts).ToArray(), tag, prp);
        }

        private IAttributeSet GetAttributeSet(Type type, int ctrlId)
        {
            var customAtts = type.GetCustomAttributes(true) ?? new object[0];

            var typeAtts = customAtts.OfType<IAttribute>();

            var name = customAtts.OfType<DisplayNameAttribute>().FirstOrDefault()?.DisplayName;
            var desc = customAtts.OfType<DescriptionAttribute>().FirstOrDefault()?.Description;
            var tag = customAtts.OfType<IControlTagAttribute>().FirstOrDefault()?.Tag;

            if (string.IsNullOrEmpty(name))
            {
                name = type.Name;
            }

            return CreateAttributeSet(ctrlId, name, desc, type, typeAtts.ToArray(), tag);
        }

        private void TraverseType<TDataModel>(Type type, List<IControlDescriptor> parents,
                    CreateBindingControlDelegate ctrlCreator, CreateDynamicControlsDelegate dynCtrlDescCreator,
                    IGroup parentCtrl, List<IBinding> bindings, IRawDependencyGroup dependencies, ref int nextCtrlId)
        {
            var metadata = new Dictionary<object, PropertyInfoMetadata>();
            CollectMetadata(type, metadata, new PropertyInfo[0]);

            foreach (var prp in type.GetProperties())
            {
                IControlDescriptor[] ctrlDescriptors;

                var dynCtrlAtt = prp.GetCustomAttribute<DynamicControlsAttribute>();

                if (dynCtrlAtt != null)
                {
                    if (dynCtrlDescCreator != null)
                    {
                        ctrlDescriptors = dynCtrlDescCreator.Invoke(dynCtrlAtt.Tag) ?? new IControlDescriptor[0];
                    }
                    else 
                    {
                        throw new DynamicControlHandlerMissingException(prp);
                    }

                    ctrlDescriptors = ctrlDescriptors.Select(d => new ControlDescriptorWrapper(d, prp)).ToArray();
                }
                else 
                {
                    ctrlDescriptors = new IControlDescriptor[] 
                    {
                        new PropertyInfoControlDescriptor(prp) 
                    };
                }
                
                foreach(var ctrlDesc in ctrlDescriptors) 
                {
                    var prpType = ctrlDesc.DataType;

                    var atts = GetAttributeSet(ctrlDesc, nextCtrlId);

                    if (!atts.Has<IIgnoreBindingAttribute>() && !atts.Has<IMetadataAttribute>())
                    {
                        PropertyInfoMetadata prpMetadata = null;

                        if (atts.Has<IHasMetadataAttribute>())
                        {
                            var metadataTag = atts.Get<IHasMetadataAttribute>().MetadataTag;

                            if (metadataTag != null)
                            {
                                if (!metadata.TryGetValue(metadataTag, out prpMetadata))
                                {
                                    throw new MissingMetadataException(metadataTag, ctrlDesc);
                                }
                            }
                        }

                        int idRange;
                        var ctrl = ctrlCreator.Invoke(prpType, atts, parentCtrl, prpMetadata, out idRange);
                        nextCtrlId += idRange;

                        var binding = new PropertyInfoBinding<TDataModel>(ctrl, ctrlDesc, parents, prpMetadata);
                        bindings.Add(binding);

                        if (atts.Has<IControlTagAttribute>())
                        {
                            var tag = atts.Get<IControlTagAttribute>().Tag;
                            dependencies.RegisterBindingTag(binding, tag);
                        }

                        if (atts.Has<IDependentOnAttribute>())
                        {
                            var depAtt = atts.Get<IDependentOnAttribute>();

                            if (depAtt.Dependencies?.Any() == true)
                            {
                                dependencies.RegisterDependency(binding,
                                    depAtt.Dependencies, depAtt.DependencyHandler);
                            }
                        }

                        var isGroup = ctrl is IGroup;

                        if (isGroup)
                        {
                            var grpParents = new List<IControlDescriptor>(parents);
                            grpParents.Add(ctrlDesc);
                            TraverseType<TDataModel>(prpType, grpParents, ctrlCreator, dynCtrlDescCreator,
                                ctrl as IGroup, bindings, dependencies, ref nextCtrlId);
                        }
                    }
                }
            }
        }

        private void CollectMetadata(Type type, Dictionary<object, PropertyInfoMetadata> metadata, PropertyInfo[] parents)
        {
            foreach (var prp in type.GetProperties())
            {
                var metadataAtt = prp.GetCustomAttribute<MetadataAttribute>();

                if (metadataAtt != null)
                {
                    if (!metadata.ContainsKey(metadataAtt.Tag))
                    {
                        metadata.Add(metadataAtt.Tag, new PropertyInfoMetadata(prp, parents));
                    }
                    else
                    {
                        throw new DuplicateMetadataTagException(metadataAtt.Tag);
                    }
                }

                var prpType = prp.PropertyType;

                if (!prpType.IsPrimitive
                    && !prpType.IsEnum
                    && !prpType.IsArray
                    && !typeof(Delegate).IsAssignableFrom(prpType)
                    && !typeof(IEnumerable).IsAssignableFrom(prpType)
                    && !typeof(IXObject).IsAssignableFrom(prpType))
                {
                    CollectMetadata(prpType, metadata, parents.Union(new PropertyInfo[] { prp }).ToArray());
                }
            }
        }
    }
}