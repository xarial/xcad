using SolidWorks.Interop.sldworks;
using System;
using System.Threading.Tasks;
using Xarial.XCad.Documents.Structures;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Enums;

namespace Xarial.XCad.Documentation
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using (var app = await SwApplication.StartAsync(SwVersion_e.Sw2020, "/b"))
            {
                ISldWorks swApp = app.Sw;
                Console.WriteLine(swApp.RevisionNumber());

                var doc = app.Documents.Open(new DocumentOpenArgs()
                {
                    Path = @"D:\model1.SLDPRT",
                    ReadOnly = true
                });

                IModelDoc2 swModel = doc.Model;

                Console.WriteLine(swModel.GetTitle());

                app.Close();
            }
        }
    }
}
