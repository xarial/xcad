//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
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
        /// <returns>Bodies</returns>
        public static IEnumerable<IXBody> IterateBodies(this IXComponent comp, bool includeHidden = false)
        {
            IEnumerable<IXComponent> SelectComponents(IXComponent parent)
            {
                var state = parent.State;

                if (!state.HasFlag(ComponentState_e.Suppressed))
                {
                    if (includeHidden || !state.HasFlag(ComponentState_e.Hidden))
                    {
                        yield return parent;

                        if (state.HasFlag(ComponentState_e.Lightweight))
                        {
                            if (parent.Document is IXAssembly)
                            {
                                comp.State = (ComponentState_e)(state - ComponentState_e.Lightweight);
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
                => srcComp.Bodies.Where(b => includeHidden || b.Visible).ToArray();

            if (comp.Document is IXPart)
            {
                foreach (var body in GetComponentBodies(comp))
                {
                    yield return body;
                }
            }
            else if (comp.Document is IXAssembly)
            {
                foreach (var body in SelectComponents(comp)
                    .SelectMany(GetComponentBodies))
                {
                    yield return body;
                }
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }
}
