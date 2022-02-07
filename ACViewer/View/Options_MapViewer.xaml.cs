using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

using ACViewer.Config;
using ACViewer.Enum;

namespace ACViewer.View
{
    /// <summary>
    /// Interaction logic for Options_MapViewer.xaml
    /// </summary>
    public partial class Options_MapViewer : Window, INotifyPropertyChanged
    {
        public static Config.Config Config => ACViewer.Config.ConfigManager.Config;

        public int Mode
        {
            get => (int)Config.MapViewer.Mode;
            set
            {
                Config.MapViewer.Mode = (MapViewerMode)value;
                NotifyPropertyChanged("Mode");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private static readonly List<string> allProperties = new List<string>() { "Mode", };

        public Options_MapViewer()
        {
            InitializeComponent();

            DataContext = this;

            ConfigManager.TakeSnapshot();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            ConfigManager.SaveConfig();
            Close();

            if (Config.MapViewer.Mode != ConfigManager.Snapshot.MapViewer.Mode)
                MapViewer.Instance.LoadMap();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            ConfigManager.RestoreSnapshot();

            foreach (var propName in allProperties)
                NotifyPropertyChanged(propName);

            Close();
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
