//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
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
        private class UpdateStateData
        {
            private readonly IBinding m_Source;
            private readonly IBinding[] m_Dependencies;
            private readonly IDependencyHandler m_Handler;
            private readonly IXApplication m_App;

            internal UpdateStateData(IXApplication app, IBinding src, IBinding[] deps, IDependencyHandler handler)
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

        private Dictionary<IBinding, List<UpdateStateData>> m_Dependencies;
        private IXApplication m_App;

        public ReadOnlyDictionary<IControl, IControl[]> Map { get; private set; }
        
        public void Init(IXApplication app, IRawDependencyGroup depGroup)
        {
            m_App = app;
            m_Dependencies = new Dictionary<IBinding, List<UpdateStateData>>();

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
                    List<UpdateStateData> updates;
                    if (!m_Dependencies.TryGetValue(dependOnBinding, out updates))
                    {
                        dependOnBinding.ModelUpdated += OnModelUpdated;

                        updates = new List<UpdateStateData>();
                        m_Dependencies.Add(dependOnBinding, updates);
                    }

                    updates.Add(new UpdateStateData(m_App, srcBnd, dependOnBindings, handler));
                }
            }
        }

        public void UpdateAll()
        {
            foreach (var state in m_Dependencies.SelectMany(b => b.Value))
            {
                state.Update();
            }
        }

        private void OnModelUpdated(IBinding binding)
        {
            m_Dependencies[binding].ForEach(u => u.Update());
        }
    }
}