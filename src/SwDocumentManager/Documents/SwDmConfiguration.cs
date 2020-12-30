//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.swdocumentmgr;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Xarial.XCad.Data;
using Xarial.XCad.Documents;
using Xarial.XCad.Features;
using Xarial.XCad.SwDocumentManager.Data;
using Xarial.XCad.SwDocumentManager.Features;

namespace Xarial.XCad.SwDocumentManager.Documents
{
    public interface ISwDmConfiguration : IXConfiguration, ISwDmObject
    {
        ISwDMConfiguration Configuration { get; }
        new ISwDmCustomPropertiesCollection Properties { get; }
        new ISwDmCutListItem[] CutLists { get; }
    }

    internal class SwDmConfiguration : SwDmObject, ISwDmConfiguration
    {
        IXPropertyRepository IPropertiesOwner.Properties => Properties;
        IXCutListItem[] IXConfiguration.CutLists => CutLists;

        private readonly Lazy<ISwDmCustomPropertiesCollection> m_Properties;

        public ISwDMConfiguration Configuration { get; }

        public ISwDmCustomPropertiesCollection Properties => m_Properties.Value;

        private readonly SwDmDocument3D m_Doc;

        internal SwDmConfiguration(ISwDMConfiguration conf, SwDmDocument3D doc) : base(conf)
        {
            Configuration = conf;
            m_Doc = doc;

            m_Properties = new Lazy<ISwDmCustomPropertiesCollection>(
                () => new SwDmConfigurationCustomPropertiesCollection(this));
        }

        public string Name 
        {
            get => Configuration.Name; 
            set => throw new NotSupportedException("Property is read-only"); 
        }

        public ISwDmCutListItem[] CutLists 
        {
            get 
            {
                object[] cutListItems = null;

                if (m_Doc.SwDmApp.IsVersionNewerOrEqual(SwDmVersion_e.Sw2019)
                    && m_Doc.Version.Major >= SwDmVersion_e.Sw2019)
                {
                    cutListItems = ((ISwDMConfiguration16)Configuration).GetCutListItems() as object[];
                }
                else 
                {
                    if (Configuration == m_Doc.Configurations.Active.Configuration)
                    {
                        cutListItems = ((ISwDMDocument13)m_Doc.Document).GetCutListItems2() as object[];
                    }
                    else 
                    {
                        throw new Exception(
                            "Cut-lists can only be extracted from the active configuration for files saved in 2018 or older");
                    }
                }

                if (cutListItems != null)
                {
                    return cutListItems.Cast<ISwDMCutListItem2>()
                        .Select(c => new SwDmCutListItem(c)).ToArray();
                }
                else 
                {
                    return new ISwDmCutListItem[0];
                }
            }
        }

        public bool IsCommitted => true;

        public void Commit(CancellationToken cancellationToken) => throw new NotSupportedException();
    }
}
