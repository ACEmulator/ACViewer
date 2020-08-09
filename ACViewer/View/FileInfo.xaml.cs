using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using ACViewer.Entity;

namespace ACViewer.View
{
    /// <summary>
    /// Interaction logic for FileInfo.xaml
    /// </summary>
    public partial class FileInfo : UserControl, INotifyPropertyChanged
    {
        public static FileInfo Instance;

        public List<TreeNode> _info;

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

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
