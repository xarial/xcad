using SolidWorks.Interop.swdocumentmgr;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Xarial.XCad.Data;
using Xarial.XCad.Documents;
using Xarial.XCad.Features;
using Xarial.XCad.SwDocumentManager.Data;

namespace Xarial.XCad.SwDocumentManager.Documents
{
    public interface ISwDmConfiguration : IXConfiguration, ISwDmObject
    {
        ISwDMConfiguration Configuration { get; }
        new ISwDmCustomPropertiesCollection Properties { get; }
    }

    internal class SwDmConfiguration : SwDmObject, ISwDmConfiguration
    {
        IXPropertyRepository IPropertiesOwner.Properties => Properties;

        private readonly Lazy<ISwDmCustomPropertiesCollection> m_Properties;

        public ISwDMConfiguration Configuration { get; }

        public ISwDmCustomPropertiesCollection Properties => m_Properties.Value;

        internal SwDmConfiguration(ISwDMConfiguration conf) : base(conf)
        {
            Configuration = conf;
            m_Properties = new Lazy<ISwDmCustomPropertiesCollection>(
                () => new SwDmConfigurationCustomPropertiesCollection(this));
        }

        public string Name 
        {
            get => Configuration.Name; 
            set => throw new NotSupportedException("Property is read-only"); 
        }

        public IXCutListItem[] CutLists => throw new NotImplementedException();

        public bool IsCommitted => true;

        public void Commit(CancellationToken cancellationToken) => throw new NotSupportedException();
    }
}
