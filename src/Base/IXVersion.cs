//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad
{
    /// <summary>
    /// Represents the version of the application or file
    /// </summary>
    public interface IXVersion : IComparable<IXVersion>, IEquatable<IXVersion>
    {
        /// <summary>
        /// Display name of this version
        /// </summary>
        string DisplayName { get; }
    }

    /// <summary>
    /// Versions equality result
    /// </summary>
    public enum VersionEquality_e 
    {
        /// <summary>
        /// Versions are the same
        /// </summary>
        Same = 0,

        /// <summary>
        /// This version is older to the version it is compared to
        /// </summary>
        Older = -1,

        /// <summary>
        /// This version is newer to the version it is compared to
        /// </summary>
        Newer = 1
    }

    /// <summary>
    /// Additional methods for version
    /// </summary>
    public static class IXVersionExtension 
    {
        /// <summary>
        /// Compares two versions
        /// </summary>
        /// <typeparam name="TVersion">Type of the version to compare</typeparam>
        /// <param name="vers">This version</param>
        /// <param name="other">Version to compare to</param>
        /// <returns>Result of comparison</returns>
        public static VersionEquality_e Compare<TVersion>(this TVersion vers, TVersion other)
            where TVersion : IXVersion
            => (VersionEquality_e)vers.CompareTo(other);
    }
}
