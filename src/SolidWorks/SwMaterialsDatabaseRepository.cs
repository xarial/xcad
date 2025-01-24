//*********************************************************************
//xCAD
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xarial.XCad.Base;
using Xarial.XCad.Toolkit.Utils;

namespace Xarial.XCad.SolidWorks
{
    internal class SwMaterialsDatabaseRepository : IXMaterialsDatabaseRepository
    {
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        IXMaterialsDatabase IXRepository<IXMaterialsDatabase>.this[string name] => this[name];

        public void AddRange(IEnumerable<IXMaterialsDatabase> ents, CancellationToken cancellationToken) => throw new NotSupportedException();
        public T PreCreate<T>() where T : IXMaterialsDatabase => throw new NotSupportedException();
        public void RemoveRange(IEnumerable<IXMaterialsDatabase> ents, CancellationToken cancellationToken) => throw new NotSupportedException();

        public SwMaterialsDatabase this[string name] => (SwMaterialsDatabase)m_RepoHelper.Get(name);

        public int Count => m_App.Sw.GetMaterialDatabaseCount();

        public IEnumerable Filter(bool reverseOrder, params RepositoryFilterQuery[] filters) 
            => m_RepoHelper.FilterDefault(this, filters, reverseOrder);

        public IEnumerator<IXMaterialsDatabase> GetEnumerator()
        {
            var matDbPaths = (string[])m_App.Sw.GetMaterialDatabases() ?? new string[0];

            foreach (var matDbPath in matDbPaths)
            {
                yield return new SwMaterialsDatabase(m_App, matDbPath);
            }
        }

        public bool TryGet(string name, out IXMaterialsDatabase ent)
        {
            if (string.IsNullOrEmpty(name)) 
            {
                name = SwMaterialsDatabase.SYSTEM_DB_NAME;
            }

            var matDbPaths = (string[])m_App.Sw.GetMaterialDatabases();

            if (matDbPaths?.Any() == true)
            {
                var matDbPath = matDbPaths.FirstOrDefault(x => string.Equals(x, name, StringComparison.CurrentCultureIgnoreCase));

                if (string.IsNullOrEmpty(matDbPath))
                {
                    var searchmatDbPaths = matDbPaths.Where(x => string.Equals(Path.GetFileNameWithoutExtension(x), name, StringComparison.CurrentCultureIgnoreCase)).ToArray();

                    if (searchmatDbPaths.Length == 1)
                    {
                        matDbPath = searchmatDbPaths.First();
                    }
                    else if (searchmatDbPaths.Length > 1)
                    {
                        throw new Exception("More than one material database found by the specified name. Use full file path instead");
                    }
                }

                if (!string.IsNullOrEmpty(matDbPath))
                {
                    ent = new SwMaterialsDatabase(m_App, matDbPath);
                    return true;
                }
            }

            ent = null;
            return false;
        }

        private readonly SwApplication m_App;

        private readonly RepositoryHelper<IXMaterialsDatabase> m_RepoHelper;

        internal SwMaterialsDatabaseRepository(SwApplication app)
        {
            m_App = app;

            m_RepoHelper = new RepositoryHelper<IXMaterialsDatabase>(this);
        }
    }
}
