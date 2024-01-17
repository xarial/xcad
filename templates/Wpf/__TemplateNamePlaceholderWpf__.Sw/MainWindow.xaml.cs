using __TemplateNamePlaceholderWpf__.UI;
using Xarial.XCad.SolidWorks;
using System.Windows;
using System.Linq;
using System.ComponentModel;

namespace __TemplateNamePlaceholderWpf__.Sw
{
    public partial class MainWindow : Window
    {
        private readonly PropertiesLoaderModel m_Model;

        public MainWindow()
        {
            InitializeComponent();
            m_Model = new PropertiesLoaderModel(() => SwApplicationFactory.PreCreate());

            this.DataContext = new PropertiesLoaderVM(
                m_Model, SwApplicationFactory.GetInstalledVersions().ToArray());
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
