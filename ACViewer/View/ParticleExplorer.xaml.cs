using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using ACE.DatLoader;
using ACE.DatLoader.FileTypes;
using ACE.Entity.Enum;

namespace ACViewer.View
{
    /// <summary>
    /// Interaction logic for ParticleExplorer.xaml
    /// </summary>
    public partial class ParticleExplorer : UserControl
    {
        public static ParticleExplorer Instance { get; set; }

        public static MainWindow MainWindow => MainWindow.Instance;
        
        public static GameView GameView => GameView.Instance;

        public ParticleExplorer()
        {
            InitializeComponent();
            Instance = this;
        }

        private void PETables_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var pEffectTableID = Convert.ToUInt32((string)PETables.SelectedItem, 16);

            ReadPhysicsScripts(pEffectTableID);
        }

        public void ReadPhysicsScripts(uint pEffectTableID)
        {
            MainWindow.Status.WriteLine($"Reading physics effect table {pEffectTableID:X8}");
            var pEffectTable = DatManager.PortalDat.ReadFromDat<PhysicsScriptTable>(pEffectTableID);

            MainWindow.Status.WriteLine($" - Found {pEffectTable.ScriptTable.Count} script table entries");
            SetPhysicsScripts(pEffectTable);
        }

        public void SetPhysicsScripts(PhysicsScriptTable table)
        {
            PETableScripts.Items.Clear();

            foreach (var script in table.ScriptTable.Keys.OrderBy(i => i))
                PETableScripts.Items.Add((PlayScript)script);

            ScriptsLabel.Content = $"{table.Id:X8} Physics Scripts";
        }

        private void Setups_OnClick(object sender, MouseButtonEventArgs e)
        {
            var selectedItem = ItemsControl.ContainerFromElement(sender as ListBox, e.OriginalSource as DependencyObject) as ListBoxItem;
            if (selectedItem == null)
                return;

            var kvp = (string)selectedItem.Content;
            var elements = kvp.Split(new string[] { " => " }, StringSplitOptions.None);
            var scriptID = Convert.ToUInt32(elements[1], 16);

            ParticleViewer.Instance.InitEmitter(scriptID, 1.0f);

            MainWindow.Status.WriteLine($"Playing particle effect {scriptID:X8}");
        }

        private void PETableScripts_OnClick(object sender, MouseButtonEventArgs e)
        {
            var selectedItem = ItemsControl.ContainerFromElement(sender as ListBox, e.OriginalSource as DependencyObject) as ListBoxItem;
            if (selectedItem == null)
                return;

            var pEffectTableID = Convert.ToUInt32((string)PETables.SelectedItem, 16);
            var playScript = (PlayScript)selectedItem.Content;

            ParticleViewer.Instance.InitEmitter(pEffectTableID, playScript, 1.0f);

            MainWindow.Status.WriteLine($"Playing particle effect {playScript}");
        }

        private void Scripts_OnClick(object sender, MouseButtonEventArgs e)
        {
            var selectedItem = ItemsControl.ContainerFromElement(sender as ListBox, e.OriginalSource as DependencyObject) as ListBoxItem;
            if (selectedItem == null)
                return;

            var scriptID = Convert.ToUInt32((string)selectedItem.Content, 16);

            ParticleViewer.Instance.InitEmitter(scriptID, 1.0f);

            MainWindow.Status.WriteLine($"Playing particle effect {scriptID:X8}");
        }

        private void Emitters_OnClick(object sender, MouseButtonEventArgs e)
        {
            var selectedItem = ItemsControl.ContainerFromElement(sender as ListBox, e.OriginalSource as DependencyObject) as ListBoxItem;
            if (selectedItem == null)
                return;

            var emitterInfoID = Convert.ToUInt32((string)selectedItem.Content, 16);

            ParticleViewer.Instance.InitEmitter(emitterInfoID, 1.0f);

            MainWindow.Status.WriteLine($"Playing particle effect {emitterInfoID:X8}");
        }
    }
}
