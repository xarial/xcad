using __TemplateNamePlaceholderConsole__.Base;
using System;
using Xarial.XCad.SwDocumentManager;

namespace __TemplateNamePlaceholderConsole__.SwDm
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using (var reader = new PropertiesReader(SwDmApplicationFactory.Create("<YOUR DOCUMENT MANAGER LICENSE KEY>"), Console.Out))
            {
                foreach (var filePath in args)
                {
                    reader.PrintProperties(filePath);
                }
            }
        }
    }
}