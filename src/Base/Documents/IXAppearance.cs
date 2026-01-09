//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Drawing;

namespace Xarial.XCad.Documents
{
    /// <summary>
    /// Represents appearance information
    /// </summary>
    public interface IXAppearance : IXObject
    {
        /// <summary>
        /// Objects of this appearance
        /// </summary>
        IHasColor[] Objects { get; set; }

        /// <summary>
        /// Color
        /// </summary>
        Color Color { get; set; }

        /// <summary>
        /// Diffuse amount (0 to 1)
        /// </summary>
        double Diffuse { get; set; }

        /// <summary>
        /// Specular amount (0 to 1)
        /// </summary>
        double Specular { get; set; }

        /// <summary>
        /// Specular color
        /// </summary>
        Color SpecularColor { get; set; }

        /// <summary>
        /// Specular spread/blurriness
        /// </summary>
        double Blurriness { get; set; }

        /// <summary>
        /// Reflection amount
        /// </summary>
        double Reflection { get; set; }

        /// <summary>
        /// Luminous intensity
        /// </summary>
        double Emission { get; set; }
    }
}
