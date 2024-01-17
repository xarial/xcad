using __TemplateNamePlaceholderWpf__.UI;
using Xarial.XCad.Inventor;
using System.Windows;
using System.Linq;
using System.ComponentModel;

namespace __TemplateNamePlaceholderWpf__.Ai
{
    public partial class MainWindow : Window
    {
        private readonly PropertiesLoaderModel m_Model;

        public MainWindow()
        {
            InitializeComponent();

            //NOTE: in order to allow the creation of multiple instances of the Inventor application,
            //register the tools\Xarial.XCad.Inventor.Tools.StandAloneConnector add-in from the NuGet package
            m_Model = new PropertiesLoaderModel(() => AiApplicationFactory.PreCreate());

            this.DataContext = new PropertiesLoaderVM(
                m_Model, AiApplicationFactory.GetInstalledVersions().ToArray());
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            m_Model.Dispose();
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
