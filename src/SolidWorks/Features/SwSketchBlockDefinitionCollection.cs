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
using Xarial.XCad.Features;
using Xarial.XCad.Sketch;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Sketch;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.SolidWorks.Features
{
    internal class SwSketchBlockDefinitionCollection : IXSketchBlockDefinitionRepository
    {
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IXSketchBlockDefinition this[string name] => m_RepoHelper.Get(name);

        public int Count => SketchManager.GetSketchBlockDefinitionCount();

        private readonly SwFeatureManager m_FeatMgr;

        private readonly RepositoryHelper<IXSketchBlockDefinition> m_RepoHelper;

        private ISketchManager SketchManager => m_FeatMgr.Document.Model.SketchManager;

        internal SwSketchBlockDefinitionCollection(SwFeatureManager featMgr) 
        {
            m_FeatMgr = featMgr;

            m_RepoHelper = new RepositoryHelper<IXSketchBlockDefinition>(this);
        }

        public void AddRange(IEnumerable<IXSketchBlockDefinition> ents, CancellationToken cancellationToken)
            => m_RepoHelper.AddRange(ents, cancellationToken);

        public IEnumerable Filter(bool reverseOrder, params RepositoryFilterQuery[] filters)
            => m_RepoHelper.FilterDefault(this, filters, reverseOrder);

        public IEnumerator<IXSketchBlockDefinition> GetEnumerator()
        {
            foreach (ISketchBlockDefinition skBlockDef in (object[])SketchManager.GetSketchBlockDefinitions() ?? Array.Empty<object>()) 
            {
                yield return m_FeatMgr.Document.CreateObjectFromDispatch<ISwSketchBlockDefinition>(skBlockDef);
            }
        }

        public T PreCreate<T>() where T : IXSketchBlockDefinition
            => m_RepoHelper.PreCreate<T>();

        public void RemoveRange(IEnumerable<IXSketchBlockDefinition> ents, CancellationToken cancellationToken)
            => m_RepoHelper.RemoveAll(ents, cancellationToken);

        public bool TryGet(string name, out IXSketchBlockDefinition ent)
        {
            if (m_FeatMgr.TryGet(name, out var feat)) 
            {
                if (feat is ISwSketchBlockDefinition) 
                {
                    ent = (ISwSketchBlockDefinition)feat;
                    return true;
                }
            }

            ent = null;
            return false;
        }
    }
}