using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using ACE.DatLoader;
using ACE.DatLoader.Entity;
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

        public static ClothingTable CurrentClothingItem { get; private set; }
        public static uint PaletteTemplate { get; private set; }
        public static float Shade { get; private set; }
        public static uint Icon { get; private set; }

        public ClothingTableList()
        {
            InitializeComponent();
            Instance = this;

            DataContext = this;
        }

        public void OnClickClothingBase(ClothingTable clothing, uint fileID, uint? paletteTemplate = null, float? shade = null)
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

            // Add 0 / Undefined PaletteTemplate. This will display the item with no PaletteTemplate/Shade. See 0x100002CE
            PaletteTemplates.Items.Add(new ListBoxItem { Content = "None", DataContext = (uint)0 });

            foreach (var subPal in CurrentClothingItem.ClothingSubPalEffects.Keys.OrderBy(i => i))
            {
                // Set the DataContext so we can more easily reference it...
                ListBoxItem palTem = new ListBoxItem();
                palTem.Content = (PaletteTemplate)subPal + " - " + subPal;
                palTem.DataContext = subPal;
                PaletteTemplates.Items.Add(palTem);
            }

            SetupIds.SelectedIndex = 0;

            if (paletteTemplate == null)
            {
                PaletteTemplates.SelectedIndex = 0;
            }
            else
            {
                // SELECT OUR PALETTE TEMPLATE
                string pal = (PaletteTemplate)paletteTemplate + " - " + paletteTemplate;
                for (var i = 0; i < PaletteTemplates.Items.Count; i++)
                {
                    ListBoxItem palItem = PaletteTemplates.Items[i] as ListBoxItem;
                    if (palItem.Content.ToString() == pal)
                    {
                        PaletteTemplates.SelectedItem = PaletteTemplates.Items[i];
                        PaletteTemplates.ScrollIntoView(PaletteTemplates.SelectedItem);
                        break;
                    }
                }
            }

            if (shade.HasValue && shade > 0 && Shades.Visibility == Visibility.Visible)
            {
                int palIndex = (int)((Shades.Maximum - 0.000001) * shade);
                Shades.Value = palIndex;
            }
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
            if (palTemp > 0)
            {
                //uint palTemp = 0;
                if (CurrentClothingItem.ClothingSubPalEffects.ContainsKey(palTemp) == false)
                    return;

                // Set this as a reference for the Color Tool
                PaletteTemplate = palTemp;

                int maxPals = 0;
                for (var i = 0; i < CurrentClothingItem.ClothingSubPalEffects[palTemp].CloSubPalettes.Count; i++)
                {
                    var palSetID = CurrentClothingItem.ClothingSubPalEffects[palTemp].CloSubPalettes[i].PaletteSet;
                    var clothing = DatManager.PortalDat.ReadFromDat<PaletteSet>(palSetID);
                    if (clothing.PaletteList.Count > maxPals)
                        maxPals = clothing.PaletteList.Count;
                }

                if (maxPals > 1)
                {
                    Shades.Maximum = maxPals - 1;
                    Shades.Visibility = Visibility.Visible;
                    Shades.IsEnabled = true;
                    MainWindow.Status.WriteLine($"Reading PaletteSets and found {maxPals} Shade options");
                }
            }
            LoadModelWithClothingBase();
        }

        /// <summary>
        /// Helper function to reset the Shades slider
        /// </summary>
        private void ResetShadesSlider()
        {
            Shades.Visibility = Visibility.Hidden;
            Shades.IsEnabled = false;
            Shades.Value = 0;
            Shades.Maximum = 1;

            // Set this as a reference for the Color Tool
            Shade = 0;
        }

        public void LoadModelWithClothingBase()
        {
            if (CurrentClothingItem == null) return;

            if (SetupIds.SelectedIndex == -1) return;

            if (PaletteTemplates.SelectedIndex == -1) return;

            float shade = 0;

            if (Shades.Visibility == Visibility.Visible)
            {
                shade = (float)(Shades.Value / Shades.Maximum);
                if (float.IsNaN(shade))
                    shade = 0;
            }

            Shade = shade;

            lblShade.Visibility = Shades.Visibility;
            lblShade.Content = "Shade: " + shade.ToString();

            var selectedSetup = SetupIds.SelectedItem as ListBoxItem;
            var setupId = (uint)selectedSetup.DataContext;

            var selectedPalette = PaletteTemplates.SelectedItem as ListBoxItem;
            var paletteTemplate = (PaletteTemplate)(uint)selectedPalette.DataContext;

            ModelViewer.LoadModel(setupId, CurrentClothingItem, paletteTemplate, shade);
        }

        private void Shades_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Shades.Visibility == Visibility.Hidden)
                return;

            LoadModelWithClothingBase();
        }

        public class VctInfo
        {
            public uint PalId;
            public uint Color;
        }

        public static List<VctInfo> GetVirindiColorToolInfo()
        {
            List<VctInfo> result = new List<VctInfo>();

            if (CurrentClothingItem == null) return result;

            // If there are no Palette Templates, there's nothing to provide...
            if (CurrentClothingItem.ClothingSubPalEffects.Count == 0) return result;

            // Make sure there is a selected Palette Template to load and it's valid
            if (!CurrentClothingItem.ClothingSubPalEffects.ContainsKey(PaletteTemplate)) return result;

            if (float.IsNaN(Shade))
                Shade = 0;

            Icon = CurrentClothingItem.GetIcon(PaletteTemplate);

            var palEffects = CurrentClothingItem.ClothingSubPalEffects[PaletteTemplate];

            for (var i = 0; i < palEffects.CloSubPalettes.Count; i++)
            {
                CloSubPalette subPal = palEffects.CloSubPalettes[i];

                var palSet = DatManager.PortalDat.ReadFromDat<PaletteSet>(subPal.PaletteSet);
                var paletteID = palSet.GetPaletteID(Shade);
                var palette = DatManager.PortalDat.ReadFromDat<Palette>(paletteID);
                foreach (var r in subPal.Ranges)
                {

                    uint mid = Convert.ToUInt32(r.NumColors / 2);
                    uint colorIdx = r.Offset + mid;

                    uint color = 0;
                    if (palette.Colors.Count >= colorIdx)
                    {
                        color = palette.Colors[(int)colorIdx];
                    }

                    VctInfo vctInfo = new VctInfo();
                    vctInfo.PalId = paletteID & 0xFFFF;
                    vctInfo.Color = color & 0xFFFFFF;
                    result.Add(vctInfo);
                }
            }

            return result;
        }

        public static uint GetIcon()
        {
            return CurrentClothingItem.GetIcon(PaletteTemplate);
        }
    }
}
