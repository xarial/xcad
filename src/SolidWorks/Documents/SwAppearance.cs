//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Drawing;
using System.Threading;
using Xarial.XCad.Documents;
using Xarial.XCad.Toolkit.Utils;
using System.Runtime.InteropServices;
using System.Linq;
using System.Windows;
using Xarial.XCad.Services;
using SolidWorks.Interop.swconst;
using System.Windows.Controls;
using Xarial.XCad.Toolkit.Exceptions;
using System.Windows.Forms;

namespace Xarial.XCad.SolidWorks.Documents
{
    /// <summary>
    /// SOLIDWORKS-specific appearance
    /// </summary>
    public interface ISwAppearance : IXAppearance
    {
        /// <summary>
        /// Appearance settings
        /// </summary>
        IAppearanceSetting Settings { get; }

        /// <summary>
        /// Appearance level
        /// </summary>
        AppearanceLevel_e Level { get; set; }
    }

    /// <summary>
    /// SOLIDWORKS-specific render material based appearance
    /// </summary>
    public interface ISwRenderMaterial : IXAppearance
    {
        /// <summary>
        /// Render material
        /// </summary>
        IRenderMaterial RenderMaterial { get; }

        /// <summary>
        /// File path to appearance file
        /// </summary>
        string AppearanceFilePath { get; set; }
    }

    internal interface ISwAppearanceBase
    {
        void Delete();
    }

    /// <summary>
    /// Appearance level
    /// </summary>
    public enum AppearanceLevel_e 
    {
        /// <summary>
        /// Component level
        /// </summary>
        Component,

        /// <summary>
        /// Part level
        /// </summary>
        Part
    }

    internal class SwAppearance : SwObject, ISwAppearance, ISwAppearanceBase
    {
        internal static SwAppearance FromObjects(SwDisplayStateDispatch displayState, IHasColor[] objs, AppearanceLevel_e level, SwConfiguration conf, SwApplication app)
        {
            var doc = conf.OwnerDocument;

            var dispSetts = CreateDisplayStateSetting(conf, displayState.Name, objs, level);

            var apps = (object[])doc.Model.Extension.DisplayStateSpecMaterialPropertyValues[dispSetts];

            if (apps?.Any() == true)
            {
                var appSetts = (IAppearanceSetting)apps.First();

                return new SwAppearance(appSetts, displayState, objs, conf, doc, app);
            }
            else 
            {
                throw new NullReferenceException("Failed to get appearance for the specified entities");
            }
        }

        private static DisplayStateSetting CreateDisplayStateSetting(SwConfiguration conf, string name, IHasColor[] objs, AppearanceLevel_e level)
        {
            var doc = conf.OwnerDocument;

            var disps = objs?.Cast<ISwObject>().Select(x =>
            {
                object disp;

                if (x is SwDocument)
                {
                    disp = conf.Configuration.GetRootComponent3(false);
                }
                else 
                {
                    disp = x.Dispatch;
                }

                return new DispatchWrapper(disp);
            }).ToArray();

            var setts = doc.Model.Extension.GetDisplayStateSetting((int)swDisplayStateOpts_e.swSpecifyDisplayState);
            setts.Option = (int)swDisplayStateOpts_e.swSpecifyDisplayState;
            setts.Entities = disps;
            setts.Names = new string[] { name };
            setts.PartLevel = level == AppearanceLevel_e.Part;
            return setts;
        }

        public override bool IsCommitted => m_Creator.IsCreated;

        private readonly SwDisplayStateDispatch m_DisplayState;

        private readonly ElementCreator<IAppearanceSetting> m_Creator;

        private IHasColor[] m_Objects;

        private AppearanceLevel_e m_Level;

        private readonly SwConfiguration m_Conf;

        internal SwAppearance(IAppearanceSetting appSetts, SwDisplayStateDispatch displayState, IHasColor[] objs, SwConfiguration conf, SwDocument doc, SwApplication app) : base(appSetts, doc, app)
        {
            m_DisplayState = displayState;

            m_Conf = conf;

            m_Objects = objs;

            m_Creator = new ElementCreator<IAppearanceSetting>(CreateAppearance, appSetts, appSetts != null);
        }

        public IAppearanceSetting Settings => m_Creator.Element;

        public Color Color
        {
            get
            {
                if (IsCommitted)
                {
                    var baseColor = ColorUtils.FromColorRef(Settings.Color);

                    var a = Settings.Transparent;
                    var alpha = a < 0 ? 1 : (int)((1 - a) * 255);

                    return Color.FromArgb(alpha, baseColor);
                }
                else
                {
                    return m_Creator.CachedProperties.Get<Color>();
                }
            }
            set
            {
                if (IsCommitted)
                {
                    SetColor(Settings, value);
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public double Diffuse 
        {
            get
            {
                if (IsCommitted)
                {
                    return Settings.Diffuse;
                }
                else 
                {
                    return m_Creator.CachedProperties.Get<double>();
                }
            }
            set
            {
                if (IsCommitted)
                {
                    Settings.Diffuse = value;
                }
                else 
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public double Specular
        {
            get
            {
                if (IsCommitted)
                {
                    return Settings.Specular;
                }
                else
                {
                    return m_Creator.CachedProperties.Get<double>();
                }
            }
            set
            {
                if (IsCommitted)
                {
                    Settings.Specular = value;
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public Color SpecularColor
        {
            get
            {
                if (IsCommitted)
                {
                    return ColorUtils.FromColorRef(Settings.SpecularColor);
                }
                else
                {
                    return m_Creator.CachedProperties.Get<Color>();
                }
            }
            set
            {
                if (IsCommitted)
                {
                    Settings.SpecularColor = ColorUtils.ToColorRef(value);
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public double Blurriness
        {
            get
            {
                if (IsCommitted)
                {
                    return Settings.SpecularSpread;
                }
                else
                {
                    return m_Creator.CachedProperties.Get<double>();
                }
            }
            set
            {
                if (IsCommitted)
                {
                    Settings.SpecularSpread = value;
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public double Reflection
        {
            get
            {
                if (IsCommitted)
                {
                    return Settings.Reflection;
                }
                else
                {
                    return m_Creator.CachedProperties.Get<double>();
                }
            }
            set
            {
                if (IsCommitted)
                {
                    Settings.Reflection = value;
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public double Emission
        {
            get
            {
                if (IsCommitted)
                {
                    return Settings.Luminous;
                }
                else
                {
                    return m_Creator.CachedProperties.Get<double>();
                }
            }
            set
            {
                if (IsCommitted)
                {
                    Settings.Luminous = value;
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        private void SetColor(IAppearanceSetting appSetts, Color value)
        {
            appSetts.Color = ColorUtils.ToColorRef(value);

            appSetts.Transparent = (255 - value.A) / 255d;
        }

        public IHasColor[] Objects 
        {
            get 
            {
                if (IsCommitted)
                {
                    return m_Objects;
                }
                else 
                {
                    return m_Creator.CachedProperties.Get<IHasColor[]>();
                }
            }
            set 
            {
                if (IsCommitted)
                {
                    if (value?.Any() == true)
                    {
                        m_Objects = value;
                        m_Creator.Set(CreateAppearance(default));
                    }
                    else 
                    {
                        Delete();
                    }
                }
                else 
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public AppearanceLevel_e Level
        {
            get
            {
                if (IsCommitted)
                {
                    return m_Level;
                }
                else
                {
                    return m_Creator.CachedProperties.Get<AppearanceLevel_e>();
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

        public void Delete()
        {
            var setts = CreateDisplayStateSetting(m_Conf, m_DisplayState.Name, Objects, 0);
            setts.RemoveAppearance = true;
            OwnerDocument.Model.Extension.DisplayStateSpecMaterialPropertyValues[setts] = new IAppearanceSetting[] { Settings };
        }

        public override void Commit(CancellationToken cancellationToken) => m_Creator.Create(cancellationToken);

        private IAppearanceSetting CreateAppearance(CancellationToken token)
        {
            var appSetts = OwnerDocument.Model.Extension.GetAppearanceSetting();

            var setts = CreateDisplayStateSetting(m_Conf, m_DisplayState.Name, Objects, Level);
            
            SetColor(appSetts, Color);
            appSetts.Diffuse = Diffuse;
            appSetts.Specular = Specular;
            appSetts.SpecularColor = ColorUtils.ToColorRef(SpecularColor);
            appSetts.SpecularSpread = Blurriness;
            appSetts.Reflection = Reflection;
            appSetts.Luminous = Emission;

            m_Level = Level;

            OwnerDocument.Model.Extension.DisplayStateSpecMaterialPropertyValues[setts] = new IAppearanceSetting[] { appSetts };
            return appSetts;
        }
    }

    internal class SwRenderMaterial : SwObject, ISwRenderMaterial, ISwAppearanceBase
    {
        public override bool IsCommitted => m_Creator.IsCreated;

        public IRenderMaterial RenderMaterial => m_Creator.Element;

        private readonly ElementCreator<IRenderMaterial> m_Creator;

        private readonly SwDisplayStateDispatch m_DisplayState;

        internal SwRenderMaterial(IRenderMaterial renderMaterial, SwDisplayStateDispatch dispState, SwDocument doc, SwApplication app) : base(renderMaterial, doc, app)
        {
            m_DisplayState = dispState;
            m_Creator = new ElementCreator<IRenderMaterial>(CreateRenderMaterial, renderMaterial, renderMaterial != null);
        }

        public Color Color
        {
            get
            {
                if (IsCommitted)
                {
                    var baseColor = ColorUtils.FromColorRef(RenderMaterial.PrimaryColor);

                    var a = RenderMaterial.Transparency;
                    var alpha = a < 0 ? 1 : (int)((1 - a) * 255);

                    return Color.FromArgb(alpha, baseColor);
                }
                else
                {
                    return m_Creator.CachedProperties.Get<Color>();
                }
            }
            set
            {
                if (IsCommitted)
                {
                    SetColor(RenderMaterial, value);
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public string AppearanceFilePath
        {
            get
            {
                if (IsCommitted)
                {
                    return RenderMaterial.FileName;
                }
                else
                {
                    return m_Creator.CachedProperties.Get<string>();
                }
            }
            set
            {
                if (IsCommitted)
                {
                    RenderMaterial.FileName = value;
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public double Diffuse
        {
            get
            {
                if (IsCommitted)
                {
                    return RenderMaterial.Diffuse;
                }
                else
                {
                    return m_Creator.CachedProperties.Get<double>();
                }
            }
            set
            {
                if (IsCommitted)
                {
                    RenderMaterial.Diffuse = value;
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public double Specular
        {
            get
            {
                if (IsCommitted)
                {
                    return RenderMaterial.Specular;
                }
                else
                {
                    return m_Creator.CachedProperties.Get<double>();
                }
            }
            set
            {
                if (IsCommitted)
                {
                    RenderMaterial.Specular = value;
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }


        public Color SpecularColor
        {
            get
            {
                if (IsCommitted)
                {
                    return ColorUtils.FromColorRef(RenderMaterial.SpecularColor);
                }
                else
                {
                    return m_Creator.CachedProperties.Get<Color>();
                }
            }
            set
            {
                if (IsCommitted)
                {
                    RenderMaterial.SpecularColor = ColorUtils.ToColorRef(value);
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public double Blurriness
        {
            get
            {
                if (IsCommitted)
                {
                    return RenderMaterial.Roughness;
                }
                else
                {
                    return m_Creator.CachedProperties.Get<double>();
                }
            }
            set
            {
                if (IsCommitted)
                {
                    RenderMaterial.Roughness = value;
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public double Reflection
        {
            get
            {
                if (IsCommitted)
                {
                    return RenderMaterial.Reflectivity;
                }
                else
                {
                    return m_Creator.CachedProperties.Get<double>();
                }
            }
            set
            {
                if (IsCommitted)
                {
                    RenderMaterial.Reflectivity = value;
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public double Emission
        {
            get
            {
                if (IsCommitted)
                {
                    return RenderMaterial.Emission;
                }
                else
                {
                    return m_Creator.CachedProperties.Get<double>();
                }
            }
            set
            {
                if (IsCommitted)
                {
                    RenderMaterial.Emission = value;
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        private void SetColor(IRenderMaterial renderMat, Color value)
        {
            var colorRef = ColorUtils.ToColorRef(value);

            renderMat.PrimaryColor = colorRef;
            renderMat.SecondaryColor = colorRef;
            renderMat.TertiaryColor = colorRef;

            renderMat.Transparency = (255 - value.A) / 255d;
        }

        public IHasColor[] Objects
        {
            get
            {
                if (IsCommitted)
                {
                    return ((object[])RenderMaterial.GetEntities()).Select(OwnerDocument.CreateObjectFromDispatch<SwObject>).Cast<IHasColor>().ToArray();
                }
                else
                {
                    return m_Creator.CachedProperties.Get<IHasColor[]>();
                }
            }
            set
            {
                if (IsCommitted)
                {
                    RenderMaterial.RemoveAllEntities();

                    if (value?.Any() == true)
                    {
                        SetEntities(RenderMaterial, value);

                        SetRenderMaterial(RenderMaterial);
                    }
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        private void SetEntities(IRenderMaterial renderMat, IHasColor[] value)
        {
            if (value?.Any() == true)
            {
                foreach (SwObject ent in value)
                {
                    if (!renderMat.AddEntity(ent.Dispatch))
                    {
                        throw new Exception("Failed to add entity to render material");
                    }
                }
            }
            else 
            {
                throw new NullReferenceException("No entities specified");
            }
        }

        private void SetRenderMaterial(IRenderMaterial renderMaterial)
        {
            if (!OwnerDocument.Model.Extension.AddDisplayStateSpecificRenderMaterial((RenderMaterial)renderMaterial,
                (int)swDisplayStateOpts_e.swSpecifyDisplayState, new string[] { m_DisplayState.Name }, out _, out _))
            {
                throw new Exception("Failed to add render material");
            }
        }

        public void Delete() => RenderMaterial.RemoveAllEntities();

        public override void Commit(CancellationToken cancellationToken) => m_Creator.Create(cancellationToken);

        private IRenderMaterial CreateRenderMaterial(CancellationToken token)
        {
            var renderMaterial = OwnerDocument.Model.Extension.CreateRenderMaterial(AppearanceFilePath);

            SetColor(renderMaterial, Color);
            renderMaterial.Diffuse = Diffuse;
            renderMaterial.Specular = Specular;
            renderMaterial.SpecularColor = ColorUtils.ToColorRef(SpecularColor);
            renderMaterial.Roughness = Blurriness;
            renderMaterial.Reflectivity = Reflection;
            renderMaterial.Emission = Emission;
            SetEntities(renderMaterial, Objects);
            SetRenderMaterial(renderMaterial);

            return renderMaterial;
        }
    }
}
