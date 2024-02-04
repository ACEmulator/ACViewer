using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using ACViewer.Data;
using ACE.DatLoader;

namespace ACViewer.View
{
    /// <summary>
    /// Interaction logic for ArmorList.xaml
    /// </summary>
    public partial class ArmorList : Window
    {
        public ArmorList()
        {
            InitializeComponent();

            this.Owner = App.Current.MainWindow;

            txtArmor.TextChanged += new TextChangedEventHandler(txtArmor_TextChanged);
            txtArmor.Focus();
        }

        private void txtArmor_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchArmor(txtArmor.Text.Trim());
        }

        private void SearchArmor(string criteria)
        {
            if (criteria != "")
            {
                dgArmorResults.Items.Clear();

                criteria = criteria.ToLower();
                var results = LootArmorList.Loot.Where(x => x.Value.Name.ToLower().Contains(criteria)).OrderBy(x => x.Key);
                foreach (var s in results)
                {
                    dgArmorResults.Items.Add(s.Value);
                }
            }
        }

        // Get the ClothingBase of the item and load it in the window
        // - This should be so much easier, Optim hates WPF
        // Ref https://wpfadventures.wordpress.com/2008/12/02/wpf-datagrid-detecting-clicked-cell-and-row/
        private void DataGridCell_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DatManager.PortalDat == null)
            {
                MessageBox.Show($"Please load the DATs before trying to view item.");
                return;
            }

            DependencyObject dep = (DependencyObject)e.OriginalSource;

            // iteratively traverse the visual tree
            while ((dep != null) &&
                    !(dep is DataGridRow))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }

            if (dep == null)
                return;

            DataGridRow row = dep as DataGridRow;
            // find the object that is related to this row
            object data = row.Item;

            // extract the property value
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(data);
            PropertyDescriptor cbProperty = properties["ClothingBase"];
            string clothingBaseString = cbProperty.GetValue(data).ToString();

            PropertyDescriptor wcidProperty = properties["WCID"];
            uint wcid = Convert.ToUInt32(wcidProperty.GetValue(data));
            var lootItem = LootArmorList.Get(wcid);

            if (!uint.TryParse(lootItem.ClothingBase, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var clothingBase))
            {
                // input invalid -- throw error?
                MessageBox.Show($"Invalid DID format: {lootItem.ClothingBase}");
                return;
            }


            uint filetype = 0;

            if (DatManager.PortalDat.AllFiles.TryGetValue(clothingBase, out var portalFile))
            {
                filetype = clothingBase >> 24;
                var fileTypeSelect = FileExplorer.FileTypes.FirstOrDefault(i => i.ID == filetype);
                if (fileTypeSelect == null)
                {
                    Console.WriteLine($"Unknown filetype {clothingBase:X8} found in Portal Dat");
                    return;
                }

                var items = FileExplorer.Instance.FileType.Items;

                foreach (var item in items)
                {
                    if (item is Entity.FileType entityFileType && entityFileType.ID == filetype)
                    {
                        FileExplorer.Instance.FileType.SelectedItem = item;
                        var didStr = clothingBase.ToString("X8");
                        foreach (var file in FileExplorer.Instance.Files.Items)
                        {
                            if (file.ToString().Equals(didStr))
                            {
                                FileExplorer.Instance.Files.SelectedItem = file;
                                FileExplorer.Instance.Files.ScrollIntoView(file);

                                var clothing = DatManager.PortalDat.ReadFromDat<ACE.DatLoader.FileTypes.ClothingTable>(clothingBase);
                                ClothingTableList.Instance.OnClickClothingBase(clothing, clothingBase, lootItem.PaletteTemplate, lootItem.Shade);
                                this.Close();
                            }
                        }
                        break;
                    }

                }
            }
        }

    }
}
