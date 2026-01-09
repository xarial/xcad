//*********************************************************************
//xCAD
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Base;

namespace Xarial.XCad.Documents
{
    /// <summary>
    /// Appearance repository
    /// </summary>
    public interface IXAppearanceRepository : IXRepository<IXAppearance> 
    {
        /// <summary>
        /// Gets appearance for the specified entities in this display state
        /// </summary>
        /// <param name="objs">Objects to get appearance for</param>
        /// <returns>Appearance</returns>
        IXAppearance this[IHasColor[] objs] { get; }
    }
}