using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

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
    }
}
