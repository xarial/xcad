using __TemplateNamePlaceholderWpf__.UI;
using System.Windows;
using System.Linq;
using System.ComponentModel;
using Xarial.XCad.SwDocumentManager;

namespace __TemplateNamePlaceholderWpf__.SwDm
{
    public partial class MainWindow : Window
    {
        private readonly PropertiesLoaderModel m_Model;

        public MainWindow()
        {
            InitializeComponent();
            
            m_Model = new PropertiesLoaderModel(() => SwDmApplicationFactory.Create("YOUR DOCUMENT MANAGER LICENSE KEY"));

            this.DataContext = new PropertiesLoaderVM(
                m_Model, SwDmApplicationFactory.GetInstalledVersions().ToArray());
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
