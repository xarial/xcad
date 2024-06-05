//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using SolidWorks.Interop.swpublished;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Xarial.XCad.Data;
using Xarial.XCad.Exceptions;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Graphics;
using Xarial.XCad.Services;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.Utils.Reflection;

namespace Xarial.XCad.SolidWorks.Graphics
{
    public interface ISwDragArrow : IXDragArrow, ISwObject
    {
        IDragArrowManipulator DragArrow { get; }
    }
    
    [ComVisible(true)]
    public abstract class SwDragArrowHandler : ISwManipulatorHandler2
    {
        internal event Action Flipped;
        internal event Action Selected;

        public void OnDirectionFlipped(object pManipulator)
        {
            Flipped?.Invoke();
        }

        public bool OnHandleLmbSelected(object pManipulator) => true;

        public void OnHandleSelected(object pManipulator, int handleIndex)
        {
            Selected?.Invoke();
        }

        public void OnUpdateDrag(object pManipulator, int handleIndex, object newPosMathPt)
        {
        }

        public void OnEndDrag(object pManipulator, int handleIndex)
        {
        }

        public void OnEndNoDrag(object pManipulator, int handleIndex)
        {
        }

        public void OnHandleRmbSelected(object pManipulator, int handleIndex)
        {
        }

        public bool OnDelete(object pManipulator) => true;

        public bool OnDoubleValueChanged(object pManipulator, int handleIndex, ref double Value) => false;

        public bool OnStringValueChanged(object pManipulator, int handleIndex, ref string Value) => false;

        public void OnItemSetFocus(object pManipulator, int handleIndex)
        {
        }
    }

    internal class SwDragArrow : SwObject, ISwDragArrow
    {
        public event DragArrowFlippedDelegate Flipped;
        public event DragArrowSelectedDelegate Selected;

        private readonly SwDocument3D m_Doc;

        private readonly IElementCreator<IDragArrowManipulator> m_Creator;
        private readonly IMathUtility m_MathUtils;

        private IManipulator m_Manipulator;

        private readonly SwDragArrowHandler m_Handler;

        private bool m_WasShown;

        internal SwDragArrow(SwDocument3D doc, SwDragArrowHandler handler) : base(null, doc, doc.OwnerApplication)
        {
            m_Creator = new ElementCreator<IDragArrowManipulator>(CreateDragArrow, null, false);

            m_Handler = handler;
            m_Handler.Selected += OnSelected;
            m_Handler.Flipped += OnFlipped;

            ValidateHandler(m_Handler);

            m_Doc = doc;
            m_MathUtils = m_Doc.OwnerApplication.Sw.IGetMathUtility();

            Visible = true;

            m_WasShown = false;
        }

        private void OnSelected()
        {
            Selected?.Invoke(this);
        }

        private void OnFlipped()
        {
            Flipped?.Invoke(this, Direction);
        }

        public override object Dispatch => DragArrow;

        public bool Visible
        {
            get
            {
                if (IsCommitted)
                {
                    return m_Manipulator.Visible;
                }
                else
                {
                    return m_Creator.CachedProperties.Get<bool>();
                }
            }
            set
            {
                if (IsCommitted)
                {
                    if (value && !m_WasShown)
                    {
                        Show(m_Manipulator);
                    }
                    else
                    {
                        m_Manipulator.Visible = value;
                    }
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        private void Show(IManipulator manipulator)
        {
            manipulator.Show(m_Doc.Model);
            m_WasShown = true;
        }

        public bool IsCommitted => m_Creator.IsCreated;

        public IDragArrowManipulator DragArrow => m_Creator.Element;

        public bool CanFlip 
        {
            get 
            {
                if (IsCommitted)
                {
                    return DragArrow.AllowFlip;
                }
                else 
                {
                    return m_Creator.CachedProperties.Get<bool>();
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

        public double Length
        {
            get
            {
                if (IsCommitted)
                {
                    return DragArrow.Length;
                }
                else
                {
                    return m_Creator.CachedProperties.Get<double>();
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
                    DragArrow.Length = value;

                    if (!DragArrow.Update())
                    {
                        throw new Exception("Failed to update drag arrow");
                    }
                }
            }
        }

        public Vector Direction
        {
            get
            {
                if (IsCommitted)
                {
                    return new Vector((double[])DragArrow.Direction.ArrayData);
                }
                else
                {
                    return m_Creator.CachedProperties.Get<Vector>();
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

        public Point Origin
        {
            get
            {
                if (IsCommitted)
                {
                    return new Point((double[])DragArrow.Origin.ArrayData);
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

        public void Commit(CancellationToken cancellationToken) => m_Creator.Create(cancellationToken);

        private void ValidateHandler(SwDragArrowHandler handler)
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

        private IDragArrowManipulator CreateDragArrow(CancellationToken cancellationToken)
        {
            if (Origin == null) 
            {
                throw new NullReferenceException("Origin is not specified");
            }

            if (Direction == null)
            {
                throw new NullReferenceException("Direction is not specified");
            }

            m_Manipulator = m_Doc.Model.ModelViewManager.CreateManipulator((int)swManipulatorType_e.swDragArrowManipulator, m_Handler);
            
            if(m_Manipulator == null) 
            {
                throw new NullReferenceException("Failed to create drag arrow manipulator");
            }

            var dragArrow = (IDragArrowManipulator)m_Manipulator.GetSpecificManipulator();

            dragArrow.AllowFlip = CanFlip;
            dragArrow.FixedLength = true;
            dragArrow.Length = Length;
            dragArrow.Origin = (MathPoint)m_MathUtils.CreatePoint(Origin.ToArray());
            dragArrow.Direction = (MathVector)m_MathUtils.CreateVector(Direction.ToArray());

            if (Visible)
            {
                Show(m_Manipulator);
            }

            return dragArrow;
        }
        
        public void Dispose()
        {
            m_Manipulator.Remove();
        }
    }
}
