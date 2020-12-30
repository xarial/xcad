using SolidWorks.Interop.swdocumentmgr;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.Exceptions;
using Xarial.XCad.SwDocumentManager.Exceptions;

namespace Xarial.XCad.SwDocumentManager.Documents
{
    public interface ISwDmConfigurationCollection : IXConfigurationRepository 
    {
        new ISwDmConfiguration this[string name] { get; }
        new ISwDmConfiguration Active { get; }
    }

    internal class SwDmConfigurationCollection : ISwDmConfigurationCollection
    {
        #region Not Supported

        public event ConfigurationActivatedDelegate ConfigurationActivated;

        public void AddRange(IEnumerable<IXConfiguration> ents) => throw new NotSupportedException();
        public IXConfiguration PreCreate() => throw new NotSupportedException();

        #endregion

        IXConfiguration IXRepository<IXConfiguration>.this[string name] => this[name];
        
        IXConfiguration IXConfigurationRepository.Active
        {
            get => Active;
            set => throw new NotSupportedException();
        }

        public ISwDmConfiguration this[string name] => (ISwDmConfiguration)this.Get(name);

        public ISwDmConfiguration Active => this[m_Doc.Document.ConfigurationManager.GetActiveConfigurationName()];

        public int Count
        {
            get
            {
                if (m_Doc.SwDmApp.IsVersionNewerOrEqual(SwDmVersion_e.Sw2019))
                {
                    var count = ((ISwDMConfigurationMgr2)m_Doc.Document.ConfigurationManager).GetConfigurationCount2(out SwDMConfigurationError err);
                    
                    if (err != SwDMConfigurationError.SwDMConfigurationError_None) 
                    {
                        throw new InvalidConfigurationsException(err);
                    }

                    return count;
                }
                else
                {
                    return m_Doc.Document.ConfigurationManager.GetConfigurationCount();
                }
            }
        }
         
        private readonly SwDmDocument3D m_Doc;

        internal SwDmConfigurationCollection(SwDmDocument3D doc) 
        {
            m_Doc = doc;
        }

        public IEnumerator<IXConfiguration> GetEnumerator()
        {
            string[] confNames;

            if (m_Doc.SwDmApp.IsVersionNewerOrEqual(SwDmVersion_e.Sw2019))
            {
                confNames = (string[])((ISwDMConfigurationMgr2)m_Doc.Document.ConfigurationManager).GetConfigurationNames2(out SwDMConfigurationError err);
                
                if (err != SwDMConfigurationError.SwDMConfigurationError_None)
                {
                    throw new InvalidConfigurationsException(err);
                }
            }
            else
            {
                confNames = (string[])m_Doc.Document.ConfigurationManager.GetConfigurationNames();
            }

            return confNames.Select(n => this[n]).GetEnumerator();
        }

        public bool TryGet(string name, out IXConfiguration ent)
        {
            ISwDMConfiguration conf;

            if (m_Doc.SwDmApp.IsVersionNewerOrEqual(SwDmVersion_e.Sw2019))
            {
                conf = ((ISwDMConfigurationMgr2)m_Doc.Document.ConfigurationManager).GetConfigurationByName2(name, out SwDMConfigurationError err);

                if (err == SwDMConfigurationError.SwDMConfigurationError_NameNotFound) 
                {
                    ent = null;
                    return false;
                }

                if (err != SwDMConfigurationError.SwDMConfigurationError_None)
                {
                    throw new InvalidConfigurationsException(err);
                }
            }
            else
            {
                conf = m_Doc.Document.ConfigurationManager.GetConfigurationByName(name);
            }

            ent = SwDmObjectFactory.FromDispatch<ISwDmConfiguration>(conf);
            return true;
        }

        public void RemoveRange(IEnumerable<IXConfiguration> ents) 
        {
            var activeConfName = m_Doc.Document.ConfigurationManager.GetActiveConfigurationName();

            foreach (ISwDmConfiguration conf in ents) 
            {
                if (string.Equals(conf.Name, activeConfName, StringComparison.CurrentCultureIgnoreCase)) 
                {
                    throw new Exception("Cannot delete active configuration");
                }

                ((ISwDMConfiguration7)conf.Configuration).Delete = true;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
