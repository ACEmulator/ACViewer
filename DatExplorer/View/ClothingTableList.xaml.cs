using ACE.DatLoader;
using ACE.DatLoader.FileTypes;
using ACE.Entity.Enum;
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
using System.Windows.Shapes;

namespace DatExplorer.View
{
    /// <summary>
    /// Interaction logic for ClothingTableList.xaml
    /// </summary>
    public partial class ClothingTableList : UserControl
    {
        public static MainWindow MainWindow { get => MainWindow.Instance; }
        public static ModelViewer ModelViewer { get => ModelViewer.Instance; }

        public static FileTypes.MotionTable MotionTable;

        public static ClothingTableList Instance;

        private static ClothingTable CurrentClothingItem;

        public ClothingTableList()
        {
            InitializeComponent();
            Instance = this;

            //BuildMotionCommands();

            DataContext = this;
        }

        public void OnClickClothingBase(ClothingTable clothing, uint fileID)
        {
            CurrentClothingItem = clothing;
            SetupIds.Items.Clear();
            PaletteTemplates.Items.Clear();
            ResetShadesSlider();

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
                //SetupIds.Items.Add(cbe.ToString("X8"));
            }

            // If no SubPalEffects, we are done adding items. Select the first setup.
            if (CurrentClothingItem.ClothingSubPalEffects.Count == 0)
            {
                return;
            }

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
                var clothing = DatManager.PortalDat.ReadFromDat<ACE.DatLoader.FileTypes.PaletteSet>(palSetID);
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

            ModelViewer.LoadModel(setupId, CurrentClothingItem, palTemplate, shade) ;
        }

        private void Shades_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Shades.IsEnabled == false)
                return;

            LoadModelWithClothingBase();
        }

        /*
               public void BuildMotionCommands()
               {
                   MotionStances.Items.Clear();

                   foreach (var motionStance in Enum.GetValues(typeof(MotionStance)))
                       MotionStances.Items.Add(motionStance);

                   MotionCommands.Items.Clear();

                   foreach (var motionCommand in Enum.GetValues(typeof(MotionCommand)))
                       MotionCommands.Items.Add(motionCommand);
               }

               public void SetStances(List<MotionStance> stances)
               {
                   MotionStances.Items.Clear();

                   foreach (var stance in stances.OrderBy(s => s))
                       MotionStances.Items.Add(stance);
               }

               public void SetCommands(List<MotionCommand> motionCommands)
               {
                   MotionCommands.Items.Clear();

                   foreach (var motionCommand in motionCommands.OrderBy(m => m))
                       MotionCommands.Items.Add(motionCommand);
               }

               private void MotionStances_OnClick(object sender, MouseButtonEventArgs e)
               {
                   if (MotionTable == null) return;

                   var selected = ItemsControl.ContainerFromElement(sender as ListBox, e.OriginalSource as DependencyObject) as ListBoxItem;
                   if (selected == null)
                       return;

                   var motionStance = (MotionStance)selected.Content;

                   MainWindow.Status.WriteLine($"Executing stance {motionStance}");

                   var motionCmds = MotionTable.GetMotionCommands(motionStance);
                   SetCommands(motionCmds);

                   ModelViewer.DoStance(motionStance);
               }

               private void MotionCommands_OnClick(object sender, MouseButtonEventArgs e)
               {
                   if (MotionTable == null) return;

                   var selected = ItemsControl.ContainerFromElement(sender as ListBox, e.OriginalSource as DependencyObject) as ListBoxItem;
                   if (selected == null)
                       return;

                   var motionCommand = (MotionCommand)selected.Content;

                   MainWindow.Status.WriteLine($"Playing motion {motionCommand}");

                   ModelViewer.DoMotion(motionCommand);
               }
               */
    }
}
