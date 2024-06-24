//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.Geometry.Wires;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Geometry.Curves;
using Xarial.XCad.SolidWorks.Utils;
using System.Runtime.InteropServices;

namespace Xarial.XCad.SolidWorks.Features
{
    /// <summary>
    /// SOLIDWORKS specific coordinate system
    /// </summary>
    public interface ISwCoordinateSystem : IXCoordinateSystem, ISwFeature
    {
        /// <summary>
        /// Definition of coordinate system
        /// </summary>
        ICoordinateSystemFeatureData CoordSys { get; }
    }

    internal class SwCoordinateSystem : SwFeature, ISwCoordinateSystem
    {
        public ICoordinateSystemFeatureData CoordSys { get; private set; }

        internal SwCoordinateSystem(IFeature feat, SwDocument doc, SwApplication app, bool created) : base(feat, doc, app, created)
        {
            if (feat != null)
            {
                CoordSys = feat.GetDefinition() as ICoordinateSystemFeatureData;
            }
        }

        public TransformMatrix Transform
            => CoordSys.Transform.ToTransformMatrix();

        public IXPoint Origin 
        {
            get 
            {
                if (IsCommitted)
                {
                    return OwnerDocument.CreateObjectFromDispatch<ISwPoint>(CoordSys.OriginEntity);
                }
                else 
                {
                    return m_Creator.CachedProperties.Get<IXPoint>();
                }
            }
            set 
            {
                if (IsCommitted)
                {
                    CoordSys.OriginEntity = ((ISwPoint)value).Dispatch;
                }
                else 
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public IXLine AxisX
        {
            get
            {
                if (IsCommitted)
                {
                    var ents = (object[])CoordSys.XAxisEntities;

                    if (ents?.Any() == true)
                    {
                        return (IXLine)OwnerDocument.CreateObjectFromDispatch<ISwObject>(ents.First());
                    }
                    else 
                    {
                        return null;
                    }
                }
                else
                {
                    return m_Creator.CachedProperties.Get<IXLine>();
                }
            }
            set
            {
                if (IsCommitted)
                {
                    if (value != null)
                    {
                        CoordSys.XAxisEntities = new DispatchWrapper[] { new DispatchWrapper(((ISwObject)value).Dispatch) };
                    }
                    else 
                    {
                        CoordSys.XAxisEntities = null;
                    }
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public IXLine AxisY
        {
            get
            {
                if (IsCommitted)
                {
                    var ents = (object[])CoordSys.YAxisEntities;

                    if (ents?.Any() == true)
                    {
                        return (IXLine)OwnerDocument.CreateObjectFromDispatch<ISwObject>(ents.First());
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return m_Creator.CachedProperties.Get<IXLine>();
                }
            }
            set
            {
                if (IsCommitted)
                {
                    if (value != null)
                    {
                        CoordSys.YAxisEntities = new DispatchWrapper[] { new DispatchWrapper(((ISwObject)value).Dispatch) };
                    }
                    else
                    {
                        CoordSys.YAxisEntities = null;
                    }
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public IXLine AxisZ
        {
            get
            {
                if (IsCommitted)
                {
                    var ents = (object[])CoordSys.ZAxisEntities;

                    if (ents?.Any() == true)
                    {
                        return (IXLine)OwnerDocument.CreateObjectFromDispatch<ISwObject>(ents.First());
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return m_Creator.CachedProperties.Get<IXLine>();
                }
            }
            set
            {
                if (IsCommitted)
                {
                    if (value != null)
                    {
                        CoordSys.ZAxisEntities = new DispatchWrapper[] { new DispatchWrapper(((ISwObject)value).Dispatch) };
                    }
                    else
                    {
                        CoordSys.ZAxisEntities = null;
                    }
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public bool AxisXFlipped 
        {
            get 
            {
                if (IsCommitted)
                {
                    return CoordSys.XFlipped;
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
                    CoordSys.XFlipped = value;
                }
                else 
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public bool AxisYFlipped
        {
            get
            {
                if (IsCommitted)
                {
                    return CoordSys.YFlipped;
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
                    CoordSys.YFlipped = value;
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        public bool AxisZFlipped
        {
            get
            {
                if (IsCommitted)
                {
                    return CoordSys.ZFlipped;
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
                    CoordSys.ZFlipped = value;
                }
                else
                {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        protected override IFeature InsertFeature(CancellationToken cancellationToken)
        {
            using (var selGrp = new SelectionGroup(OwnerDocument, true)) 
            {
                var selMgr = OwnerDocument.Model.ISelectionManager;

                if (Origin != null) 
                {
                    var selData = selMgr.CreateSelectData();
                    selData.Mark = 1;
                    selGrp.Add(((ISwObject)Origin).Dispatch, selData);
                }

                if (AxisX != null)
                {
                    var selData = selMgr.CreateSelectData();
                    selData.Mark = 2;
                    selGrp.Add(((ISwObject)AxisX).Dispatch, selData);
                }

                if (AxisY != null)
                {
                    var selData = selMgr.CreateSelectData();
                    selData.Mark = 4;
                    selGrp.Add(((ISwObject)AxisY).Dispatch, selData);
                }

                if (AxisZ != null)
                {
                    var selData = selMgr.CreateSelectData();
                    selData.Mark = 8;
                    selGrp.Add(((ISwObject)AxisZ).Dispatch, selData);
                }

                var feat = OwnerDocument.Model.FeatureManager.InsertCoordinateSystem(AxisXFlipped, AxisYFlipped, AxisZFlipped);

                if (feat != null)
                {
                    CoordSys = (ICoordinateSystemFeatureData)feat.GetDefinition();
                }

                return feat;
            }
        }
    }
}
