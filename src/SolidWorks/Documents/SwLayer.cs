//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Drawing;
using System.Threading;
using Xarial.XCad.Documents;
using Xarial.XCad.Services;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.SolidWorks.Documents
{
    public interface ISwLayer : IXLayer, ISwObject
    {
        ILayer Layer { get; }
    }

    internal class SwLayer : SwObject, ISwLayer
    {
        private readonly ElementCreator<ILayer> m_Creator;

        private readonly SwDocument m_Doc;

        internal SwLayer(ILayer layer, SwDocument doc, SwApplication app) : base(layer, doc, app)
        {
            m_Doc = doc;

            m_Creator = new ElementCreator<ILayer>(CreateLayer, layer, layer != null);
        }

        public override object Dispatch => Layer;

        public ILayer Layer => m_Creator.Element;

        public string Name
        {
            get
            {
                if (IsCommitted)
                {
                    return Layer.Name;
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
                    Layer.Name = value;
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public bool Visible
        {
            get
            {
                if (IsCommitted)
                {
                    return Layer.Visible;
                }
                else
                {
                    return m_Creator.CachedProperties.Get<bool>();
                }
            }
            set
            {
                if (IsCommitted)
                {
                    Layer.Visible = value;
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public bool IsCommitted => m_Creator.IsCreated;

        public Color? Color
        {
            get
            {
                if (IsCommitted)
                {
                    var colorRef = Layer.Color;

                    if (colorRef != 0)
                    {
                        return ColorUtils.FromColorRef(colorRef);
                    }
                    else 
                    {
                        return null;
                    }
                }
                else
                {
                    return m_Creator.CachedProperties.Get<Color?>();
                }
            }
            set
            {
                if (IsCommitted)
                {
                    if (value.HasValue)
                    {
                        Layer.Color = ColorUtils.ToColorRef(value.Value);
                    }
                    else 
                    {
                        Layer.Color = 0;
                    }
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public void Commit(CancellationToken cancellationToken) => m_Creator.Create(cancellationToken);

        private ILayer CreateLayer(CancellationToken cancellationToken)
        {
            var layerMgr = m_Doc.Model.IGetLayerManager();

            if (string.IsNullOrEmpty(Name)) 
            {
                throw new Exception("Layer name is not specified");
            }

            const int SUCCEEDED = 1;

            if (layerMgr.AddLayer(Name, "", Color.HasValue ? ColorUtils.ToColorRef(Color.Value) : 0, (int)swLineStyles_e.swLineDEFAULT, (int)swLineWeights_e.swLW_NORMAL) == SUCCEEDED)
            {
                return layerMgr.IGetLayer(Name);
            }
            else 
            {
                throw new Exception("Failed to create layer");
            }
        }
    }
}
