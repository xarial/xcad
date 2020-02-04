//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using Xarial.XCad.Utils.PageBuilder.Base;

namespace Xarial.XCad.Utils.PageBuilder.Core
{
    public class DependencyManager : IDependencyManager
    {
        private class UpdateStateData
        {
            private IBinding m_Source;
            private IBinding[] m_Dependencies;
            private IDependencyHandler m_Handler;

            internal UpdateStateData(IBinding src, IBinding[] deps, IDependencyHandler handler)
            {
                m_Source = src;
                m_Dependencies = deps;
                m_Handler = handler;
            }

            internal void Update()
            {
                m_Handler.UpdateState(m_Source, m_Dependencies);
            }
        }

        private Dictionary<IBinding, List<UpdateStateData>> m_Dependencies;

        public void Init(IRawDependencyGroup depGroup)
        {
            m_Dependencies = new Dictionary<IBinding, List<UpdateStateData>>();

            var handlersCache = new Dictionary<Type, IDependencyHandler>();

            foreach (var data in depGroup.DependenciesTags)
            {
                var srcBnd = data.Key;
                var dependOnTags = data.Value.Item1;
                var depHandlerType = data.Value.Item2;

                var dependOnBindings = new IBinding[dependOnTags.Length];

                for (int i = 0; i < dependOnTags.Length; i++)
                {
                    var dependOnTag = dependOnTags[i];

                    IBinding dependOnBinding;
                    if (!depGroup.TaggedBindings.TryGetValue(dependOnTag, out dependOnBinding))
                    {
                        throw new Exception("Dependent on binding is not fond for tag");
                    }

                    dependOnBindings[i] = dependOnBinding;
                }

                IDependencyHandler handler;

                if (!handlersCache.TryGetValue(depHandlerType, out handler))
                {
                    handler = Activator.CreateInstance(depHandlerType) as IDependencyHandler;
                    handlersCache.Add(depHandlerType, handler);
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

                    updates.Add(new UpdateStateData(srcBnd, dependOnBindings, handler));
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