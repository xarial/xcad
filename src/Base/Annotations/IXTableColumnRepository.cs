//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Base;

namespace Xarial.XCad.Annotations
{
    /// <summary>
    /// Represents the collection of columns of the <see cref="IXTable"/>
    /// </summary>
    public interface IXTableColumnRepository : IXRepository<IXTableColumn>
    {
        /// <summary>
        /// Returns the columns by index
        /// </summary>
        /// <param name="index">Index of the column</param>
        /// <returns>Column</returns>
        IXTableColumn this[int index] { get; }
    }

    /// <summary>
    /// Extension methods for <see cref="IXTableColumnRepository"/>
    /// </summary>
    public static class XTableColumnRepositoryExtension
    {
        /// <summary>
        /// Inserts the column to the specified index
        /// </summary>
        /// <param name="tableCols"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static IXTableColumn Insert(this IXTableColumnRepository tableCols, int index)
        {
            var col = tableCols.PreCreate();
            col.Index = index;
            col.Commit();

            return col;
        }
    }
}
