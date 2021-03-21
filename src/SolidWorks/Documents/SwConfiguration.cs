//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using Xarial.XCad.Data;
using Xarial.XCad.Documents;
using Xarial.XCad.Features;
using Xarial.XCad.Reflection;
using Xarial.XCad.Services;
using Xarial.XCad.SolidWorks.Data;
using Xarial.XCad.SolidWorks.Documents.Exceptions;
using Xarial.XCad.SolidWorks.Enums;
using Xarial.XCad.SolidWorks.Features;
using Xarial.XCad.UI;

namespace Xarial.XCad.SolidWorks.Documents
{
    public interface ISwConfiguration : ISwObject, IXConfiguration, IDisposable
    {
        IConfiguration Configuration { get; }
        new ISwCustomPropertiesCollection Properties { get; }
    }

    internal class SwConfiguration : SwObject, ISwConfiguration
    {
        internal const string QTY_PROPERTY = "UNIT_OF_MEASURE";

        public IConfiguration Configuration => m_Creator.Element;

        private readonly new SwDocument3D m_Doc;

        public virtual string Name
        {
            get
            {
                if (m_Creator.IsCreated)
                {
                    return Configuration.Name;
                }
                else
                {
                    return m_Creator.CachedProperties.Get<string>();
                }
            }
            set
            {
                if (m_Creator.IsCreated)
                {
                    Configuration.Name = value;
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        IXPropertyRepository IPropertiesOwner.Properties => Properties;

        public virtual ISwCustomPropertiesCollection Properties => m_PropertiesLazy.Value;

        private readonly Lazy<ISwCustomPropertiesCollection> m_PropertiesLazy;

        public bool IsCommitted => m_Creator.IsCreated;

        public virtual IXCutListItem[] CutLists
        {
            get
            {
                var activeConf = m_Doc.Configurations.Active;

                if (activeConf.Configuration != this.Configuration) 
                {
                    throw new InactiveConfigurationCutListPropertiesNotSupportedException();
                };

                return m_Doc.Features.GetCutLists();
            }
        }

        private readonly ElementCreator<IConfiguration> m_Creator;

        internal SwConfiguration(SwDocument3D doc, IConfiguration conf, bool created) : base(conf)
        {
            m_Doc = doc;

            m_Creator = new ElementCreator<IConfiguration>(Create, conf, created);

            m_PropertiesLazy = new Lazy<ISwCustomPropertiesCollection>(
                () => new SwConfigurationCustomPropertiesCollection(m_Doc, Name));
        }

        public override object Dispatch => Configuration;

        public IXImage Preview
        {
            get
            {
                var getPictureFromIPictureFund = typeof(System.Windows.Forms.AxHost)
                    .GetMethod("GetPictureFromIPicture", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

                var pictDisp = m_Doc.App.Sw.GetPreviewBitmap(m_Doc.Path, Name);

                if (pictDisp == null)
                {
                    throw new NullReferenceException("Failed to extract IPictureDisp from the document");
                }

                var bmp = getPictureFromIPictureFund.Invoke(null, new object[] { pictDisp }) as Bitmap;

                return new BaseImage(BitmapToByteArray(bmp));
            }
        }

        public string PartNumber => GetPartNumber(Configuration);

        public double Quantity 
        {
            get 
            {
                var qtyPrp = GetPropertyValue(Configuration.CustomPropertyManager, QTY_PROPERTY);

                if (string.IsNullOrEmpty(qtyPrp))
                {
                    qtyPrp = GetPropertyValue(m_Doc.Model.Extension.CustomPropertyManager[""], QTY_PROPERTY);
                }

                if (!string.IsNullOrEmpty(qtyPrp))
                {
                    var qtyStr = GetPropertyValue(Configuration.CustomPropertyManager, qtyPrp);

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
                        qtyStr = GetPropertyValue(m_Doc.Model.Extension.CustomPropertyManager[""], qtyPrp);

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

        private string GetPropertyValue(ICustomPropertyManager prpMgr, string prpName) 
        {
            string resVal;

            if (m_Doc.App.IsVersionNewerOrEqual(SwVersion_e.Sw2018))
            {
                prpMgr.Get6(prpName, false, out _, out resVal, out _, out _);
            }
            else if (m_Doc.App.IsVersionNewerOrEqual(SwVersion_e.Sw2014))
            {
                prpMgr.Get5(prpName, false, out _, out resVal, out _);
            }
            else
            {
                prpMgr.Get4(prpName, false, out _, out resVal);
            }

            return resVal;
        }

        private string GetPartNumber(IConfiguration conf) 
        {
            switch ((swBOMPartNumberSource_e)conf.BOMPartNoSource)
            {
                case swBOMPartNumberSource_e.swBOMPartNumber_ConfigurationName:
                    return conf.Name;
                case swBOMPartNumberSource_e.swBOMPartNumber_DocumentName:
                    return m_Doc.Title;
                case swBOMPartNumberSource_e.swBOMPartNumber_ParentName:
                    return GetPartNumber(conf.GetParent());
                case swBOMPartNumberSource_e.swBOMPartNumber_UserSpecified:
                    return conf.AlternateName;
                default:
                    throw new NotSupportedException();
            }
        }

        public virtual void Commit(CancellationToken cancellationToken) => m_Creator.Create(cancellationToken);

        private IConfiguration Create(CancellationToken cancellationToken) 
        {
            IConfiguration conf;

            if (m_Doc.App.IsVersionNewerOrEqual(Enums.SwVersion_e.Sw2018))
            {
                conf = m_Doc.Model.ConfigurationManager.AddConfiguration2(Name, "", "", (int)swConfigurationOptions2_e.swConfigOption_DontActivate, "", "", false);
            }
            else 
            {
                conf = m_Doc.Model.ConfigurationManager.AddConfiguration(Name, "", "", (int)swConfigurationOptions2_e.swConfigOption_DontActivate, "", "");
            }

            if (conf == null) 
            {
                throw new Exception("Failed to create configuration");
            }

            return conf;
        }

        private byte[] BitmapToByteArray(Bitmap bmp)
        {
            using (var ms = new MemoryStream())
            {
                bmp.Save(ms, ImageFormat.Png);
                return ms.ToArray();
            }
        }

        public void Dispose()
        {
            if (m_PropertiesLazy.IsValueCreated) 
            {
                m_PropertiesLazy.Value.Dispose();
            }
        }
    }

    internal class SwLdrUnloadedConfiguration : SwConfiguration
    {
        public override string Name 
        {
            get => m_LdrConfName;
            set => throw new NotSupportedException("Name of inactive LDR configuration cannot be changed");
        }

        private string m_LdrConfName;

        internal SwLdrUnloadedConfiguration(SwDocument3D doc, string confName) 
            : base(doc, null, false)
        {
            m_LdrConfName = confName;
        }

        public override void Commit(CancellationToken cancellationToken) => throw new InactiveLdrConfgurationNotSupportedException();
        public override IXCutListItem[] CutLists => throw new InactiveLdrConfgurationNotSupportedException();
        public override object Dispatch => throw new InactiveLdrConfgurationNotSupportedException();
        public override ISwCustomPropertiesCollection Properties => throw new InactiveLdrConfgurationNotSupportedException();
    }
}