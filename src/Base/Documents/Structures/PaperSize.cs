using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Documents.Enums;

namespace Xarial.XCad.Documents.Structures
{
    /// <summary>
    /// Defines the size of the drawing sheet paper
    /// </summary>
    public class PaperSize
    {
        /// <summary>
        /// Standard paper size or null if custom
        /// </summary>
        public StandardPaperSize_e? StandardPaperSize { get; }
        
        /// <summary>
        /// Custom width if <see cref="StandardPaperSize"/> is null
        /// </summary>
        public double? Width { get; }

        /// <summary>
        /// Custom height if <see cref="StandardPaperSize"/> is null
        /// </summary>
        public double? Height { get; }

        /// <summary>
        /// Standard paper size constructor
        /// </summary>
        /// <param name="standardPaperSize">Standard paper size</param>
        public PaperSize(StandardPaperSize_e standardPaperSize) 
        {
            StandardPaperSize = standardPaperSize;
            Width = null;
            Height = null;
        }

        /// <summary>
        /// Custom paper size constructor
        /// </summary>
        /// <param name="width">Custom width</param>
        /// <param name="height">Custom height</param>
        public PaperSize(double width, double height)
        {
            StandardPaperSize = null;
            Width = width;
            Height = height;
        }
    }
}
