using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ACViewer.Extensions
{
    public class ColorDialogEx : ColorDialog
    {
        #region private const
        //Windows Message Constants
        private const int WM_INITDIALOG   = 0x0110;
        private const int WM_CTLCOLOREDIT = 0x0133;
        private const int WM_CTLCOLORSTATIC = 0x0138;

        //Window Controls
        private const int COLOR_RED = 706;
        private const int COLOR_GREEN = 707;
        private const int COLOR_BLUE = 708;

        //uFlag Constants
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_SHOWWINDOW = 0x0040;
        private const uint SWP_NOZORDER = 0x0004;
        private const uint UFLAGS = SWP_NOSIZE | SWP_NOZORDER | SWP_SHOWWINDOW;
        #endregion

        #region private readonly
        //Windows Handle Constants
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        private static readonly IntPtr HWND_TOP = new IntPtr(0);
        private static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
        #endregion

        #region private vars
        //Module vars
        private int _x;
        private int _y;
        private string _title = null;
        #endregion

        #region private static methods imports
        //WinAPI definitions

        /// <summary>
        /// Sets the window text.
        /// </summary>
        /// <param name="hWnd">The h WND.</param>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool SetWindowText(IntPtr hWnd, string text);

        /// <summary>
        /// Sets the window pos.
        /// </summary>
        /// <param name="hWnd">The h WND.</param>
        /// <param name="hWndInsertAfter">The h WND insert after.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="cx">The cx.</param>
        /// <param name="cy">The cy.</param>
        /// <param name="uFlags">The u flags.</param>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDlgItem(IntPtr hDlg, int nIDDlgItem);

        [DllImport("user32.dll")]
        private static extern int GetDlgItemInt(IntPtr hDlg, int nIDDlgItem, IntPtr lpTranslated, bool bSigned);
        #endregion

        #region public constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="ColorDialogEx"/> class.
        /// </summary>
        /// <param name="x">The X position</param>
        /// <param name="y">The Y position</param>
        /// <param name="title">The title of the windows. If set to null(by default), the title will not be changed</param>
        public ColorDialogEx(int x, int y, String title = null)
        {
            _x = x;
            _y = y;
            _title = title;
        }
        #endregion

        #region protected override methods
        /// <summary>
        /// Defines the common dialog box hook procedure that is overridden to add specific functionality to a common dialog box.
        /// </summary>
        /// <param name="hWnd">The handle to the dialog box window.</param>
        /// <param name="msg">The message being received.</param>
        /// <param name="wparam">Additional information about the message.</param>
        /// <param name="lparam">Additional information about the message.</param>
        /// <returns>
        /// A zero value if the default dialog box procedure processes the message; a nonzero value if the default dialog box procedure ignores the message.
        /// </returns>
        protected override IntPtr HookProc(IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam)
        {
            //We do the base initialization
            IntPtr hookProc = base.HookProc(hWnd, msg, wparam, lparam);
            //When we init the dialog
            if (msg == WM_INITDIALOG)
            {
                //We change the title
                if (!string.IsNullOrEmpty(_title))
                {
                    SetWindowText(hWnd, _title);
                }
                //We move the position
                SetWindowPos(hWnd, HWND_TOP, _x, _y, 0, 0, UFLAGS);
            }
            else if (msg == WM_CTLCOLOREDIT)
            {
                if (ColorEditCallback != null)
                {
                    var r = GetDlgItemInt(hWnd, COLOR_RED, IntPtr.Zero, false);
                    var g = GetDlgItemInt(hWnd, COLOR_GREEN, IntPtr.Zero, false);
                    var b = GetDlgItemInt(hWnd, COLOR_BLUE, IntPtr.Zero, false);
                    ColorEditCallback(r, g, b);
                }
            }
            return hookProc;
        }
        #endregion

        public Action<int, int, int> ColorEditCallback { get; set; }
    }
}
