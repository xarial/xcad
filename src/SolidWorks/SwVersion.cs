//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
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
    public interface ISwVersion : IXVersion
    {
        SwVersion_e Major { get; }
    }

    internal class SwVersion : ISwVersion
    {
        public SwVersion_e Major { get; }

        public string DisplayName
            => $"SOLIDWORKS {Major.ToString().Substring("Sw".Length)}";

        public Version Version { get; }

        internal SwVersion(Version version) 
        {
            Version = version;
            Major = (SwVersion_e)version.Major;
        }

        public int CompareTo(IXVersion other)
        {
            if (other is ISwVersion)
            {
                return this.Version.CompareTo(other.Version);
            }
            else 
            {
                throw new InvalidCastException("Can only compare SOLIDWORKS versions");
            }
        }

        public override int GetHashCode()
        {
            return (int)Major;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ISwVersion))
                return false;

            return Equals((SwVersion)obj);
        }

        public bool Equals(SwVersion other)
            => Major == other.Major;

        public bool Equals(IXVersion other) => Equals((object)other);

        public static bool operator ==(SwVersion version1, SwVersion version2)
            => version1.Equals(version2);

        public static bool operator !=(SwVersion version1, SwVersion version2)
            => !version1.Equals(version2);

        public override string ToString() => DisplayName;
    }
}
