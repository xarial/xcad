using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.XCad.SolidWorks.Utils
{
    internal class LazyComponentsIndexer
    {
        private readonly Lazy<IComponent2[]> m_CompsLazy;

        private readonly Lazy<Dictionary<int, IComponent2>> m_IdMap;
        private readonly Lazy<Dictionary<string, IComponent2>> m_NameMap;

        internal LazyComponentsIndexer(Func<IComponent2[]> compsGetter)
        {
            m_CompsLazy = new Lazy<IComponent2[]>(compsGetter);

            m_IdMap = new Lazy<Dictionary<int, IComponent2>>(() => m_CompsLazy.Value.ToDictionary(c => c.GetID()));
            m_NameMap = new Lazy<Dictionary<string, IComponent2>>(() => m_CompsLazy.Value.ToDictionary(c =>
            {
                var fullName = c.Name2;
                return fullName.Substring(fullName.LastIndexOf('/') + 1);
            }, StringComparer.CurrentCultureIgnoreCase));
        }

        internal IComponent2 this[int id]
        {
            get
            {
                if (m_IdMap.Value.TryGetValue(id, out var comp))
                {
                    return comp;
                }
                else
                {
                    throw new Exception($"Failed to get the indexed component by id: {id}");
                }
            }
        }

        internal IComponent2 this[string name]
        {
            get
            {
                if (m_NameMap.Value.TryGetValue(name, out var comp))
                {
                    return comp;
                }
                else
                {
                    throw new Exception($"Failed to get the indexed component by name: {name}");
                }
            }
        }
    }
}
