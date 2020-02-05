//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Runtime.InteropServices;

namespace Xarial.XCad.SolidWorks
{
    public class SwSelObject : SwObject, IXSelObject
    {
        protected readonly IModelDoc2 m_ModelDoc;

        internal SwSelObject(object disp) : this(null, disp)
        {
        }

        internal SwSelObject(IModelDoc2 model, object disp) : base(disp)
        {
            m_ModelDoc = model;
        }

        public virtual void Select(bool append)
        {
            if (m_ModelDoc != null)
            {
                if (m_ModelDoc.Extension.MultiSelect2(new DispatchWrapper[] { new DispatchWrapper(Dispatch) }, append, null) != 1)
                {
                    throw new Exception("Failed to select");
                }
            }
            else 
            {
                throw new Exception("Model doc is not initialized");
            }
        }
    }
}