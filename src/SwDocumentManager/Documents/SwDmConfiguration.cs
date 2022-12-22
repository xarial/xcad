//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.swdocumentmgr;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Xarial.XCad.Annotations;
using Xarial.XCad.Data;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Features;
using Xarial.XCad.Reflection;
using Xarial.XCad.SwDocumentManager.Data;
using Xarial.XCad.SwDocumentManager.Exceptions;
using Xarial.XCad.SwDocumentManager.Features;
using Xarial.XCad.UI;

namespace Xarial.XCad.SwDocumentManager.Documents
{
    public interface ISwDmConfiguration : IXConfiguration, ISwDmSelObject
    {
        ISwDMConfiguration Configuration { get; }
        new ISwDmCustomPropertiesCollection Properties { get; }
    }

    [DebuggerDisplay("{" + nameof(Name) + "}")]
    internal abstract class SwDmConfiguration : SwDmSelObject, ISwDmConfiguration
    {
        #region Not Supported
        public IXDimensionRepository Dimensions => throw new NotSupportedException();
        #endregion

        internal const string QTY_PROPERTY = "UNIT_OF_MEASURE";

        IXPropertyRepository IPropertiesOwner.Properties => Properties;
        
        private readonly Lazy<ISwDmCustomPropertiesCollection> m_Properties;

        public virtual ISwDMConfiguration Configuration { get; }

        public ISwDmCustomPropertiesCollection Properties => m_Properties.Value;

        internal SwDmConfiguration(ISwDMConfiguration conf, SwDmDocument3D doc) : base(conf, doc.OwnerApplication, doc)
        {
            Configuration = conf;
            Document = doc;

            m_Properties = new Lazy<ISwDmCustomPropertiesCollection>(
                () => new SwDmConfigurationCustomPropertiesCollection(this));
        }

        public virtual string Name
        {
            get => Configuration.Name;
            set
            {
                ((ISwDMConfiguration7)Configuration).Name2 = value;
                Document.IsDirty = true;
            }
        }

        public virtual bool IsCommitted => true;

        public string PartNumber => GetPartNumber(this);

        internal protected virtual SwDmDocument3D Document { get; }

        public IXImage Preview
        {
            get
            {
                SwDmPreviewError previewErr;
                var imgBytes = ((ISwDMConfiguration9)Configuration)
                    .GetPreviewPNGBitmapBytes(out previewErr) as byte[];

                if (previewErr == SwDmPreviewError.swDmPreviewErrorNone)
                {
                    return new BaseImage(imgBytes);
                }
                else
                {
                    throw new Exception($"Failed to extract preview from the configuration: {previewErr}");
                }
            }
        }

        public double Quantity
        {
            get
            {
                var qtyPrp = TryGetConfigurationPropertyValue(QTY_PROPERTY);

                if (string.IsNullOrEmpty(qtyPrp))
                {
                    qtyPrp = TryGetDocumentPropertyValue(QTY_PROPERTY);
                }

                if (!string.IsNullOrEmpty(qtyPrp))
                {
                    var qtyStr = TryGetConfigurationPropertyValue(qtyPrp);

                    double qty;

                    if (!string.IsNullOrEmpty(qtyStr))
                    {
                        if (double.TryParse(qtyStr, out qty))
                        {
                            return qty;
                        }
                        else 
                        {
                            return 1;
                        }
                    }
                    else
                    {
                        qtyStr = TryGetDocumentPropertyValue(qtyPrp);

                        if (double.TryParse(qtyStr, out qty))
                        {
                            return qty;
                        }
                        else
                        {
                            return 1;
                        }
                    }
                }
                else
                {
                    return 1;
                }
            }
        }

        public BomChildrenSolving_e BomChildrenSolving 
        {
            get 
            {
                if (Document is ISwDmAssembly)
                {
                    swDmShowChildComponentsInBOMResult childBomShowType;

                    if (Document.IsVersionNewerOrEqual(SwDmVersion_e.Sw2018))
                    {
                        childBomShowType = (swDmShowChildComponentsInBOMResult)((ISwDMConfiguration15)Configuration).ShowChildComponentsInBOM2;
                    }
                    else
                    {
                        childBomShowType = (swDmShowChildComponentsInBOMResult)((ISwDMConfiguration11)Configuration).ShowChildComponentsInBOM;
                    }

                    switch (childBomShowType)
                    {
                        case swDmShowChildComponentsInBOMResult.swDmShowChildComponentsInBOM_TRUE:
                            return BomChildrenSolving_e.Show;

                        case swDmShowChildComponentsInBOMResult.swDmShowChildComponentsInBOM_FALSE:
                            return BomChildrenSolving_e.Hide;

                        case swDmShowChildComponentsInBOMResult.swDmShowChildComponentsInBOM_Promote:
                            return BomChildrenSolving_e.Promote;

                        default:
                            throw new NotSupportedException();
                    }
                }
                else 
                {
                    return BomChildrenSolving_e.Show;
                }
            }
        }

        private string TryGetDocumentPropertyValue(string prpName)
        {
            try
            {
                return ((ISwDMDocument5)Document.Document).GetCustomPropertyValues(prpName, out _, out _);
            }
            catch
            {
                return "";
            }
        }

        private string TryGetConfigurationPropertyValue(string prpName)
        {
            ISwDMConfiguration5 conf;

            try
            {
                conf = (ISwDMConfiguration5)Configuration;
            }
            catch (InvalidConfigurationsException) 
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidConfigurationsException($"Failed to access configuration '{Name}' to extract quantity value", ex);
            }

            try
            {
                return conf.GetCustomPropertyValues(prpName, out _, out _);
            }
            catch 
            {
                return "";
            }
        }

        private string GetPartNumber(ISwDmConfiguration conf) 
        {
            switch ((swDmBOMPartNumberSource)((ISwDMConfiguration11)(conf.Configuration)).BOMPartNoSource)
            {
                case swDmBOMPartNumberSource.swDmBOMPartNumber_ConfigurationName:
                    return conf.Name;
                case swDmBOMPartNumberSource.swDmBOMPartNumber_DocumentName:
                    return Path.GetFileNameWithoutExtension(Document.Title);
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

    public interface ISwDmAssemblyConfiguration : ISwDmConfiguration, IXAssemblyConfiguration
    {
    }

    internal class SwDmAssemblyConfiguration : SwDmConfiguration, ISwDmAssemblyConfiguration
    {
        internal SwDmAssemblyConfiguration(ISwDMConfiguration conf, SwDmAssembly assm) : base(conf, assm)
        {
            Components = new SwDmComponentCollection(assm, this);
        }

        public IXComponentRepository Components { get; }
    }

    public interface ISwDmPartConfiguration : ISwDmConfiguration, IXPartConfiguration
    {
    }

    internal class SwDmPartConfiguration : SwDmConfiguration, ISwDmPartConfiguration
    {
        #region Not Supported
        public IXMaterial Material => throw new NotSupportedException();
        #endregion

        internal SwDmPartConfiguration(ISwDMConfiguration conf, SwDmPart part) : base(conf, part)
        {
            CutLists = new SwDmCutListItemCollection(this, part);
        }

        public IXCutListItemRepository CutLists { get; }
    }
}
