using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swpublished;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Xarial.XCad.Data;
using Xarial.XCad.Exceptions;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Services;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.UI;
using Xarial.XCad.Utils.Reflection;

namespace Xarial.XCad.SolidWorks.UI
{
    public interface ISwCalloutBase : IXCalloutBase
    {
        ICallout Callout { get; }
    }

    public interface ISwCallout : ISwCalloutBase, IXCallout 
    {
    }

    public interface ISwSelCallout : ISwCalloutBase, IXSelCallout
    {
    }

    [ComVisible(true)]
    public abstract class SwCalloutBaseHandler : ISwCalloutHandler
    {
        internal event Func<SwCalloutBaseHandler, int, string, bool> ValueChanged;

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public bool OnStringValueChanged(object pManipulator, int rowID, string text)
            => ValueChanged?.Invoke(this, rowID, text) == true;
    }

    internal class SwCalloutRow : IXCalloutRow
    {
        public event CalloutRowValueChangedDelegate ValueChanged;
        
        private readonly SwCalloutBase m_Owner;
        private readonly ElementCreator<object> m_Creator;

        private int? m_RowIndex;
        
        internal SwCalloutRow(SwCalloutBase owner) 
        {
            m_Owner = owner;

            m_Creator = new ElementCreator<object>(c => null, null, false);
        }
        
        internal bool HandleValueChanged(string newValue)
            => ValueChanged?.Invoke(m_Owner, this, newValue) != false;

        public bool IsReadOnly
        {
            get
            {
                if (m_Owner.IsCommitted)
                {
                    return m_Owner.Callout.ValueInactive[RowIndex];
                }
                else
                {
                    return m_Creator.CachedProperties.Get<bool>();
                }
            }
            set
            {
                if (!m_Owner.IsCommitted)
                {
                    m_Creator.CachedProperties.Set(value);
                }
                else
                {
                    throw new CommitedElementReadOnlyParameterException();
                }
            }
        }

        public string Name
        {
            get
            {
                if (m_Owner.IsCommitted)
                {
                    return m_Owner.Callout.Label2[RowIndex];
                }
                else
                {
                    return m_Creator.CachedProperties.Get<string>();
                }
            }
            set
            {
                if (!m_Owner.IsCommitted)
                {
                    m_Creator.CachedProperties.Set(value);
                }
                else
                {
                    throw new CommitedElementReadOnlyParameterException();
                }
            }
        }

        public string Value
        {
            get
            {
                if (m_Owner.IsCommitted)
                {
                    return m_Owner.Callout.Value[RowIndex];
                }
                else
                {
                    return m_Creator.CachedProperties.Get<string>();
                }
            }
            set
            {
                if (!m_Owner.IsCommitted)
                {
                    m_Creator.CachedProperties.Set(value);
                }
                else
                {
                    if (!IsReadOnly)
                    {
                        m_Owner.Callout.Value[RowIndex] = value;
                    }
                    else
                    {
                        throw new Exception("Value of the read-only row cannot be modified");
                    }
                }
            }
        }

        private int RowIndex
        {
            get
            {
                if (m_RowIndex.HasValue)
                {
                    return m_RowIndex.Value;
                }
                else
                {
                    if (m_Owner.IsCommitted)
                    {
                        m_RowIndex = Array.IndexOf(m_Owner.Rows, this);

                        if (m_RowIndex.Value != -1)
                        {
                            return m_RowIndex.Value;
                        }
                        else
                        {
                            throw new Exception("This row does not belong to a owner callout");
                        }
                    }
                    else
                    {
                        throw new NonCommittedElementAccessException();
                    }
                }
            }
        }
    }

    internal abstract class SwCalloutBase : ISwCalloutBase
    {
        protected readonly ElementCreator<ICallout> m_Creator;

        public IXCalloutRow[] Rows
        {
            get => m_Creator.CachedProperties.Get<IXCalloutRow[]>();
            set
            {
                if (!IsCommitted)
                {
                    m_Creator.CachedProperties.Set(value);
                }
                else 
                {
                    throw new CommitedElementReadOnlyParameterException();
                }
            }
        }

        public bool IsCommitted => m_Creator.IsCreated;

        public ICallout Callout => m_Creator.Element;

        private readonly SwCalloutBaseHandler m_Handler;

        internal SwCalloutBase(SwCalloutBaseHandler handler) 
        {
            m_Handler = handler;

            ValidateHandler(m_Handler);

            m_Handler.ValueChanged += OnRowValueChanged;

            m_Creator = new ElementCreator<ICallout>(CreateCallout, null, false);
        }

        private void ValidateHandler(SwCalloutBaseHandler handler)
        {
            if (handler == null)
            {
                throw new NullReferenceException("Handler is null");
            }

            var type = handler.GetType();

            if (!type.IsComVisible())
            {
                throw new Exception($"Handler type '{type.FullName}' must be COM visible");
            }

            if (!(type.IsPublic || type.IsNestedPublic))
            {
                throw new Exception($"Handler type '{type.FullName}' must be a public class");
            }
        }

        public void Commit(CancellationToken cancellationToken) => m_Creator.Create(cancellationToken);

        public IXCalloutRow PreCreateRow() => new SwCalloutRow(this);

        public virtual void Show()
        {
            if (!Callout.Display(true)) 
            {
                throw new Exception("Failed to display callout");
            }
        }

        public void Hide()
        {
            if (!Callout.Display(false))
            {
                throw new Exception("Failed to hide callout");
            }
        }

        public void Dispose()
        {
            if (IsCommitted)
            {
                Callout.Display(false);
            }
        }

        private ICallout CreateCallout(CancellationToken cancellationToken) 
        {
            if (Rows?.Any() == true)
            {
                var callout = NewCallout(Rows.Length, m_Handler);

                for (int i = 0; i < Rows.Length; i++) 
                {
                    callout.Label2[i] = Rows[i].Name;
                    callout.Value[i] = Rows[i].Value;
                    callout.ValueInactive[i] = Rows[i].IsReadOnly;
                }

                SetPosition(callout);

                return callout;
            }
            else 
            {
                throw new Exception("No rows specified for this callout");
            }
        }

        protected virtual void SetPosition(ICallout callout) 
        {
        }

        protected abstract ICallout NewCallout(int rowsCount, ISwCalloutHandler handler);

        private bool OnRowValueChanged(SwCalloutBaseHandler handler, int rowIndex, string newValue)
        {
            if (rowIndex < Rows.Length)
            {
                return ((SwCalloutRow)Rows[rowIndex]).HandleValueChanged(newValue);
            }
            else
            {
                throw new IndexOutOfRangeException("Value of the changed callout row is outside of the rows range");
            }
        }
    }

    internal class SwCallout : SwCalloutBase, ISwCallout
    {
        private readonly SwDocument3D m_Doc;
        private readonly IMathUtility m_MathUtils;

        public SwCallout(SwDocument3D doc, SwCalloutBaseHandler handler) : base(handler)
        {
            m_Doc = doc;
            m_MathUtils = m_Doc.OwnerApplication.Sw.IGetMathUtility();
        }

        public Point Location
        {
            get
            {
                if (IsCommitted)
                {
                    return new Point((double[])Callout.Position.ArrayData);
                }
                else
                {
                    return m_Creator.CachedProperties.Get<Point>();
                }
            }
            set
            {
                if (!IsCommitted)
                {
                    m_Creator.CachedProperties.Set(value);
                }
                else
                {
                    throw new CommitedElementReadOnlyParameterException();
                }
            }
        }

        public Point Anchor
        {
            get
            {
                if (IsCommitted)
                {
                    Callout.GetTargetPoint(0, out double x, out double y, out double z);
                    return new Point(x, y, z);
                }
                else
                {
                    return m_Creator.CachedProperties.Get<Point>();
                }
            }
            set
            {
                if (!IsCommitted)
                {
                    m_Creator.CachedProperties.Set(value);
                }
                else
                {
                    throw new CommitedElementReadOnlyParameterException();
                }
            }
        }

        protected override ICallout NewCallout(int rowsCount, ISwCalloutHandler handler)
            => m_Doc.Model.Extension.CreateCallout(rowsCount, handler);

        protected override void SetPosition(ICallout callout)
        {
            callout.Position = (MathPoint)m_MathUtils.CreatePoint(Location.ToArray());

            for (int i = 0; i < Rows.Length; i++)
            {
                callout.SetTargetPoint(i, Anchor.X, Anchor.Y, Anchor.Z);
            }

            callout.MultipleLeaders = false;
        }
    }

    internal class SwSelCallout : SwCalloutBase, ISwSelCallout
    {
        private readonly SwSelectionCollection m_Sel;

        public SwSelCallout(SwSelectionCollection sel, SwCalloutBaseHandler handler) : base(handler)
        {
            m_Sel = sel;
        }

        public IXSelObject Owner
        {
            get => m_Creator.CachedProperties.Get<IXSelObject>();
            set
            {
                if (!IsCommitted)
                {
                    m_Creator.CachedProperties.Set(value);
                }
                else
                {
                    throw new CommitedElementReadOnlyParameterException();
                }
            }
        }

        public override void Show()
        {
            var selIndex = ((SwSelObject)Owner).SelectionIndex;
            
            if (selIndex != -1)
            {
                m_Sel.SelMgr.DeSelect2(selIndex, -1);
            }

            var selData = m_Sel.SelMgr.CreateSelectData();
            selData.Callout = (Callout)Callout;
            ((SwSelObject)Owner).Select(true, selData);
        }

        protected override ICallout NewCallout(int rowsCount, ISwCalloutHandler handler)
            => m_Sel.SelMgr.CreateCallout2(rowsCount, handler);
    }
}
