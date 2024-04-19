using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xarial.XCad.Base;
using Xarial.XCad.Services;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.Toolkit.Data
{
    /// <summary>
    /// Represents row or column table element
    /// </summary>
    public abstract class TableElement : IXTransaction
    {
        private const int DELETED_INDEX = -1;

        public int Index
        {
            get
            {
                if (IsCommitted)
                {
                    CheckDeleted();
                    return m_Creator.Element.Value;
                }
                else
                {
                    return m_Creator.CachedProperties.Get<int>();
                }
            }
            set
            {
                if (IsCommitted)
                {
                    CheckDeleted();
                    Move(value);
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        protected void CheckDeleted()
        {
            if (m_IsDeleted)
            {
                throw new InvalidOperationException("Element is deleted");
            }
        }

        private bool m_IsDeleted;

        protected abstract void Move(int to);

        public bool IsCommitted => m_Creator.IsCreated;

        protected readonly ChangeTracker m_ChangeTracker;

        private readonly ElementCreator<int?> m_Creator;

        protected TableElement(int? index, ChangeTracker changeTracker)
        {
            m_Creator = new ElementCreator<int?>(Create, index, index.HasValue);

            m_ChangeTracker = changeTracker;

            m_ChangeTracker.Inserted += OnInserted;
            m_ChangeTracker.Moved += OnMoved;
            m_ChangeTracker.Deleted += OnDeleted;
        }

        private int? Create(CancellationToken cancellationToken)
        {
            CreateElement(Index, cancellationToken);
            return Index;
        }

        protected abstract void CreateElement(int index, CancellationToken cancellationToken);

        private void OnInserted(int to)
        {
            if (IsCommitted && !m_IsDeleted)
            {
                var index = Index;
                m_ChangeTracker.HandleInserted(to, ref index);
                m_Creator.Set(index);
            }
        }

        private void OnMoved(int from, int to)
        {
            if (IsCommitted && !m_IsDeleted)
            {
                var index = Index;
                m_ChangeTracker.HandleMoved(from, to, ref index);
                m_Creator.Set(index);
            }
        }

        private void OnDeleted(int from)
        {
            if (IsCommitted && !m_IsDeleted)
            {
                var index = Index;

                if (index == from)
                {
                    m_IsDeleted = true;
                    m_Creator.Set(DELETED_INDEX);
                }
                else
                {
                    m_ChangeTracker.HandleDeleted(from, ref index);
                    m_Creator.Set(index);
                }
            }
        }

        public void Commit(CancellationToken cancellationToken)
            => m_Creator.Create(cancellationToken);
    }
}
