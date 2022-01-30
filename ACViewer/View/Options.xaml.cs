using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
//using System.Windows.Forms;

using Microsoft.Win32;

using ACViewer.Config;

namespace ACViewer.View
{
    /// <summary>
    /// Interaction logic for Options.xaml
    /// </summary>
    public partial class Options : Window, INotifyPropertyChanged
    {
        public static Config.Config Config => ConfigManager.Config;

        public string ACFolder
        {
            get => Config.ACFolder;
            set
            {
                Config.ACFolder = value;
                NotifyPropertyChanged("ACFolder");
            }
        }

        public bool AutomaticallyLoadDATsOnStartup
        {
            get => Config.AutomaticallyLoadDATsOnStartup;
            set
            {
                Config.AutomaticallyLoadDATsOnStartup = value;
                NotifyPropertyChanged("AutomaticallyLoadACFolderOnStartup");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Options()
        {
            InitializeComponent();

            DataContext = this;
        }

        private void SelectACFolderButton_Click(object sender, RoutedEventArgs e)
        {
            //var folderBrowserDialog = new FolderBrowserDialog();
            //folderBrowserDialog.ShowDialog();

            var openFileDialog = new OpenFileDialog();
            
            var success = openFileDialog.ShowDialog();

            if (success != true) return;

            var fi = new System.IO.FileInfo(openFileDialog.FileName);

            ACFolder = fi.DirectoryName;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            ConfigManager.SaveConfig();
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
