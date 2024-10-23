using System;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Xna.Framework.Input;
using ACViewer.Entity;
using ACViewer.Input;
using MessageBox = System.Windows.MessageBox;

namespace ACViewer.View
{
    public partial class KeyBindingDialog : Window
    {
        private readonly DispatcherTimer _timer;
        private int _remainingTime;
        private const int TIMEOUT_SECONDS = 5;
        private readonly KeyRecorder _recorder;
        private bool _isClosing;

        public GameKeyBinding ResultBinding { get; private set; }

        public KeyBindingDialog()
        {
            InitializeComponent();

            _remainingTime = TIMEOUT_SECONDS;
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += Timer_Tick;
        
            _recorder = new KeyRecorder();
            _recorder.OnKeyStateChanged += Recorder_KeyStateChanged;
            _recorder.RecordingComplete += Recorder_RecordingComplete;
            _recorder.RecordingCancelled += Recorder_RecordingCancelled;
            _recorder.RecordingError += Recorder_RecordingError;
        
            Loaded += KeyBindingDialog_Loaded;
            Closing += KeyBindingDialog_Closing;
        }
        
        private void KeyBindingDialog_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateTimeoutText();
            _timer.Start();
            _recorder.StartRecording(this);
        }

        private void KeyBindingDialog_Closing(object sender, CancelEventArgs e)
        {
            _isClosing = true;
            _timer.Stop();
            _recorder.StopRecording();
        }

        private void Recorder_KeyStateChanged(Keys mainKey, ModifierKeys modifiers)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                string displayText;
                if (mainKey == Keys.None)
                {
                    if (modifiers != ModifierKeys.None)
                        displayText = $"{modifiers}+";
                    else
                        displayText = $"Waiting for input... ({_remainingTime}s)";
                }
                else
                {
                    var modifierStr = modifiers != ModifierKeys.None ? $"{modifiers}+" : "";
                    displayText = $"{modifierStr}{mainKey}";
                }
            
                txtCurrentKeys.Text = displayText;
            }));
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _remainingTime--;
            UpdateTimeoutText();

            if (_remainingTime <= 0)
            {
                _timer.Stop();
                SafeClose(false);
            }
        }

        private void UpdateTimeoutText()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                txtCurrentKeys.Text = $"Waiting for input... ({_remainingTime}s)";
            }));
        }

        private void SafeClose(bool? dialogResult)
        {
            if (!_isClosing)
            {
                _isClosing = true;
                DialogResult = dialogResult;
                Close();
            }
        }

        private void Recorder_RecordingComplete(GameKeyBinding binding)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (!_isClosing)
                {
                    ResultBinding = binding;
                    SafeClose(true);
                }
            }));
        }

        private void Recorder_RecordingCancelled()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (!_isClosing)
                {
                    SafeClose(false);
                }
            }));
        }

        private void Recorder_RecordingError(string error)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (!_isClosing)
                {
                    MessageBox.Show(this, error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    SafeClose(false);
                }
            }));
        }
    }
}