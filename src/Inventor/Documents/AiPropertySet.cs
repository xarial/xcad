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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xarial.XCad.Base;
using Xarial.XCad.Data;
using Xarial.XCad.Data.Delegates;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.Inventor.Documents
{
    /// <summary>
    /// Autodesk Inventor property set
    /// </summary>
    public interface IAiPropertySet : IXPropertyRepository 
    {
        /// <summary>
        /// Pointer to the property set
        /// </summary>
        PropertySet PropertySet { get; }
    }

    internal abstract class AiPropertySet : IAiPropertySet
    {
        private const string USER_DEFINED_PRP_SET_NAME = "Inventor User Defined Properties";

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private readonly AiDocument m_Doc;

        protected readonly RepositoryHelper<IXProperty> m_RepoHelper;

        internal AiPropertySet(AiDocument doc) 
        {
            m_Doc = doc;

            m_RepoHelper = new RepositoryHelper<IXProperty>(this);
        }

        public IXProperty this[string name] => m_RepoHelper.Get(name);

        public abstract int Count { get; }

        public PropertySet PropertySet => m_Doc.Document.PropertySets[USER_DEFINED_PRP_SET_NAME];

        public abstract IXObject Owner { get; }

        public void AddRange(IEnumerable<IXProperty> ents, CancellationToken cancellationToken)
            => throw new NotImplementedException();

        public IEnumerable Filter(bool reverseOrder, params RepositoryFilterQuery[] filters)
            => m_RepoHelper.FilterDefault(this, filters, reverseOrder);

        public IEnumerator<IXProperty> GetEnumerator() => EnumerateProperties().GetEnumerator();

        public T PreCreate<T>() where T : IXProperty
            => throw new NotImplementedException();

        public void RemoveRange(IEnumerable<IXProperty> ents, CancellationToken cancellationToken)
            => throw new NotImplementedException();

        public abstract bool TryGet(string name, out IXProperty ent);

        protected abstract IEnumerable<AiProperty> EnumerateProperties();
    }

    internal class AiDocumentPropertySet : AiPropertySet
    {
        private readonly AiDocument m_Doc;

        internal AiDocumentPropertySet(AiDocument doc) : base(doc)
        {
            m_Doc = doc;
        }

        public override int Count => PropertySet.Count;

        public override IXObject Owner => m_Doc;

        public override bool TryGet(string name, out IXProperty ent) => m_RepoHelper.TryFindByName(name, out ent);

        protected override IEnumerable<AiProperty> EnumerateProperties()
        {
            foreach (Property prp in PropertySet) 
            {
                yield return m_Doc.OwnerApplication.CreateObjectFromDispatch<AiProperty>(prp, m_Doc);
            }
        }
    }

    internal class AiAssemblyRowPropertySet : AiDocumentPropertySet
    {
        private readonly AiAssemblyRow m_Row;

        internal AiAssemblyRowPropertySet(AiAssemblyRow row) : base(row.OwnerDocument)
        {
            m_Row = row;
        }

        public override int Count => EnumerateProperties().Count();
        public override IXObject Owner => m_Row;

        public override bool TryGet(string name, out IXProperty ent) => m_RepoHelper.TryFindByName(name, out ent);

        protected override IEnumerable<AiProperty> EnumerateProperties()
        {
            foreach (iAssemblyTableColumn column in m_Row.Row.Parent.TableColumns) 
            {
                var refObj = column.ReferencedObject;

                if (refObj is Property) 
                {
                    var cell = m_Row.Row[column];
                    yield return new AiAssemblyCellProperty((Property)refObj, cell, m_Row.OwnerDocument, m_Row.OwnerApplication);
                }
            }
        }
    }

    internal class AiPartRowPropertySet : AiDocumentPropertySet
    {
        private readonly AiPartRow m_Row;

        internal AiPartRowPropertySet(AiPartRow row) : base(row.OwnerDocument)
        {
            m_Row = row;
        }

        public override int Count => EnumerateProperties().Count();
        public override IXObject Owner => m_Row;

        public override bool TryGet(string name, out IXProperty ent) => m_RepoHelper.TryFindByName(name, out ent);

        protected override IEnumerable<AiProperty> EnumerateProperties()
        {
            foreach (iPartTableColumn column in m_Row.Row.Parent.TableColumns)
            {
                var refObj = column.ReferencedObject;

                if (refObj is Property)
                {
                    var cell = m_Row.Row[column.Index];
                    yield return new AiPartCellProperty((Property)refObj, cell, m_Row.OwnerDocument, m_Row.OwnerApplication);
                }
            }
        }
    }
}
