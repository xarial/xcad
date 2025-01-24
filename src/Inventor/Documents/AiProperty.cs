//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Inventor;
using System;
using System.Diagnostics;
using System.Threading;
using Xarial.XCad.Data;
using Xarial.XCad.Data.Delegates;

namespace Xarial.XCad.Inventor.Documents
{
    public interface IAiProperty : IXProperty
    {
        Property Property { get; }
    }

    public interface IAiPartCellProperty : IAiProperty 
    {
        iPartTableCell Cell { get; }
    }

    public interface IAiAssemblyCellProperty : IAiProperty
    {
        iAssemblyTableCell Cell { get; }
    }

    [DebuggerDisplay("{" + nameof(Name) + "} = {" + nameof(Value) + "} ({" + nameof(Expression) + "})")]
    internal class AiProperty : AiObject, IAiProperty
    {
        public event PropertyValueChangedDelegate ValueChanged;

        internal AiProperty (Property prp, AiDocument doc, AiApplication app) : base(prp, doc, app)
        {
            Property = prp;
        }

        public Property Property { get; }

        public string Name 
        {
            get => Property.Name; 
            set => throw new NotSupportedException(); 
        }

        public virtual object Value 
        {
            get => Property.Value; 
            set => Property.Value = value; 
        }

        public string Expression 
        {
            get => Property.Expression; 
            set => Property.Expression = value; 
        }

        public override bool IsCommitted => true;
    }

    internal class AiPartCellProperty : AiProperty, IAiPartCellProperty
    {
        public iPartTableCell Cell { get; }

        internal AiPartCellProperty(Property prp, iPartTableCell cell, AiDocument doc, AiApplication app) : base(prp, doc, app)
        {
            Cell = cell;
        }

        public override object Value 
        {
            get => Cell.Value; 
            set => Cell.Value = value?.ToString(); 
        }
    }

    internal class AiAssemblyCellProperty : AiProperty, IAiAssemblyCellProperty
    {
        public iAssemblyTableCell Cell { get; }

        internal AiAssemblyCellProperty(Property prp, iAssemblyTableCell cell, AiDocument doc, AiApplication app) : base(prp, doc, app)
        {
            Cell = cell;
        }

        public override object Value
        {
            get => Cell.Value;
            set => Cell.Value = value?.ToString();
        }
    }
}
