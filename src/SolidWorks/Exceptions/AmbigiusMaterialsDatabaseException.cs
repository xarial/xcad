//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using static Xarial.XCad.SolidWorks.SwMaterialsDatabaseRepository;

namespace Xarial.XCad.SolidWorks.Exceptions
{
    /// <summary>
    /// Thrown when property of <see cref="ISwAmbigiusMaterialsDatabase"/> is accessed without index set
    /// </summary>
    public class AmbigiusMaterialsDatabaseException : Exception
    {
        /// <summary>
        /// Constructor
        /// </summary>
        internal AmbigiusMaterialsDatabaseException() : base($"Material database is ambigius. Use {nameof(ISwAmbigiusMaterialsDatabase.MaterialDatabaseIndex)} to set database index to use")
        {
        }
    }
}
