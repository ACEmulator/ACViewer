using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using ACE.Entity;
using ACE.Server.Entity;

using ACViewer.Data;

namespace ACViewer.View
{
    /// <summary>
    /// Interaction logic for Teleloc.xaml
    /// </summary>
    public partial class Teleport : Window
    {
        public static Vector3 Origin { get; set; }
        public static Quaternion Orientation { get; set; }

        private static readonly string Filename = @"Data\Locations.txt";

        private static List<TeleportRow> TeleportRows { get; set; }

        private List<TeleportRow> Filtered { get; set; }

        private static string PrevSearch { get; set; }

        private static bool PrevCheckbox_Town { get; set; } = true;
        private static bool PrevCheckbox_Dungeon { get; set; } = true;
        private static bool PrevCheckbox_POI { get; set; } = true;

        private bool InitComplete { get; set; }

        public Teleport()
        {
            InitializeComponent();

            this.Owner = App.Current.MainWindow;

            DataContext = this;

            Filtered = TeleportRows;

            Search.Text = PrevSearch;

            if (!PrevCheckbox_Town)
                Towns_Checkbox.IsChecked = false;

            if (!PrevCheckbox_Dungeon)
                Dungeons_Checkbox.IsChecked = false;

            if (!PrevCheckbox_POI)
                POIs_Checkbox.IsChecked = false;

            ApplyFilters();
            UpdateSummary();

            InitComplete = true;
        }

        static Teleport()
        {
            ReadFile();
        }

        private static void ReadFile()
        {
            TeleportRows = new List<TeleportRow>();
            
            var lines = File.ReadAllLines(Filename);

            var sep = new string[] { " | " };
            
            foreach (var line in lines)
            {
                var pieces = line.Split(sep, StringSplitOptions.None);

                if (pieces.Length < 3)
                {
                    Console.WriteLine($"Teleport.ReadFile() - failed to parse {line}");
                    continue;
                }

                TeleportRows.Add(new TeleportRow(pieces[0], pieces[1], pieces[2]));
            }
        }

        private void Row_Click(object sender, MouseButtonEventArgs e)
        {
            var dataGridRow = sender as DataGridRow;

            if (dataGridRow == null) return;

            var teleportRow = dataGridRow.Item as TeleportRow;

            if (teleportRow == null) return;

            Location.Text = teleportRow.Location;
        }

        private void Row_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            var dataGridRow = sender as DataGridRow;

            if (dataGridRow == null) return;

            var teleportRow = dataGridRow.Item as TeleportRow;

            if (teleportRow == null) return;

            teleloc(teleportRow.Location);

            Close();
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!InitComplete) return;
            
            ApplyFilters();
            
            PrevSearch = Search.Text;
        }

        private void CheckBoxChanged(object sender, RoutedEventArgs e)
        {
            if (!InitComplete) return;

            ApplyFilters();

            PrevCheckbox_Town = Towns_Checkbox.IsChecked ?? true;
            PrevCheckbox_Dungeon = Dungeons_Checkbox.IsChecked ?? true;
            PrevCheckbox_POI = POIs_Checkbox.IsChecked ?? true;
        }

        private void ApplyFilters()
        {
            var search = Search.Text/*.Trim()*/;

            if (search.Length > 0 || Towns_Checkbox.IsChecked == false || Dungeons_Checkbox.IsChecked == false || POIs_Checkbox.IsChecked == false)
            {
                Filtered = new List<TeleportRow>();

                foreach (var teleportRow in TeleportRows)
                {
                    switch (teleportRow.LocationType)
                    {
                        case TeleportLocationType.Town:
                            if (Towns_Checkbox.IsChecked == false)
                                continue;
                            break;

                        case TeleportLocationType.Dungeon:
                            if (Dungeons_Checkbox.IsChecked == false)
                                continue;
                            break;

                        case TeleportLocationType.POI:
                            if (POIs_Checkbox.IsChecked == false)
                                continue;
                            break;

                    }

                    if (!teleportRow.Contains(search))
                        continue;

                    Filtered.Add(teleportRow);
                }
            }
            else
                Filtered = TeleportRows;

            TeleportDests.ItemsSource = Filtered;

            UpdateSummary();
        }

        private void UpdateSummary()
        {
            var suffix = Filtered.Count != 1 ? "s" : "";
            
            Summary.Content = $"Found {Filtered.Count:N0} location{suffix}";
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            teleloc(Location.Text);

            Close();
        }

        public static bool teleloc(string locStr)
        {
            var match = Regex.Match(locStr, @"([0-9A-F]{8}) \[?([0-9.-]+) ([0-9.-]+) ([0-9.-]+)\]? ([0-9.-]+) ([0-9.-]+) ([0-9.-]+) ([0-9.-]+)", RegexOptions.IgnoreCase);

            if (!match.Success)
                return teleloc_radar(locStr);

            var objCellID = uint.Parse(match.Groups[1].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture);

            Origin = new Vector3(float.Parse(match.Groups[2].Value), float.Parse(match.Groups[3].Value), float.Parse(match.Groups[4].Value));
            Orientation = new Quaternion(float.Parse(match.Groups[6].Value), float.Parse(match.Groups[7].Value), float.Parse(match.Groups[8].Value), float.Parse(match.Groups[5].Value));

            return teleport(objCellID);
        }

        public static bool teleport(uint objCellID)
        {
            uint filetype = 0xFFFF;

            var items = FileExplorer.Instance.FileType.Items;

            foreach (var item in items)
            {
                if (item is Entity.FileType entityFileType && entityFileType.ID == filetype)
                {
                    if (FileExplorer.Instance.FileType.SelectedItem == item && FileExplorer.Instance.Files.Items.Count == 0)
                        FileExplorer.Instance.FileType.SelectedItem = null;

                    FileExplorer.Instance.FileType.SelectedItem = item;

                    var didStr = objCellID.ToString("X8").Substring(0, 4) + "FFFF";

                    foreach (var file in FileExplorer.Instance.Files.Items)
                    {
                        if (file.ToString().Equals(didStr))
                        {
                            FileExplorer.Instance.TeleportMode = true;

                            if (FileExplorer.Instance.Files.SelectedItem == file)
                                FileExplorer.Instance.Files.SelectedItem = null;

                            FileExplorer.Instance.Files.SelectedItem = file;
                            FileExplorer.Instance.Files.ScrollIntoView(file);

                            return true;
                        }
                    }
                    return false;
                }
            }
            return false;
        }

        public static bool teleloc_radar(string locStr)
        {
            var match = Regex.Match(locStr, @"([0-9.]+)\s*([NS])\s*,\s*([0-9.]+)\s*([EW])", RegexOptions.IgnoreCase);

            if (!match.Success)
                return false;

            float.TryParse(match.Groups[1].Value, out var latitude);
            float.TryParse(match.Groups[3].Value, out var longitude);

            if (match.Groups[2].Value.Equals("S", StringComparison.InvariantCultureIgnoreCase))
                latitude = -latitude;

            if (match.Groups[4].Value.Equals("W", StringComparison.InvariantCultureIgnoreCase))
                longitude = -longitude;

            var position = new Position(latitude, longitude);
            position.AdjustMapCoords();

            Origin = position.Pos;
            Orientation = Quaternion.Identity;

            return teleport(position.Cell);
        }
    }
}
