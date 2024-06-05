//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Data;

namespace Xarial.XCad.Data
{
    /// <summary>
    /// Indicates that the data can be read via <see cref="IDataReader"/>
    /// </summary>
    public interface IReadable 
    {
        /// <summary>
        /// Provides a data reader
        /// </summary>
        /// <param name="visibleOnly">Only read visible data</param>
        /// <returns>Daat reader</returns>
        IDataReader ExecuteReader(bool visibleOnly);
    }
}
