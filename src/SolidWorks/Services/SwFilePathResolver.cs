using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xarial.XCad.Documents;
using Xarial.XCad.SolidWorks.Documents;
using System.Linq;
using SolidWorks.Interop.swconst;
using Xarial.XCad.SolidWorks.Exceptions;

namespace Xarial.XCad.SolidWorks.Services
{
    /// <summary>
    /// Resolves the reference path for the document
    /// </summary>
    /// <remarks>This logic implemented according to <see href="https://help.solidworks.com/2016/english/SolidWorks/sldworks/c_Search_Routine_for_Referenced_Documents.htm"/></remarks>
    public abstract class SwFilePathResolverBase : IFilePathResolver
    {
        public string ResolvePath(string parentDocPath, string path)
        {
            if (TryGetLoadedDocumentPath(path, out string loadedPath)) 
            {
                return loadedPath;
            }

            var resolvedPath = "";

            foreach (var searchFolder in GetSearchFolders()) 
            {
                if (TrySearchRecursively(searchFolder, path, out resolvedPath)) 
                {
                    return resolvedPath;
                }
            }

            if (TrySearchRecursively(Path.GetDirectoryName(parentDocPath), path, out resolvedPath))
            {
                return resolvedPath;
            }

            if (IsReferenceExists(path)) 
            {
                return path;
            }

            throw new FilePathResolveFailedException(path);
        }

        private bool TrySearchRecursively(string targetDir, string searchPath, out string resultPath) 
        {
            var fileName = Path.GetFileName(searchPath);
            var fileDir = Path.GetDirectoryName(searchPath);

            var targetDirInfo = new DirectoryInfo(targetDir);

            var isFirst = true;

            while (targetDirInfo != null) 
            {
                var relDirs = ReverseDirectory(fileDir);

                if (isFirst) 
                {
                    relDirs = new string[] { "" }.Union(relDirs);
                    isFirst = false;
                }

                foreach (var curRelDir in relDirs)
                {
                    var pathToCheck = Path.Combine(targetDirInfo.FullName, curRelDir, fileName);
                    
                    if (IsReferenceExists(pathToCheck)) 
                    {
                        resultPath = pathToCheck;
                        return true;
                    }
                }

                targetDirInfo = targetDirInfo.Parent;
            }

            resultPath = "";
            return false;
        }

        private IEnumerable<string> ReverseDirectory(string dir)
        {
            var dirInfo = new DirectoryInfo(dir);

            var thisDir = "";

            while (dirInfo.Parent != null)
            {
                thisDir = Path.Combine(dirInfo.Name, thisDir);

                yield return thisDir;

                dirInfo = dirInfo.Parent;
            }
        }

        protected abstract bool TryGetLoadedDocumentPath(string path, out string loadedPath);
        protected abstract string[] GetSearchFolders();
        protected abstract bool IsReferenceExists(string path);
    }

    public class SwFilePathResolver : SwFilePathResolverBase
    {
        private readonly ISwApplication m_App;

        public SwFilePathResolver(ISwApplication app)
        {
            m_App = app;
        }

        protected override bool TryGetLoadedDocumentPath(string path, out string loadedPath)
        {
            var title = Path.GetFileNameWithoutExtension(path);

            var doc = m_App.Documents.FirstOrDefault(d => string.Equals(Path.GetFileNameWithoutExtension(d.Title),
                title, StringComparison.CurrentCultureIgnoreCase));

            if (doc != null)
            {
                loadedPath = doc.Path;
                return true;
            }
            else 
            {
                loadedPath = "";
                return false;
            }
        }

        protected override string[] GetSearchFolders()
        {
            var searchFolders = new string[0];

            var useSearchRule = m_App.Sw.GetUserPreferenceToggle((int)swUserPreferenceToggle_e.swUseFolderSearchRules);

            if (useSearchRule)
            {
                var searchFoldersStr = m_App.Sw.GetSearchFolders((int)swSearchFolderTypes_e.swDocumentType);

                if (!string.IsNullOrEmpty(searchFoldersStr))
                {
                    searchFolders = searchFoldersStr.Split(';');
                }
            }

            return searchFolders;
        }

        protected override bool IsReferenceExists(string path) => File.Exists(path);
    }
}
