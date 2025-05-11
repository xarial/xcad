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

    [DebuggerDisplay("{" + nameof(Name) + "}")]
    internal class SwMaterialsDatabase : ISwMaterialsDatabase
    {
        public const string SYSTEM_DB_NAME = "solidworks materials";

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public virtual string FilePath { get; }

        public void AddRange(IEnumerable<IXMaterial> ents, CancellationToken cancellationToken) => throw new NotSupportedException();
        public void Commit(CancellationToken cancellationToken) => throw new NotSupportedException();
        public T PreCreate<T>() where T : IXMaterial => throw new NotSupportedException();
        public void RemoveRange(IEnumerable<IXMaterial> ents, CancellationToken cancellationToken) => throw new NotSupportedException();

        public IXMaterial this[string name] => m_RepoHelper.Get(name);

        public string Name { get; }

        public int Count => m_MatDbXml.Value.SelectNodes("//classification/material")?.Count ?? 0;

        public bool IsCommitted => true;

        public IEnumerable Filter(bool reverseOrder, params RepositoryFilterQuery[] filters) => m_RepoHelper.FilterDefault(this, filters, reverseOrder);

        public IEnumerator<IXMaterial> GetEnumerator()
        {
            foreach (XmlNode matXmlNode in m_MatDbXml.Value.SelectNodes("//classification/material"))
            {
                yield return new SwMaterial(matXmlNode, this);
            }
        }

        public bool TryGet(string name, out IXMaterial ent)
        {
            var matXmlNode = m_MatDbXml.Value.SelectSingleNode($"//classification/material[@name='{name}']");

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

        internal XmlNode FindMaterialXmlNode(string name) => m_MatDbXml.Value.SelectSingleNode($"//classification/material[@name='{name}']");

        private readonly SwApplication m_App;

        private readonly Lazy<XmlDocument> m_MatDbXml;

        private static string GetMaterialDbName(string dbFilePath)
        {
            var dbFileName = Path.GetFileNameWithoutExtension(dbFilePath);

            if (string.Equals(dbFileName, SYSTEM_DB_NAME, StringComparison.CurrentCultureIgnoreCase))
            {
                return "";
            }
            else
            {
                return dbFilePath;
            }
        }

        private static XmlDocument LoadXmlFromFile(string dbFilePath)
        {
            var matDbXml = new XmlDocument();
            matDbXml.LoadXml(File.ReadAllText(dbFilePath));
            return matDbXml;
        }

        private readonly RepositoryHelper<IXMaterial> m_RepoHelper;

        internal SwMaterialsDatabase(SwApplication app, string dbFilePath)
            : this(app, GetMaterialDbName(dbFilePath), new Lazy<XmlDocument>(() => LoadXmlFromFile(dbFilePath)))
        {
            FilePath = dbFilePath;
        }

        protected SwMaterialsDatabase(SwApplication app, string name, Lazy<XmlDocument> matDbXml)
        {
            m_App = app;
            Name = name;
            m_MatDbXml = matDbXml;

            m_RepoHelper = new RepositoryHelper<IXMaterial>(this);
        }
    }
}
