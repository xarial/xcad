//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.IO;
using System.Threading;
using Xarial.XCad.Base;
using Xarial.XCad.Data;
using Xarial.XCad.Documents;
using Xarial.XCad.Exceptions;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.Toolkit.Data;

namespace Xarial.XCad.SolidWorks
{
    /// <summary>
    /// Represents base interface for all SOLIDWORKS objects
    /// </summary>
    public interface ISwObject : IXObject
    {
        /// <inheritdoc/>
        new ISwApplication OwnerApplication { get; }

        /// <inheritdoc/>
        new ISwDocument OwnerDocument { get; }

        /// <summary>
        /// SOLIDWORKS specific dispatch
        /// </summary>
        object Dispatch { get; }
    }

    /// <inheritdoc/>
    internal class SwObject : ISwObject
    {
        IXApplication IXObject.OwnerApplication => OwnerApplication;
        IXDocument IXObject.OwnerDocument => OwnerDocument;
        ISwApplication ISwObject.OwnerApplication => OwnerApplication;
        ISwDocument ISwObject.OwnerDocument => OwnerDocument;

        protected IModelDoc2 OwnerModelDoc => OwnerDocument.Model;

        internal SwApplication OwnerApplication { get; }
        internal virtual SwDocument OwnerDocument { get; }

        public virtual object Dispatch { get; }

        public virtual bool IsAlive 
        {
            get 
            {
                try
                {
                    if (Dispatch != null)
                    {
                        if (OwnerDocument != null)
                        {
                            if (OwnerModelDoc.Extension.GetPersistReference3(Dispatch) != null)
                            {
                                return true;
                            }
                        }
                        else 
                        {
                            //this is an assumption as memory object can still be destroyed
                            //TODO: find how to capture the object has been disconnected from its client exception
                            return true;
                        }
                    }
                }
                catch 
                {
                }

                return false;
            }
        }

        public ITagsManager Tags => m_TagsLazy.Value;

        private readonly Lazy<ITagsManager> m_TagsLazy;
        
        internal SwObject(object disp, SwDocument doc, SwApplication app) 
        {
            Dispatch = disp;
            m_TagsLazy = new Lazy<ITagsManager>(() => new GlobalTagsManager(this, app.TagsRegistry));
            OwnerDocument = doc;
            OwnerApplication = app;
        }

        public virtual bool Equals(IXObject other)
        {
            if (object.ReferenceEquals(this, other)) 
            {
                return true;
            }

            if (other is ISwObject)
            {
                if (this is IXTransaction && !((IXTransaction)this).IsCommitted) 
                {
                    return false;
                }

                if (other is IXTransaction && !((IXTransaction)other).IsCommitted)
                {
                    return false;
                }

                if (Dispatch == (other as ISwObject).Dispatch)
                {
                    return true;
                }
                else
                {
                    return OwnerApplication.Sw.IsSame(Dispatch, (other as ISwObject).Dispatch) == (int)swObjectEquality.swObjectSame;
                }
            }
            else
            {
                return false;
            }
        }

        public virtual void Serialize(Stream stream)
        {
            if (OwnerModelDoc != null)
            {
                var disp = GetSerializationDispatch();

                if (disp != null)
                {
                    var persRef = OwnerModelDoc.Extension.GetPersistReference3(disp) as byte[];

                    if (persRef == null)
                    {
                        throw new ObjectSerializationException("Failed to serialize the object", -1);
                    }

                    stream.Write(persRef, 0, persRef.Length);
                    return;
                }
                else 
                {
                    throw new ObjectSerializationException("Dispatch is null", -1);
                }
            }
            else 
            {
                throw new ObjectSerializationException("Model is not set for this object", -1);
            }
        }

        /// <summary>
        /// In some instances it is required to serialize different dispatch (e.g. specific or base feature)
        /// </summary>
        /// <returns></returns>
        protected virtual object GetSerializationDispatch() => Dispatch;

        public virtual bool IsCommitted => throw new NotImplementedException();

        public virtual void Commit(CancellationToken cancellationToken) => throw new NotImplementedException();
    }

    internal static class SwObjectExtension
    {
        internal static bool CheckIsAlive(this SwObject obj, Action checker)
        {
            try
            {
                checker.Invoke();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}