using System;
using System.IO;

namespace Xarial.XCad.Tests.Common
{
    public class DataFile : IDisposable
    {
        public string FilePath { get; }

        public string WorkFolderPath { get; }

        private bool m_IsDisposed;

        public DataFile(string filePath, string workFolderPath)
        {
            FilePath = filePath;
            WorkFolderPath = workFolderPath;

            m_IsDisposed = false;
        }

        public void Dispose()
        {
            if (!m_IsDisposed)
            {
                m_IsDisposed = true;

                Dispose(true);
            }
        }

        protected virtual void Dispose(bool disposing) 
        {
            try
            {
                Directory.Delete(WorkFolderPath, true);
            }
            catch
            {
            }
        }
    }
}
