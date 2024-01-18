using System;
using System.Data;
using System.IO;
using Xarial.XCad;
using Xarial.XCad.Data;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Documents.Extensions;

namespace __TemplateNamePlaceholderConsole__.Base
{
    /// <summary>
    /// Properties reader service
    /// </summary>
    public interface IPropertiesReader : IDisposable
    {
        /// <summary>
        /// Application
        /// </summary>
        IXApplication Application { get; }

        /// <summary>
        /// Loads properties of the specified file
        /// </summary>
        /// <param name="filePath">File to read and print properties from</param>
        DataTable Load(string filePath);
    }

    /// <summary>
    /// Service to read custom properties from file, configuration and cut-list to the output text writer
    /// </summary>
    public class PropertiesReader : IPropertiesReader
    {
        private const string COL_FILE = "<File>";
        private const string COL_CONFIGURATION = "<Configuration>";
        private const string COL_CUT_LIST = "<Cut-List>";

        /// <inheritdoc/>
        public IXApplication Application { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="app">Pointer to the application</param>
        public PropertiesReader(IXApplication app)
        {
            Application = app;
        }
                
        /// <inheritdoc/>
        public DataTable Load(string filePath) 
        {
            var table = new DataTable();
            table.Columns.Add(COL_FILE);
            table.Columns.Add(COL_CONFIGURATION);
            table.Columns.Add(COL_CUT_LIST);

            using (var doc = Application.Documents.Open(filePath, DocumentState_e.ReadOnly)) 
            {
                var fileRow = table.Rows.Add();
                fileRow[COL_FILE] = doc.Title;

                WriteProperties(doc.Properties, fileRow);

                if (doc is IXDocument3D) 
                {
                    try
                    {
                        foreach (var conf in ((IXDocument3D)doc).Configurations)
                        {
                            var confRow = table.Rows.Add();
                            confRow[COL_FILE] = doc.Title;
                            confRow[COL_CONFIGURATION] = conf.Name;

                            WriteProperties(conf.Properties, confRow);

                            if (conf is IXPartConfiguration)
                            {
                                try
                                {
                                    foreach (var cutList in ((IXPartConfiguration)conf).CutLists)
                                    {
                                        var cutListRow = table.Rows.Add();
                                        cutListRow[COL_FILE] = doc.Title;
                                        cutListRow[COL_CONFIGURATION] = conf.Name;
                                        cutListRow[COL_CUT_LIST] = cutList.Name;

                                        WriteProperties(cutList.Properties, cutListRow);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    confRow.SetColumnError(COL_FILE, ex.Message);
                                }
                            }
                            
                        }
                    }
                    catch (Exception ex)
                    {
                        fileRow.SetColumnError(COL_FILE, ex.Message);
                    }
                }
            }

            return table;
        }

        private void WriteProperties(IXPropertyRepository prps, DataRow row)
        {
            foreach (var prp in prps) 
            {
                if (!row.Table.Columns.Contains(prp.Name)) 
                {
                    row.Table.Columns.Add(prp.Name);
                }

                try
                {
                    row[prp.Name] = prp.Value;
                }
                catch (Exception ex)
                {
                    row.SetColumnError(prp.Name, ex.Message);
                }
            }
        }

        public void Dispose()
        {
            Application.Dispose();
        }
    }
}
