//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Base;

namespace Xarial.XCad.Annotations
{
    /// <summary>
    /// Represents the collection of rows in the <see cref="IXTable"/>
    /// </summary>
    public interface IXTableRowRepository : IXRepository<IXTableRow>
    {
        /// <summary>
        /// Returns the row by index
        /// </summary>
        /// <param name="index">Index of the row</param>
        /// <returns>Table row</returns>
        IXTableRow this[int index] { get; }
    }

    /// <summary>
    /// Represents rows of <see cref="IXBomTable"/>
    /// </summary>
    public interface IXBomTableRowRepository : IXTableRowRepository, IXRepository<IXBomTableRow>
    {
        /// <summary>
        /// Returns the BOM row by index
        /// </summary>
        /// <param name="index">Index of the row</param>
        /// <returns>Table row</returns>
        new IXBomTableRow this[int index] { get; }
    }

    /// <summary>
    /// Additional methods for the <see cref="IXTableRowRepository"/>
    /// </summary>
    public static class XTableRowRepositoryExtension
    {
        /// <summary>
        /// Inserts row into the specified location
        /// </summary>
        /// <param name="tableRows">Table rows</param>
        /// <param name="index">Index to insert row to</param>
        /// <returns>New row</returns>
        public static IXTableRow Insert(this IXTableRowRepository tableRows, int index)
        {
            var row = tableRows.PreCreate();
            row.Index = index;
            row.Commit();

            return row;
        }
    }
}
