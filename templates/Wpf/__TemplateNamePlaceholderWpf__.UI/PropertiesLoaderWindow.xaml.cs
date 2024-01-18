using System.Windows;
using System.Linq;
using System.ComponentModel;
using Xarial.XCad;
using System;

namespace __TemplateNamePlaceholderWpf__.UI
{
    public partial class PropertiesLoaderWindow : Window
    {
        private readonly PropertiesLoaderModel m_Model;

        public PropertiesLoaderWindow(Func<IXApplication> appTemplateProvider, IXVersion[] versions)
        {
            InitializeComponent();

            m_Model = new PropertiesLoaderModel(appTemplateProvider);

            this.DataContext = new PropertiesLoaderVM(m_Model, versions);
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
