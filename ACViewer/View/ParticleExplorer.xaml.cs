using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ACE.DatLoader;
using ACE.DatLoader.Entity.AnimationHooks;
using ACE.DatLoader.FileTypes;
using ACE.Entity.Enum;

namespace ACViewer.View
{
    /// <summary>
    /// Interaction logic for ParticleExplorer.xaml
    /// </summary>
    public partial class ParticleExplorer : UserControl
    {
        public static MainWindow MainWindow { get => MainWindow.Instance; }

        public static GameView GameView { get => GameView.Instance; }

        public static ParticleExplorer Instance;

        public ParticleExplorer()
        {
            InitializeComponent();
            Instance = this;
        }

        public void ReadFiles()
        {
            ReadSetups();
            ReadPhysicsEffectTables();
            ReadScripts();
            ReadEmitterInfos();
        }

        public void ReadSetups()
        {
            var setupIDs = DatManager.PortalDat.AllFiles.Keys.Where(i => i >> 24 == 0x2).ToList();

            var defaultScripts = new Dictionary<uint, uint>();
            var defaultScriptTables = new Dictionary<uint, uint>();

            foreach (var setupID in setupIDs)
            {
                var setup = DatManager.PortalDat.ReadFromDat<SetupModel>(setupID);
                if (setup.DefaultScript != 0)
                    defaultScripts.Add(setupID, setup.DefaultScript);
                if (setup.DefaultScriptTable != 0)
                    defaultScriptTables.Add(setupID, setup.DefaultScriptTable);
            }

            MainWindow.Status.WriteLine($"Found {defaultScripts.Count} setups with default scripts");
            //Status.WriteLine($"Found {defaultScriptTables.Count} setups with default script tables");

            PopulateTable(Setups, defaultScripts);
        }

        public void PopulateTable(ListBox listBox, List<uint> ids)
        {
            listBox.Items.Clear();

            foreach (var id in ids)
                listBox.Items.Add($"{id:X8}");
        }

        public void PopulateTable(ListBox listBox, Dictionary<uint, uint> kvps)
        {
            listBox.Items.Clear();

            foreach (var kvp in kvps)
                listBox.Items.Add($"{kvp.Key:X8} => {kvp.Value:X8}");
        }

        public void ReadPhysicsEffectTables()
        {
            var pEffectTableIDs = DatManager.PortalDat.AllFiles.Keys.Where(i => i >> 24 == 0x34).ToList();
            MainWindow.Status.WriteLine($"Found {pEffectTableIDs.Count} physics effect tables");

            PopulateTable(PETables, pEffectTableIDs);
        }

        public void ReadEmitterInfos()
        {
            var emitterInfoIDs = DatManager.PortalDat.AllFiles.Keys.Where(i => i >> 24 == 0x32).ToList();

            MainWindow.Status.WriteLine($"Found {emitterInfoIDs.Count} particle emitters");

            PopulateTable(Emitters, emitterInfoIDs);
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

        public void ReadScripts()
        {
            var scriptIDs = DatManager.PortalDat.AllFiles.Keys.Where(i => i >> 24 == 0x33).ToList();

            var particleScripts = new List<uint>();

            foreach (var scriptID in scriptIDs)
            {
                var script = DatManager.PortalDat.ReadFromDat<PhysicsScript>(scriptID);

                foreach (var scriptData in script.ScriptData)
                {
                    if (scriptData.Hook is CreateParticleHook)
                    {
                        particleScripts.Add(scriptID);
                        break;
                    }
                }
            }
            MainWindow.Status.WriteLine($"Found {particleScripts.Count} particle scripts");
            PopulateTable(Scripts, particleScripts);
        }

        private void Setups_OnClick(object sender, MouseButtonEventArgs e)
        {
            var selectedItem = ItemsControl.ContainerFromElement(sender as ListBox, e.OriginalSource as DependencyObject) as ListBoxItem;
            if (selectedItem == null)
                return;

            var kvp = (string)selectedItem.Content;
            var elements = kvp.Split(new string[] { " => " }, StringSplitOptions.None);
            var setupID = Convert.ToUInt32(elements[0], 16);
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
