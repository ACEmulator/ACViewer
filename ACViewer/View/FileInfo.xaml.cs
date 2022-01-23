using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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

            FileInfo_TreeView.ItemContainerGenerator.StatusChanged += new EventHandler(ItemContainerGenerator_StatusChanged);
        }

        private bool pendingLoad;

        public void SetInfo(TreeNode treeNode)
        {
            pendingLoad = true;
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

        private void TreeView_Expand(object sender, RoutedEventArgs e)
        {
            SetExpanded(true);
        }

        private void TreeView_Collapse(object sender, RoutedEventArgs e)
        {
            pendingLoad = true;
            SetExpanded(false);
        }

        private void SetExpanded(bool expanded)
        {
            var style = new Style();
            style.Setters.Add(new EventSetter(MouseLeftButtonUpEvent, new MouseButtonEventHandler(TreeViewItem_Selected)));
            style.Setters.Add(new EventSetter(MouseEnterEvent, new MouseEventHandler(TreeViewItem_MouseEnter)));
            style.Setters.Add(new EventSetter(MouseLeaveEvent, new MouseEventHandler(TreeViewItem_MouseLeave)));
            style.Setters.Add(new Setter(TreeViewItem.IsExpandedProperty, expanded));

            FileInfo_TreeView.ItemContainerStyle = style;
        }

        private void TreeView_Copy(object sender, RoutedEventArgs e)
        {
            var lines = new List<string>();

            foreach (var node in Info)
                TreeView_CopyAppend(lines, node);

            /*foreach (var line in lines)
                Console.WriteLine(line);*/

            Clipboard.SetText(string.Join(Environment.NewLine, lines));
        }

        private void TreeView_CopyAppend(List<string> lines, TreeNode node, int depth = 0)
        {
            var padding = new string(' ', depth * 4);
            
            lines.Add($"{padding}{node.Name}");

            foreach (var childNode in node.Items)
                TreeView_CopyAppend(lines, childNode, depth + 1);
        }

        private void ScrollTop()
        {
            if (FileInfo_TreeView.Items.Count == 0) return;

            var firstItem = FileInfo_TreeView.Items[0];

            var control = FileInfo_TreeView.ItemContainerGenerator.ContainerFromItem(firstItem) as ItemsControl;

            if (control == null) return;

            control.BringIntoView();
        }

        private void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
        {
            if (!pendingLoad || FileInfo_TreeView.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated) return;

            pendingLoad = false;

            ScrollTop();

            ExpandFirstItem();
        }

        private void ExpandFirstItem()
        {
            if (FileInfo_TreeView.Items.Count == 0) return;

            var firstItem = FileInfo_TreeView.Items[0];

            var control = FileInfo_TreeView.ItemContainerGenerator.ContainerFromItem(firstItem) as ItemsControl;

            if (control == null) return;

            var tvi = control as TreeViewItem;

            if (tvi == null) return;

            tvi.IsExpanded = true;
        }
    }
}
