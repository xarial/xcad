using __TemplateNamePlaceholderWpf__.UI;
using System.Linq;
using System.Windows;
using Xarial.XCad.SwDocumentManager;

namespace __TemplateNamePlaceholderWpf__.SwDm
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            this.MainWindow = new PropertiesLoaderWindow(
                () => SwDmApplicationFactory.Create("<YOUR DOCUMENT MANAGER LICENSE KEY>"),
                SwDmApplicationFactory.GetInstalledVersions().ToArray())
            {
                Title = "SOLIDWORKS Document Manager Properties Reader"
            };

            this.MainWindow.Show();
        }
    }
}
