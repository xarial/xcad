using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Data;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.SolidWorks.Documents;
using SolidWorks.Interop.sldworks;
using System.IO;
using Xarial.XCad.Services;
using System.Diagnostics;

namespace Xarial.XCad.SolidWorks.Data
{
    [DebuggerDisplay("{" + nameof(Value) + "}")]
    internal class SwPartNumber : IPartNumber
    {
        public PartNumberSourceType_e Type 
        {
            get
            {
                if (m_Conf.IsCommitted)
                {
                    switch ((swBOMPartNumberSource_e)m_Conf.Configuration.BOMPartNoSource)
                    {
                        case swBOMPartNumberSource_e.swBOMPartNumber_ConfigurationName:
                            return PartNumberSourceType_e.ConfigurationName;
                        case swBOMPartNumberSource_e.swBOMPartNumber_DocumentName:
                            return PartNumberSourceType_e.DocumentName;
                        case swBOMPartNumberSource_e.swBOMPartNumber_ParentName:
                            return PartNumberSourceType_e.ParentName;
                        case swBOMPartNumberSource_e.swBOMPartNumber_UserSpecified:
                            return PartNumberSourceType_e.Custom;
                        default:
                            throw new NotSupportedException();
                    }
                }
                else
                {
                    if (m_CachedType.HasValue)
                    {
                        return m_CachedType.Value;
                    }
                    else 
                    {
                        return (PartNumberSourceType_e)(-1);
                    }
                }
            }
            set 
            {
                if (m_Conf.IsCommitted)
                {
                    switch (value)
                    {
                        case PartNumberSourceType_e.DocumentName:
                            m_Conf.Configuration.BOMPartNoSource = (int)swBOMPartNumberSource_e.swBOMPartNumber_DocumentName;
                            break;
                        case PartNumberSourceType_e.ConfigurationName:
                            m_Conf.Configuration.BOMPartNoSource = (int)swBOMPartNumberSource_e.swBOMPartNumber_ConfigurationName;
                            break;
                        case PartNumberSourceType_e.ParentName:
                            m_Conf.Configuration.BOMPartNoSource = (int)swBOMPartNumberSource_e.swBOMPartNumber_ParentName;
                            break;
                        case PartNumberSourceType_e.Custom:
                            m_Conf.Configuration.AlternateName = Value;//NOTE: need to set the value, otherwise user specified option cannot be set
                            m_Conf.Configuration.UseAlternateNameInBOM = true;
                            m_Conf.Configuration.BOMPartNoSource = (int)swBOMPartNumberSource_e.swBOMPartNumber_UserSpecified;
                            break;
                        default:
                            throw new NotSupportedException();
                    }
                }
                else
                {
                    m_CachedType = value;
                }
            }
        }
        
        public string Value 
        {
            get 
            {
                if (m_Conf.IsCommitted)
                {
                    return GetPartNumber(m_Conf.Configuration);
                }
                else 
                {
                    return m_CachedValue;
                }
            }
            set 
            {
                if (Type == PartNumberSourceType_e.Custom)
                {
                    if (m_Conf.IsCommitted)
                    {
                        m_Conf.Configuration.AlternateName = value;
                    }
                    else
                    {
                        m_CachedValue = value;
                    }
                }
                else 
                {
                    throw new Exception("Part number can only be set to a custom part number source");
                }
            }
        }

        private readonly SwConfiguration m_Conf;

        private PartNumberSourceType_e? m_CachedType;
        private string m_CachedValue;

        internal SwPartNumber(SwConfiguration conf) 
        {
            m_Conf = conf;
        }

        internal void Commit() 
        {
            if (m_CachedType.HasValue) 
            {
                Type = m_CachedType.Value;

                if (m_CachedType.Value == PartNumberSourceType_e.Custom) 
                {
                    Value = m_CachedValue;
                }
            }
        }

        private string GetPartNumber(IConfiguration conf)
        {
            switch ((swBOMPartNumberSource_e)conf.BOMPartNoSource)
            {
                case swBOMPartNumberSource_e.swBOMPartNumber_ConfigurationName:
                    return conf.Name;
                case swBOMPartNumberSource_e.swBOMPartNumber_DocumentName:
                    return Path.GetFileNameWithoutExtension(m_Conf.OwnerDocument.Title);
                case swBOMPartNumberSource_e.swBOMPartNumber_ParentName:
                    return GetPartNumber(conf.GetParent());
                case swBOMPartNumberSource_e.swBOMPartNumber_UserSpecified:
                    return conf.AlternateName;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
