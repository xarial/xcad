using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.XCad.Toolkit.Attributes
{
    /// <summary>
    /// Release year of the application
    /// </summary>
    /// <remarks>Used in <see cref="Services.IVersionMapper{TVersion}"/></remarks>
    [AttributeUsage(AttributeTargets.Field)]
    public class ReleaseYearAttribute : Attribute
    {
        /// <summary>
        /// Year or version number
        /// </summary>
        public int Year { get; }

        /// <summary>
        /// Optional version suffix
        /// </summary>
        public string Suffix { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="year">Year or version number</param>
        /// <param name="suffix">Optional version suffix</param>
        public ReleaseYearAttribute(int year, string suffix = "")
        {
            Year = year;
            Suffix = suffix;
        }
    }
}
