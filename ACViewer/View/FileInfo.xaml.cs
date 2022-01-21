using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using ACViewer.Entity;

namespace ACViewer.View
{
    /// <summary>
    /// Interaction logic for FileInfo.xaml
    /// </summary>
    public partial class FileInfo : UserControl, INotifyPropertyChanged
    {
        public static FileInfo Instance { get; set; }

        private List<TreeNode> _info;

        public List<TreeNode> Info
        {
            get => _info;
            set
            {
                _info = value;
                NotifyPropertyChanged("Info");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public FileInfo()
        {
            InitializeComponent();
            Instance = this;

            DataContext = this;
        }

        public void SetInfo(TreeNode treeNode)
        {
            Info = new List<TreeNode>() { treeNode };
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void TreeViewItem_Selected(object sender, RoutedEventArgs e)
        {
            var source = e.Source as ContentPresenter;

            if (source == null) return;

            var item = source.Content as TreeNode;

            if (item == null || !item.Clickable) return;

            var matches = Regex.Matches(item.Name, @"([0-9A-F]{8})");

            if (matches.Count == 0)
            {
                Console.WriteLine($"Couldn't find DID in {item.Name}");
                return;
            }

            var match = matches[matches.Count - 1];

            var didStr = match.Groups[1].Value;

            if (item.Name.Contains("ObjCellID") && uint.TryParse(didStr, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var objCellID) && (objCellID & 0xFFFF) < 0x100)
            {
                didStr = (objCellID | 0xFFFF).ToString("X8");
            }

            Finder.Navigate(didStr);
        }

        private void TreeViewItem_MouseEnter(object sender, RoutedEventArgs e)
        {
            var source = e.Source as TreeViewItem;

            if (source == null) return;

            var item = source.DataContext as TreeNode;

            if (item == null || !item.Clickable) return;

            Mouse.OverrideCursor = Cursors.Hand;

            source.FontWeight = FontWeights.Bold;
        }

        private void TreeViewItem_MouseLeave(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = null;

            var source = e.Source as TreeViewItem;

            if (source == null) return;

            source.FontWeight = FontWeights.Normal;
        }
    }
}
