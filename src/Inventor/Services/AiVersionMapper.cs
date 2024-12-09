using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Inventor.Enums;
using Xarial.XCad.Toolkit.Services;

namespace Xarial.XCad.Inventor.Services
{
    /// <summary>
    /// SOLIDWORKS version mapper
    /// </summary>
    public interface IAiVersionMapper : IVersionMapper<AiVersion_e>
    {
    }

    /// <summary>
    /// SOLIDWORKS version mapper
    /// </summary>
    public class AiVersionMapper : VersionMapper<AiVersion_e>, IAiVersionMapper
    {
        protected override string VersionNameBase => "Inventor ";

        protected override AiVersion_e CreateUnknownVersion(int appRev) => (AiVersion_e)appRev;
    }
}
