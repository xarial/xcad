//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Xarial.XCad.Base;
using Xarial.XCad.Sketch;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.SolidWorks.Sketch
{
    internal class SwSketchBlockInstanceCollection : IXSketchBlockInstanceRepository
    {
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private readonly SwSketchBlockDefinition m_BlockDef;

        private readonly RepositoryHelper<IXSketchBlockInstance> m_RepoHelper;

        internal SwSketchBlockInstanceCollection(SwSketchBlockDefinition blockDef) 
        {
            m_BlockDef = blockDef;
            m_RepoHelper = new RepositoryHelper<IXSketchBlockInstance>(this,
                TransactionFactory<IXSketchBlockInstance>.Create(() => new SwSketchBlockInstance(blockDef, m_BlockDef.OwnerDocument, m_BlockDef.OwnerApplication)));
        }

        public IXSketchBlockInstance this[string name] => m_RepoHelper.Get(name);

        public int Count => m_BlockDef.SketchBlockDefinition.GetInstanceCount();

        public void AddRange(IEnumerable<IXSketchBlockInstance> ents, CancellationToken cancellationToken)
            => m_RepoHelper.AddRange(ents, cancellationToken);

        public IEnumerable Filter(bool reverseOrder, params RepositoryFilterQuery[] filters) 
            => m_RepoHelper.FilterDefault(this, filters, reverseOrder);

        public IEnumerator<IXSketchBlockInstance> GetEnumerator() => IterateInstances().GetEnumerator();

        public T PreCreate<T>() where T : IXSketchBlockInstance
            => m_RepoHelper.PreCreate<T>();

        public void RemoveRange(IEnumerable<IXSketchBlockInstance> ents, CancellationToken cancellationToken)
            => m_RepoHelper.RemoveAll(ents, cancellationToken);

        public bool TryGet(string name, out IXSketchBlockInstance ent)
        {
            if (m_BlockDef.OwnerDocument.Features.TryGet(name, out var feat))
            {
                if (feat is ISwSketchBlockInstance)
                {
                    ent = (ISwSketchBlockInstance)feat;
                    return true;
                }
            }

            ent = null;
            return false;
        }

        private IEnumerable<IXSketchBlockInstance> IterateInstances()
        {
            var instances = (object[])m_BlockDef.SketchBlockDefinition.GetInstances() ?? Array.Empty<object>();

            foreach (ISketchBlockInstance inst in instances)
            {
                yield return m_BlockDef.OwnerDocument.CreateObjectFromDispatch<ISwSketchBlockInstance>(inst);
            }
        }
    }
}
