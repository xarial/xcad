//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Xarial.XCad.Exceptions;
using Xarial.XCad.SolidWorks.Documents;

namespace Xarial.XCad.SolidWorks
{
    public interface ISwSelObject : ISwObject, IXSelObject
    {
    }

    /// <inheritdoc/>
    internal class SwSelObject : SwObject, ISwSelObject
    {
        internal static new SwSelObject FromDispatch(object disp, ISwDocument doc)
            => (SwSelObject)SwSelObject.FromDispatch(disp, doc, o => new SwSelObject(o, doc));
        
        public virtual bool IsCommitted => true;

        public bool IsSelected
        {
            get 
            {
                for (int i = 1; i < ModelDoc.ISelectionManager.GetSelectedObjectCount2(-1) + 1; i++)
                {
                    if (ModelDoc.ISelectionManager.GetSelectedObject6(i, -1) == Dispatch) 
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        internal SwSelObject(object disp) : this(disp, null)
        {
        }

        internal SwSelObject(object disp, ISwDocument doc) : base(disp, doc)
        {
        }

        public virtual void Select(bool append)
        {
            if (ModelDoc != null)
            {
                if (ModelDoc.Extension.MultiSelect2(new DispatchWrapper[] { new DispatchWrapper(Dispatch) }, append, null) != 1)
                {
                    throw new Exception("Failed to select");
                }
            }
            else 
            {
                throw new Exception("Model doc is not initialized");
            }
        }

        public override void Serialize(Stream stream)
        {
            if (ModelDoc != null) 
            {
                var disp = Dispatch;

                if (disp != null) 
                {
                    var persRef = ModelDoc.Extension.GetPersistReference3(disp) as byte[];

                    if (persRef == null) 
                    {
                        throw new ObjectSerializationException("Failed to serialize the object", -1);
                    }

                    stream.Write(persRef, 0, persRef.Length);
                    return;
                }
            }

            base.Serialize(stream);
        }

        public virtual void Commit(CancellationToken cancellationToken)
        {
        }
    }
}