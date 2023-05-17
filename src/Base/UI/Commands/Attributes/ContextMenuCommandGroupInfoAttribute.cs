using System;

namespace Xarial.XCad.UI.Commands.Attributes
{
    /// <summary>
    /// Allwos to customize the context menu command created with <see cref="IXCommandManager.AddContextMenu(Structures.ContextMenuCommandGroupSpec)"/>
    /// </summary>
    public class ContextMenuCommandGroupInfoAttribute : CommandGroupInfoAttribute
    {
        /// <summary>
        /// Type where context menu is attached to
        /// </summary>
        public Type Owner { get; }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="owner">Type to wheer attach the context menu</param>
        /// <inheritdoc/>
        public ContextMenuCommandGroupInfoAttribute(int userId, Type owner) : base(userId)
        {
            Owner = owner;
        }
    }
}
