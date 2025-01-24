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
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xarial.XCad.Annotations;
using Xarial.XCad.Base;
using Xarial.XCad.Data;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Features;
using Xarial.XCad.Features.Delegates;
using Xarial.XCad.Toolkit.Utils;
using Xarial.XCad.UI;

namespace Xarial.XCad.Inventor.Documents
{
    public interface IAiDocumentRow : IXConfiguration
    {
    }

    public interface IAiPartRow : IAiDocumentRow, IXPartConfiguration
    {
        iPartTableRow Row { get; }
    }

    public interface IAiAssemblyRow : IAiDocumentRow, IXAssemblyConfiguration
    {
        iAssemblyTableRow Row { get; }
    }

    [DebuggerDisplay("{" + nameof(Name) + "}")]
    internal abstract class AiDocumentRow : AiObject, IAiDocumentRow
    {
        protected AiDocumentRow(object disp, AiDocument3D doc) : base(disp, doc, doc.OwnerApplication)
        {
        }

        public IXIdentifier Id => throw new NotSupportedException();

        public double Quantity => throw new NotSupportedException();

        public string PartNumber => throw new NotSupportedException();

        public IXConfiguration Parent { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public BomChildrenSolving_e BomChildrenSolving { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public ConfigurationOptions_e Options { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public IXImage Preview => throw new NotSupportedException();

        public bool IsSelected => throw new NotSupportedException();

        public override bool IsCommitted => true;

        public abstract IXPropertyRepository Properties { get; }

        public IXDimensionRepository Dimensions => throw new NotSupportedException();

        public TSelObject ConvertObject<TSelObject>(TSelObject obj)
            where TSelObject : class, IXSelObject => throw new NotSupportedException();

        public override void Commit(CancellationToken cancellationToken) => throw new NotSupportedException();

        public void Delete() => throw new NotSupportedException();

        public void Select(bool append) => throw new NotSupportedException();

        public abstract string Name { get; set; }
        public string Description { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Comment { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }

    internal class AiPartRow : AiDocumentRow, IAiPartRow 
    {
        private readonly iPartTableRow m_Row;

        internal AiPartRow(iPartTableRow row, AiPart part) : base(row, part)
        {
            m_Row = row;
            Properties = new AiPartRowPropertySet(this);
            CutLists = new AiCutLists();
        }

        public iPartTableRow Row => m_Row;
        public override IXPropertyRepository Properties { get; }

        public IXCutListItemRepository CutLists { get; }
        public IXMaterial Material { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        
        public override string Name 
        {
            get => m_Row.MemberName; 
            set => throw new NotSupportedException(); 
        }
    }

    internal class AiAssemblyRow : AiDocumentRow, IAiAssemblyRow
    {
        private readonly iAssemblyTableRow m_Row;

        internal AiAssemblyRow(iAssemblyTableRow row, AiAssembly assm) : base(row, assm)
        {
            m_Row = row;
            Properties = new AiAssemblyRowPropertySet(this);
        }

        public iAssemblyTableRow Row => m_Row;
        public override IXPropertyRepository Properties { get; }

        public override string Name
        {
            get => m_Row.MemberName;
            set => throw new NotSupportedException();
        }

        public IXComponentRepository Components => throw new NotImplementedException();
    }
}
