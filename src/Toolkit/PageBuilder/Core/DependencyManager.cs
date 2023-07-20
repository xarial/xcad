//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
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
    public class DependencyManager : IDependencyManager
    {
        private class ControlUpdateStateData
        {
            private readonly IBinding m_Source;
            private readonly IBinding[] m_Dependencies;
            private readonly IDependencyHandler m_Handler;
            private readonly IXApplication m_App;

            internal ControlUpdateStateData(IXApplication app, IBinding src, IBinding[] deps, IDependencyHandler handler)
            {
                m_App = app;
                m_Source = src;
                m_Dependencies = deps;
                m_Handler = handler;
            }

            internal void Update()
            {
                m_Handler.UpdateState(m_App, m_Source.Control, m_Dependencies.Select(d => d.Control).ToArray());
            }
        }

        private class MetadataUpdateStateData
        {
            private readonly IControl m_Ctrl;
            private readonly IMetadata[] m_Dependencies;
            private readonly IMetadataDependencyHandler m_Handler;
            private readonly IXApplication m_App;

            internal MetadataUpdateStateData(IXApplication app, IControl ctrl, IMetadata[] deps, IMetadataDependencyHandler handler)
            {
                m_App = app;
                m_Ctrl = ctrl;
                m_Dependencies = deps;
                m_Handler = handler;
            }

            internal void Update()
            {
                m_Handler.UpdateState(m_App, m_Ctrl, m_Dependencies);
            }
        }

        private Dictionary<IBinding, List<ControlUpdateStateData>> m_ControlDependencies;
        private Dictionary<IMetadata, List<MetadataUpdateStateData>> m_MetadataDependencies;

        private IXApplication m_App;

        public ReadOnlyDictionary<IControl, IControl[]> Map { get; private set; }
        
        public void Init(IXApplication app, IRawDependencyGroup depGroup)
        {
            m_App = app;
            m_ControlDependencies = new Dictionary<IBinding, List<ControlUpdateStateData>>();
            m_MetadataDependencies = new Dictionary<IMetadata, List<MetadataUpdateStateData>>();

            foreach (var data in depGroup.DependenciesTags)
            {
                var srcBnd = data.Key;
                var dependOnTags = data.Value.Item1;
                var handler = data.Value.Item2;

                var dependOnBindings = new IBinding[dependOnTags.Length];

                for (int i = 0; i < dependOnTags.Length; i++)
                {
                    var dependOnTag = dependOnTags[i];

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

                    updates.Add(new ControlUpdateStateData(m_App, srcBnd, dependOnBindings, handler));
                }
            }

            foreach (var data in depGroup.MetadataDependencies) 
            {
                var state = new MetadataUpdateStateData(m_App, data.Key, data.Value.Item1, data.Value.Item2);

                foreach (var md in data.Value.Item1) 
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