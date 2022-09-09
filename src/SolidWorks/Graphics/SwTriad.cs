//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
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
    public interface ISwTriad : IXTriad, ISwObject
    {
        ITriadManipulator Triad { get; }
    }
    
    [ComVisible(true)]
    public abstract class SwTriadHandler : ISwManipulatorHandler2
    {
        internal event Action<swTriadManipulatorControlPoints_e> Selected;

        public void OnDirectionFlipped(object pManipulator)
        {
        }

        public bool OnHandleLmbSelected(object pManipulator) => true;

        public void OnHandleSelected(object pManipulator, int handleIndex)
        {
            Selected?.Invoke((swTriadManipulatorControlPoints_e)handleIndex);
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

    internal class SwTriad : SwObject, ISwTriad
    {
        public event TriadSelectedDelegate Selected;

        private readonly SwDocument3D m_Doc;

        private readonly IElementCreator<ITriadManipulator> m_Creator;
        private readonly IMathUtility m_MathUtils;

        private IManipulator m_Manipulator;

        private readonly SwTriadHandler m_Handler;

        private bool m_WasShown;

        internal SwTriad(SwDocument3D doc, SwTriadHandler handler) : base(null, doc, doc.OwnerApplication)
        {
            m_Creator = new ElementCreator<ITriadManipulator>(CreateTriad, null, false);

            m_Handler = handler;

            ValidateHandler(m_Handler);

            m_Handler.Selected += OnSelected;

            m_Doc = doc;
            m_MathUtils = m_Doc.OwnerApplication.Sw.IGetMathUtility();

            Elements = TriadElements_e.All;
            Transform = TransformMatrix.Identity;
            Visible = true;

            m_WasShown = false;
        }

        private void OnSelected(swTriadManipulatorControlPoints_e handlerType)
        {
            TriadElements_e elem;

            switch (handlerType) 
            {
                case swTriadManipulatorControlPoints_e.swTriadManipulatorOrigin:
                    elem = TriadElements_e.Origin;
                    break;

                case swTriadManipulatorControlPoints_e.swTriadManipulatorXAxis:
                    elem = TriadElements_e.AxisX;
                    break;

                case swTriadManipulatorControlPoints_e.swTriadManipulatorYAxis:
                    elem = TriadElements_e.AxisY;
                    break;

                case swTriadManipulatorControlPoints_e.swTriadManipulatorZAxis:
                    elem = TriadElements_e.AxisZ;
                    break;

                default:
                    throw new NotSupportedException();
            }

            Selected?.Invoke(this, elem);
        }

        public override object Dispatch => Triad;

        public TriadElements_e Elements
        {
            get
            {
                if (IsCommitted)
                {
                    var doNotShow = (swTriadManipulatorDoNotShow_e)Triad.DoNotShow;

                    if (doNotShow == swTriadManipulatorDoNotShow_e.swTriadManipulatorShowAll)
                    {
                        return TriadElements_e.All;
                    }
                    else 
                    {
                        TriadElements_e res = 0;

                        if (!doNotShow.HasFlag(swTriadManipulatorDoNotShow_e.swTriadManipulatorDoNotShowOrigin)) 
                        {
                            res |= TriadElements_e.Origin;
                        }

                        if (!doNotShow.HasFlag(swTriadManipulatorDoNotShow_e.swTriadManipulatorDoNotShowXAxis))
                        {
                            res |= TriadElements_e.AxisX;
                        }

                        if (!doNotShow.HasFlag(swTriadManipulatorDoNotShow_e.swTriadManipulatorDoNotShowYAxis))
                        {
                            res |= TriadElements_e.AxisY;
                        }

                        if (!doNotShow.HasFlag(swTriadManipulatorDoNotShow_e.swTriadManipulatorDoNotShowZAxis))
                        {
                            res |= TriadElements_e.AxisZ;
                        }

                        return res;
                    }
                }
                else
                {
                    return m_Creator.CachedProperties.Get<TriadElements_e>();
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

        public TransformMatrix Transform
        {
            get
            {
                if (!IsCommitted)
                {
                    return m_Creator.CachedProperties.Get<TransformMatrix>();
                }
                else
                {
                    return TransformMatrix.Compose(
                        new Vector((double[])Triad.XAxis.ArrayData),
                        new Vector((double[])Triad.YAxis.ArrayData),
                        new Vector((double[])Triad.ZAxis.ArrayData),
                        new Point((double[])Triad.Origin.ArrayData));
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
                    SetPosition(Triad, value);

                    if (!Triad.UpdatePosition())
                    {
                        throw new Exception("Failed to update triad manipulator position");
                    }
                }
            }
        }

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

        public ITriadManipulator Triad => m_Creator.Element;

        public void Commit(CancellationToken cancellationToken) => m_Creator.Create(cancellationToken);

        private void ValidateHandler(SwTriadHandler handler)
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

        private ITriadManipulator CreateTriad(CancellationToken cancellationToken)
        {
            m_Manipulator = m_Doc.Model.ModelViewManager.CreateManipulator((int)swManipulatorType_e.swTriadManipulator, m_Handler);
            
            if(m_Manipulator == null) 
            {
                throw new NullReferenceException("Failed to create triad manipulator");
            }

            var triad = (ITriadManipulator)m_Manipulator.GetSpecificManipulator();

            var doNotShow = swTriadManipulatorDoNotShow_e.swTriadManipulatorDoNotShowXYPlane
                | swTriadManipulatorDoNotShow_e.swTriadManipulatorDoNotShowXYRING
                | swTriadManipulatorDoNotShow_e.swTriadManipulatorDoNotShowYZPlane
                | swTriadManipulatorDoNotShow_e.swTriadManipulatorDoNotShowYZRING
                | swTriadManipulatorDoNotShow_e.swTriadManipulatorDoNotShowZXPlane
                | swTriadManipulatorDoNotShow_e.swTriadManipulatorDoNotShowZXRING;

            if (!Elements.HasFlag(TriadElements_e.Origin)) 
            {
                doNotShow |= swTriadManipulatorDoNotShow_e.swTriadManipulatorDoNotShowOrigin;
            }

            if (!Elements.HasFlag(TriadElements_e.AxisX))
            {
                doNotShow |= swTriadManipulatorDoNotShow_e.swTriadManipulatorDoNotShowXAxis;
            }

            if (!Elements.HasFlag(TriadElements_e.AxisY))
            {
                doNotShow |= swTriadManipulatorDoNotShow_e.swTriadManipulatorDoNotShowYAxis;
            }

            if (!Elements.HasFlag(TriadElements_e.AxisZ))
            {
                doNotShow |= swTriadManipulatorDoNotShow_e.swTriadManipulatorDoNotShowZAxis;
            }

            triad.DoNotShow = (int)doNotShow;

            SetPosition(triad, Transform);

            if (Visible)
            {
                Show(m_Manipulator);
            }

            return triad;
        }

        private void SetPosition(ITriadManipulator triad, TransformMatrix matrix)
        {
            if (matrix == null) 
            {
                throw new NullReferenceException("Transformation matrix is not specified");
            }

            var origin = new Point(0, 0, 0);
            var axisX = new Vector(1, 0, 0);
            var axisY = new Vector(0, 1, 0);
            var axisZ = new Vector(0, 0, 1);

            origin = origin.Transform(matrix);
            axisX = axisX.Transform(matrix);
            axisY = axisY.Transform(matrix);
            axisZ = axisZ.Transform(matrix);

            triad.Origin = (MathPoint)m_MathUtils.CreatePoint(origin.ToArray());
            triad.XAxis = (MathVector)m_MathUtils.CreateVector(axisX.ToArray());
            triad.YAxis = (MathVector)m_MathUtils.CreateVector(axisY.ToArray());
            triad.ZAxis = (MathVector)m_MathUtils.CreateVector(axisZ.ToArray());
        }

        public void Dispose()
        {
            if (m_Manipulator != null)
            {
                m_Manipulator.Remove();
                m_Manipulator = null;
            }
        }
    }
}
