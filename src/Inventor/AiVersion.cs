//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Inventor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Inventor.Enums;

namespace Xarial.XCad.Inventor
{
    /// <summary>
    /// Autodesk Inventor Version
    /// </summary>
    public interface IAiVersion : IXVersion
    {
        /// <summary>
        /// Major version identifier
        /// </summary>
        AiVersion_e Major { get; }
    }

    internal class AiVersion : IAiVersion
    {
        public AiVersion_e Major { get; }

        public string DisplayName { get; }

        public Version Version { get; }

        internal AiVersion(SoftwareVersion softwareVersion, AiVersion_e major)
        {
            Version = new Version(softwareVersion.Major, softwareVersion.Minor, softwareVersion.ServicePack);
            DisplayName = softwareVersion.ProductName + " " + softwareVersion.DisplayVersion;

            Major = major;
        }

        internal AiVersion(Version version, AiVersion_e major, string dispName)
        {
            Version = version;
            Major = major;
            DisplayName = dispName;
        }

        public int CompareTo(IXVersion other)
        {
            if (other is IAiVersion)
            {
                return Version.CompareTo(other.Version);
            }
            else
            {
                throw new InvalidCastException("Can only compare Inventor versions");
            }
        }

        public override int GetHashCode()
        {
            return (int)Major;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is IAiVersion))
            {
                return false;
            }

            return IsSame((IAiVersion)obj);
        }

        private bool IsSame(IAiVersion other) => Major == other.Major;

        public bool Equals(IXVersion other) => Equals((object)other);

        public static bool operator ==(AiVersion version1, AiVersion version2)
            => version1.IsSame(version2);

        public static bool operator !=(AiVersion version1, AiVersion version2)
            => !version1.IsSame(version2);

        public override string ToString() => DisplayName;
    }
}
