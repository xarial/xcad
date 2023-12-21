using Inventor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
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
            var app = AiApplicationFactory.FromProcess(Process.GetProcessesByName("Inventor").FirstOrDefault());
            
            using (var doc = app.Documents.Open(@"C:\Users\artem\Desktop\Inventor\C001.ipt"))
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

                    //var active = doc3D.Configurations.Active;

                    //doc3D.Configurations.Active = doc3D.Configurations.Last();
                }

                foreach (var prp in doc.Properties)
                {
                }
            }

            Console.ReadLine();
        }
    }
}
