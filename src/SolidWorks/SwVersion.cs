//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Xarial.XCad.SolidWorks.Enums;

namespace Xarial.XCad.SolidWorks
{
    /// <summary>
    /// SOLIDWORKS specific version
    /// </summary>
    public interface ISwVersion : IXVersion
    {
        /// <summary>
        /// Major version
        /// </summary>
        SwVersion_e Major { get; }

        /// <summary>
        /// Service pack
        /// </summary>
        int ServicePack { get; }

        /// <summary>
        /// Revision of service pack
        /// </summary>
        int ServicePackRevision { get; }
    }

    internal class SwVersion : ISwVersion
    {
        public SwVersion_e Major { get; }

        public string DisplayName { get; }

        public Version Version { get; }

        public int ServicePack { get; }
        public int ServicePackRevision { get; }

        internal SwVersion(Version version, SwVersion_e major, int sp, int spRev, string dispName)
        {
            Version = version;
            Major = major;
            DisplayName = dispName;

            ServicePack = sp;
            ServicePackRevision = spRev;
        }

        public int CompareTo(IXVersion other)
        {
            const int EQUAL = 0;

            if (other is ISwVersion)
            {
                //NOTE: cannot compare Version as for the pre-release SP and SP Rev can be negative which is not supported for the Version

                var res = Major.CompareTo(((ISwVersion)other).Major);

                if (res == EQUAL)
                {
                    res = ServicePack.CompareTo(((ISwVersion)other).ServicePack);

                    if (res == EQUAL)
                    {
                        res = ServicePack.CompareTo(((ISwVersion)other).ServicePackRevision);
                    }
                }

                return res;
            }
            else 
            {
                throw new InvalidCastException("Can only compare SOLIDWORKS versions");
            }
        }

        public override int GetHashCode() => (int)Major;

        public override bool Equals(object obj)
        {
            if (!(obj is ISwVersion))
            {
                return false;
            }

            return IsSame((ISwVersion)obj);
        }

        private bool IsSame(ISwVersion other) => Major == other.Major;

        public bool Equals(IXVersion other) => Equals((object)other);

        public static bool operator ==(SwVersion version1, SwVersion version2)
            => version1.IsSame(version2);

        public static bool operator !=(SwVersion version1, SwVersion version2)
            => !version1.IsSame(version2);

        public override string ToString() => DisplayName;
    }
}
