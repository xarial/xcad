//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Xarial.XCad.Base;
using Xarial.XCad.Documents.Delegates;
using Xarial.XCad.Graphics;
using Xarial.XCad.UI;
using static System.Net.Mime.MediaTypeNames;

namespace Xarial.XCad.Documents
{
    /// <summary>
    /// Handles the selection objects
    /// </summary>
    public interface IXSelectionRepository : IXRepository<IXSelObject>
    {
        /// <summary>
        /// Raised when new object is selected
        /// </summary>
        event NewSelectionDelegate NewSelection;

        /// <summary>
        /// Raised when the selection is cleared
        /// </summary>
        event ClearSelectionDelegate ClearSelection;

        /// <summary>
        /// Clears all current selections
        /// </summary>
        void Clear();

        /// <summary>
        /// Replaces the selection (clears previous selection)
        /// </summary>
        /// <param name="ents">Entities to select</param>
        /// <param name="cancellationToken">Cancellation token</param>
        void ReplaceRange(IEnumerable<IXSelObject> ents, CancellationToken cancellationToken);

        /// <summary>
        /// Pre-creates selection callout instance
        /// </summary>
        /// <returns>Instance of the selection callout</returns>
        IXSelCallout PreCreateCallout();
    }

    /// <summary>
    /// Additional methods for <see cref="IXSelectionRepository"/>
    /// </summary>
    public static class XSelectionRepositoryExtension 
    {
        /// <summary>
        /// Replaces the selection (clears previous selection)
        /// </summary>
        /// <param name="selRepo">Selection repository</param>
        /// <param name="ents">Entities to select</param>
        public static void ReplaceRange(this IXSelectionRepository selRepo, IEnumerable<IXSelObject> ents)
            => selRepo.ReplaceRange(ents, default);
    }
}
