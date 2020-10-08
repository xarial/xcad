//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

namespace Xarial.XCad.Documents.Structures
{
    /// <summary>
    /// Arguments passed to <see cref="IXDocumentCollection.Open(DocumentOpenArgs)"/> method
    /// </summary>
    public class DocumentOpenArgs
    {
        /// <summary>
        /// Path to the document
        /// </summary>
        public string Path { get; set; }
        
        /// <summary>
        /// Opens document in read-only mode
        /// </summary>
        public bool ReadOnly { get; set; }

        /// <summary>
        /// Opens document in view only mode
        /// </summary>
        public bool ViewOnly { get; set; }

        /// <summary>
        /// Opens document without displaying any popup messages
        /// </summary>
        public bool Silent { get; set; }

        /// <summary>
        /// Opens document in the rapid mode
        /// </summary>
        /// <remarks>This mode significantly improves the performance of opening but certain functionality and API migth not be available</remarks>
        public bool Rapid { get; set; }
    }
}