using SolidWorks.Interop.sldworks;
using System;
using System.Threading.Tasks;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Structures;
using Xarial.XCad.Enums;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Documents;
using Xarial.XCad.SolidWorks.Enums;

namespace Xarial.XCad.Documentation
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var app = SwApplicationFactory.Create(SwVersion_e.Sw2020, ApplicationState_e.Background))
            {
                ISldWorks swApp = app.Sw;
                Console.WriteLine(swApp.RevisionNumber());

                var doc = app.Documents.Open(@"D:\model1.SLDPRT", Documents.Enums.DocumentState_e.ReadOnly);

                var swModel = (doc as ISwDocument).Model;

                Console.WriteLine(swModel.GetTitle());

                app.Close();
            }
        }
    }
}
