using SolidWorks.Interop.sldworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Base;
using Xarial.XCad.Base.Enums;
using Xarial.XCad.SolidWorks.Features;

namespace Xarial.XCad.SolidWorks.Utils
{
    /// <summary>
    /// This is a helper class for the components collection in the feature tree order
    /// </summary>
    /// <remarks>Features can only be traversed in the context of the active configuration, while components are configuration specific
    /// Excess features can be returned from the traverse (e.g. the features which are not used in this configuration or ghost components.
    /// This class is optimizing performance by using the Lazy pattern where possible</remarks>
    internal class OrderedComponentsCollection : IEnumerable<IComponent2>
    {
        private readonly Lazy<IComponent2[]> m_CompsLazy;

        private readonly Lazy<Dictionary<int, IComponent2>> m_IdMap;
        private readonly Lazy<Dictionary<string, IComponent2>> m_NameMap;

        private readonly Lazy<List<IComponent2>> m_CompsStackLazy;

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private readonly IFeature m_FirstFeature;
        private readonly IXLogger m_Logger;

        internal OrderedComponentsCollection(Func<IComponent2[]> compsGetter, IFeature firstFeature, IXLogger logger)
        {
            m_FirstFeature = firstFeature;
            m_Logger = logger;

            m_CompsLazy = new Lazy<IComponent2[]>(compsGetter);

            m_IdMap = new Lazy<Dictionary<int, IComponent2>>(() => m_CompsLazy.Value.ToDictionary(c => c.GetID()));
            m_NameMap = new Lazy<Dictionary<string, IComponent2>>(() => m_CompsLazy.Value.ToDictionary(c =>
            {
                var fullName = c.Name2;
                return fullName.Substring(fullName.LastIndexOf('/') + 1);
            }, StringComparer.CurrentCultureIgnoreCase));

            m_CompsStackLazy = new Lazy<List<IComponent2>>(() => m_CompsLazy.Value.ToList());
        }

        public IEnumerator<IComponent2> GetEnumerator() => IterateOrderedComponents().GetEnumerator();

        private IReadOnlyList<IComponent2> RemainingStack => m_CompsStackLazy.Value;

        private IEnumerable<IComponent2> IterateOrderedComponents() 
        {
            foreach (var feat in IterateFeatureComponents(m_FirstFeature))
            {
                if (TryPull(feat, out var comp))
                {
                    yield return comp;
                }
            }

            if (RemainingStack.Any())
            {
                System.Diagnostics.Debug.Assert(false, "This is a safety method, all feature should have been returned at this point");

                m_Logger.Log($"{RemainingStack.Count}) unclaimed component(s) in the stack", LoggerMessageSeverity_e.Warning);

                foreach (var comp in RemainingStack)
                {
                    yield return comp;
                }
            }
        }

        private bool TryPull(IFeature feat, out IComponent2 comp)
        {
            var featSpecComp = (IComponent2)feat.GetSpecificFeature2();

            if (featSpecComp != null)
            {
                var id = featSpecComp.GetID();
                if (m_IdMap.Value.TryGetValue(id, out comp))
                {
                    m_CompsStackLazy.Value.Remove(comp);
                    return true;
                }
                else 
                {
                    m_Logger.Log($"Failed to get the pointer to component from feature by id: '{id}'", LoggerMessageSeverity_e.Debug);
                    return false;
                }
            }
            else 
            {
                var name = feat.Name;
                if (m_NameMap.Value.TryGetValue(name, out comp))
                {
                    m_CompsStackLazy.Value.Remove(comp);
                    return true;
                }
                else 
                {
                    m_Logger.Log($"Failed to get the pointer to component from feature by name: '{name}'", LoggerMessageSeverity_e.Debug);
                    return false;
                }
            }
        }

        private IEnumerable<IFeature> IterateFeatureComponents(IFeature firstFeat)
        {
            foreach (var feat in FeatureEnumerator.IterateFeatures(firstFeat, true))
            {
                var typeName = feat.GetTypeName2();

                if (typeName == "Reference" || typeName == "ReferencePattern")
                {
                    yield return feat;
                }
            }
        }
    }
}
