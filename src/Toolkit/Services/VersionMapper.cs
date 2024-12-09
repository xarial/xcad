using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Toolkit.Attributes;
using Xarial.XCad.Utils.Reflection;

namespace Xarial.XCad.Toolkit.Services
{
    /// <summary>
    /// Service to map the version enumeration to the version information
    /// </summary>
    /// <typeparam name="TVersion">Version enumeration</typeparam>
    /// <remarks>This service can extrapolate to the future version</remarks>
    public interface IVersionMapper<TVersion>
        where TVersion : Enum
    {
        /// <summary>
        /// Create version from file revision
        /// </summary>
        /// <param name="fileRev">File revision</param>
        /// <returns>Version</returns>
        TVersion FromFileRevision(int fileRev);

        /// <summary>
        /// Create version from application revision
        /// </summary>
        /// <param name="appRev">Application revision</param>
        /// <returns>Version</returns>
        TVersion FromApplicationRevision(int appRev);

        /// <summary>
        /// Create version from the application release year
        /// </summary>
        /// <param name="year">Release year or version number</param>
        /// <param name="suffix">Optional suffix of the release</param>
        /// <returns>Version</returns>
        TVersion FromReleaseYear(int year, string suffix = "");

        /// <summary>
        /// Gets the user friendly name of the version
        /// </summary>
        /// <param name="version">Version</param>
        /// <returns>Name of the version</returns>
        string GetVersionName(TVersion version);
    }

    /// <summary>
    /// Base implementation of version mapper based on <see cref="FileRevisionAttribute"/> and <see cref="FileRevisionAttribute"/>
    /// </summary>
    /// <typeparam name="TVersion">Version enumeration</typeparam>
    public class VersionMapper<TVersion> : IVersionMapper<TVersion>
        where TVersion : Enum
    {
        private struct VersionYear
        {
            public int Year { get; }
            public string Suffix { get; }

            internal VersionYear(int year, string suffix)
            {
                Year = year;
                Suffix = suffix;
            }
        }

        private readonly IReadOnlyDictionary<int, TVersion> m_FileRevisionToVersionMap;
        private readonly IReadOnlyDictionary<int, TVersion> m_RevisionToVersionMap;
        private readonly IReadOnlyDictionary<VersionYear, TVersion> m_YearToVersionMap;
        private readonly IReadOnlyDictionary<TVersion, VersionYear> m_VersionToYearMap;

        private readonly int? m_LastFileVersion;
        private readonly int? m_LastAppVersion;
        private readonly int? m_LastAppYear;

        /// <summary>
        /// Default constructor
        /// </summary>
        public VersionMapper()
        {
            var fileRevisionToVersionMap = new Dictionary<int, TVersion>();
            m_FileRevisionToVersionMap = fileRevisionToVersionMap;

            var revisionToVersionMap = new Dictionary<int, TVersion>();
            m_RevisionToVersionMap = revisionToVersionMap;

            var yearToVersionMap = new Dictionary<VersionYear, TVersion>();
            m_YearToVersionMap = yearToVersionMap;

            var versionToYearMap = new Dictionary<TVersion, VersionYear>();
            m_VersionToYearMap = versionToYearMap;

            //need to keep the original order of enum values to correctly find the last item
            var fields = typeof(TVersion).GetFields(BindingFlags.Static | BindingFlags.Public);
            var versions = Array.ConvertAll(fields, x => (TVersion)x.GetValue(null));

            for (int i = 0; i < versions.Length; i++)
            {
                var isLast = i == versions.Length - 1;

                var version = versions[i];

                var appRev = Convert.ToInt32(version);

                if (!revisionToVersionMap.ContainsKey(appRev))
                {
                    revisionToVersionMap.Add(appRev, version);
                }
                else
                {
                    throw new Exception($"Duplicate application revision '{appRev}'");
                }

                if (isLast)
                {
                    m_LastAppVersion = appRev;
                }

                var fileVersAtt = version.TryGetAttribute<FileRevisionAttribute>();

                if (fileVersAtt != null)
                {
                    var fileRev = fileVersAtt.Revision;

                    if (!fileRevisionToVersionMap.ContainsKey(fileRev))
                    {
                        fileRevisionToVersionMap.Add(fileRev, version);
                    }
                    else
                    {
                        throw new Exception($"Duplicate file revision '{fileRev}'");
                    }

                    if (isLast)
                    {
                        m_LastFileVersion = fileRev;
                    }
                }

                var releaseYearAtt = version.TryGetAttribute<ReleaseYearAttribute>();

                if (releaseYearAtt != null)
                {
                    var year = new VersionYear(releaseYearAtt.Year, releaseYearAtt.Suffix);

                    if (!yearToVersionMap.ContainsKey(year))
                    {
                        yearToVersionMap.Add(year, version);
                    }
                    else
                    {
                        throw new Exception($"Duplicate application revision '{appRev}'");
                    }

                    versionToYearMap.Add(version, year);

                    if (isLast)
                    {
                        m_LastAppYear = year.Year;
                    }
                }
            }
        }

        /// <inheritdoc/>
        public TVersion FromFileRevision(int fileRev)
        {
            if (!m_FileRevisionToVersionMap.TryGetValue(fileRev, out var vers))
            {
                if (m_LastFileVersion.HasValue && m_LastAppVersion.HasValue)
                {
                    if (fileRev > m_LastFileVersion.Value)
                    {
                        if ((fileRev - m_LastFileVersion.Value) % FileRevisionStep == 0)
                        {
                            var appVersOffset = (fileRev - m_LastFileVersion.Value) / FileRevisionStep;

                            vers = CreateUnknownVersion(m_LastAppVersion.Value + appVersOffset);
                        }
                        else
                        {
                            throw new Exception($"Unknown file revision must be incremented by step {FileRevisionStep}");
                        }
                    }
                    else
                    {
                        throw new Exception($"Unknown file revision is expected to be larger than {m_LastFileVersion.Value}");
                    }
                }
                else 
                {
                    throw new Exception("File revisions are not set");
                }
            }

            return vers;
        }

        /// <inheritdoc/>
        public TVersion FromApplicationRevision(int appRev)
        {
            if (!m_RevisionToVersionMap.TryGetValue(appRev, out var vers))
            {
                if (m_LastAppVersion.HasValue)
                {
                    if (appRev > m_LastAppVersion.Value)
                    {
                        var appVersOffset = appRev - m_LastAppVersion.Value;

                        vers = CreateUnknownVersion(m_LastAppVersion.Value + appVersOffset);
                    }
                    else
                    {
                        throw new Exception($"Unknown application revision is expected to be larger than {m_LastAppVersion.Value}");
                    }
                }
                else
                {
                    throw new Exception("Application revisions are not set");
                }
            }

            return vers;
        }

        /// <inheritdoc/>
        public TVersion FromReleaseYear(int year, string suffix = "")
        {
            if (!m_YearToVersionMap.TryGetValue(new VersionYear(year, suffix), out var vers))
            {
                if (m_LastAppYear.HasValue && m_LastAppVersion.HasValue)
                {
                    if (year > m_LastAppYear.Value)
                    {
                        var appVersOffset = year - m_LastAppYear.Value;

                        vers = CreateUnknownVersion(m_LastAppVersion.Value + appVersOffset);
                    }
                    else
                    {
                        throw new Exception($"Unknown application years is expected to be larger than {m_LastAppYear.Value}");
                    }
                }
                else 
                {
                    throw new Exception("Application release years are not set");
                }
            }

            return vers;
        }

        /// <inheritdoc/>
        public string GetVersionName(TVersion version)
        {
            if (!m_VersionToYearMap.TryGetValue(version, out var year))
            {
                if (m_LastAppYear.HasValue && m_LastAppVersion.HasValue)
                {
                    var appRev = Convert.ToInt32(version);

                    if (appRev > m_LastAppVersion.Value)
                    {
                        var appYearOffset = appRev - m_LastAppVersion.Value;

                        year = new VersionYear(m_LastAppYear.Value + appYearOffset, "");
                    }
                    else
                    {
                        throw new Exception($"Unknown application revision is expected to be larger than {m_LastAppVersion.Value}");
                    }
                }
                else
                {
                    throw new Exception("Application release years are not set");
                }
            }

            return VersionNameBase + year.Year.ToString() + year.Suffix;
        }

        /// <summary>
        /// Base name of the version to append year to
        /// </summary>
        protected virtual string VersionNameBase => "";

        /// <summary>
        /// Default step between major versions of the files relative to the major release of the application
        /// </summary>
        protected virtual int FileRevisionStep => 1;

        /// <summary>
        /// Create version from the application revision, where the revision is not known (future)
        /// </summary>
        /// <param name="appRev">Application revision</param>
        /// <returns>Version</returns>
        protected virtual TVersion CreateUnknownVersion(int appRev) => (TVersion)Enum.ToObject(typeof(TVersion), appRev);
    }
}
