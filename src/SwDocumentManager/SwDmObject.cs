using SolidWorks.Interop.swdocumentmgr;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xarial.XCad.SwDocumentManager.Documents;

namespace Xarial.XCad.SwDocumentManager
{
    public interface ISwDmObject : IXObject
    {
        object Dispatch { get; }
    }

    internal class SwDmObject : ISwDmObject
    {
        public SwDmObject(object disp)
        {
            Dispatch = disp;
        }

        public object Dispatch { get; }

        public bool IsSame(IXObject other)
        {
            if (other is ISwDmObject)
            {
                return (other as ISwDmObject).Dispatch == Dispatch;
            }
            else 
            {
                return false;
            }
        }

        public virtual void Serialize(Stream stream)
        {
            throw new NotSupportedException();
        }
    }

    public static class SwDmObjectFactory 
    {
        public static TObj FromDispatch<TObj>(object disp)
            where TObj : ISwDmObject
        {
            return (TObj)FromDispatch(disp);
        }

        public static ISwDmObject FromDispatch(object disp)
        {
            switch (disp) 
            {
                case ISwDMConfiguration conf:
                    return new SwDmConfiguration(conf);

                default:
                    return new SwDmObject(disp);
            }
        }
    }
}
