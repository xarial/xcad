using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.XCad
{
    /// <summary>
    /// Reresents the unique ID of the various <see cref="IXObject"/>
    /// </summary>
    public interface IXIdentifier : IEquatable<IXIdentifier>
    {
        /// <summary>
        /// Provides the thumbprint of the ID for comarison purposes
        /// </summary>
        byte[] Thumbprint { get; }
    }

    /// <summary>
    /// Extension of <see cref="IXIdentifier"/>
    /// </summary>
    public static class XIdentifierExtension 
    {
        /// <summary>
        /// Comparase thumbprints of two identifiers
        /// </summary>
        /// <param name="id">This identifiers</param>
        /// <param name="otherThumbprint">Other's identifier thumbprint</param>
        /// <returns>True if thumbprints are identical</returns>
        public static bool CompareThumbprints(this IXIdentifier id, byte[] otherThumbprint)
            => id.Thumbprint.SequenceEqual(otherThumbprint);
    }
}
