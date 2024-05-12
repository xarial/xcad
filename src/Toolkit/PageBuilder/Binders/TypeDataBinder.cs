//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Xarial.XCad.Base;
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
        private readonly IXLogger m_Logger;

        public TypeDataBinder(IXLogger logger) 
        {
            m_Logger = logger;
        }

        public void Bind<TDataModel>(CreateBindingPageDelegate pageCreator,
            CreateBindingControlDelegate ctrlCreator, CreateDynamicControlsDelegate dynCtrlDescCreator,
            IContextProvider contextProvider,
            out IEnumerable<IBinding> bindings, out IRawDependencyGroup dependencies, out IMetadata[] metadata)
        {
            var type = typeof(TDataModel);

            var bindingsList = new List<IBinding>();
            bindings = bindingsList;

            var pageAttSet = GetAttributeSet(type, -1);

            OnGetPageAttributeSet(type, ref pageAttSet);

            var page = pageCreator.Invoke(pageAttSet);

            var firstCtrlId = 0;

            dependencies = new RawDependencyGroup();

            var metadataMap = new Dictionary<object, PropertyInfoMetadata>();
            CollectMetadata(type, metadataMap, new PropertyInfo[0], new List<Type>(), contextProvider);

            TraverseType<TDataModel>(type, new List<IControlDescriptor>(),
                ctrlCreator, dynCtrlDescCreator, page, metadataMap, bindingsList, dependencies, contextProvider, ref firstCtrlId);

            metadata = metadataMap.Values.ToArray();

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
                    IGroup parentCtrl, IReadOnlyDictionary<object, PropertyInfoMetadata> metadata,
                    List<IBinding> bindings, IRawDependencyGroup dependencies, IContextProvider contextProvider, ref int nextCtrlId)
        {
            foreach (var prp in type.GetProperties().OrderBy(p => 
            {
                var orderAtt = p.GetCustomAttribute<OrderAttribute>();

                if (orderAtt != null)
                {
                    return orderAtt.Order;
                }
                else 
                {
                    return 0;
                }
            }))
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
                        var prpMetadata = new List<IMetadata>();

                        if (atts.Has<IHasMetadataAttribute>())
                        {
                            var metadataTagsAtts = atts.GetAll<IHasMetadataAttribute>();

                            foreach (var metadataTagAtts in metadataTagsAtts)
                            {
                                if (metadataTagAtts.HasMetadata)
                                {
                                    var metadataTag = metadataTagAtts.LinkedMetadataTag;

                                    if (metadataTag != null)
                                    {
                                        if (metadata.TryGetValue(metadataTag, out PropertyInfoMetadata md))
                                        {
                                            prpMetadata.Add(md);
                                        }
                                        else
                                        {
                                            throw new MissingMetadataException(metadataTag, ctrlDesc);
                                        }
                                    }
                                    else 
                                    {
                                        var staticMetadataVal = metadataTagAtts.StaticValue;

                                        if (staticMetadataVal != null)
                                        {
                                            prpMetadata.Add(new StaticMetadata(staticMetadataVal));
                                        }
                                        else 
                                        {
                                            throw new NullReferenceException($"Neither metadata tag nor static value is not set for {ctrlDesc.Name}");
                                        }
                                    }
                                }
                            }
                        }

                        var prpMetadataArr = prpMetadata.ToArray();

                        int numberOfUsedIds;
                        var ctrl = ctrlCreator.Invoke(prpType, atts, parentCtrl, prpMetadataArr, out numberOfUsedIds);
                        
                        if (numberOfUsedIds > 0)
                        {
                            nextCtrlId += numberOfUsedIds;
                        }
                        else 
                        {
                            nextCtrlId++;
                        }

                        var binding = new PropertyInfoBinding<TDataModel>(ctrl, ctrlDesc, parents, prpMetadataArr, contextProvider, atts.Has<ISilentBindingAttribute>());
                        bindings.Add(binding);

                        if (atts.Has<IControlTagAttribute>())
                        {
                            var tag = atts.Get<IControlTagAttribute>().Tag;
                            dependencies.RegisterBindingTag(binding, tag);
                        }

                        if (atts.Has<IDependentOnAttribute>())
                        {
                            foreach (var depAtt in atts.GetAll<IDependentOnAttribute>()) 
                            {
                                if (depAtt.Dependencies?.Any() == true)
                                {
                                    dependencies.RegisterDependency(binding,
                                        depAtt.Dependencies, depAtt.DependencyHandler);
                                }
                            }
                        }

                        if (atts.Has<IDependentOnMetadataAttribute>())
                        {
                            var depAtt = atts.Get<IDependentOnMetadataAttribute>();
                            
                            var depMds = depAtt.Dependencies.Select(t => 
                            {
                                if (!metadata.TryGetValue(t, out PropertyInfoMetadata md)) 
                                {
                                    throw new MissingMetadataException(t, ctrlDesc);
                                }

                                return md;
                            }).ToArray();

                            dependencies.RegisterMetadataDependency(ctrl, depMds, depAtt.DependencyHandler);
                        }

                        var isGroup = ctrl is IGroup;

                        if (isGroup)
                        {
                            var grpParents = new List<IControlDescriptor>(parents);
                            grpParents.Add(ctrlDesc);
                            TraverseType<TDataModel>(prpType, grpParents, ctrlCreator, dynCtrlDescCreator,
                                ctrl as IGroup, metadata, bindings, dependencies, contextProvider, ref nextCtrlId);
                        }
                    }
                }
            }
        }

        private void CollectMetadata(Type type, Dictionary<object, PropertyInfoMetadata> metadata,
            PropertyInfo[] parents, List<Type> processedTypes, IContextProvider contextProvider)
        {
            foreach (var prp in type.GetProperties())
            {
                var metadataAtt = prp.GetCustomAttribute<MetadataAttribute>();

                if (metadataAtt != null)
                {
                    if (!metadata.ContainsKey(metadataAtt.Tag))
                    {
                        metadata.Add(metadataAtt.Tag, new PropertyInfoMetadata(prp, parents, metadataAtt.Tag, contextProvider));
                    }
                    else
                    {
                        throw new DuplicateMetadataTagException(metadataAtt.Tag);
                    }
                }

                var prpType = prp.PropertyType;

                if (!processedTypes.Contains(prpType))
                {
                    if (!prpType.IsPrimitive
                        && !prpType.IsEnum
                        && !prpType.IsArray
                        && !typeof(Delegate).IsAssignableFrom(prpType)
                        && !typeof(IEnumerable).IsAssignableFrom(prpType)
                        && !typeof(IXObject).IsAssignableFrom(prpType))
                    {
                        processedTypes.Add(prpType);
                        CollectMetadata(prpType, metadata, parents.Union(new PropertyInfo[] { prp }).ToArray(), processedTypes, contextProvider);
                    }
                }
                else
                {
                    m_Logger.Log($"Type '{prpType.FullName}' is skipped as it was already processed while extracting metadata", XCad.Base.Enums.LoggerMessageSeverity_e.Debug);
                }
            }
        }
    }
}