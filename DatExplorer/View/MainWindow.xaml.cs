using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using MonoGame.Framework.WpfInterop;

namespace DatExplorer.View
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

    }
}
