//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using Xarial.XCad.Geometry;

namespace Xarial.XCad.SolidWorks.Geometry
{
    public interface ISwEntity : IXEntity
    {
        IEntity Entity { get; }

        new IEnumerable<ISwEntity> AdjacentEntities { get; }
        new ISwBody Body { get; }
    }

    internal abstract class SwEntity : SwSelObject, ISwEntity
    {
        IXBody IXEntity.Body => Body;
        IEnumerable<IXEntity> IXEntity.AdjacentEntities => AdjacentEntities;

        public IEntity Entity { get; }

        public abstract ISwBody Body { get; }

        public abstract IEnumerable<ISwEntity> AdjacentEntities { get; }

        internal SwEntity(IEntity entity) : base(null, entity)
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
    }
}