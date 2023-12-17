using __TemplateNamePlaceholder__Console.Base;
using System;
using Xarial.XCad.Inventor;
using Xarial.XCad.Inventor.Enums;

namespace __TemplateNamePlaceholder__Console.Sw
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var app = AiApplicationFactory.Create(AiVersion_e.Inventor2023);
            var controller = new Controller(app);
            controller.PrintProperties("");
        }
    }
}
