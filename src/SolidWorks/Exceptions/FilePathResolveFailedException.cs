using System;
using System.Collections.Generic;
using System.Text;

namespace Xarial.XCad.SolidWorks.Exceptions
{
    public class FilePathResolveFailedException : Exception
    {
        internal FilePathResolveFailedException(string inputPath) : base($"Failed to resolve file path for {inputPath}")
        {
        }
    }
}
