using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Utils.Reflection;

namespace Xarial.XCad.SolidWorks.Services
{
    /// <summary>
    /// Service to provide specific type of macro feature
    /// </summary>
    /// <remarks>This service can be useful if COM type cannot be automatically matched to .NET type (this behaviro is observed in .NET6+)</remarks>
    public interface IMacroFeatureTypeProvider
    {
        /// <summary>
        /// Provide the type of this macro feature instance
        /// </summary>
        /// <param name="featData">Maco feature to get type for</param>
        /// <returns>Macro feature type</returns>
        Type ProvideType(IMacroFeatureData featData);
    }

    internal class ComMacroFeatureTypeProvider : IMacroFeatureTypeProvider
    {
        public Type ProvideType(IMacroFeatureData featData)
        {
            //NOTE: definition of rolled back macro feature is null
            var progId = featData?.GetProgId();

            if (!string.IsNullOrEmpty(progId))
            {
                return Type.GetTypeFromProgID(progId);
            }
            else 
            {
                return null;
            }
        }
    }
}
