//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using Xarial.XCad.Toolkit.Attributes;
using Xarial.XCad.Utils.Reflection;

namespace Xarial.XCad.SolidWorks.Enums
{
    /// <summary>
    /// SOLIDWORKS version
    /// </summary>
    public enum SwVersion_e
    {
        [FileRevision(44)]
        [ReleaseYear(1995)]
        Sw95,

        [FileRevision(243)]
        [ReleaseYear(1996)]
        Sw96,

        [FileRevision(483)]
        [ReleaseYear(1997)]
        Sw97,

        [FileRevision(629)]
        [ReleaseYear(1997, "+")]
        Sw97Plus,

        [FileRevision(822)]
        [ReleaseYear(1998)]
        Sw98,

        [FileRevision(1008)]
        [ReleaseYear(1998, "+")]
        Sw98Plus,

        [FileRevision(1137)]
        [ReleaseYear(1999)]
        Sw99,

        [FileRevision(1500)]
        [ReleaseYear(2000)]
        Sw2000 = 8,

        [FileRevision(1750)]
        [ReleaseYear(2001)]
        Sw2001 = 9,

        [FileRevision(1950)]
        [ReleaseYear(2001, "+")]
        Sw2001Plus = 10,

        [FileRevision(2200)]
        [ReleaseYear(2003)]
        Sw2003 = 11,

        [FileRevision(2500)]
        [ReleaseYear(2004)]
        Sw2004 = 12,

        [FileRevision(2800)]
        [ReleaseYear(2005)]
        Sw2005 = 13,

        [FileRevision(3100)]
        [ReleaseYear(2006)]
        Sw2006 = 14,

        [FileRevision(3400)]
        [ReleaseYear(2007)]
        Sw2007 = 15,

        [FileRevision(3800)]
        [ReleaseYear(2008)]
        Sw2008 = 16,

        [FileRevision(4100)]
        [ReleaseYear(2009)]
        Sw2009 = 17,

        [FileRevision(4400)]
        [ReleaseYear(2010)]
        Sw2010 = 18,

        [FileRevision(4700)]
        [ReleaseYear(2011)]
        Sw2011 = 19,

        [FileRevision(5000)]
        [ReleaseYear(2012)]
        Sw2012 = 20,

        [FileRevision(6000)]
        [ReleaseYear(2013)]
        Sw2013 = 21,

        [FileRevision(7000)]
        [ReleaseYear(2014)]
        Sw2014 = 22,

        [FileRevision(8000)]
        [ReleaseYear(2015)]
        Sw2015 = 23,

        [FileRevision(9000)]
        [ReleaseYear(2016)]
        Sw2016 = 24,

        [FileRevision(10000)]
        [ReleaseYear(2017)]
        Sw2017 = 25,

        [FileRevision(11000)]
        [ReleaseYear(2018)]
        Sw2018 = 26,

        [FileRevision(12000)]
        [ReleaseYear(2019)]
        Sw2019 = 27,

        [FileRevision(13000)]
        [ReleaseYear(2020)]
        Sw2020 = 28,

        [FileRevision(14000)]
        [ReleaseYear(2021)]
        Sw2021 = 29,

        [FileRevision(15000)]
        [ReleaseYear(2022)]
        Sw2022 = 30,

        [FileRevision(16000)]
        [ReleaseYear(2023)]
        Sw2023 = 31,

        [FileRevision(17000)]
        [ReleaseYear(2024)]
        Sw2024 = 32,

        [FileRevision(18000)]
        [ReleaseYear(2025)]
        Sw2025 = 33
    }
}