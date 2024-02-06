using System;
using System.Globalization;
using System.Reflection;
using System.Windows;

namespace ACViewer.View
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Window
    {
        public string RunText => "ACViewer - build " + GetBuildDate(Assembly.GetExecutingAssembly()).ToString("yyyy.MM.dd");
       
        public About()
        {
            InitializeComponent();

            this.Owner = App.Current.MainWindow;

            DataContext = this;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            Close();   
        }

        // https://www.meziantou.net/getting-the-date-of-build-of-a-dotnet-assembly-at-runtime.htm
        private static DateTime GetBuildDate(Assembly assembly)
        {
            const string BuildVersionMetadataPrefix = "+build";

            var attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            if (attribute?.InformationalVersion != null)
            {
                var value = attribute.InformationalVersion;
                var index = value.IndexOf(BuildVersionMetadataPrefix);
                if (index > 0)
                {
                    value = value.Substring(index + BuildVersionMetadataPrefix.Length);
                    if (DateTime.TryParseExact(value, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var result))
                    {
                        return result;
                    }
                }
            }
            return default;
        }
    }
}
