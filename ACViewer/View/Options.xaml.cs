using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Microsoft.Win32;

using ACE.Database.Models.World;

using ACViewer.Extensions;

namespace ACViewer.View
{
    /// <summary>
    /// Interaction logic for Options.xaml
    /// </summary>
    public partial class Options : Window, INotifyPropertyChanged
    {
        public static Config.Config Config => ACViewer.Config.ConfigManager.Config;

        public string ACFolder
        {
            get => Config.ACFolder;
            set
            {
                Config.ACFolder = value;
                NotifyPropertyChanged("ACFolder");
            }
        }

        public bool AutomaticallyLoadDATsOnStartup
        {
            get => Config.AutomaticallyLoadDATsOnStartup;
            set
            {
                Config.AutomaticallyLoadDATsOnStartup = value;
                NotifyPropertyChanged("AutomaticallyLoadACFolderOnStartup");
            }
        }

        public string Host
        {
            get => Config.Database.Host;
            set
            {
                Config.Database.Host = value;
                NotifyPropertyChanged("Host");
            }
        }

        public int Port
        {
            get => Config.Database.Port;
            set
            {
                Config.Database.Port = value;
                NotifyPropertyChanged("Port");
            }
        }

        public string Database
        {
            get => Config.Database.DatabaseName;
            set
            {
                Config.Database.DatabaseName = value;
                NotifyPropertyChanged("Database");
            }
        }

        public string Username
        {
            get => Config.Database.Username;
            set
            {
                Config.Database.Username = value;
                NotifyPropertyChanged("Username");
            }
        }

        public string Password
        {
            get => Config.Database.Password;
            set
            {
                Config.Database.Password = value;
                NotifyPropertyChanged("Password");
            }
        }

        public float MouseSpeed
        {
            get => Config.Mouse.Speed;
            set
            {
                Config.Mouse.Speed = value;
                NotifyPropertyChanged("MouseSpeed");
            }
        }

        public bool AltMouselook
        {
            get => Config.Mouse.AltMethod;
            set
            {
                Config.Mouse.AltMethod = value;
                NotifyPropertyChanged("AltMouselook");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private static readonly List<string> allProperties = new List<string>()
        {
            "ACFolder",
            "AutomaticallyLoadACFolderOnStartup",
            "Host",
            "Port",
            "Database",
            "Username",
            "Password",
            "ModelViewer_BackgroundColor",
            "TextureViewer_BackgroundColor",
            "ParticleViewer_BackgroundColor",
            "WorldViewer_BackgroundColor",
            "MouseSpeed",
            "AltMouselook",
        };

        public static Options Instance { get; set; }

        public bool Initting { get; set; }

        public Options()
        {
            Initting = true;

            InitializeComponent();

            DataContext = this;

            ACViewer.Config.ConfigManager.TakeSnapshot();

            Instance = this;

            SliderMouseSpeed.Value = MouseSpeed;

            Initting = false;
        }

        private void SelectACFolderButton_Click(object sender, RoutedEventArgs e)
        {
            //var folderBrowserDialog = new FolderBrowserDialog();
            //folderBrowserDialog.ShowDialog();

            var openFileDialog = new OpenFileDialog();

            var success = openFileDialog.ShowDialog();

            if (success != true) return;

            var fi = new System.IO.FileInfo(openFileDialog.FileName);

            ACFolder = fi.DirectoryName;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            ACViewer.Config.ConfigManager.SaveConfig();
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            ACViewer.Config.ConfigManager.RestoreSnapshot();

            foreach (var propName in allProperties)
                NotifyPropertyChanged(propName);

            Close();
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool DatabaseConnected { get; set; }

        private void TestConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            DBStatus.Source = null;
            Spinner.Visibility = Visibility.Visible;

            var worker = new BackgroundWorker();

            worker.DoWork += (sender, doWorkEventArgs) =>
            {
                Server.SetDatabaseConfig();

                DatabaseConnected = false;

                using (var ctx = new WorldDbContext())
                {
                    try
                    {
                        var query = ctx.Weenie.FirstOrDefault();
                        DatabaseConnected = true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            };

            worker.RunWorkerCompleted += (sender, runWorkerCompletedEventArgs) =>
            {
                Spinner.Visibility = Visibility.Hidden;

                if (DatabaseConnected)
                    DBStatus.Source = new BitmapImage(new Uri(@"/ACViewer;component/Icons/Checkmark_16x.png", UriKind.Relative));
                else
                    DBStatus.Source = new BitmapImage(new Uri(@"/ACViewer;component/Icons/StatusCriticalError_16x.png", UriKind.Relative));
            };

            worker.RunWorkerAsync();
        }

        public SolidColorBrush ModelViewer_BackgroundColor
        {
            get => Config.BackgroundColors.ModelViewer.ToSolidColorBrush();
            set
            {
                Config.BackgroundColors.ModelViewer = value.ToXNAColor();
                NotifyPropertyChanged("ModelViewer_BackgroundColor");
            }
        }

        public SolidColorBrush TextureViewer_BackgroundColor
        {
            get => Config.BackgroundColors.TextureViewer.ToSolidColorBrush();
            set
            {
                Config.BackgroundColors.TextureViewer = value.ToXNAColor();
                NotifyPropertyChanged("TextureViewer_BackgroundColor");
            }
        }

        public SolidColorBrush ParticleViewer_BackgroundColor
        {
            get => Config.BackgroundColors.ParticleViewer.ToSolidColorBrush();
            set
            {
                Config.BackgroundColors.ParticleViewer = value.ToXNAColor();
                NotifyPropertyChanged("ParticleViewer_BackgroundColor");
            }
        }

        public SolidColorBrush WorldViewer_BackgroundColor
        {
            get => Config.BackgroundColors.WorldViewer.ToSolidColorBrush();
            set
            {
                Config.BackgroundColors.WorldViewer = value.ToXNAColor();
                NotifyPropertyChanged("WorldViewer_BackgroundColor");
            }
        }

        private static readonly SolidColorBrush borderColor = new SolidColorBrush(Color.FromArgb(255, 102, 102, 102));

        public static SolidColorBrush BorderColor => borderColor;

        public ICommand Swatch_Click => new CommandHandler(OnSwatchClick);

        public static int[] CustomColors { get; set; }

        public void OnSwatchClick(object obj)
        {
            if (obj == null) return;

            CurrentParam = obj.ToString();

            var brush = GetBrush(CurrentParam);

            if (brush == null) return;

            var window = Instance;

            //var startX = (int)(window.Left + window.Width / 2 - 224 / 2);
            var startX = (int)(window.Left + window.Width / 2 - 449 / 2);
            var startY = (int)(window.Top + window.Height / 2 - 331 / 2);

            ColorPicker = new ColorDialogEx(startX, startY);
            ColorPicker.Color = brush.ToColor();
            ColorPicker.FullOpen = true;
            ColorPicker.ColorEditCallback = ColorEditCallback;

            //colorPicker.CustomColors = new int[1] { colorPicker.Color.A << 24 | colorPicker.Color.R << 16 | colorPicker.Color.G << 8 | colorPicker.Color.B };
            ColorPicker.CustomColors = CustomColors;

            var result = ColorPicker.ShowDialog();

            //Console.WriteLine(colorPicker.Color);

            if (result != System.Windows.Forms.DialogResult.OK)
            {
                SetBrush(CurrentParam, brush);
                return;
            }

            CustomColors = ColorPicker.CustomColors;

            SetBrush(CurrentParam, ColorPicker.Color.ToSolidColorBrush());
        }

        public ColorDialogEx ColorPicker { get; set; }

        public string CurrentParam { get; set; }

        public void ColorEditCallback(int r, int g, int b)
        {
            var brush = new SolidColorBrush(Color.FromArgb(255, (byte)r, (byte)g, (byte)b));
            //Console.WriteLine($"ColorEditCallback: {brush.Color}");
            SetBrush(CurrentParam, brush);
        }

        public SolidColorBrush GetBrush(string param)
        {
            switch (param)
            {
                case "ModelViewer":
                    return ModelViewer_BackgroundColor;

                case "TextureViewer":
                    return TextureViewer_BackgroundColor;

                case "ParticleViewer":
                    return ParticleViewer_BackgroundColor;

                case "WorldViewer":
                    return WorldViewer_BackgroundColor;
            }
            return null;
        }

        public void SetBrush(string param, SolidColorBrush brush)
        {
            switch (param)
            {
                case "ModelViewer":
                    ModelViewer_BackgroundColor = brush;
                    break;

                case "TextureViewer":
                    TextureViewer_BackgroundColor = brush;
                    break;

                case "ParticleViewer":
                    ParticleViewer_BackgroundColor = brush;
                    break;

                case "WorldViewer":
                    WorldViewer_BackgroundColor = brush;
                    break;
            }
        }

        private void SliderMouseSpeed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Initting) return;

            Initting = true;
            
            MouseSpeed = (float)Math.Round(e.NewValue, 1, MidpointRounding.ToEven);

            Initting = false;
        }

        private void TextBoxMouseSpeed_ValueChanged(object sender, TextChangedEventArgs args)
        {
            if (Initting) return;

            if (!float.TryParse(TextBoxMouseSpeed.Text, out var speed)) return;

            Initting = true;

            SliderMouseSpeed.Value = speed;

            Initting = false;
        }
    }
}
