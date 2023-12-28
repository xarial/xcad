using System;
using System.IO;
using Xarial.XCad;
using Xarial.XCad.Data;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Enums;
using Xarial.XCad.Documents.Extensions;

namespace __TemplateNamePlaceholder__Console.Base
{
    /// <summary>
    /// Service to print custom properties from file, configuration and cut-list to the output text writer
    /// </summary>
    public class PropertiesReader : IDisposable
    {
        private readonly IXApplication m_App;
        private readonly TextWriter m_OutWriter;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="app">Pointer to the application</param>
        /// <param name="outWriter">Text writer to output property values</param>
        public PropertiesReader(IXApplication app, TextWriter outWriter)
        {
            m_App = app;
            m_OutWriter = outWriter;
        }

        /// <summary>
        /// Prints properties of the specified file
        /// </summary>
        /// <param name="filePath">File to read and print properties from</param>
        public void PrintProperties(string filePath) 
        {
            using (var doc = m_App.Documents.Open(filePath, DocumentState_e.ReadOnly)) 
            {
                m_OutWriter.WriteLine($"{doc.Title}:");

                OutputProperties(doc.Properties);

                if (doc is IXDocument3D) 
                {
                    foreach (var conf in ((IXDocument3D)doc).Configurations) 
                    {
                        m_OutWriter.WriteLine($"{doc.Title} ({conf.Name}):");
                        OutputProperties(conf.Properties);

                        if (conf is IXPartConfiguration) 
                        {
                            foreach (var cutList in ((IXPartConfiguration)conf).CutLists) 
                            {
                                m_OutWriter.WriteLine($"{doc.Title} ({conf.Name}) [{cutList.Name}]:");
                                OutputProperties(cutList.Properties);
                            }
                        }
                    }
                }
            }
        }

        private void OutputProperties(IXPropertyRepository prps)
        {
            foreach (var prp in prps) 
            {
                m_OutWriter.WriteLine($"\t{prp.Name} = {prp.Value}");
            }
        }

        public void Dispose()
        {
            m_App.Dispose();
        }
    }
}
