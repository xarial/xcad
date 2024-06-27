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
using Xarial.XCad.Enums;

namespace Xarial.XCad.Toolkit
{
    /// <summary>
    /// Represents instance of the font
    /// </summary>
    public class Font : IFont
    {
        /// <summary>
        /// Font name
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Font size
        /// </summary>
        /// <remarks>Null if size is in points</remarks>
        public double? Size { get; set; }

        /// <summary>
        /// Size in points
        /// </summary>
        /// <remarks>Null if size is not in points</remarks>
        public int? SizeInPoints { get; set; }
        
        /// <summary>
        /// Style of the font
        /// </summary>
        public FontStyle_e Style { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public Font() 
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="size">Size</param>
        /// <param name="sizeInPoints">Size in points</param>
        /// <param name="style">Style</param>
        public Font(string name, double? size, int? sizeInPoints, FontStyle_e style)
        {
            Name = name;
            Size = size;
            SizeInPoints = sizeInPoints;
            Style = style;
        }
    }
}
