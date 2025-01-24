//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Reflection;
using Xarial.XCad.UI.PropertyPage.Base;
using Xarial.XCad.Utils.PageBuilder.Binders;

namespace Xarial.XCad.Utils.PageBuilder.Base
{
    /// <summary>
    /// Represents the attributes set of the property builder
    /// </summary>
    public interface IAttributeSet
    {
        /// <summary>
        /// Id of the control
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Tag attached to a control
        /// </summary>
        object Tag { get; }

        /// <summary>
        /// Name of the control
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Description of the control
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Context of the control
        /// </summary>
        Type ContextType { get; }

        /// <summary>
        /// Control descriptor
        /// </summary>
        IControlDescriptor ControlDescriptor { get; }

        /// <summary>
        /// Checks if custom attribute is assigned
        /// </summary>
        /// <typeparam name="TAtt">Type of the attribute</typeparam>
        /// <returns>True if attribute is set</returns>
        bool Has<TAtt>() where TAtt : IAttribute;

        /// <summary>
        /// Gets the specified attribute
        /// </summary>
        /// <typeparam name="TAtt">Type of the attribute</typeparam>
        /// <returns>Attribute</returns>
        TAtt Get<TAtt>() where TAtt : IAttribute;

        /// <summary>
        /// Gets all custrom attributes
        /// </summary>
        /// <typeparam name="TAtt">Type of attribute</typeparam>
        /// <returns>All attributes</returns>
        IEnumerable<TAtt> GetAll<TAtt>() where TAtt : IAttribute;

        /// <summary>
        /// Adds the attribute to the set
        /// </summary>
        /// <typeparam name="TAtt">Type of the attribute</typeparam>
        /// <param name="att">Attribute to add</param>
        void Add<TAtt>(TAtt att) where TAtt : IAttribute;
    }
}