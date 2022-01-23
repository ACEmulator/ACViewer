using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using Microsoft.Win32;

using ACE.DatLoader;

using ACViewer.Enum;
using ACViewer.Render;

namespace ACViewer.View
{
    /// <summary>
    /// Interaction logic for MainMenu.xaml
    /// </summary>
    public partial class MainMenu : UserControl
    {
        public static MainWindow MainWindow => MainWindow.Instance;

        public static MainMenu Instance { get; set; }

        public static GameView GameView => GameView.Instance;

        public static Options Options { get; set; }

        public static bool ShowHUD { get; set; }

        public static bool ShowParticles { get; set; }

        public static bool UseMipMaps
        {
            get => TextureCache.UseMipMaps;
            set => TextureCache.UseMipMaps = value;
        }

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

            MapViewer.Instance.Init();
        }

        private void ShowHUD_Click(object sender, RoutedEventArgs e)
        {
            ToggleHUD();
        }

        private void ShowParticles_Click(object sender, RoutedEventArgs e)
        {
            ToggleParticles();
        }

        private void UseMipMaps_Click(object sender, RoutedEventArgs e)
        {
            ToggleMipMaps();
        }

        public static bool ToggleHUD()
        {
            ShowHUD = !ShowHUD;
            Instance.optionShowHUD.IsChecked = ShowHUD;

            return ShowHUD;
        }

        public static bool ToggleParticles()
        {
            ShowParticles = !ShowParticles;
            Instance.optionShowParticles.IsChecked = ShowParticles;

            if (ShowParticles && !GameView.Render.ParticlesInitted && GameView.ViewMode == ViewMode.World)
                GameView.Render.InitEmitters();

            return ShowHUD;
        }

        public static bool ToggleMipMaps()
        {
            UseMipMaps = !UseMipMaps;
            Instance.optionUseMipMaps.IsChecked = UseMipMaps;

            return UseMipMaps;
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

        private void FindDID_Click(object sender, RoutedEventArgs e)
        {
            var findDID = new Finder();
            findDID.ShowDialog();
        }

        private void Teleport_Click(object sender, RoutedEventArgs e)
        {
            var teleport = new Teleport();
            teleport.ShowDialog();
        }
    }
}
