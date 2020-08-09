using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ACViewer
{
    public static class TextBoxExtensions
    {
        public static void WriteLine(this TextBox textBox, string line)
        {
            if (textBox.Text.Length != 0)
                textBox.AppendText("\n");

            textBox.AppendText(line);
            textBox.ScrollToEnd();
        }
    }
}
