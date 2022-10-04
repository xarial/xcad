using Inventor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
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
            var apps = new List<IAiApplication>();

            //foreach (var prc in Process.GetProcessesByName("Inventor")) 
            //{
            //    apps.Add(AiApplicationFactory.FromProcess(p));
            //}

            apps.Add(AiApplicationFactory.Create(AiVersion_e.Inventor2023, ApplicationState_e.Default, StartApplicationConnectStrategy_e.Default));
            apps.Add(AiApplicationFactory.Create(AiVersion_e.Inventor2023, ApplicationState_e.Default, StartApplicationConnectStrategy_e.Default));
            apps.Add(AiApplicationFactory.Create(AiVersion_e.Inventor2023, ApplicationState_e.Default, StartApplicationConnectStrategy_e.Default));

            foreach (var app in apps)
            {
                Console.WriteLine($"Inventor: {app.Version.DisplayName} [{app.Process.Id}] : [{app.WindowHandle.ToInt32()}]");
                app.Close();
            }
        }
    }
}
