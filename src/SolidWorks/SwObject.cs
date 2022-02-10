//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.IO;
using Xarial.XCad.Base;
using Xarial.XCad.Data;
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
        /// <summary>
        /// SOLIDWORKS specific dispatch
        /// </summary>
        object Dispatch { get; }
    }

    /// <inheritdoc/>
    internal class SwObject : ISwObject
    {
        protected IModelDoc2 OwnerModelDoc => OwnerDocument.Model;

        internal ISwApplication OwnerApplication { get; }
        internal virtual ISwDocument OwnerDocument { get; }

        public virtual object Dispatch { get; }

        public virtual bool IsAlive 
        {
            get 
            {
                try
                {
                    if (Dispatch != null)
                    {
                        if (OwnerModelDoc.Extension.GetPersistReference3(Dispatch) != null)
                        {
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
        
        internal SwObject(object disp, ISwDocument doc, ISwApplication app) 
        {
            Dispatch = disp;
            m_TagsLazy = new Lazy<ITagsManager>(() => new TagsManager());
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

                return Dispatch == (other as ISwObject).Dispatch;
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
                var disp = Dispatch;

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
            }
            else 
            {
                throw new ObjectSerializationException("Model is not set for this object", -1);
            }
        }
    }
}