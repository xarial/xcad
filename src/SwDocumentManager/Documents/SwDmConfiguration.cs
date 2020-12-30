using SolidWorks.Interop.swdocumentmgr;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Xarial.XCad.Data;
using Xarial.XCad.Documents;
using Xarial.XCad.Features;

namespace Xarial.XCad.SwDocumentManager.Documents
{
    public interface ISwDmConfiguration : IXConfiguration, ISwDmObject
    {
        ISwDMConfiguration Configuration { get; }
    }

    internal class SwDmConfiguration : SwDmObject, ISwDmConfiguration
    {
        public ISwDMConfiguration Configuration { get; }

        internal SwDmConfiguration(ISwDMConfiguration conf) : base(conf)
        {
            Configuration = conf;
        }

        public string Name 
        {
            get => Configuration.Name; 
            set => throw new NotSupportedException("Property is read-only"); 
        }

        public IXCutListItem[] CutLists => throw new NotImplementedException();

        public bool IsCommitted => true;

        public IXPropertyRepository Properties => throw new NotImplementedException();

        public void Commit(CancellationToken cancellationToken) => throw new NotSupportedException();
    }
}
