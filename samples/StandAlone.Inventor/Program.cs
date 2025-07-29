using Inventor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Controls;
using Xarial.XCad.Base;
using Xarial.XCad.Documents;
using Xarial.XCad.Documents.Extensions;
using Xarial.XCad.Enums;
using Xarial.XCad.Inventor;
using Xarial.XCad.Inventor.Enums;
using Xarial.XCad.Toolkit.Windows;
using Xarial.XCad.Utils.Diagnostics;

namespace StandAlone.Ai
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //using (var app = AiApplicationFactory.Create(AiVersion_e.Inventor2023)) 
            //{
            //var app = AiApplicationFactory.Create(AiVersion_e.Inventor2023);
            var app = AiApplicationFactory.FromProcess(Process.GetProcessesByName("Inventor").FirstOrDefault());

            //using (var doc = app.Documents.Open(@"C:\Users\artem\Desktop\Inventor\C001.ipt"))
            //{

            //}

            TestSheets((IXDrawing)app.Documents.Active);

            TestExport(app.Documents.Active);

            //TestProperties(doc);

            Console.ReadLine();
        }

        private static void TestSheets(IXDrawing drw) 
        {
            foreach (var sheet in drw.Sheets)
            {
            }
        }

        private static void TestProperties(IXDocument doc)
        {
            if (doc is IXDocument3D)
            {
                var doc3D = (IXDocument3D)doc;

                foreach (var conf in doc3D.Configurations)
                {
                    foreach (var prp in conf.Properties)
                    {
                    }
                }
            }

            foreach (var prp in doc.Properties)
            {
            }
        }

        private static void TestExport(IXDocument doc) 
        {
            var saveOp = (IXDxfDwgSaveOperation)doc.PreCreateSaveAsOperation(@"D:\Temp\" + System.IO.Path.GetFileNameWithoutExtension(doc.Name) + ".dwg");
            saveOp.ConfigurationFilePath = @"D:\Temp\acad.ini";
            saveOp.Commit();
        }
    }
}
