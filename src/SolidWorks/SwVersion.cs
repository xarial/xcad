//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
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

        internal SwVersion(SwVersion_e major) 
        {
            Major = major;
        }

        public int CompareTo(IXVersion other)
        {
            if (other is ISwVersion)
            {
                return ((int)Major).CompareTo((int)((ISwVersion)other).Major);
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
