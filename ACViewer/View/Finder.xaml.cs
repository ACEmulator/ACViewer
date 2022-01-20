using System;
using System.Linq;
using System.Globalization;
using System.Windows;

using ACE.DatLoader;

using ACViewer.Enum;

namespace ACViewer.View
{
    /// <summary>
    /// Interaction logic for Finder.xaml
    /// </summary>
    public partial class Finder : Window
    {
        public Finder()
        {
            InitializeComponent();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            Navigate(DID.Text);

            Close();
        }

        public static bool Navigate(string didStr)
        {
            if (didStr.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                didStr = didStr.Substring(2);

            if (!uint.TryParse(didStr, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var did))
            {
                // input invalid -- throw error?
                Console.WriteLine($"Invalid DID format: {did:X8}");
                return false;
            }

            uint filetype = 0;
            var datType = DatType.Undef;

            if (DatManager.PortalDat == null)
            {
                Console.WriteLine($"Please load the DATs before using finder");
                return false;
            }

            // try lookup in portal.dat
            if (DatManager.PortalDat.AllFiles.TryGetValue(did, out var portalFile))
            {
                datType = DatType.Portal;
                //Console.WriteLine($"Found {did:X8} in portal");
                filetype = did >> 24;
                if (filetype == 0xE)
                    filetype = did;
            }
            // try lookup in cell.dat
            else if (DatManager.CellDat.AllFiles.TryGetValue(did, out var cellFile))
            {
                datType = DatType.Cell;
                //Console.WriteLine($"Found {did:X8} in cell");
                if ((ushort)did == 0xFFFF)
                    filetype = 0xFFFF;
                else if ((ushort)did == 0xFFFE)
                    filetype = 0xFFFE;
                else
                    filetype = 0x100;   // there is a slight overlap of ~600 EnvCell IDs that are also in portal
            }
            // try lookup in language.dat
            else if (DatManager.LanguageDat.AllFiles.TryGetValue(did, out var languageFile))
            {
                datType = DatType.Language;
                //Console.WriteLine($"Found {did:X8} in language");
                filetype = did >> 24;
            }
            else if (DatManager.HighResDat.AllFiles.TryGetValue(did, out var highResFile))
            {
                datType = DatType.HighRes;
                //Console.WriteLine($"Found {did:X8} in highres");
                filetype = did >> 24;
            }
            else
            {
                Console.WriteLine($"Couldn't find {did:X8} in DATs");
                return false;
            }

            var fileTypeSelect = FileExplorer.FileTypes.FirstOrDefault(i => i.ID == filetype);

            if (fileTypeSelect == null)
            {
                Console.WriteLine($"Unknown filetype {did:X8} found in {datType} Dat");
                return false;
            }

            var items = FileExplorer.Instance.FileType.Items;

            foreach (var item in items)
            {
                if (item is Entity.FileType entityFileType && entityFileType.ID == filetype)
                {
                    FileExplorer.Instance.FileType.SelectedItem = item;
                    didStr = did.ToString("X8");
                    foreach (var file in FileExplorer.Instance.Files.Items)
                    {
                        if (file.ToString().Equals(didStr))
                        {
                            FileExplorer.Instance.Files.SelectedItem = file;
                            FileExplorer.Instance.Files.ScrollIntoView(file);
                            return true;
                        }
                    }
                    break;
                }
            }
            Console.WriteLine($"Error selecting file");
            return false;
        }
    }
}
