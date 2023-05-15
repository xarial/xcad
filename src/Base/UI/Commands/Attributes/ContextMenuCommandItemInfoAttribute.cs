using System;

namespace Xarial.XCad.UI.Commands.Attributes
{
    /// <summary>
    /// Allwos to customize the context menu command created with <see cref="IXCommandManager.AddContextMenu(Structures.ContextMenuCommandGroupSpec)"/>
    /// </summary>
    public class ContextMenuCommandItemInfoAttribute : Attribute
    {
        /// <summary>
        /// Type where context menu is attached to
        /// </summary>
        public Type Owner { get; }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="owner">Type to wheer attach the context menu</param>
        public ContextMenuCommandItemInfoAttribute(Type owner)
        {
            Owner = owner;
        }
    }
}
