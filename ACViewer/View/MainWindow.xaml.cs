using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using ACViewer.Config;

namespace ACViewer.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Instance { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            Instance = this;
            DataContext = this;

            //WpfGame.UseASingleSharedGraphicsDevice = true;

            LoadConfig();
        }

        private static Config.Config Config => ConfigManager.Config;
        
        private static void LoadConfig()
        {
            ConfigManager.LoadConfig();

            if (Config.AutomaticallyLoadDATsOnStartup)
            {
                MainMenu.Instance.LoadDATs(Config.ACFolder);
            }

            if (ConfigManager.HasDBInfo)
                Server.TryPrimeDatabase();

            if (ConfigManager.Config.Toggles.UseMipMaps)
                MainMenu.ToggleMipMaps(false);

            if (ConfigManager.Config.Toggles.ShowHUD)
                MainMenu.ToggleHUD(false);

            if (ConfigManager.Config.Toggles.ShowParticles)
                MainMenu.ToggleParticles(false);

            if (ConfigManager.Config.Toggles.LoadInstances)
                MainMenu.ToggleInstances(false);

            if (ConfigManager.Config.Toggles.LoadEncounters)
                MainMenu.ToggleEncounters(false);
        }

        private DateTime lastUpdateTime { get; set; }

        private static readonly TimeSpan maxUpdateInterval = TimeSpan.FromMilliseconds(1000);

        private readonly List<string> statusLines = new List<string>();

        private static readonly int maxLines = 100;

        private bool pendingUpdate { get; set; }

        public bool SuppressStatusText { get; set; }

        public async void AddStatusText(string line)
        {
            if (SuppressStatusText) return;
            
            statusLines.Add(line);

            var timeSinceLastUpdate = DateTime.Now - lastUpdateTime;

            if (timeSinceLastUpdate < maxUpdateInterval)
            {
                if (pendingUpdate)
                    return;

                pendingUpdate = true;
                await Task.Delay((int)maxUpdateInterval.TotalMilliseconds);
                pendingUpdate = false;
            }

            if (statusLines.Count > maxLines)
                statusLines.RemoveRange(0, statusLines.Count - maxLines);

            Status.Text = string.Join("\n", statusLines);
            Status.ScrollToEnd();

            lastUpdateTime = DateTime.Now;
        }

        public ICommand FindCommand { get; } = new ActionCommand(() =>
        {
            var finder = new Finder();
            finder.ShowDialog();
        });

        public ICommand TeleportCommand { get; } = new ActionCommand(() =>
        {
            var teleport = new Teleport();
            teleport.ShowDialog();
        });

        public ICommand HistoryCommand { get; } = new ActionCommand(() =>
        {
            var prevDID = FileExplorer.Instance.History.Pop();

            if (prevDID == null) return;

            Finder.Navigate(prevDID.Value.ToString("X8"));
        });

        public static bool DebugMode { get; set; }
        
        public ICommand DebugCommand { get; } = new ActionCommand(() =>
        {
            DebugMode = !DebugMode;

            Console.WriteLine($"Debug mode {(DebugMode ? "enabled" : "disabled")}");
        });
    }
}
