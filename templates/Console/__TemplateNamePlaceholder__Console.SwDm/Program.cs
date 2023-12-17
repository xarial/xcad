using __TemplateNamePlaceholder__Console.Base;
using Xarial.XCad.SwDocumentManager;

namespace __TemplateNamePlaceholder__Console.Sw
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var app = SwDmApplicationFactory.Create("");
            var controller = new Controller(app);
            controller.PrintProperties("");
        }
    }
}