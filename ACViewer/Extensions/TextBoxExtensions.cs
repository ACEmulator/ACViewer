using System.Windows.Controls;

using ACViewer.View;

namespace ACViewer
{
    public static class TextBoxExtensions
    {
        public static void WriteLine(this TextBox textBox, string line)
        {
            MainWindow.Instance.AddStatusText(line);
        }
    }
}
