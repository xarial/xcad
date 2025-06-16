using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Documents.Extensions;
using System.Xml.Linq;

namespace Xarial.XCad.Tests.Common
{
    public class TestManager<TApp, TDoc> : IDisposable
        where TApp : IXApplication
        where TDoc : IXDocument
    {
        public TApp Application { get; }

        private readonly List<DataDocument<TDoc>> m_Docs;

        private readonly string m_RootWorkFolder;

        private readonly ZipArchive m_Zip;

        public TestManager(TApp app, string dataArchiveFilePath)
        {
            Application = app;

            m_Zip = ZipFile.OpenRead(dataArchiveFilePath);

            m_RootWorkFolder = Path.Combine(Path.GetTempPath(), "xCAD_Tests_" + Guid.NewGuid().ToString());

            m_Docs = new List<DataDocument<TDoc>>();
        }

        public DataFile GetDataFile(string name)
        {
            var dataFilePath = ExtractDataFile(name, out var workFolderPath);

            return new DataFile(dataFilePath, workFolderPath);
        }

        public DataDocument<TDoc> OpenDataDocument(string name, bool readOnly = true)
        {
            var dataFilePath = ExtractDataFile(name, out var workFolderPath);

            var doc = (TDoc)Application.Documents.Open(dataFilePath,
                readOnly
                ? DocumentState_e.ReadOnly
                : DocumentState_e.Default);

            if (doc != null)
            {
                var dataDoc = new DataDocument<TDoc>(doc, dataFilePath, workFolderPath);
                m_Docs.Add(dataDoc);
                return dataDoc;
            }
            else
            {
                throw new NullReferenceException($"Failed to open the the data document at '{dataFilePath}'");
            }
        }

        private string ExtractDataFile(string name, out string workFolderPath)
        {
            workFolderPath = CreateDataFolder();

            try
            {
                var hasRoot = TryGetRootEntryName(name, out var rootName);

                var entries = m_Zip.Entries.Where(e =>
                {
                    if (!string.IsNullOrEmpty(e.Name))
                    {
                        if (hasRoot)
                        {
                            return e.FullName.StartsWith(rootName + "/", StringComparison.CurrentCultureIgnoreCase);
                        }
                        else
                        {
                            return string.Equals(e.FullName, name, StringComparison.CurrentCultureIgnoreCase);
                        }
                    }
                    else
                    {
                        return false;
                    }
                }).ToArray();

                if (entries.Any())
                {
                    foreach (var entry in entries)
                    {
                        var targFilePath = Path.Combine(workFolderPath, entry.FullName);

                        var dir = Path.GetDirectoryName(targFilePath);

                        if (!Directory.Exists(dir))
                        {
                            Directory.CreateDirectory(dir);
                        }

                        entry.ExtractToFile(targFilePath);
                    }

                    var filePath = Path.Combine(workFolderPath, name);

                    if (File.Exists(filePath))
                    {
                        return filePath;
                    }
                    else
                    {
                        throw new FileNotFoundException($"File is not found");
                    }
                }
                else
                {
                    throw new FileNotFoundException($"Entry is not found in the archive");
                }
            }
            catch 
            {
                try
                {
                    Directory.Delete(workFolderPath, true);
                }
                catch
                {
                }

                throw;
            }
        }

        private bool TryGetRootEntryName(string name, out string rootName)
        {
            var dirName = Path.GetDirectoryName(name);

            if (!string.IsNullOrEmpty(dirName))
            {
                do
                {
                    rootName = dirName;
                    dirName = Path.GetDirectoryName(dirName);
                } while (!string.IsNullOrEmpty(dirName));

                return true;
            }
            else
            {
                rootName = "";
                return false;
            }
        }

        private string CreateDataFolder()
        {
            var dirPath = Path.Combine(m_RootWorkFolder, Guid.NewGuid().ToString());
            Directory.CreateDirectory(dirPath);

            return dirPath;
        }

        public void Dispose()
        {
            m_Zip.Dispose();

            foreach (var doc in m_Docs)
            {
                try
                {
                    doc.Dispose();
                }
                catch
                {
                }
            }

            m_Docs.Clear();

            foreach (var doc in Application.Documents.ToArray()) 
            {
                try
                {
                    doc.Close();
                }
                catch
                {
                }
            }

            try
            {
                Directory.Delete(m_RootWorkFolder, true);
            }
            catch
            {
            }
        }
    }
}
