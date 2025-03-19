using Inventor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xarial.XCad.Base;
using Xarial.XCad.Geometry;
using Xarial.XCad.Inventor.Documents;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.Inventor.Geometry
{
    internal class AiBodyCollection : IXBodyRepository
    {
        public IXBody this[string name] => m_RepoHelper.Get(name);

        public int Count => m_Part.Part.ComponentDefinition.SurfaceBodies.Count + m_Part.Part.ComponentDefinition.WorkSurfaces.Count;

        private readonly AiPart m_Part;

        private readonly RepositoryHelper<IXBody> m_RepoHelper;

        internal AiBodyCollection(AiPart part) 
        {
            m_Part = part;

            m_RepoHelper = new RepositoryHelper<IXBody>(this);
        }

        public void AddRange(IEnumerable<IXBody> ents, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public IEnumerable Filter(bool reverseOrder, params RepositoryFilterQuery[] filters)
        {
            bool solid;
            bool sheet;

            if (filters?.Any() == true)
            {
                solid = false;
                sheet = false;

                foreach (var filter in filters)
                {
                    if (typeof(IXSolidBody).IsAssignableFrom(filter.Type))
                    {
                        solid = true;
                    }
                    else if (typeof(IXSheetBody).IsAssignableFrom(filter.Type))
                    {
                        sheet = true;
                    }
                    else if (filter.Type == null || typeof(IXBody).IsAssignableFrom(filter.Type))
                    {
                        solid = true;
                        sheet = true;
                        break;
                    }
                }
            }
            else
            {
                solid = true;
                sheet = true;
            }

            foreach (var ent in m_RepoHelper.FilterDefault(IterateBodies(solid, sheet), filters, reverseOrder))
            {
                yield return ent;
            }
        }

        private IEnumerable<AiBody> IterateBodies(bool solid, bool sheet)
        {
            if (solid)
            {
                foreach (SurfaceBody body in m_Part.Part.ComponentDefinition.SurfaceBodies)
                {
                    yield return m_Part.OwnerApplication.CreateObjectFromDispatch<AiSolidBody>(body, m_Part);
                }
            }
            
            if (sheet)
            {
                foreach (WorkSurface surf in m_Part.Part.ComponentDefinition.WorkSurfaces)
                {
                    foreach (SurfaceBody body in surf.SurfaceBodies)
                    {
                        yield return m_Part.OwnerApplication.CreateObjectFromDispatch<AiSheetBody>(body, m_Part);
                    }
                }
            }
        }

        public IEnumerator<IXBody> GetEnumerator() => IterateBodies(true, true).GetEnumerator();

        public T PreCreate<T>() where T : IXBody
        {
            throw new NotImplementedException();
        }

        public void RemoveRange(IEnumerable<IXBody> ents, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public bool TryGet(string name, out IXBody ent)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    }
}
