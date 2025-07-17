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

    internal class SwAppearance : SwObject, ISwAppearance
    {
        internal static SwAppearance FromObjects(SwDisplayStateDispatch displayState, IHasColor[] objs, AppearanceLevel_e level, SwDocument doc, SwApplication app)
        {
            var dispSetts = CreateDisplayStateSetting(doc, displayState.Name, objs, level);

            var apps = (object[])doc.Model.Extension.DisplayStateSpecMaterialPropertyValues[dispSetts];

            var appSetts = (IAppearanceSetting)apps.First();

            return new SwAppearance(appSetts, displayState, objs, doc, app);
        }

        private static DisplayStateSetting CreateDisplayStateSetting(SwDocument ownerDoc, string name, IHasColor[] objs, AppearanceLevel_e level)
        {
            var disps = objs?.Cast<ISwObject>().Select(x => new DispatchWrapper(x.Dispatch)).ToArray();

            var setts = ownerDoc.Model.Extension.GetDisplayStateSetting((int)swDisplayStateOpts_e.swSpecifyDisplayState);
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

        internal SwAppearance(IAppearanceSetting appSetts, SwDisplayStateDispatch displayState, IHasColor[] objs, SwDocument doc, SwApplication app) : base(appSetts, doc, app)
        {
            m_DisplayState = displayState;

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
                    m_Objects = value;
                    m_Creator.Set(CreateAppearance(default));
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

        internal void Delete()
        {
            var setts = CreateDisplayStateSetting(OwnerDocument, m_DisplayState.Name, Objects, 0);
            setts.RemoveAppearance = true;
            OwnerDocument.Model.Extension.DisplayStateSpecMaterialPropertyValues[setts] = new IAppearanceSetting[] { Settings };
        }

        public override void Commit(CancellationToken cancellationToken) => m_Creator.Create(cancellationToken);

        private IAppearanceSetting CreateAppearance(CancellationToken token)
        {
            var appSetts = OwnerDocument.Model.Extension.GetAppearanceSetting();

            var setts = CreateDisplayStateSetting(OwnerDocument, m_DisplayState.Name, Objects, Level);
            
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
}
