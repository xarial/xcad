﻿//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Xarial.XCad.SwDocumentManager
{
    public enum SwDmVersion_e
    {
        Sw2000 = 1500,
        Sw2001 = 1750,
        Sw2001Plus = 1950,
        Sw2003 = 2200,
        Sw2004 = 2500,
        Sw2005 = 2800,
        Sw2006 = 3100,
        Sw2007 = 3400,
        Sw2008 = 3800,
        Sw2009 = 4100,
        Sw2010 = 4400,
        Sw2011 = 4700,
        Sw2012 = 5000,
        Sw2013 = 6000,
        Sw2014 = 7000,
        Sw2015 = 8000,
        Sw2016 = 9000,
        Sw2017 = 10000,
        Sw2018 = 11000,
        Sw2019 = 12000,
        Sw2020 = 13000,
        Sw2021 = 14000,
        Sw2022 = 15000,
        Sw2023 = 16000,
        Sw2024 = 17000
    }

    public interface ISwDmVersion : IXVersion
    {
        SwDmVersion_e Major { get; }
    }

    internal class SwDmVersion : ISwDmVersion
    {
        public SwDmVersion_e Major { get; }

        public string DisplayName
            => $"SOLIDWORKS {Major.ToString().Substring("Sw".Length)}";

        public Version Version { get; }

        internal SwDmVersion(Version version)
        {
            Version = version;
            Major = (SwDmVersion_e)version.Major;
        }

        public int CompareTo(IXVersion other)
        {
            if (other is ISwDmVersion)
            {
                return Version.CompareTo(other.Version);
            }
            else
            {
                throw new InvalidCastException("Can only compare SOLIDWORKS versions");
            }
        }

        public override int GetHashCode() => (int)Major;

        public override bool Equals(object obj)
        {
            if (!(obj is ISwDmVersion))
            {
                return false;
            }

            return IsSame((ISwDmVersion)obj);
        }

        private bool IsSame(ISwDmVersion other) => Major == other.Major;

        public bool Equals(IXVersion other) => Equals((object)other);

        public static bool operator ==(SwDmVersion version1, SwDmVersion version2)
            => version1.IsSame(version2);

        public static bool operator !=(SwDmVersion version1, SwDmVersion version2)
            => !version1.IsSame(version2);

        public override string ToString() => DisplayName;
    }

    public static class SwDmVersionExtension 
    {
        public static bool IsVersionNewerOrEqual(this ISwDmVersion vers, SwDmVersion_e version)
            => vers.Major >= version;
    }
}
