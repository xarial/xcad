using __TemplateNamePlaceholder__Console.Base;
using System;
using Xarial.XCad.SwDocumentManager;

namespace __TemplateNamePlaceholder__Console.Sw
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using (var reader = new PropertiesReader(SwDmApplicationFactory.Create("DOC_LIC_KEY"), Console.Out))
            {
                foreach (var filePath in args)
                {
                    reader.PrintProperties(filePath);
                }
            }
        }
    }
}