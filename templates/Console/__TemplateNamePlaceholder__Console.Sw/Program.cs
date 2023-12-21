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
            using (var reader = new PropertiesReader(SwApplicationFactory.Create(SwVersion_e.Sw2023), Console.Out))
            {
                foreach (var filePath in args)
                {
                    reader.PrintProperties(filePath);
                }
            }
        }
    }
}
