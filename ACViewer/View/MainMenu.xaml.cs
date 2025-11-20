using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

using Microsoft.Win32;

using ACE.DatLoader;
using ACE.DatLoader.Entity;
using ACE.DatLoader.FileTypes;

using ACViewer.Config;
using ACViewer.Data;
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

        public static bool LoadInstances { get; set; }

        public static bool LoadEncounters { get; set; }

        public MainMenu()
        {
            InitializeComponent();
            Instance = this;
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "DAT files (*.dat)|*.dat|All files (*.*)|*.*";

            var success = openFileDialog.ShowDialog();

            if (success != true) return;

            var filenames = openFileDialog.FileNames;
            
            if (filenames.Length < 1) return;
            
            var filename = filenames[0];

            LoadDATs(filename);
        }

        public void LoadDATs(string filename)
        {
            if (!File.Exists(filename) && !Directory.Exists(filename)) return;
            
            MainWindow.Status.WriteLine("Reading " + filename);

            var worker = new BackgroundWorker();

            worker.DoWork += (sender, doWorkEventArgs) =>
            {
                ReadDATFile(filename);
            };

            worker.RunWorkerCompleted += (sender, runWorkerCompletedEventArgs) =>
            {
                /*var cellFiles = DatManager.CellDat.AllFiles.Count;
                var portalFiles = DatManager.PortalDat.AllFiles.Count;

                MainWindow.Status.WriteLine($"CellFiles={cellFiles}, PortalFiles={portalFiles}");*/
                MainWindow.Status.WriteLine(runWorkerCompletedEventArgs.Error?.Message ?? "Done");
                    

                if (DatManager.CellDat == null || DatManager.PortalDat == null) return;

                GameView.PostInit();
            };
            
            worker.RunWorkerAsync();
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            // get currently selected file from FileExplorer
            var selectedFileID = FileExplorer.Instance.Selected_FileID;

            if (selectedFileID == 0)
            {
                MainWindow.Instance.AddStatusText($"You must first select a file to export");
                return;
            }

            var saveFileDialog = new SaveFileDialog();

            var fileType = selectedFileID >> 24;
            var isModel = fileType == 0x1 || fileType == 0x2;
            var isImage = fileType == 0x5 || fileType == 0x6 || fileType == 08;
            var isSound = fileType == 0xA;

            if (isModel)
            {
                saveFileDialog.Filter = "OBJ files (*.obj)|*.obj|FBX files (*.fbx)|*.fbx|DAE files (*.dae)|*.dae|RAW files (*.raw)|*.raw";
                saveFileDialog.FileName = $"{selectedFileID:X8}.obj";
            }
            else if (isImage)
            {
                saveFileDialog.Filter = "PNG files (*.png)|*.png|RAW files (*.raw)|*.raw";
                saveFileDialog.FileName = $"{selectedFileID:X8}.png";
            }
            else if (isSound)
            {
                var sound = DatManager.PortalDat.ReadFromDat<Wave>(selectedFileID);

                if (sound.Header[0] == 0x55)
                {
                    saveFileDialog.Filter = "MP3 files (*.mp3)|*.mp3|RAW files (*.raw)|*.raw";
                    saveFileDialog.FileName = $"{selectedFileID:X8}.mp3";
                }
                else
                {
                    saveFileDialog.Filter = "WAV files (*.wav)|*.wav|RAW files (*.raw)|*.raw";
                    saveFileDialog.FileName = $"{selectedFileID:X8}.wav";
                }
            }
            else
            {
                saveFileDialog.Filter = "RAW files (*.raw)|*.raw";
                saveFileDialog.FileName = $"{selectedFileID:X8}.raw";
            }

            var success = saveFileDialog.ShowDialog();

            if (success != true) return;

            var saveFilename = saveFileDialog.FileName;

            if (isModel && saveFileDialog.FilterIndex == 1)
                FileExport.ExportModel(selectedFileID, saveFilename);
            else if (isModel && saveFileDialog.FilterIndex > 1)
            {
                // try to get animation id, if applicable
                var rawState = ModelViewer.Instance?.ViewObject?.PhysicsObj?.MovementManager?.MotionInterpreter?.RawState;

                MotionData motionData = null;

                if (rawState != null)
                {
                    var didTable = DIDTables.Get(selectedFileID);   // setup ID

                    if (didTable != null)
                    {
                        motionData = ACE.Server.Physics.Animation.MotionTable.GetMotionData(didTable.MotionTableID, rawState.ForwardCommand, rawState.CurrentStyle) ??
                            ACE.Server.Physics.Animation.MotionTable.GetLinkData(didTable.MotionTableID, rawState.ForwardCommand, rawState.CurrentStyle);
                    }
                }

                //FileExport.ExportModel_Aspose(selectedFileID, motionData, saveFilename);
                FileExport.ExportModel_Assimp(selectedFileID, motionData, saveFilename);
            }
            else if (isImage && saveFileDialog.FilterIndex == 1)
                FileExport.ExportImage(selectedFileID, saveFilename);
            else if (isSound && saveFileDialog.FilterIndex == 1)
                FileExport.ExportSound(selectedFileID, saveFilename);
            else
                FileExport.ExportRaw(DatType.Portal, selectedFileID, saveFilename);
        }

        public static void ReadDATFile(string filename)
        {
            var fi = new System.IO.FileInfo(filename);
            var di = fi.Attributes.HasFlag(FileAttributes.Directory) ? new DirectoryInfo(filename) : fi.Directory;

            var loadCell = true;

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            DatManager.Initialize(di.FullName, true, loadCell);
        }

        private void Options_Click(object sender, RoutedEventArgs e)
        {
            Options = new Options();
            Options.WindowStartupLocation = WindowStartupLocation.CenterScreen;
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

        public static bool ToggleHUD(bool updateConfig = true)
        {
            ShowHUD = !ShowHUD;
            Instance.optionShowHUD.IsChecked = ShowHUD;

            if (updateConfig)
            {
                ConfigManager.Config.Toggles.ShowHUD = ShowHUD;
                ConfigManager.SaveConfig();
            }

            return ShowHUD;
        }

        public static bool ToggleParticles(bool updateConfig = true)
        {
            ShowParticles = !ShowParticles;
            Instance.optionShowParticles.IsChecked = ShowParticles;

            if (updateConfig)
            {
                ConfigManager.Config.Toggles.ShowParticles = ShowParticles;
                ConfigManager.SaveConfig();
            }

            if (GameView.ViewMode == ViewMode.World)
            {
                if (ShowParticles && !GameView.Render.ParticlesInitted)
                    GameView.Render.InitEmitters();

                if (!ShowParticles && GameView.Render.ParticlesInitted)
                    GameView.Render.DestroyEmitters();
            }
            return ShowHUD;
        }

        public static bool ToggleMipMaps(bool updateConfig = true)
        {
            UseMipMaps = !UseMipMaps;
            Instance.optionUseMipMaps.IsChecked = UseMipMaps;

            if (updateConfig)
            {
                ConfigManager.Config.Toggles.UseMipMaps = UseMipMaps;
                ConfigManager.SaveConfig();
            }

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
            about.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            about.ShowDialog();
        }

        private void Guide_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("cmd", @"/c docs\index.html");
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

        private void LoadInstances_Click(object sender, RoutedEventArgs e)
        {
            ToggleInstances();
        }

        public static void ToggleInstances(bool updateConfig = true)
        {
            if (Server.Initting) return;

            LoadInstances = !LoadInstances;
            Instance.optionLoadInstances.IsChecked = LoadInstances;

            if (updateConfig)
            {
                ConfigManager.Config.Toggles.LoadInstances = LoadInstances;
                ConfigManager.SaveConfig();
            }

            if (GameView.ViewMode != ViewMode.World) return;

            Server.ClearInstances();

            if (!LoadInstances)
            {
                if (ShowParticles)
                {
                    // todo: optimize
                    GameView.Instance.Render.DestroyEmitters();
                    GameView.Instance.Render.InitEmitters();
                }
                return;
            }

            var worker = new BackgroundWorker();

            worker.DoWork += (sender, doWorkEventArgs) => Server.LoadInstances();

            worker.RunWorkerCompleted += (sender, runWorkerCompletedEventArgs) => Server.LoadInstances_Finalize();

            worker.RunWorkerAsync();
        }

        private void LoadEncounters_Click(object sender, RoutedEventArgs e)
        {
            ToggleEncounters();
        }

        public static void ToggleEncounters(bool updateConfig = true)
        {
            if (Server.Initting) return;

            LoadEncounters = !LoadEncounters;
            Instance.optionLoadEncounters.IsChecked = LoadEncounters;

            if (updateConfig)
            {
                ConfigManager.Config.Toggles.LoadEncounters = LoadEncounters;
                ConfigManager.SaveConfig();
            }

            if (GameView.ViewMode != ViewMode.World) return;

            Server.ClearEncounters();

            if (!LoadEncounters)
            {
                if (ShowParticles)
                {
                    // todo: optimize
                    GameView.Instance.Render.DestroyEmitters();
                    GameView.Instance.Render.InitEmitters();
                }
                return;
            }

            var worker = new BackgroundWorker();

            worker.DoWork += (sender, doWorkEventArgs) => Server.LoadEncounters();

            worker.RunWorkerCompleted += (sender, runWorkerCompletedEventArgs) => Server.LoadEncounters_Finalize();

            worker.RunWorkerAsync();
        }

        private void miVirindiColorTool_Click(object sender, RoutedEventArgs e)
        {
            var vct = new VirindiColorTool();
            vct.ShowDialog();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var armorWindow = new ArmorList();
            armorWindow.ShowDialog();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (FileExplorer.Instance?.History == null) return;

            var prevDID = FileExplorer.Instance.History.Back();

            if (prevDID == null) return;

            // Navigate without adding to history (we're already in history)
            FileExplorer.Instance.SuppressHistory = true;
            Finder.Navigate(prevDID.Value.ToString("X8"));
            FileExplorer.Instance.SuppressHistory = false;

            UpdateNavigationButtons();
        }

        private void Forward_Click(object sender, RoutedEventArgs e)
        {
            if (FileExplorer.Instance?.History == null) return;

            var nextDID = FileExplorer.Instance.History.Forward();

            if (nextDID == null) return;

            // Navigate without adding to history (we're already in history)
            FileExplorer.Instance.SuppressHistory = true;
            Finder.Navigate(nextDID.Value.ToString("X8"));
            FileExplorer.Instance.SuppressHistory = false;

            UpdateNavigationButtons();
        }

        public void UpdateNavigationButtons()
        {
            var history = FileExplorer.Instance?.History;
            if (history == null || menuBack == null || menuForward == null) return;

            menuBack.IsEnabled = history.CanGoBack();
            menuForward.IsEnabled = history.CanGoForward();
        }

        private void BackContext_Opened(object sender, RoutedEventArgs e)
        {
            var contextMenu = sender as ContextMenu;
            if (contextMenu == null) return;

            contextMenu.Items.Clear();

            var history = FileExplorer.Instance?.History;
            if (history == null) return;

            var backList = history.GetBackList();
            for (int i = 0; i < backList.Count; i++)
            {
                var did = backList[i];
                var menuItem = new MenuItem
                {
                    Header = GetFileTypeLabel(did),
                    Tag = -(i + 1) // Negative offset for going back
                };
                menuItem.Click += HistoryItem_Click;
                contextMenu.Items.Add(menuItem);
            }
        }

        private void ForwardContext_Opened(object sender, RoutedEventArgs e)
        {
            var contextMenu = sender as ContextMenu;
            if (contextMenu == null) return;

            contextMenu.Items.Clear();

            var history = FileExplorer.Instance?.History;
            if (history == null) return;

            var forwardList = history.GetForwardList();
            for (int i = 0; i < forwardList.Count; i++)
            {
                var did = forwardList[i];
                var menuItem = new MenuItem
                {
                    Header = GetFileTypeLabel(did),
                    Tag = i + 1 // Positive offset for going forward
                };
                menuItem.Click += HistoryItem_Click;
                contextMenu.Items.Add(menuItem);
            }
        }

        private void HistoryItem_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem?.Tag == null) return;

            var offset = (int)menuItem.Tag;
            var history = FileExplorer.Instance?.History;
            if (history == null) return;

            var targetDID = history.NavigateByOffset(offset);
            if (targetDID == null) return;

            // Navigate without adding to history
            FileExplorer.Instance.SuppressHistory = true;
            Finder.Navigate(targetDID.Value.ToString("X8"));
            FileExplorer.Instance.SuppressHistory = false;

            UpdateNavigationButtons();
        }

        private string GetFileTypeLabel(uint did)
        {
            var fileTypeID = did >> 24;
            var fileTypeName = "Unknown";

            // Check FileExplorer's FileTypes list
            if (FileExplorer.FileTypes != null)
            {
                // First try exact match (for special IDs like CharGen)
                var exactMatch = FileExplorer.FileTypes.FirstOrDefault(ft => ft.ID == did);
                if (exactMatch != null)
                {
                    fileTypeName = exactMatch.Name;
                }
                else
                {
                    // Then try by high byte
                    var typeMatch = FileExplorer.FileTypes.FirstOrDefault(ft => ft.ID == fileTypeID);
                    if (typeMatch != null)
                    {
                        fileTypeName = typeMatch.Name;
                    }
                    else
                    {
                        // Special cases for Cell and Landblock
                        if ((did & 0xFFFF) == 0xFFFF)
                            fileTypeName = "Landblock";
                        else if ((did & 0xFFFF) == 0xFFFE)
                            fileTypeName = "LandblockInfo";
                        else if (fileTypeID == 0)
                            fileTypeName = "EnvCell";
                    }
                }
            }

            return $"{fileTypeName} - 0x{did:X8}";
        }
    }
}
