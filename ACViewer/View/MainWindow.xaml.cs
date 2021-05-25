using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using MonoGame.Framework.WpfInterop;

namespace ACViewer.View
{
    /// <summary>
    /// Interaction logic for Particle.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public GameView GameView { get => GameView.Instance; }

        public static MainWindow Instance;

        public static ParticleExplorer Particle { get => ParticleExplorer.Instance; }

        public MainWindow()
        {
            InitializeComponent();
            Instance = this;

            //WpfGame.UseASingleSharedGraphicsDevice = true;
        }

        private DateTime lastUpdateTime;

        private static readonly TimeSpan maxUpdateInterval = TimeSpan.FromMilliseconds(1000);

        private readonly List<string> statusLines = new List<string>();

        private static readonly int maxLines = 100;

        private bool pendingUpdate;

        public async void AddStatusText(string line)
        {
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
    }
}
