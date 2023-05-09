using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Exceptions;

namespace Xarial.XCad.SolidWorks.Documents.Exceptions
{
    /// <summary>
    /// Inidicatest that flat pattern view does not refer the correct configuration
    /// </summary>
    public class InvalidFlatPatternConfigurationException : Exception, IUserException
    {
        internal InvalidFlatPatternConfigurationException(Exception innerException) 
            : base("The flat pattern drawing view is invalid as it does not contain the flat pattern feature in the flattened state. This is usually caused by an invalid SM-FLAT-PATTERN configuration in the part file. Try removing this configuration", innerException) 
        {
        }
    }
}
