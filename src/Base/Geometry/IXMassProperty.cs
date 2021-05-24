using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.Geometry
{
    public interface IXMassProperty : IXTransaction, IDisposable
    {
        Point CenterOfGravity { get; }
        double SurfaceArea { get; }
        double Volume { get; }
        double Mass { get; }
        double Density { get; }
        PrincipalAxesOfInertia PrincipalAxesOfInertia { get; }
        PrincipalMomentOfInertia PrincipalMomentOfInertia { get; }
        MomentOfInertia MomentOfInertia { get; }

        TransformMatrix RelativeTo { get; set; }
        IXBody[] Scope { get; set; }
        bool SystemUnits { get; set; }
    }

    public interface IXAssemblyMassProperty : IXMassProperty 
    {
        new IXComponent[] Scope { get; set; }
        bool IncludeHidden { get; set; }
    }
}