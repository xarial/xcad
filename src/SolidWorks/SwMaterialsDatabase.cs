//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Xml;
using Xarial.XCad.Base;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.SolidWorks
{
    /// <summary>
    /// SOLIDWORKS-specific materials database
    /// </summary>
    public interface ISwMaterialsDatabase : IXMaterialsDatabase
    {
        /// <summary>
        /// Full path to the materials database file
        /// </summary>
        string FilePath { get; }
    }

    /// <summary>
    /// Temp material database
    /// </summary>
    /// <remarks>This database is used if the database file is not available on the PC</remarks>
    public interface ISwTempMaterialsDatabase : ISwMaterialsDatabase
    {
    }

    /// <summary>
    /// This database is used if there are multiple database files matching the material database
    /// </summary>
    public interface ISwAmbigiusMaterialsDatabase : ISwMaterialsDatabase
    {
        /// <summary>
        /// Database paths
        /// </summary>
        string[] FilePaths { get; }

        /// <summary>
        /// Index of database to use
        /// </summary>
        /// <remarks>If not set <see cref="Exceptions.AmbigiusMaterialsDatabaseException"/> will be thrown when database properties are accessed</remarks>
        int? MaterialDatabaseIndex { get; set; }
    }

    [DebuggerDisplay("{" + nameof(Name) + "}")]
    internal class SwMaterialsDatabase : ISwMaterialsDatabase
    {
        protected static string GetMaterialDbName(string dbFilePath)
        {
            var dbFileName = Path.GetFileNameWithoutExtension(dbFilePath);

            if (string.Equals(dbFileName, SYSTEM_DB_NAME, StringComparison.CurrentCultureIgnoreCase))
            {
                return "";
            }
            else
            {
                return dbFileName;
            }
        }

        protected static XmlDocument LoadXmlFromFile(string dbFilePath)
        {
            var matDbXml = new XmlDocument();
            matDbXml.LoadXml(File.ReadAllText(dbFilePath));
            return matDbXml;
        }

        internal const string SYSTEM_DB_NAME = "solidworks materials";

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public virtual string FilePath { get; }

        public void AddRange(IEnumerable<IXMaterial> ents, CancellationToken cancellationToken) => throw new NotSupportedException();
        public void Commit(CancellationToken cancellationToken) => throw new NotSupportedException();
        public T PreCreate<T>() where T : IXMaterial => throw new NotSupportedException();
        public void RemoveRange(IEnumerable<IXMaterial> ents, CancellationToken cancellationToken) => throw new NotSupportedException();

        public IXMaterial this[string name] => m_RepoHelper.Get(name);

        public virtual string Name { get; }

        public int Count => MatDbXml.SelectNodes("//classification/material")?.Count ?? 0;

        public bool IsCommitted => true;

        public IEnumerable Filter(bool reverseOrder, params RepositoryFilterQuery[] filters) => m_RepoHelper.FilterDefault(this, filters, reverseOrder);

        public IEnumerator<IXMaterial> GetEnumerator()
        {
            foreach (XmlNode matXmlNode in MatDbXml.SelectNodes("//classification/material"))
            {
                yield return new SwMaterial(matXmlNode, this);
            }
        }

        public bool TryGet(string name, out IXMaterial ent)
        {
            var matXmlNode = MatDbXml.SelectSingleNode($"//classification/material[@name='{name}']");

            if (matXmlNode != null)
            {
                ent = new SwMaterial(matXmlNode, this);
                return true;
            }
            else
            {
                ent = null;
                return false;
            }
        }

        internal XmlNode FindMaterialXmlNode(string name) => MatDbXml.SelectSingleNode($"//classification/material[@name='{name}']");

        private readonly SwApplication m_App;

        private readonly Lazy<XmlDocument> m_MatDbXmlLazy;

        private readonly RepositoryHelper<IXMaterial> m_RepoHelper;

        protected virtual XmlDocument MatDbXml => m_MatDbXmlLazy.Value;

        internal SwMaterialsDatabase(SwApplication app, string dbFilePath)
            : this(app)
        {
            Name = GetMaterialDbName(dbFilePath);
            FilePath = dbFilePath;
            m_MatDbXmlLazy = new Lazy<XmlDocument>(() => LoadXmlFromFile(dbFilePath));
        }

        protected SwMaterialsDatabase(SwApplication app)
        {
            m_App = app;
            
            m_RepoHelper = new RepositoryHelper<IXMaterial>(this);
        }
    }
}
