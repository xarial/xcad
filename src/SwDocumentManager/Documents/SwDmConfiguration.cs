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
using Xarial.XCad.Reflection;
using Xarial.XCad.SwDocumentManager.Data;
using Xarial.XCad.SwDocumentManager.Features;
using Xarial.XCad.UI;

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

        public virtual ISwDMConfiguration Configuration { get; }

        public ISwDmCustomPropertiesCollection Properties => m_Properties.Value;
        
        internal SwDmConfiguration(ISwDMConfiguration conf, SwDmDocument3D doc) : base(conf)
        {
            Configuration = conf;
            Document = doc;

            m_Properties = new Lazy<ISwDmCustomPropertiesCollection>(
                () => new SwDmConfigurationCustomPropertiesCollection(this));
        }

        public virtual string Name
        {
            get => Configuration.Name;
            set => throw new NotSupportedException("Property is read-only");
        }

        public ISwDmCutListItem[] CutLists
        {
            get
            {
                object[] cutListItems = null;

                if (Document.SwDmApp.IsVersionNewerOrEqual(SwDmVersion_e.Sw2019)
                    && Document.Version.Major >= SwDmVersion_e.Sw2019)
                {
                    cutListItems = ((ISwDMConfiguration16)Configuration).GetCutListItems() as object[];
                }
                else
                {
                    if (Configuration == Document.Configurations.Active.Configuration)
                    {
                        cutListItems = ((ISwDMDocument13)Document.Document).GetCutListItems2() as object[];
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

        public virtual bool IsCommitted => true;

        public string PartNumber => GetPartNumber(this);

        protected virtual SwDmDocument3D Document { get; }

        public IXImage Preview 
        {
            get 
            {
                SwDmPreviewError previewErr;
                var imgBytes = ((ISwDMConfiguration9)Configuration)
                    .GetPreviewPNGBitmapBytes(out previewErr) as byte[];

                if (previewErr == SwDmPreviewError.swDmPreviewErrorNone)
                {
                    return ResourceHelper.FromBytes(imgBytes);
                }
                else
                {
                    throw new Exception($"Failed to extract preview from the configuration: {previewErr}");
                }
            }
        }

        private string GetPartNumber(ISwDmConfiguration conf) 
        {
            switch ((swDmBOMPartNumberSource)((ISwDMConfiguration11)(conf.Configuration)).BOMPartNoSource)
            {
                case swDmBOMPartNumberSource.swDmBOMPartNumber_ConfigurationName:
                    return conf.Name;
                case swDmBOMPartNumberSource.swDmBOMPartNumber_DocumentName:
                    return Document.Title;
                case swDmBOMPartNumberSource.swDmBOMPartNumber_ParentName:
                    return GetPartNumber(Document.Configurations[conf.Configuration.GetParentConfigurationName()]);
                case swDmBOMPartNumberSource.swDmBOMPartNumber_UserSpecified:
                    return ((ISwDMConfiguration7)conf.Configuration).AlternateName2;
                default:
                    throw new NotSupportedException();
            }
        }

        public virtual void Commit(CancellationToken cancellationToken) => throw new NotSupportedException();
    }
}
