﻿//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.UI.PropertyPage.Services;
using Xarial.XCad.Utils.PageBuilder.Base;

namespace Xarial.XCad.Utils.PageBuilder.Core
{
    /// <inheritdoc/>
    public class DependencyManager : IDependencyManager
    {
        private class ControlUpdateStateData
        {
            private readonly IBinding m_Source;
            private readonly IControl[] m_Controls;
            private readonly object m_Parameter;
            private readonly IDependencyHandler m_Handler;
            private readonly IXApplication m_App;

            internal ControlUpdateStateData(IXApplication app, IBinding src, IBinding[] deps, object parameter, IDependencyHandler handler)
            {
                m_App = app;
                m_Source = src;
                m_Controls = deps.Select(d => d.Control).ToArray();
                m_Parameter = parameter;
                m_Handler = handler;
            }

            internal void Update()
            {
                m_Handler.UpdateState(m_App, m_Source.Control, m_Controls, m_Parameter);
            }
        }

        private class MetadataUpdateStateData
        {
            private readonly IControl m_Ctrl;
            private readonly IMetadata[] m_Dependencies;
            private readonly IMetadataDependencyHandler m_Handler;
            private readonly IXApplication m_App;
            private readonly object m_Parameter;

            internal MetadataUpdateStateData(IXApplication app, IControl ctrl, IMetadata[] deps, object parameter, IMetadataDependencyHandler handler)
            {
                m_App = app;
                m_Ctrl = ctrl;
                m_Dependencies = deps;
                m_Parameter = parameter;
                m_Handler = handler;
            }

            internal void Update()
            {
                m_Handler.UpdateState(m_App, m_Ctrl, m_Dependencies, m_Parameter);
            }
        }

        private Dictionary<IBinding, List<ControlUpdateStateData>> m_ControlDependencies;
        private Dictionary<IMetadata, List<MetadataUpdateStateData>> m_MetadataDependencies;

        private IXApplication m_App;

        /// <inheritdoc/>
        public void Init(IXApplication app, IRawDependencyGroup depGroup)
        {
            m_App = app;
            m_ControlDependencies = new Dictionary<IBinding, List<ControlUpdateStateData>>();
            m_MetadataDependencies = new Dictionary<IMetadata, List<MetadataUpdateStateData>>();

            foreach (var depInfo in depGroup.DependenciesTags)
            {
                var dependOnBindings = new IBinding[depInfo.DependentOnTags.Length];

                for (int i = 0; i < depInfo.DependentOnTags.Length; i++)
                {
                    var dependOnTag = depInfo.DependentOnTags[i];

                    IBinding dependOnBinding;
                    if (!depGroup.TaggedBindings.TryGetValue(dependOnTag, out dependOnBinding))
                    {
                        throw new Exception("Dependent on binding is not found for tag");
                    }

                    dependOnBindings[i] = dependOnBinding;
                }

                foreach (var dependOnBinding in dependOnBindings)
                {
                    List<ControlUpdateStateData> updates;
                    if (!m_ControlDependencies.TryGetValue(dependOnBinding, out updates))
                    {
                        dependOnBinding.Changed += OnBindingChanged;

                        updates = new List<ControlUpdateStateData>();
                        m_ControlDependencies.Add(dependOnBinding, updates);
                    }

                    updates.Add(new ControlUpdateStateData(m_App, depInfo.Binding, dependOnBindings, depInfo.Parameter, depInfo.DependencyHandler));
                }
            }

            foreach (var metDepInfo in depGroup.MetadataDependencies) 
            {
                var state = new MetadataUpdateStateData(m_App, metDepInfo.Control,
                    metDepInfo.Metadata, metDepInfo.Parameter, metDepInfo.DependencyHandler);

                foreach (var md in metDepInfo.Metadata) 
                {
                    if (!m_MetadataDependencies.TryGetValue(md, out List<MetadataUpdateStateData> states))
                    {
                        md.Changed += OnMetadataChanged;
                        states = new List<MetadataUpdateStateData>();
                        m_MetadataDependencies.Add(md, states);
                    }

                    states.Add(state);
                }
            }
        }

        /// <inheritdoc/>
        public void UpdateAll()
        {
            foreach (var state in m_ControlDependencies.SelectMany(b => b.Value))
            {
                state.Update();
            }

            foreach (var state in m_MetadataDependencies.SelectMany(b => b.Value))
            {
                state.Update();
            }
        }

        private void OnMetadataChanged(IMetadata metadata, object val)
            => m_MetadataDependencies[metadata].ForEach(s => s.Update());

        private void OnBindingChanged(IBinding binding)
            => m_ControlDependencies[binding].ForEach(u => u.Update());
    }
}