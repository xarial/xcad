//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using Xarial.XCad.Annotations;
using Xarial.XCad.Data;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Features;
using Xarial.XCad.Reflection;
using Xarial.XCad.Services;
using Xarial.XCad.SolidWorks.Annotations;
using Xarial.XCad.SolidWorks.Data;
using Xarial.XCad.SolidWorks.Documents.Exceptions;
using Xarial.XCad.SolidWorks.Enums;
using Xarial.XCad.SolidWorks.Features;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.Toolkit;
using Xarial.XCad.Toolkit.Exceptions;
using Xarial.XCad.Toolkit.Graphics;
using Xarial.XCad.UI;

namespace Xarial.XCad.SolidWorks.Documents
{
    /// <summary>
    /// SOLIDWORKS-specific configuration
    /// </summary>
    public interface ISwConfiguration : ISwSelObject, IXConfiguration, IDisposable
    {
        /// <summary>
        /// Pointer to configuration
        /// </summary>
        IConfiguration Configuration { get; }

        /// <inheritdoc/>
        new ISwCustomPropertiesCollection Properties { get; }
    }

    [DebuggerDisplay("{" + nameof(Name) + "}")]
    internal class SwConfiguration : SwSelObject, ISwConfiguration
    {
        internal const string QTY_PROPERTY = "UNIT_OF_MEASURE";

        public IConfiguration Configuration => m_Creator.Element;

        private readonly SwDocument3D m_Doc;

        public IXIdentifier Id => new XIdentifier(Configuration.GetID());

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

        public virtual string Description
        {
            get
            {
                if (m_Creator.IsCreated)
                {
                    return Configuration.Description;
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
                    Configuration.Description = value;
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public virtual string Comment
        {
            get
            {
                if (m_Creator.IsCreated)
                {
                    return Configuration.Comment;
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
                    Configuration.Comment = value;
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        IXPropertyRepository IXConfiguration.Properties => Properties;
        IXDimensionRepository IDimensionable.Dimensions => Dimensions;

        public virtual ISwCustomPropertiesCollection Properties => m_PropertiesLazy.Value;
        public ISwDimensionsCollection Dimensions => m_DimensionsLazy.Value;

        private readonly Lazy<SwCustomPropertiesCollection> m_PropertiesLazy;
        private readonly Lazy<SwDimensionsCollection> m_DimensionsLazy;

        public override bool IsCommitted => m_Creator.IsCreated;

        private readonly IElementCreator<IConfiguration> m_Creator;

        private readonly SwPartNumber m_PartNumber;

        internal SwConfiguration(IConfiguration conf, SwDocument3D doc, SwApplication app, bool created) : base(conf, doc, app)
        {
            m_Doc = doc;

            m_Creator = new ElementCreator<IConfiguration>(Create, OnCreated, conf, created);

            m_PropertiesLazy = new Lazy<SwCustomPropertiesCollection>(
                () => new SwConfigurationCustomPropertiesCollection(this, m_Doc, OwnerApplication));

            m_DimensionsLazy = new Lazy<SwDimensionsCollection>(CreateDimensions);

            m_PartNumber = new SwPartNumber(this);
        }

        public override object Dispatch => Configuration;

        public IXImage Preview
        {
            get
            {
                if (OwnerApplication.IsInProcess())
                {
                    return PictureDispUtils.PictureDispToXImage(OwnerApplication.Sw.GetPreviewBitmap(m_Doc.Path, Name));
                }
                else
                {
                    return new XDrawingImage(m_Doc.GetThumbnailImage());
                }
            }
        }

        public IPartNumber PartNumber => m_PartNumber;

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

        public BomChildrenSolving_e BomChildrenSolving
        {
            get
            {
                if (IsCommitted)
                {
                    if (m_Doc is ISwAssembly)
                    {
                        var bomDispOpt = Configuration.ChildComponentDisplayInBOM;

                        switch ((swChildComponentInBOMOption_e)bomDispOpt)
                        {
                            case swChildComponentInBOMOption_e.swChildComponent_Show:
                                return BomChildrenSolving_e.Show;

                            case swChildComponentInBOMOption_e.swChildComponent_Hide:
                                return BomChildrenSolving_e.Hide;

                            case swChildComponentInBOMOption_e.swChildComponent_Promote:
                                return BomChildrenSolving_e.Promote;

                            default:
                                throw new NotSupportedException($"Not supported BOM display option: {bomDispOpt}");
                        }
                    }
                    else
                    {
                        return BomChildrenSolving_e.Show;
                    }
                }
                else 
                {
                    return m_Creator.CachedProperties.Get<BomChildrenSolving_e>();
                }
            }
            set 
            {
                if (IsCommitted)
                {
                    SetBomChildrenSolving(Configuration, value);
                }
                else 
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public ConfigurationOptions_e Options 
        {
            get 
            {
                if (IsCommitted)
                {
                    ConfigurationOptions_e opts = 0;

                    if (Configuration.SuppressNewComponentModels)
                    {
                        opts |= ConfigurationOptions_e.SuppressNewComponents;
                    }

                    if (Configuration.SuppressNewFeatures)
                    {
                        opts |= ConfigurationOptions_e.SuppressNewFeatures;
                    }

                    if (Configuration.UseDescriptionInBOM)
                    {
                        opts |= ConfigurationOptions_e.UseConfigurationDescriptionInBom;
                    }

                    if (Configuration.UseAlternateNameInBOM)
                    {
                        opts |= ConfigurationOptions_e.UseUserDefinedNameInBom;
                    }

                    return opts;
                }
                else 
                {
                    return m_Creator.CachedProperties.Get<ConfigurationOptions_e>();
                }
            }
            set 
            {
                if (IsCommitted)
                {
                    Configuration.SuppressNewComponentModels = value.HasFlag(ConfigurationOptions_e.SuppressNewComponents);
                    Configuration.SuppressNewFeatures = value.HasFlag(ConfigurationOptions_e.SuppressNewFeatures);
                    Configuration.UseDescriptionInBOM = value.HasFlag(ConfigurationOptions_e.UseConfigurationDescriptionInBom);
                    Configuration.UseAlternateNameInBOM = value.HasFlag(ConfigurationOptions_e.UseUserDefinedNameInBom);
                }
                else 
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public virtual IXConfiguration Parent 
        {
            get 
            {
                if (IsCommitted)
                {
                    var conf = Configuration.GetParent();

                    if (conf != null)
                    {
                        return OwnerDocument.CreateObjectFromDispatch<ISwConfiguration>(conf);
                    }
                    else
                    {
                        return null;
                    }
                }
                else 
                {
                    return m_Creator.CachedProperties.Get<IXConfiguration>();
                }
            }
            set 
            {
                if (IsCommitted)
                {
                    throw new CommittedElementPropertyChangeNotSupported();
                }
                else 
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public TSelObject ConvertObject<TSelObject>(TSelObject obj)
            where TSelObject : class, IXSelObject
        {
            switch (obj) 
            {
                case SwDimension dim:
                    return dim.Clone(new Context(this)) as TSelObject;

                case SwFeature feat:
                    return feat.Clone(new Context(this)) as TSelObject;

                default:
                    throw new NotSupportedException();
            }
        }

        internal override void Select(bool append, ISelectData selData)
        {
            if (!Configuration.Select2(append, (SelectData)selData)) 
            {
                throw new Exception("Failed to select configuration");
            }
        }

        public override void Delete()
        {
            if (m_Doc.Configurations.Active.Equals(this)) 
            {
                var otherConf = (ISwConfiguration)m_Doc.Configurations.Except(new ISwConfiguration[] { this }, new XObjectEqualityComparer<IXConfiguration>()).FirstOrDefault();

                if (otherConf != null)
                {
                    m_Doc.Configurations.Active = otherConf;
                }
                else 
                {
                    throw new Exception("Cannot delete the last configuration");
                }
            }

            if (!m_Doc.Model.DeleteConfiguration2(Name))
            {
                throw new Exception($"Failed to delete configuration '{Name}'");
            }
        }

        private void SetBomChildrenSolving(IConfiguration conf, BomChildrenSolving_e value)
        {
            swChildComponentInBOMOption_e bomDispOpt;

            switch (value)
            {
                case BomChildrenSolving_e.Show:
                    bomDispOpt = swChildComponentInBOMOption_e.swChildComponent_Show;
                    break;

                case BomChildrenSolving_e.Hide:
                    bomDispOpt = swChildComponentInBOMOption_e.swChildComponent_Hide;
                    break;

                case BomChildrenSolving_e.Promote:
                    bomDispOpt = swChildComponentInBOMOption_e.swChildComponent_Promote;
                    break;

                default:
                    throw new NotSupportedException();
            }

            conf.ChildComponentDisplayInBOM = (int)bomDispOpt;
        }

        private string GetPropertyValue(ICustomPropertyManager prpMgr, string prpName) 
        {
            string resVal;

            if (OwnerApplication.IsVersionNewerOrEqual(SwVersion_e.Sw2018))
            {
                prpMgr.Get6(prpName, false, out _, out resVal, out _, out _);
            }
            else if (OwnerApplication.IsVersionNewerOrEqual(SwVersion_e.Sw2014))
            {
                prpMgr.Get5(prpName, false, out _, out resVal, out _);
            }
            else
            {
                prpMgr.Get4(prpName, false, out _, out resVal);
            }

            return resVal;
        }

        private void OnCreated(IConfiguration conf, CancellationToken cancellationToken)
        {
            if (m_DimensionsLazy.IsValueCreated) 
            {
                m_DimensionsLazy.Value.CommitCache(cancellationToken);
            }

            if (m_PropertiesLazy.IsValueCreated) 
            {
                m_PropertiesLazy.Value.CommitCache(cancellationToken);
            }

            m_PartNumber.Commit();
        }

        public override void Commit(CancellationToken cancellationToken) => m_Creator.Create(cancellationToken);

        protected virtual SwDimensionsCollection CreateDimensions()
            => new SwFeatureManagerDimensionsCollection(new SwDocumentFeatureManager(m_Doc, m_Doc.OwnerApplication, new Context(this)), new Context(this));

        private IConfiguration Create(CancellationToken cancellationToken) 
        {
            IConfiguration conf;

            var name = Name;
            var desc = Description;
            var comment = Comment;

            if (!string.IsNullOrEmpty(name))
            {
                var opts = Options;

                var bomOpts = BomChildrenSolving;

                var confOpts = swConfigurationOptions2_e.swConfigOption_DontActivate;

                if (bomOpts == BomChildrenSolving_e.Promote)
                {
                    confOpts |= swConfigurationOptions2_e.swConfigOption_DoDisolveInBOM;
                }

                if (opts.HasFlag(ConfigurationOptions_e.SuppressNewComponents))
                {
                    confOpts |= swConfigurationOptions2_e.swConfigOption_MinFeatureManager;
                }

                if (opts.HasFlag(ConfigurationOptions_e.SuppressNewFeatures))
                {
                    confOpts |= swConfigurationOptions2_e.swConfigOption_SuppressByDefault;
                }

                if (opts.HasFlag(ConfigurationOptions_e.UseConfigurationDescriptionInBom))
                {
                    confOpts |= swConfigurationOptions2_e.swConfigOption_UseDescriptionInBOM;
                }

                if (opts.HasFlag(ConfigurationOptions_e.UseUserDefinedNameInBom))
                {
                    confOpts |= swConfigurationOptions2_e.swConfigOption_UseAlternateName;
                }

                if (OwnerApplication.IsVersionNewerOrEqual(SwVersion_e.Sw2018))
                {
                    conf = m_Doc.Model.ConfigurationManager.AddConfiguration2(name, comment, "", (int)confOpts, Parent?.Name, desc, false);
                }
                else
                {
                    conf = m_Doc.Model.ConfigurationManager.AddConfiguration(name, comment, "", (int)confOpts, Parent?.Name, desc);
                }

                if (conf != null)
                {
                    if (bomOpts != BomChildrenSolving_e.Promote) 
                    {
                        SetBomChildrenSolving(conf, bomOpts);
                    }

                    return conf;
                }
                else 
                {
                    throw new Exception("Failed to create configuration");
                }
            }
            else 
            {
                throw new Exception("Name is not specified");
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

    internal abstract class SwComponentConfiguration : SwConfiguration
    {
        private static IConfiguration GetConfiguration(SwComponent comp, string compName)
        {
            var doc = comp.ReferencedDocument;

            if (doc.IsCommitted)
            {
                return (IConfiguration)doc.Model.GetConfigurationByName(compName);
            }
            else
            {
                return null;
            }
        }

        protected readonly SwComponent m_Comp;

        internal SwComponentConfiguration(SwComponent comp, SwApplication app, string confName)
            : this(GetConfiguration(comp, confName), (SwDocument3D)comp.ReferencedDocument, app, comp.Component.ReferencedConfiguration)
        {
            m_Comp = comp;
        }

        public override IXConfiguration Parent
        {
            get
            {
                var conf = Configuration.GetParent();

                if (conf != null)
                {
                    return m_Comp.GetReferencedConfiguration(conf.Name);
                }
                else
                {
                    return null;
                }
            }
        }

        private SwComponentConfiguration(IConfiguration conf, SwDocument3D doc, SwApplication app, string name)
            : base(conf, doc, app, conf != null)
        {
            if (conf == null)
            {
                Name = name;
            }
        }

        protected override SwDimensionsCollection CreateDimensions()
            => new SwFeatureManagerDimensionsCollection(
                new SwComponentFeatureManager(m_Comp, m_Comp.RootAssembly, OwnerApplication, new Context(this)), new Context(this));
    }

    internal class SwPartComponentConfiguration : SwComponentConfiguration, ISwPartConfiguration
    {
        public SwPartComponentConfiguration(SwPartComponent comp, SwApplication app, string confName) : base(comp, app, confName)
        {
            CutLists = new SwPartComponentCutListItemCollection(comp);
        }

        public IXCutListItemRepository CutLists { get; }

        public IXMaterial Material
        {
            get => ((SwPart)m_Comp.ReferencedDocument).GetMaterial(Name);
            set => ((SwPart)m_Comp.ReferencedDocument).SetMaterial(value, Name);
        }
    }

    internal class SwAssemblyComponentConfiguration : SwComponentConfiguration, ISwAssemblyConfiguration
    {
        public SwAssemblyComponentConfiguration(SwComponent comp, SwApplication app, string confName) : base(comp, app, confName)
        {
        }

        public IXComponentRepository Components => m_Comp.Children;
    }

    internal class SwViewOnlyUnloadedConfiguration : SwConfiguration
    {
        public override string Name
        {
            get => m_ViewOnlyConfName;
            set => throw new NotSupportedException("Name of view-only configuration cannot be changed");
        }

        public override string Description { get => throw new InactiveLdrConfigurationNotSupportedException(); set => throw new InactiveLdrConfigurationNotSupportedException(); }
        public override string Comment { get => throw new InactiveLdrConfigurationNotSupportedException(); set => throw new InactiveLdrConfigurationNotSupportedException(); }

        private string m_ViewOnlyConfName;

        internal SwViewOnlyUnloadedConfiguration(string confName, SwDocument3D doc, SwApplication app)
            : base(null, doc, app, false)
        {
            m_ViewOnlyConfName = confName;
        }

        public override void Commit(CancellationToken cancellationToken) => throw new InactiveLdrConfigurationNotSupportedException();
        public override object Dispatch => throw new InactiveLdrConfigurationNotSupportedException();
        public override ISwCustomPropertiesCollection Properties => throw new InactiveLdrConfigurationNotSupportedException();
    }

    internal class SwLdrAssemblyUnloadedConfiguration : SwAssemblyConfiguration
    {
        public override string Name 
        {
            get => m_LdrConfName;
            set => throw new NotSupportedException("Name of inactive LDR configuration cannot be changed");
        }

        private string m_LdrConfName;

        internal SwLdrAssemblyUnloadedConfiguration(SwAssembly assm, SwApplication app, string confName) 
            : base(null, assm, app, false)
        {
            m_LdrConfName = confName;
        }

        public override string Description { get => throw new InactiveLdrConfigurationNotSupportedException(); set => throw new InactiveLdrConfigurationNotSupportedException(); }
        public override string Comment { get => throw new InactiveLdrConfigurationNotSupportedException(); set => throw new InactiveLdrConfigurationNotSupportedException(); }
        public override void Commit(CancellationToken cancellationToken) => throw new InactiveLdrConfigurationNotSupportedException();
        public override object Dispatch => throw new InactiveLdrConfigurationNotSupportedException();
        public override ISwCustomPropertiesCollection Properties => throw new InactiveLdrConfigurationNotSupportedException();
    }

    internal class SwLdrPartUnloadedConfiguration : SwPartConfiguration
    {
        public override string Name
        {
            get => m_LdrConfName;
            set => throw new NotSupportedException("Name of inactive LDR configuration cannot be changed");
        }

        private string m_LdrConfName;

        internal SwLdrPartUnloadedConfiguration(SwPart part, SwApplication app, string confName)
            : base(null, part, app, false)
        {
            m_LdrConfName = confName;
        }

        public override string Description { get => throw new InactiveLdrConfigurationNotSupportedException(); set => throw new InactiveLdrConfigurationNotSupportedException(); }
        public override string Comment { get => throw new InactiveLdrConfigurationNotSupportedException(); set => throw new InactiveLdrConfigurationNotSupportedException(); }
        public override void Commit(CancellationToken cancellationToken) => throw new InactiveLdrConfigurationNotSupportedException();
        public override object Dispatch => throw new InactiveLdrConfigurationNotSupportedException();
        public override ISwCustomPropertiesCollection Properties => throw new InactiveLdrConfigurationNotSupportedException();
    }
}