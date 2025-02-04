using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Data;
using Xarial.XCad.Documents.Enums;
using System.IO;
using Xarial.XCad.Services;
using System.Diagnostics;
using SolidWorks.Interop.swdocumentmgr;
using Xarial.XCad.SwDocumentManager.Documents;
using Xarial.XCad.Exceptions;

namespace Xarial.XCad.SolidWorks.Data
{
    [DebuggerDisplay("{" + nameof(Value) + "}")]
    internal class SwDmPartNumber : IPartNumber
    {
        public PartNumberSourceType_e Type 
        {
            get
            {
                if (m_Conf.IsCommitted)
                {
                    switch ((swDmBOMPartNumberSource)((ISwDMConfiguration11)m_Conf.Configuration).BOMPartNoSource)
                    {
                        case swDmBOMPartNumberSource.swDmBOMPartNumber_ConfigurationName:
                            return PartNumberSourceType_e.ConfigurationName;
                        case swDmBOMPartNumberSource.swDmBOMPartNumber_DocumentName:
                            return PartNumberSourceType_e.DocumentName;
                        case swDmBOMPartNumberSource.swDmBOMPartNumber_ParentName:
                            return PartNumberSourceType_e.ParentName;
                        case swDmBOMPartNumberSource.swDmBOMPartNumber_UserSpecified:
                            return PartNumberSourceType_e.Custom;
                        default:
                            throw new NotSupportedException();
                    }
                }
                else
                {
                    throw new NonCommittedElementAccessException();
                }
            }
            set 
            {
                if (m_Conf.IsCommitted)
                {
                    switch (value)
                    {
                        case PartNumberSourceType_e.DocumentName:
                            ((ISwDMConfiguration11)m_Conf.Configuration).BOMPartNoSource = (int)swDmBOMPartNumberSource.swDmBOMPartNumber_DocumentName;
                            break;
                        case PartNumberSourceType_e.ConfigurationName:
                            ((ISwDMConfiguration11)m_Conf.Configuration).BOMPartNoSource = (int)swDmBOMPartNumberSource.swDmBOMPartNumber_ConfigurationName;
                            break;
                        case PartNumberSourceType_e.ParentName:
                            ((ISwDMConfiguration11)m_Conf.Configuration).BOMPartNoSource = (int)swDmBOMPartNumberSource.swDmBOMPartNumber_ParentName;
                            break;
                        case PartNumberSourceType_e.Custom:
                            ((ISwDMConfiguration11)m_Conf.Configuration).BOMPartNoSource = (int)swDmBOMPartNumberSource.swDmBOMPartNumber_UserSpecified;
                            break;
                        default:
                            throw new NotSupportedException();
                    }
                }
                else
                {
                    throw new NonCommittedElementAccessException();
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
                    throw new NonCommittedElementAccessException();
                }
            }
            set 
            {
                if (Type == PartNumberSourceType_e.Custom)
                {
                    if (m_Conf.IsCommitted)
                    {
                        ((ISwDMConfiguration7)m_Conf.Configuration).AlternateName2 = value;
                    }
                    else
                    {
                        throw new NonCommittedElementAccessException();
                    }
                }
                else 
                {
                    throw new Exception("Part number can only be set to a custom part number source");
                }
            }
        }

        private readonly SwDmConfiguration m_Conf;

        internal SwDmPartNumber(SwDmConfiguration conf) 
        {
            m_Conf = conf;
        }

        private string GetPartNumber(ISwDMConfiguration conf)
        {
            switch ((swDmBOMPartNumberSource)((ISwDMConfiguration11)conf).BOMPartNoSource)
            {
                case swDmBOMPartNumberSource.swDmBOMPartNumber_ConfigurationName:
                    return conf.Name;
                case swDmBOMPartNumberSource.swDmBOMPartNumber_DocumentName:
                    return Path.GetFileNameWithoutExtension(m_Conf.Document.Title);
                case swDmBOMPartNumberSource.swDmBOMPartNumber_ParentName:
                    return GetPartNumber(m_Conf.Document.Configurations[conf.GetParentConfigurationName()].Configuration);
                case swDmBOMPartNumberSource.swDmBOMPartNumber_UserSpecified:
                    return ((ISwDMConfiguration7)conf).AlternateName2;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
