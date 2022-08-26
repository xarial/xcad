//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Enums;

namespace Xarial.XCad.Toolkit
{
    public class Font : IFont
    {
        public string Name { get; }
        public double? Size { get; }
        public double? SizeInPoints { get; }
        public FontStyle_e Style { get; }

        public Font(string name, double? size, double? sizeInPoints, FontStyle_e style)
        {
            Name = name;
            Size = size;
            SizeInPoints = sizeInPoints;
            Style = style;
        }
    }
}
