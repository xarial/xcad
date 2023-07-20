//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Data;
using Xarial.XCad.Documents;
using Xarial.XCad.Inventor.Documents;

namespace Xarial.XCad.Inventor
{
    public interface IAiObject : IXObject
    {
        object Dispatch { get; }
    }

    internal class AiObject : IAiObject
    {
        IXApplication IXObject.OwnerApplication => OwnerApplication;
        IXDocument IXObject.OwnerDocument => OwnerDocument;

        public virtual object Dispatch { get; }

        internal virtual AiDocument OwnerDocument { get; }
        internal AiApplication OwnerApplication { get; }

        public virtual bool IsAlive => true;

        public ITagsManager Tags => throw new NotImplementedException();

        internal AiObject(object dispatch, AiDocument ownerDoc, AiApplication ownerApp) 
        {
            Dispatch = dispatch;
            OwnerDocument = ownerDoc;
            OwnerApplication = ownerApp;
        }

        public virtual bool Equals(IXObject other)
        {
            throw new NotImplementedException();
        }

        public void Serialize(Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}
