using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ACViewer.View
{
    /// <summary>
    /// Interaction logic for Particle.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Instance;

        public GameView GameView => GameView.Instance;

        public MainWindow()
        {
            InitializeComponent();
            Instance = this;
            DataContext = this;

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

        public ICommand FindCommand { get; } = new ActionCommand(() =>
        {
            var finder = new Finder();
            finder.ShowDialog();
        });
    }
}
