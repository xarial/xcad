//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xarial.XCad.Annotations;
using Xarial.XCad.Base;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry;
using Xarial.XCad.Geometry.Structures;

namespace Xarial.XCad.Documents
{
    /// <summary>
    /// Represents components in the <see cref="IXAssembly"/>
    /// </summary>
    public interface IXComponent : IXSelObject, IXObjectContainer, IXTransaction, IXColorizable, IDimensionable
    {
        /// <summary>
        /// Full name of the component including the hierarchical path
        /// </summary>
        string FullName { get; }

        /// <summary>
        /// Name of the component
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the path of this component
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Returns the referenced configuration of this component
        /// </summary>
        /// <remarks>For unloaded or rapid components this configuration may be uncommitted</remarks>
        IXConfiguration ReferencedConfiguration { get; }

        /// <summary>
        /// State of this component
        /// </summary>
        ComponentState_e State { get; set; }

        /// <summary>
        /// Document of the component
        /// </summary>
        /// <remarks>If component is rapid, view only or suppressed document migth not be loaded into the memory. Use <see cref="IXTransaction.IsCommitted"/> to check the state and call <see cref="IXTransaction.Commit(System.Threading.CancellationToken)"/> to load document if needed</remarks>
        IXDocument3D ReferencedDocument { get; }
        
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

        /// <summary>
        /// Transformation of this component in the assembly relative to the global coordinate system
        /// </summary>
        TransformMatrix Transformation { get; set; }
    }

    /// <summary>
    /// Specific component of the <see cref="IXPart"/>
    /// </summary>
    public interface IXPartComponent : IXComponent
    {
        /// <inheritdoc/>>
        new IXPart ReferencedDocument { get; }

        /// <inheritdoc/>>
        new IXPartConfiguration ReferencedConfiguration { get; }
    }

    /// <summary>
    /// Specific component of the <see cref="IXAssembly"/>
    /// </summary>
    public interface IXAssemblyComponent : IXComponent
    {
        /// <inheritdoc/>>
        new IXAssembly ReferencedDocument { get; }

        /// <inheritdoc/>>
        new IXAssemblyConfiguration ReferencedConfiguration { get; }
    }

    /// <summary>
    /// Additional methods for <see cref="IXComponent"/>
    /// </summary>
    public static class IXComponentExtension 
    {
        /// <summary>
        /// Gets all bodies from the components
        /// </summary>
        /// <param name="comp">Component</param>
        /// <param name="includeHidden">True to include all bodies, false to only include visible</param>
        /// <param name="transform">True to transform body to assembly space</param>
        /// <returns>Bodies</returns>
        public static IEnumerable<IXBody> IterateBodies(this IXComponent comp, bool includeHidden = false, bool transform = false)
            => IterateComponentBodies(new IXComponent[] { comp }, includeHidden, transform);

        /// <inheritdoc cref="IterateBodies(IXComponent, bool, bool)"/>
        public static IEnumerable<IXBody> IterateBodies(this IXComponentRepository comps, bool includeHidden = false, bool transform = false)
            => IterateComponentBodies(comps, includeHidden, transform);

        private static IEnumerable<IXBody> IterateComponentBodies(IEnumerable<IXComponent> comps, bool includeHidden, bool transform)
        {
            IEnumerable<IXComponent> SelectComponents(IXComponent parent)
            {
                var state = parent.State;

                if (!state.HasFlag(ComponentState_e.Suppressed) && !state.HasFlag(ComponentState_e.SuppressedIdMismatch))
                {
                    if (includeHidden || !state.HasFlag(ComponentState_e.Hidden))
                    {
                        yield return parent;

                        if (state.HasFlag(ComponentState_e.Lightweight))
                        {
                            if (parent.ReferencedDocument is IXAssembly)
                            {
                                parent.State = (ComponentState_e)(state - ComponentState_e.Lightweight);
                            }
                        }

                        foreach (var child in parent.Children.SelectMany(c => SelectComponents(c)))
                        {
                            yield return child;
                        }
                    }
                }
            }

            IXBody[] GetComponentBodies(IXComponent srcComp)
                => srcComp.Bodies.Where(b => includeHidden || b.Visible)
                .Select(b => 
                {
                    if (transform)
                    {
                        var copy = b.Copy();
                        copy.Transform(srcComp.Transformation);
                        return copy;
                    }
                    else
                    {
                        return b;
                    }
                }).ToArray();

            foreach (var comp in comps)
            {
                foreach (var body in SelectComponents(comp)
                    .SelectMany(GetComponentBodies))
                {
                    yield return body;
                }
            }
        }
    }
}
