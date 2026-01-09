//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.swconst;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.SolidWorks.Documents
{
    internal class SwDisplayStateCollection : IXDisplayStateRepository
    {
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IXDisplayState this[string name] => m_RepoHelper.Get(name);

        public IXDisplayState Active 
        {
            get
            {
                //NOTE: as per the documentation, active display state is first in the array
                var activeDispStateName = ((string[])m_Conf.Configuration.GetDisplayStates()).First();
                return GetDisplayState(activeDispStateName);
            }
            set 
            {
                if (!m_Conf.Configuration.ApplyDisplayState(value.Name)) 
                {
                    throw new Exception("Failed to apply display state");
                }
            }
        }

        public int Count => m_Conf.Configuration.GetDisplayStatesCount();

        private readonly SwConfiguration m_Conf;

        private readonly RepositoryHelper<IXDisplayState> m_RepoHelper;

        internal SwDisplayStateCollection(SwConfiguration conf) 
        {
            m_Conf = conf;

            m_RepoHelper = new RepositoryHelper<IXDisplayState>(this,
                TransactionFactory<IXDisplayState>.Create(() => new SwDisplayState(new SwDisplayStatePlaceholderDispatch("", m_Conf), (SwDocument3D)m_Conf.OwnerDocument, m_Conf.OwnerApplication)));
        }

        public void AddRange(IEnumerable<IXDisplayState> ents, CancellationToken cancellationToken) => m_RepoHelper.AddRange(ents, cancellationToken);

        public IEnumerable Filter(bool reverseOrder, params RepositoryFilterQuery[] filters) => m_RepoHelper.FilterDefault(this, filters, reverseOrder);

        public IEnumerator<IXDisplayState> GetEnumerator()
        {
            foreach (var dispStateName in (string[])m_Conf.Configuration.GetDisplayStates())
            {
                yield return GetDisplayState(dispStateName);
            }
        }

        public T PreCreate<T>() where T : IXDisplayState => m_RepoHelper.PreCreate<T>();

        public void RemoveRange(IEnumerable<IXDisplayState> ents, CancellationToken cancellationToken)
        {
            foreach (var dispStateName in ents.Select(e => e.Name).ToArray()) 
            {
                if (!m_Conf.Configuration.DeleteDisplayState(dispStateName)) 
                {
                    throw new Exception("Failed to delete display state");
                }
            }
        }

        public bool TryGet(string name, out IXDisplayState ent)
        {
            if (((string[])m_Conf.Configuration.GetDisplayStates()).Contains(name, StringComparer.CurrentCultureIgnoreCase))
            {
                ent = GetDisplayState(name);
                return true;
            }
            else 
            {
                ent = null;
                return false;
            }
        }

        private SwDisplayState GetDisplayState(string name)
            => new SwDisplayState(new SwDisplayStateDispatch(name, m_Conf), (SwDocument3D)m_Conf.OwnerDocument, m_Conf.OwnerApplication);
    }
}