//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Inventor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.Inventor.Documents
{
    /// <summary>
    /// Represents iParts and iAssemblies
    /// </summary>
    public interface IAiDocumentTable : IXConfigurationRepository
    {
    }

    public interface IAiPartTable : IAiDocumentTable, IXPartConfigurationRepository
    {
        iPartFactory Factory { get; }
    }

    public interface IAiAssemblyTable : IAiDocumentTable, IXAssemblyConfigurationRepository
    {
        iAssemblyFactory Factory { get; }
    }

    internal abstract class AiDocumentTable : IAiDocumentTable
    {
        public event ConfigurationActivatedDelegate ConfigurationActivated;

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        
        public IXConfiguration this[string name] => m_RepoHelper.Get(name);

        private readonly RepositoryHelper<IXConfiguration> m_RepoHelper;

        protected AiDocumentTable() 
        {
            m_RepoHelper = new RepositoryHelper<IXConfiguration>(this);
        }

        public void AddRange(IEnumerable<IXConfiguration> ents, CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public void Refresh() => throw new NotSupportedException();

        public T PreCreate<T>() where T : IXConfiguration
            => throw new NotSupportedException();

        public void RemoveRange(IEnumerable<IXConfiguration> ents, CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public IEnumerable Filter(bool reverseOrder, params RepositoryFilterQuery[] filters)
            => m_RepoHelper.FilterDefault(this, filters, reverseOrder);

        public IEnumerator<IXConfiguration> GetEnumerator() => EnumerateRows().GetEnumerator();

        public bool TryGet(string name, out IXConfiguration ent) => m_RepoHelper.TryFindByName(name, out ent);

        public abstract IXConfiguration Active { get; set; }
        public abstract int Count { get; }

        protected abstract IEnumerable<AiDocumentRow> EnumerateRows();
        protected abstract AiDocumentRow CurrentRow { get; set; }
    }

    internal class AiPartTable : AiDocumentTable, IAiPartTable
    {
        public override IXConfiguration Active 
        {
            get => CurrentRow; 
            set => CurrentRow = (AiDocumentRow)value; 
        }

        IXPartConfiguration IXPartConfigurationRepository.Active
        {
            get => (IXPartConfiguration)CurrentRow;
            set => CurrentRow = (AiDocumentRow)value;
        }

        public iPartFactory Factory => m_Part.Part.ComponentDefinition.iPartFactory;

        public override int Count => Factory?.TableRows?.Count ?? 0;

        protected override AiDocumentRow CurrentRow 
        {
            get => new AiPartRow(Factory.DefaultRow, m_Part);
            set => Factory.DefaultRow = ((AiPartRow)value).Row; 
        }

        private readonly AiPart m_Part;

        internal AiPartTable(AiPart part)
        {
            m_Part = part;
        }

        public IXPartConfiguration PreCreate() => throw new NotSupportedException();

        protected override IEnumerable<AiDocumentRow> EnumerateRows()
        {
            var tableRows = Factory?.TableRows;

            if (tableRows != null)
            {
                foreach (iPartTableRow row in tableRows)
                {
                    yield return new AiPartRow(row, m_Part);
                }
            }
        }
    }

    internal class AiAssemblyTable : AiDocumentTable, IAiAssemblyTable
    {
        public override IXConfiguration Active
        {
            get => CurrentRow;
            set => CurrentRow = (AiDocumentRow)value;
        }

        IXAssemblyConfiguration IXAssemblyConfigurationRepository.Active
        {
            get => (IXAssemblyConfiguration)CurrentRow;
            set => CurrentRow = (AiDocumentRow)value;
        }

        public iAssemblyFactory Factory => m_Assm.Assembly.ComponentDefinition.iAssemblyFactory;

        protected override AiDocumentRow CurrentRow
        {
            get => new AiAssemblyRow(Factory.DefaultRow, m_Assm);
            set => Factory.DefaultRow = ((AiAssemblyRow)value).Row;
        }

        public override int Count => Factory?.TableRows?.Count ?? 0;

        private readonly AiAssembly m_Assm;

        internal AiAssemblyTable(AiAssembly assm)
        {
            m_Assm = assm;
        }

        public IXAssemblyConfiguration PreCreate() => throw new NotSupportedException();

        protected override IEnumerable<AiDocumentRow> EnumerateRows()
        {
            var tableRows = Factory?.TableRows;

            if (tableRows != null)
            {
                foreach (iAssemblyTableRow row in tableRows)
                {
                    yield return new AiAssemblyRow(row, m_Assm);
                }
            }
        }
    }
}
