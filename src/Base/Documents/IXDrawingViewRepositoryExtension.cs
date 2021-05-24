//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Xarial.XCad.Base;

namespace Xarial.XCad.Documents
{
    /// <summary>
    /// Extension methods for the <see cref="IXDrawingViewRepository"/>
    /// </summary>
    public static class IXDrawingViewRepositoryExtension
    {
        /// <summary>
        /// Creates a view based on the model 3D view
        /// </summary>
        /// <param name="repo">Views repositry</param>
        /// <param name="view">Model based view to create drawing view from</param>
        /// <returns>Created drawing view</returns>
        public static IXModelViewBasedDrawingView CreateModelViewBased(this IXDrawingViewRepository repo, IXModelView view)
        {
            var drwView = repo.PreCreate<IXModelViewBasedDrawingView>();
            drwView.SourceModelView = view;

            repo.Add(drwView);

            return drwView;
        }
    }
}
