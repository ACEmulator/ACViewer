using System.Linq;
using System.Windows;
using System.Windows.Controls;

using ACE.DatLoader;
using ACE.DatLoader.FileTypes;
using ACE.Entity.Enum;

namespace ACViewer.View
{
    /// <summary>
    /// Interaction logic for ClothingTableList.xaml
    /// </summary>
    public partial class ClothingTableList : UserControl
    {
        public static ClothingTableList Instance { get; set; }

        public static MainWindow MainWindow => MainWindow.Instance;
        public static ModelViewer ModelViewer => ModelViewer.Instance;

        private static ClothingTable CurrentClothingItem { get; set; }

        public ClothingTableList()
        {
            InitializeComponent();
            Instance = this;

            DataContext = this;
        }

        public void OnClickClothingBase(ClothingTable clothing, uint fileID)
        {
            CurrentClothingItem = clothing;
            SetupIds.Items.Clear();
            PaletteTemplates.Items.Clear();
            ResetShadesSlider();    // triggers Shades_ValueChanged -> LoadModelWithClothingBase automatically...

            // Nothing to do if there is no ClothingBaseEffects... Does this even exist in the data?
            if (CurrentClothingItem.ClothingBaseEffects.Count == 0)
                return;

            // Add all the Setup IDs in the ClothingTable entry
            foreach (var cbe in CurrentClothingItem.ClothingBaseEffects.Keys.OrderBy(i => i))
            {
                ListBoxItem newSetup = new ListBoxItem();
                newSetup.Content = cbe.ToString("X8");
                newSetup.DataContext = cbe;
                SetupIds.Items.Add(newSetup);
            }

            // If no SubPalEffects, we are done adding items. Select the first setup.
            if (CurrentClothingItem.ClothingSubPalEffects.Count == 0)
                return;

            foreach (var subPal in CurrentClothingItem.ClothingSubPalEffects.Keys.OrderBy(i => i))
            {
                // Set the DataContext so we can more easily reference it...
                ListBoxItem palTem = new ListBoxItem();
                palTem.Content = (PaletteTemplate)subPal + " - " + subPal;
                palTem.DataContext = subPal;
                PaletteTemplates.Items.Add(palTem);
            }

            SetupIds.SelectedIndex = 0;
            PaletteTemplates.SelectedIndex = 0;
        }

        private void SetupIDs_OnClick(object sender, SelectionChangedEventArgs e)
        {
            if (CurrentClothingItem == null) return;

            LoadModelWithClothingBase();
        }

        private void PaletteTemplates_OnClick(object sender, SelectionChangedEventArgs e)
        {
            ResetShadesSlider();

            if (CurrentClothingItem == null) return;

            ListBoxItem selectedItem = PaletteTemplates.SelectedItem as ListBoxItem;
            if (selectedItem == null)
                return;

            uint palTemp = (uint)selectedItem.DataContext;
            //uint palTemp = 0;
            if (CurrentClothingItem.ClothingSubPalEffects.ContainsKey(palTemp) == false)
                return;

            int maxPals = 0;
            for(var i = 0; i < CurrentClothingItem.ClothingSubPalEffects[palTemp].CloSubPalettes.Count; i++)
            {
                var palSetID = CurrentClothingItem.ClothingSubPalEffects[palTemp].CloSubPalettes[i].PaletteSet;
                var clothing = DatManager.PortalDat.ReadFromDat<PaletteSet>(palSetID);
                if (clothing.PaletteList.Count > maxPals)
                    maxPals = clothing.PaletteList.Count;
            }

            if (maxPals > 0)
            {
                Shades.Maximum = maxPals - 1;
                Shades.IsEnabled = true;
                MainWindow.Status.WriteLine($"Reading PaletteSets and found {maxPals} Shade options");
            }

            LoadModelWithClothingBase();
        }

        /// <summary>
        /// Helper function to reset the Shades slider
        /// </summary>
        private void ResetShadesSlider()
        {
            Shades.IsEnabled = false;
            Shades.Value = 0; 
            Shades.Maximum = 1;
        }

        public void LoadModelWithClothingBase()
        {
            if (CurrentClothingItem == null) return;

            if (SetupIds.SelectedIndex == -1) return;
            if (PaletteTemplates.SelectedIndex == -1) return;

            float shade = 0;
            if (Shades.IsEnabled)
            {
                shade = (float)(Shades.Value / Shades.Maximum);
                if (float.IsNaN(shade)) 
                    shade = 0;
            }

            var selectedSetup = SetupIds.SelectedItem as ListBoxItem;
            uint setupId = (uint)selectedSetup.DataContext;

            var selectedPalTemp = PaletteTemplates.SelectedItem as ListBoxItem;
            uint palTemplate = (uint)selectedPalTemp.DataContext;

            ModelViewer.LoadModel(setupId, CurrentClothingItem, palTemplate, shade);
        }

        private void Shades_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Shades.IsEnabled == false)
                return;

            LoadModelWithClothingBase();
        }
    }
}
