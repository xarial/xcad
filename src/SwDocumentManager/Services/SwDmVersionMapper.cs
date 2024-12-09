using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Toolkit.Services;

namespace Xarial.XCad.SwDocumentManager.Services
{
    /// <summary>
    /// SOLIDWORKS version mapper
    /// </summary>
    public interface ISwVersionMapper : IVersionMapper<SwDmVersion_e>
    {
    }

    /// <summary>
    /// SOLIDWORKS version mapper
    /// </summary>
    public class SwDmVersionMapper : VersionMapper<SwDmVersion_e>, ISwVersionMapper
    {
        protected override int FileRevisionStep => 1000;

        protected override string VersionNameBase => "SOLIDWORKS ";

        protected override SwDmVersion_e CreateUnknownVersion(int appRev) => (SwDmVersion_e)appRev;
    }
}
