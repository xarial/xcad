//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Toolkit.Attributes;

namespace Xarial.XCad.Inventor.Enums
{
    /// <summary>
    /// Autodesk Inventor Version
    /// </summary>
    public enum AiVersion_e
    {
        [ReleaseYear(1)]
        Inventor1 = 1,

        [ReleaseYear(2)]
        Inventor2 = 2,

        [ReleaseYear(3)]
        Inventor3 = 3,

        [ReleaseYear(4)]
        Inventor4 = 4,

        [ReleaseYear(5)]
        Inventor5 = 5,

        [ReleaseYear(5, ".3")]
        Inventor5dot3 = -1,

        [ReleaseYear(6)]
        Inventor6 = 6,

        [ReleaseYear(7)]
        Inventor7 = 7,

        [ReleaseYear(8)]
        Inventor8 = 8,

        [ReleaseYear(9)]
        Inventor9 = 9,

        [ReleaseYear(10)]
        Inventor10 = 10,

        [ReleaseYear(11)]
        Inventor11 = 11,

        [ReleaseYear(2008)]
        Inventor2008 = 12,

        [ReleaseYear(2009)]
        Inventor2009 = 13,

        [ReleaseYear(2010)]
        Inventor2010 = 14,

        [ReleaseYear(2011)]
        Inventor2011 = 15,

        [ReleaseYear(2012)]
        Inventor2012 = 16,

        [ReleaseYear(2013)]
        Inventor2013 = 17,

        [ReleaseYear(2014)]
        Inventor2014 = 18,

        [ReleaseYear(2015)]
        Inventor2015 = 19,

        [ReleaseYear(2016)]
        Inventor2016 = 20,

        [ReleaseYear(2017)]
        Inventor2017 = 21,

        [ReleaseYear(2018)]
        Inventor2018 = 22,

        [ReleaseYear(2019)]
        Inventor2019 = 23,

        [ReleaseYear(2020)]
        Inventor2020 = 24,

        [ReleaseYear(2021)]
        Inventor2021 = 25,

        [ReleaseYear(2022)]
        Inventor2022 = 26,

        [ReleaseYear(2023)]
        Inventor2023 = 27,

        [ReleaseYear(2024)]
        Inventor2024 = 28,

        [ReleaseYear(2025)]
        Inventor2025 = 29
    }
}
