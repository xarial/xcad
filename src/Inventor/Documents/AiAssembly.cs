//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Inventor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.Geometry;

namespace Xarial.XCad.Inventor.Documents
{
    public interface IAiAssembly : IAiDocument, IXAssembly 
    {
        AssemblyDocument Assembly { get; }
    }

    internal class AiAssembly : AiDocument3D, IAiAssembly
    {
        public event ComponentInsertedDelegate ComponentInserted;
        public event ComponentDeletingDelegate ComponentDeleting;
        public event ComponentDeletedDelegate ComponentDeleted;

        public AssemblyDocument Assembly { get; }

        internal AiAssembly(AssemblyDocument assm, AiApplication ownerApp) : base((Document)assm, ownerApp)
        {
            Assembly = assm;
        }

        public IXComponent EditingComponent => throw new NotImplementedException();

        IXDocumentEvaluation IXDocument3D.Evaluation => throw new NotImplementedException();

        IXModelView3DRepository IXDocument3D.ModelViews => throw new NotImplementedException();

        IXConfigurationRepository IXDocument3D.Configurations => throw new NotImplementedException();

        IXAssemblyConfigurationRepository IXAssembly.Configurations => throw new NotImplementedException();

        IXAssemblyEvaluation IXAssembly.Evaluation => throw new NotImplementedException();
    }
}
