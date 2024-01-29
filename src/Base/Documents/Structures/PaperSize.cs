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
using Xarial.XCad.Documents.Enums;
using System.Diagnostics;

namespace Xarial.XCad.Documents.Structures
{
    /// <summary>
    /// Defines the size of the drawing sheet paper
    /// </summary>
    [DebuggerDisplay("{" + nameof(StandardPaperSize) + "}" + " ({" + nameof(Width) + "} x {" + nameof(Height) + "}")]
    public class PaperSize
    {
        /// <summary>
        /// Standard paper size or null if custom
        /// </summary>
        public StandardPaperSize_e? StandardPaperSize { get; }
        
        /// <summary>
        /// Width of the paper
        /// </summary>
        public double Width { get; }

        /// <summary>
        /// Height of the paper
        /// </summary>
        public double Height { get; }

        /// <summary>
        /// Standard paper size constructor
        /// </summary>
        public PaperSize(StandardPaperSize_e standardPaperSize)
        {
            StandardPaperSize = standardPaperSize;

            switch (standardPaperSize)
            {
                case StandardPaperSize_e.ALandscape:
                    Width = 0.279;
                    Height = 0.2159;
                    break;
                case StandardPaperSize_e.APortrait:
                    Width = 0.2159;
                    Height = 0.279;
                    break;
                case StandardPaperSize_e.BLandscape:
                    Width = 0.4318;
                    Height = 0.2794;
                    break;
                case StandardPaperSize_e.CLandscape:
                    Width = 0.5588;
                    Height = 0.4318;
                    break;
                case StandardPaperSize_e.DLandscape:
                    Width = 0.8636;
                    Height = 0.5588;
                    break;
                case StandardPaperSize_e.ELandscape:
                    Width = 1.1176;
                    Height = 0.8636;
                    break;
                case StandardPaperSize_e.A4Landscape:
                    Width = 0.297;
                    Height = 0.21;
                    break;
                case StandardPaperSize_e.A4Portrait:
                    Width = 0.21;
                    Height = 0.297;
                    break;
                case StandardPaperSize_e.A3Landscape:
                    Width = 0.42;
                    Height = 0.297;
                    break;
                case StandardPaperSize_e.A2Landscape:
                    Width = 0.594;
                    Height = 0.42;
                    break;
                case StandardPaperSize_e.A1Landscape:
                    Width = 0.841;
                    Height = 0.594;
                    break;
                case StandardPaperSize_e.A0Landscape:
                    Width = 1.189;
                    Height = 0.841;
                    break;
            }
        }

        /// <summary>
        /// Custom paper size constructor
        /// </summary>
        ///<inheritdoc/>
        public PaperSize(double width, double height) : this(null, width, height)
        {
        }

        /// <summary>
        /// Constructor with all parameters
        /// </summary>
        /// <param name="standardPaperSize">Standard paper size</param>
        /// <param name="width">Custom width</param>
        /// <param name="height">Custom height</param>
        public PaperSize(StandardPaperSize_e? standardPaperSize, double width, double height)
        {
            StandardPaperSize = standardPaperSize;
            Width = width;
            Height = height;
        }
    }
}
