using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Inventor.Documents;

namespace Xarial.XCad.Inventor
{
    public interface IAiSelObject : IAiObject, IXSelObject 
    {
    }

    internal class AiSelObject : AiObject, IAiSelObject
    {
        internal AiSelObject(object dispatch, AiDocument ownerDoc, AiApplication ownerApp) : base(dispatch, ownerDoc, ownerApp)
        {
        }

        public bool IsSelected => throw new NotImplementedException();

        public void Delete()
        {
            throw new NotImplementedException();
        }

        public void Select(bool append)
        {
            if (Dispatch is IAiArtificialEntity) 
            {
                throw new NotSupportedException();
            }

            if (!append) 
            {
                OwnerDocument.Document.SelectSet.Clear();
            }

            OwnerDocument.Document.SelectSet.Select(Dispatch);
        }
    }
}
