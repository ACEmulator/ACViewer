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
    /// Interaction logic for ScriptList.xaml
    /// </summary>
    public partial class ScriptList : UserControl
    {
        public static ScriptList Instance { get; set; }

        public static MainWindow MainWindow => MainWindow.Instance;
        public static ParticleViewer ParticleViewer => ParticleViewer.Instance;

        public ScriptList()
        {
            InitializeComponent();
            Instance = this;

            DataContext = this;
        }

        public void ScriptTable_OnClick(uint scriptTableID)
        {
            MotionList.Instance.Visibility = Visibility.Hidden;
            Visibility = Visibility.Visible;

            ReadScriptTable(scriptTableID);
        }

        private void Script_OnClick(object sender, MouseButtonEventArgs e)
        {
            var selectedItem = ItemsControl.ContainerFromElement(sender as ListBox, e.OriginalSource as DependencyObject) as ListBoxItem;
            if (selectedItem == null)
                return;

            var pEffectTableID = FileExplorer.Instance.Selected_FileID;
            var playScript = (PlayScript)selectedItem.Content;

            ParticleViewer.InitEmitter(pEffectTableID, playScript, 1.0f);

            MainWindow.Status.WriteLine($"Playing particle effect {playScript}");
        }

        public void ReadScriptTable(uint scriptTableID)
        {
            MainWindow.Status.WriteLine($"Reading physics script table {scriptTableID:X8}");
            var pEffectTable = DatManager.PortalDat.ReadFromDat<PhysicsScriptTable>(scriptTableID);

            MainWindow.Status.WriteLine($" - Found {pEffectTable.ScriptTable.Count} script table entries");
            SetPhysicsScripts(pEffectTable);
        }

        public void SetPhysicsScripts(PhysicsScriptTable table)
        {
            Scripts.Items.Clear();

            foreach (var script in table.ScriptTable.Keys.OrderBy(i => i))
                Scripts.Items.Add((PlayScript)script);
        }
    }
}
