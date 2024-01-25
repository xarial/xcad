//*********************************************************************
//xCAD
//Copyright(C) 2023 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Inventor.Enums;

namespace Xarial.XCad.Inventor
{
    public interface IAiVersion : IXVersion
    {
        AiVersion_e Major { get; }
    }

    internal class AiVersion : IAiVersion
    {
        public AiVersion_e Major { get; }

        public string DisplayName
        {
            get 
            {
                string vers;

                if (Major == AiVersion_e.Inventor5dot3)
                {
                    vers = "5.3";
                }
                else 
                {
                    vers = Major.ToString().Substring("Inventor".Length);
                }

                return $"Inventor {vers}";
            }
        }

        public Version Version { get; }

        internal AiVersion(Version version)
        {
            Version = version;
            Major = (AiVersion_e)version.Major;
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
