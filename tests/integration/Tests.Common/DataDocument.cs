using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.Documents;

namespace Xarial.XCad.Tests.Common
{
    public class DataDocument<TDoc> : DataFile
            where TDoc : IXDocument
    {
        public TDoc Document { get; }

        public DataDocument(TDoc doc, string filePath, string workFolderPath) : base(filePath, workFolderPath)
        {
            Document = doc;
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                Document.Close();
            }
            catch
            {
            }

            base.Dispose(disposing);
        }
    }
}
