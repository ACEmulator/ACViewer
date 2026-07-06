using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace ACViewer.View
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            // last-resort safety net -- without this, any unhandled exception anywhere in the
            // app (on any thread) can silently take down the whole process with no indication why
            DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            LogFatal(e.Exception);

            // this one CAN be kept alive -- it's on the UI thread, so marking it handled
            // prevents the crash instead of just logging it on the way down.
            // Fully qualified: "MainWindow" here would otherwise resolve to Application.MainWindow
            if (ACViewer.View.MainWindow.Instance != null)
                ACViewer.View.MainWindow.Instance.Status.WriteLine($"Unhandled error: {e.Exception.Message} (see error.log)");

            e.Handled = true;
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // exceptions on non-UI threads can't be stopped from here (the process will still
            // terminate) -- this exists purely so the failure is logged instead of the process
            // just vanishing with no explanation. Note: don't touch WPF UI elements here --
            // this may not be firing on the UI thread
            LogFatal(e.ExceptionObject as Exception);
        }

        private static void LogFatal(Exception ex)
        {
            try
            {
                File.AppendAllText("error.log", $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {ex}\n\n");
            }
            catch
            {
                // if we can't even write the log, there's nothing more we can safely do here
            }
        }
    }
}
