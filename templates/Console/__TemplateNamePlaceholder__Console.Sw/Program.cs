using __TemplateNamePlaceholder__Console.Base;
using System;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Enums;

namespace __TemplateNamePlaceholder__Console.Sw
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var app = SwApplicationFactory.Create(SwVersion_e.Sw2023);
            var controller = new Controller(app);
            controller.PrintProperties("");
        }
    }
}
