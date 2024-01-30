using __TemplateNamePlaceholderWpf__.UI;
using System.Linq;
using System.Windows;
using Xarial.XCad.SolidWorks;

namespace __TemplateNamePlaceholderWpf__.Sw
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            this.MainWindow = new PropertiesLoaderWindow(
                () => SwApplicationFactory.PreCreate(),
                SwApplicationFactory.GetInstalledVersions().ToArray())
            {
                Title = "SOLIDWORKS Properties Reader"
            };

            this.MainWindow.Show();
        }
    }
}
