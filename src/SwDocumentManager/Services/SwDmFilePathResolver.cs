//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xarial.XCad.Documents.Exceptions;

namespace Xarial.XCad.SwDocumentManager.Services
{
    /// <summary>
    /// Service which provides custom path resolvers
    /// </summary>
    public interface IFilePathResolver 
    {
        /// <summary>
        /// Resolves the path of the document
        /// </summary>
        /// <param name="parentDocPath">Parent document full path</param>
        /// <param name="path">Cached document path</param>
        /// <returns>Resolved path</returns>
        string ResolvePath(string parentDocPath, string path);
    }

    /// <summary>
    /// Resolves the reference path for the document
    /// </summary>
    /// <remarks>This logic implemented according to <see href="https://help.solidworks.com/2016/english/SolidWorks/sldworks/c_Search_Routine_for_Referenced_Documents.htm"/></remarks>
    public class SwDmFilePathResolver : IFilePathResolver
    {
        public string ResolvePath(string parentDocDirPath, string path)
        {
            string resolvedPath;
            
            if (TrySearchRecursively(parentDocDirPath, path, out resolvedPath))
            {
                return resolvedPath;
            }

            if (IsReferenceExists(path))
            {
                return path;
            }

            throw new FilePathResolveFailedException(path);
        }

        private bool TrySearchRecursively(string targetDirPath, string searchPath, out string resultPath)
        {
            var targetDir = new DirectoryInfo(targetDirPath);
            var searchDir = new DirectoryInfo(Path.GetDirectoryName(searchPath));

            var fileName = Path.GetFileName(searchPath);

            var pathToCheck = Path.Combine(targetDirPath, fileName);

            if (IsReferenceExists(pathToCheck))
            {
                resultPath = pathToCheck;
                return true;
            }

            var parentDir = targetDir;

            while (parentDir != null)
            {
                var compSubDir = new DirectoryInfo(searchDir.FullName);

                var curSubPath = "";

                while (compSubDir.Parent != null)
                {
                    curSubPath = Path.Combine(compSubDir.Name, curSubPath);

                    pathToCheck = Path.Combine(parentDir.FullName, curSubPath, fileName);

                    if (IsReferenceExists(pathToCheck))
                    {
                        resultPath = pathToCheck;
                        return true;
                    }

                    compSubDir = compSubDir.Parent;
                }

                parentDir = parentDir.Parent;
            }

            resultPath = "";
            return false;
        }

        protected virtual bool IsReferenceExists(string path)
            => File.Exists(path);
    }
}
