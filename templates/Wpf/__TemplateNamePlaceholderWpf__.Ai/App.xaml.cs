using __TemplateNamePlaceholderWpf__.UI;
using System.Linq;
using System.Windows;
using Xarial.XCad.Inventor;

namespace __TemplateNamePlaceholderWpf__.Ai
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            this.MainWindow = new PropertiesLoaderWindow(
                () => AiApplicationFactory.PreCreate(),
                AiApplicationFactory.GetInstalledVersions().ToArray());

            this.MainWindow.Show();
        }
    }
}
