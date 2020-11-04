using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Primitives;
using Xarial.XCad.Services;

namespace Xarial.XCad.SolidWorks.Geometry.Primitives
{
    public interface ISwTempPrimitive : IXPrimitive
    {
        new SwTempBody[] Bodies { get; }
    }

    public class SwTempPrimitive : ISwTempPrimitive
    {
        IXBody[] IXPrimitive.Bodies => Bodies;

        public SwTempBody[] Bodies => m_Creator.Element;

        public bool IsCommitted => m_Creator.IsCreated;

        protected readonly IModeler m_Modeler;
        protected readonly IMathUtility m_MathUtils;

        protected readonly ElementCreator<SwTempBody[]> m_Creator;
        
        internal SwTempPrimitive(IMathUtility mathUtils, IModeler modeler, SwTempBody[] bodies, bool isCreated) 
        {
            m_MathUtils = mathUtils;
            m_Modeler = modeler;

            m_Creator = new ElementCreator<SwTempBody[]>(CreateBodies, bodies, isCreated);
        }

        protected virtual SwTempBody[] CreateBodies() 
        {
            throw new NotSupportedException();
        }

        public void Commit() => m_Creator.Create();
    }
}
