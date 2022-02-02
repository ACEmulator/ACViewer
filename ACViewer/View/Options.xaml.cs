using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Imaging;

using Microsoft.Win32;

using ACE.Common;
using ACE.Database.Models.World;

using ACViewer.Config;

namespace ACViewer.View
{
    /// <summary>
    /// Interaction logic for Options.xaml
    /// </summary>
    public partial class Options : Window, INotifyPropertyChanged
    {
        public static Config.Config Config => ACViewer.Config.ConfigManager.Config;

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

        public string Host
        {
            get => Config.Database.Host;
            set
            {
                Config.Database.Host = value;
                NotifyPropertyChanged("Host");
            }
        }

        public int Port
        {
            get => Config.Database.Port;
            set
            {
                Config.Database.Port = value;
                NotifyPropertyChanged("Port");
            }
        }

        public string Database
        {
            get => Config.Database.DatabaseName;
            set
            {
                Config.Database.DatabaseName = value;
                NotifyPropertyChanged("Database");
            }
        }

        public string Username
        {
            get => Config.Database.Username;
            set
            {
                Config.Database.Username = value;
                NotifyPropertyChanged("Username");
            }
        }

        public string Password
        {
            get => Config.Database.Password;
            set
            {
                Config.Database.Password = value;
                NotifyPropertyChanged("Password");
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
            ACViewer.Config.ConfigManager.SaveConfig();
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool DatabaseConnected { get; set; }

        private void TestConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            DBStatus.Source = null;
            Spinner.Visibility = Visibility.Visible;

            var worker = new BackgroundWorker();

            worker.DoWork += (sender, doWorkEventArgs) =>
            {
                Server.SetDatabaseConfig();

                DatabaseConnected = false;

                using (var ctx = new WorldDbContext())
                {
                    try
                    {
                        var query = ctx.Weenie.FirstOrDefault();
                        DatabaseConnected = true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            };

            worker.RunWorkerCompleted += (sender, runWorkerCompletedEventArgs) =>
            {
                Spinner.Visibility = Visibility.Hidden;

                if (DatabaseConnected)
                    DBStatus.Source = new BitmapImage(new Uri(@"/ACViewer;component/Icons/Checkmark_16x.png", UriKind.Relative));
                else
                    DBStatus.Source = new BitmapImage(new Uri(@"/ACViewer;component/Icons/StatusCriticalError_16x.png", UriKind.Relative));
            };

            worker.RunWorkerAsync();
        }
    }
}
