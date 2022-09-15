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
    public interface IXComponent : IXSelObject, IXObjectContainer, IXTransaction, IXColorizable, IDimensionable, INameable
    {
        /// <summary>
        /// Full name of the component including the hierarchical path
        /// </summary>
        string FullName { get; }

        /// <summary>
        /// Reference label of the component
        /// </summary>
        string Reference { get; set; }

        /// <summary>
        /// Returns the referenced configuration of this component
        /// </summary>
        /// <remarks>For unloaded or rapid components this configuration may be uncommitted</remarks>
        IXConfiguration ReferencedConfiguration { get; set; }

        /// <summary>
        /// State of this component
        /// </summary>
        ComponentState_e State { get; set; }

        /// <summary>
        /// Document of the component
        /// </summary>
        /// <remarks>If component is rapid, view only or suppressed document migth not be loaded into the memory. Use <see cref="IXTransaction.IsCommitted"/> to check the state and call <see cref="IXTransaction.Commit(System.Threading.CancellationToken)"/> to load document if needed</remarks>
        IXDocument3D ReferencedDocument { get; set; }
        
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

        /// <summary>
        /// Enables an editing mode for the component
        /// </summary>
        /// <returns>Component editor</returns>
        IEditor<IXComponent> Edit();
    }

    /// <summary>
    /// Specific component of the <see cref="IXPart"/>
    /// </summary>
    public interface IXPartComponent : IXComponent
    {
        /// <inheritdoc/>>
        new IXPart ReferencedDocument { get; set; }

        /// <inheritdoc/>>
        new IXPartConfiguration ReferencedConfiguration { get; set; }
    }

    /// <summary>
    /// Specific component of the <see cref="IXAssembly"/>
    /// </summary>
    public interface IXAssemblyComponent : IXComponent
    {
        /// <inheritdoc/>>
        new IXAssembly ReferencedDocument { get; set; }

        /// <inheritdoc/>>
        new IXAssemblyConfiguration ReferencedConfiguration { get; set; }
    }

    /// <summary>
    /// Additional methods for <see cref="IXComponent"/>
    /// </summary>
    public static class XComponentExtension 
    {
        /// <summary>
        /// Iterates all bodies from the components
        /// </summary>
        /// <param name="comp">Component</param>
        /// <param name="includeHidden">True to include all bodies, false to only include visible</param>
        /// <returns>Bodies</returns>
        public static IEnumerable<IXBody> IterateBodies(this IXComponent comp, bool includeHidden = false)
            => IterateBodies(comp,
                c => includeHidden || !c.State.HasFlag(ComponentState_e.Hidden),
                b => includeHidden || b.Visible);
        
        /// <summary>
        /// Iterates all bodies from the component with the specified filter
        /// </summary>
        /// <param name="comp">Component to get bodies from</param>
        /// <param name="compFilter">Filter for components</param>
        /// <param name="bodyFilter">Filter for bodies</param>
        /// <returns>Bodies enumeration</returns>
        public static IEnumerable<IXBody> IterateBodies(this IXComponent comp, Predicate<IXComponent> compFilter, Predicate<IXBody> bodyFilter)
        {
            IEnumerable<IXComponent> SelectComponents(IXComponent parent)
            {
                var state = parent.State;

                if (!state.HasFlag(ComponentState_e.Suppressed) && !state.HasFlag(ComponentState_e.SuppressedIdMismatch))
                {
                    if (compFilter.Invoke(parent))
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
                => srcComp.Bodies.Where(b => bodyFilter.Invoke(b)).ToArray();

            foreach (var body in SelectComponents(comp)
                    .SelectMany(GetComponentBodies))
            {
                yield return body;
            }
        }
    }
}
