using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.SolidWorks.Utils
{
    /// <summary>
    /// This structure defines the owner of the specific object or entity
    /// </summary>
    /// <remarks>This structure is intended to handle the configuration specific entitites
    /// that are otherwise are not natively supported as configuration specific (e.g. feature, dimension)</remarks>
    internal class Context
    {
        /// <summary>
        /// Owner of this object
        /// </summary>
        /// <remarks>This is typically either <see cref="XCad.Documents.IXDocument"/>, <see cref="XCad.Documents.IXConfiguration"/> or <see cref="XCad.Documents.IXComponent"/></remarks>
        internal ISwObject Owner { get; }

        internal Context(ISwObject owner) 
        {
            Owner = owner;
        }
    }
}
