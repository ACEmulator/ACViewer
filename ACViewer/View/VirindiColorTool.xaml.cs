using System;
using System.Linq;
using System.Windows;
using System.Data;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;

using ACViewer.Data;
using ACE.DatLoader;

namespace ACViewer.View
{
    /// <summary>
    /// Interaction logic for VirindiColorTool.xaml
    /// </summary>
    public partial class VirindiColorTool : Window
    {
        public VirindiColorTool()
        {
            InitializeComponent();

            this.Owner = App.Current.MainWindow;

            var colorInfo = ClothingTableList.GetVirindiColorToolInfo();

            for (var i = 0; i < colorInfo.Count; i++)
            {
                ColorItem item = new ColorItem();
                item.slot = i;
                item.palID = "0x" + colorInfo[i].PalId.ToString("X4");
                item.color = "0x" + colorInfo[i].Color.ToString("X6");

                // Create as a Color to set the cell background property
                var r = (byte)(colorInfo[i].Color >> 16);
                var g = (byte)(colorInfo[i].Color >> 8);
                var b = (byte)(colorInfo[i].Color);
                item.swatchColor = Color.FromRgb(r, g, b);

                dgVCT.Items.Add(item);
            }

            var icon = ClothingTableList.GetIcon();
            if(icon == 0)
            {
                imgIcon.Visibility = Visibility.Hidden;
            }
            else
            {
                imgIcon.Visibility = Visibility.Visible;
                var datIcon = DatManager.PortalDat.ReadFromDat<ACE.DatLoader.FileTypes.Texture>(icon);
                var bitmap = datIcon.GetBitmap();

                using (MemoryStream memory = new MemoryStream())
                {
                    bitmap.Save(memory, ImageFormat.Bmp);
                    memory.Position = 0;
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = memory;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();

                    imgIcon.Source = bitmapImage;
                }
            }

            // Find a matching item with this clothing base. May or may not be a valid loot item...
            try
            {
                var match = LootArmorList.Loot.OrderBy(x => x.Key).First(x => x.Value.ClothingBase == ClothingTableList.CurrentClothingItem.Id.ToString("X8"));
                lblName.Content = match.Value.Name;
            }catch (Exception ex)
            {
                lblName.Content = "";
            }
        }

        public class ColorItem
        {
            public int slot { get; set; }
            public string palID { get; set; }
            public string color { get; set; }
            public Color swatchColor { get; set; }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string lootRule = "ACViewer Color Rule\r\n\r\n0;1";

            var colorInfo = ClothingTableList.GetVirindiColorToolInfo();
            for (var i = 0; i < colorInfo.Count; i++)
            {
                lootRule += ";17";
            }

            for (var i = 0; i < colorInfo.Count; i++)
            {
                lootRule += "\r\n9\r\n" + i.ToString() + "\r\n" + colorInfo[i].PalId.ToString();
            }
            Clipboard.SetText(lootRule);

            MessageBox.Show("The loot rule has been copied to your clipboard.");
        }
    }
}
