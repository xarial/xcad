using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.SolidWorks.Enums;
using Xarial.XCad.Toolkit.Services;

namespace Xarial.XCad.SolidWorks.Services
{
    /// <summary>
    /// SOLIDWORKS version mapper
    /// </summary>
    public interface ISwVersionMapper : IVersionMapper<SwVersion_e>
    {
    }

    /// <summary>
    /// SOLIDWORKS version mapper
    /// </summary>
    public class SwVersionMapper : VersionMapper<SwVersion_e>, ISwVersionMapper
    {
        protected override int FileRevisionStep => 1000;

        protected override string VersionNameBase => "SOLIDWORKS ";

        protected override SwVersion_e CreateUnknownVersion(int appRev) => (SwVersion_e)appRev;
    }
}
