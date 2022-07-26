using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Enums;

namespace Xarial.XCad
{
    /// <summary>
    /// Font
    /// </summary>
    public interface IFont
    {
        /// <summary>
        /// Face name of the font
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Size of the font in meters if <see cref="SizeInPoints"/> is null
        /// </summary>
        double? Size { get; }

        /// <summary>
        /// Size of the font in points if <see cref="Size"/> is null
        /// </summary>
        double? SizeInPoints { get; }

        /// <summary>
        /// Font style
        /// </summary>
        FontStyle_e Style { get; }
    }
}
