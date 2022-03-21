//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Xarial.XCad.Base.Enums;
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
        public virtual bool IsCommitted => true;

        public bool IsSelected => SelectionIndex != -1;

        internal int SelectionIndex
        {
            get 
            {
                var selMgr = OwnerModelDoc.ISelectionManager;

                for (int i = 1; i < selMgr.GetSelectedObjectCount2(-1) + 1; i++)
                {
                    if (OwnerApplication.Sw.IsSame(selMgr.GetSelectedObject6(i, -1), Dispatch) == (int)swObjectEquality.swObjectSame)
                    {
                        return i;
                    }
                }

                return -1;
            }
        }

        public SelectType_e Type 
        {
            get 
            {
                if (OwnerApplication.IsVersionNewerOrEqual(Enums.SwVersion_e.Sw2015))
                {
                    OwnerModelDoc.ISelectionManager.GetSelectByIdSpecification(Dispatch, out _, out _, out int type);

                    return (SelectType_e)type;
                }
                else 
                {
                    throw new NotSupportedException();
                }
            }
        }

        internal SwSelObject(object disp, ISwDocument doc, ISwApplication app) : base(disp, doc, app)
        {
        }

        public void Select(bool append) => Select(append, null);

        internal virtual void Select(bool append, ISelectData selData) 
        {
            if (OwnerModelDoc != null)
            {
                if (OwnerModelDoc.Extension.MultiSelect2(new DispatchWrapper[] { new DispatchWrapper(Dispatch) }, append, selData) != 1)
                {
                    throw new Exception("Failed to select");
                }
            }
            else
            {
                throw new Exception("Model doc is not initialized");
            }
        }

        public virtual void Commit(CancellationToken cancellationToken)
        {
        }
    }
}