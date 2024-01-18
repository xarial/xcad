using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Xarial.XCad;

namespace __TemplateNamePlaceholderWpf__.UI
{
    public class PropertiesLoaderVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string FilePath 
        {
            get => m_FilePath;
            set
            {
                m_FilePath = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilePath)));
                TryLoadProperties();
            }
        }

        public IXVersion SelectedVersion
        {
            get => m_SelectedVersion;
            set
            {
                m_SelectedVersion = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedVersion)));
                TryLoadProperties();
            }
        }

        public DataTable PropertiesTable
        {
            get => m_PropertiesTable;
            set
            {
                m_PropertiesTable = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PropertiesTable)));
            }
        }

        public IXVersion[] Versions { get; }

        public ICommand BrowseFileCommand { get; }

        private string m_FilePath;
        private IXVersion m_SelectedVersion;
        private DataTable m_PropertiesTable;

        private readonly PropertiesLoaderModel m_Model;

        public PropertiesLoaderVM(PropertiesLoaderModel model, IXVersion[] versions) 
        {
            m_Model = model;
            Versions = versions;
            SelectedVersion = versions.FirstOrDefault();
            BrowseFileCommand = new RelayCommand(x => BrowseFile(), x => SelectedVersion != null);
        }

        private void BrowseFile()
        {
            var openFileDlg = new OpenFileDialog();
            openFileDlg.Filter = "All Files (*.*)|*.*";
            
            if (openFileDlg.ShowDialog() == true) 
            {
                FilePath = openFileDlg.FileName;
            }
        }

        private void TryLoadProperties() 
        {
            DataTable table = null;

            try
            {
                if (!string.IsNullOrEmpty(FilePath) && SelectedVersion != null)
                {
                    table = m_Model.Load(SelectedVersion, FilePath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "xCAD Properties Reader", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            PropertiesTable = table;
        }
    }
}
