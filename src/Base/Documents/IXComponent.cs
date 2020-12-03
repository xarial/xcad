//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Xarial.XCad.Base;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry;

namespace Xarial.XCad.Documents
{
    /// <summary>
    /// Represents components in the <see cref="IXAssembly"/>
    /// </summary>
    public interface IXComponent : IXSelObject, IXObjectContainer, IXTransaction
    {
        /// <summary>
        /// Name of the component
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the path of this component
        /// </summary>
        string Path { get; }

        /// <summary>
        /// State of this component
        /// </summary>
        ComponentState_e State { get; }

        /// <summary>
        /// Document of the component
        /// </summary>
        /// <remarks>If component is rapid, view only or suppressed document migth not be loaded into the memory. Use <see cref="IXTransaction.IsCommitted"/> to check the state and call <see cref="IXTransaction.Commit(System.Threading.CancellationToken)"/> to load document if needed</remarks>
        IXDocument3D Document { get; }
        
        /// <summary>
        /// Children components
        /// </summary>
        IXComponentRepository Children { get; }

        /// <summary>
        /// Features of this components
        /// </summary>
        IXFeatureRepository Features { get; }

        /// <summary>
        /// Bodies in this component
        /// </summary>
        IXBodyRepository Bodies { get; }
    }
}
