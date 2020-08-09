using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using ACE.DatLoader;

namespace ACViewer.View
{
    /// <summary>
    /// Interaction logic for MainMenu.xaml
    /// </summary>
    public partial class MainMenu : UserControl
    {
        public static MainWindow MainWindow { get => MainWindow.Instance; }

        public static ParticleExplorer Particle { get => ParticleExplorer.Instance; }

        public static GameView GameView { get => GameView.Instance; }

        public static Options Options;

        public static bool ShowHUD;

        public static MainMenu Instance;

        public MainMenu()
        {
            InitializeComponent();
            Instance = this;
        }

        private async void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "DAT files (*.dat)|*.dat|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                var files = openFileDialog.FileNames;
                if (files.Length < 1) return;
                var file = files[0];

                MainWindow.Status.WriteLine("Reading " + file);

                await Task.Run(() => ReadDATFile(file));

                //Particle.ReadFiles();

                //var cellFiles = DatManager.CellDat.AllFiles.Count;
                //var portalFiles = DatManager.PortalDat.AllFiles.Count;

                //MainWindow.Status.WriteLine($"CellFiles={cellFiles}, PortalFiles={portalFiles}");

                MainWindow.Status.WriteLine("Done");

                GameView.PostInit();
            }
        }

        public void ReadDATFile(string filename)
        {
            var fi = new System.IO.FileInfo(filename);
            var di = fi.Directory;

            var loadCell = true;
            DatManager.Initialize(di.FullName, true, loadCell);
        }

        private void Options_Click(object sender, RoutedEventArgs e)
        {
            Options = new Options();
            Options.ShowDialog();
        }

        private void WorldMap_Click(object sender, RoutedEventArgs e)
        {
            if (DatManager.CellDat == null || DatManager.PortalDat == null)
                return;

            GameView.ViewMode = ViewMode.Map;
        }

        private void ShowHUD_Click(object sender, RoutedEventArgs e)
        {
            ToggleHUD();
        }

        public static bool ToggleHUD()
        {
            ShowHUD = !ShowHUD;
            Instance.optionShowHUD.IsChecked = ShowHUD;

            return ShowHUD;
        }

        private void ShowLocation_Click(object sender, RoutedEventArgs e)
        {
            if (WorldViewer.Instance != null)
                WorldViewer.Instance.ShowLocation();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            var about = new About();
            about.ShowDialog();
        }

        private void Guide_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(@"docs\index.html");
        }
    }
}
