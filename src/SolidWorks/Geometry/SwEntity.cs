//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using Xarial.XCad.Documents;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;
using Xarial.XCad.SolidWorks.Documents;

namespace Xarial.XCad.SolidWorks.Geometry
{
    public interface ISwEntity : ISwSelObject, IXEntity, IResilientibleObject<ISwEntity>
    {
        IEntity Entity { get; }

        new ISwComponent Component { get; }
        new IEnumerable<ISwEntity> AdjacentEntities { get; }
        new ISwBody Body { get; }
    }

    internal abstract class SwEntity : SwSelObject, ISwEntity
    {
        IXBody IXEntity.Body => Body;
        IEnumerable<IXEntity> IXEntity.AdjacentEntities => AdjacentEntities;
        IXComponent IXEntity.Component => Component;
        IXObject IResilientibleObject.CreateResilient() => CreateResilient();

        public IEntity Entity { get; }

        public override object Dispatch => Entity;

        public abstract ISwBody Body { get; }

        public abstract IEnumerable<ISwEntity> AdjacentEntities { get; }

        public ISwComponent Component 
        {
            get 
            {
                var comp = (IComponent2)Entity.GetComponent();

                if (comp != null)
                {
                    return OwnerDocument.CreateObjectFromDispatch<ISwComponent>(comp);
                }
                else 
                {
                    return null;
                }
            }
        }

        public bool IsResilient => Entity.IsSafe;

        internal SwEntity(IEntity entity, ISwDocument doc, ISwApplication app) : base(entity, doc, app)
        {
            Entity = entity;
        }

        public override void Select(bool append)
        {
            if (!Entity.Select4(append, null))
            {
                throw new Exception("Failed to select entity");
            }
        }

        public abstract Point FindClosestPoint(Point point);

        public ISwEntity CreateResilient()
        {
            var safeEnt = Entity.GetSafeEntity();

            if (safeEnt == null) 
            {
                throw new NullReferenceException("Failed to get safe entity");
            }

            return OwnerApplication.CreateObjectFromDispatch<SwEntity>(safeEnt, OwnerDocument);
        }
    }
}