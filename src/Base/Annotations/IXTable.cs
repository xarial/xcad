//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.XCad.Annotations
{
    /// <summary>
    /// Represents the table annotation
    /// </summary>
    public interface IXTable : IXAnnotation
    {
        /// <summary>
        /// Returns the table data reader
        /// </summary>
        IDataReader CreateReader();
    }

    /// <summary>
    /// Adds additional methods for the table
    /// </summary>
    public static class TableExtension
    {
        /// <summary>
        /// Reads the content of the table
        /// </summary>
        /// <param name="table">Table to read from</param>
        /// <returns>Data table</returns>
        public static DataTable Read(this IXTable table) 
        {
            var dataTable = new DataTable();
            dataTable.Load(table.CreateReader());
            return dataTable;
        }
    }
}
