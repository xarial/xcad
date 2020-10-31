//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Runtime.InteropServices;
using Xarial.XCad.SolidWorks.Documents;

namespace Xarial.XCad.SolidWorks
{
    /// <inheritdoc/>
    public class SwSelObject : SwObject, IXSelObject
    {
        public static new SwSelObject FromDispatch(object disp, SwDocument doc)
            => (SwSelObject)SwSelObject.FromDispatch(disp, doc, o => new SwSelObject(doc.Model, o));
        
        protected readonly IModelDoc2 m_ModelDoc;

        public virtual bool IsCommitted => true;
        
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

        public virtual void Commit()
        {
        }
    }
}