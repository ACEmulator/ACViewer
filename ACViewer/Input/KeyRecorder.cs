using System;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows;
using Microsoft.Xna.Framework.Input;
using ACViewer.Entity;
using Keyboard = System.Windows.Input.Keyboard;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace ACViewer.Input
{
    public class KeyRecorder
    {
        private bool _isRecording;
        private ModifierKeys _currentModifiers;
        private Window _parentWindow;
        
        public event Action<Keys, ModifierKeys> OnKeyStateChanged;
        public event Action<GameKeyBinding> RecordingComplete;
        public event Action RecordingCancelled;
        public event Action<string> RecordingError;

        private readonly DispatcherTimer _timer;
        private int _remainingTime;
        private const int TIMEOUT_SECONDS = 3;

        public KeyRecorder()
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!_isRecording) return;

            _remainingTime--;
            System.Diagnostics.Debug.WriteLine($"Timer tick: {_remainingTime} seconds remaining");

            if (_remainingTime <= 0)
            {
                StopRecording();
                RecordingCancelled?.Invoke();
            }
        }

        public void StartRecording(Window parentWindow)
        {
            try
            {
                if (_isRecording) return;

                System.Diagnostics.Debug.WriteLine("Starting key recording...");
                _isRecording = true;
                _parentWindow = parentWindow;
                _remainingTime = TIMEOUT_SECONDS;
                _currentModifiers = ModifierKeys.None;

                _parentWindow.PreviewKeyDown += Window_PreviewKeyDown;
                _timer.Start();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error starting recording: {ex}");
                RecordingError?.Invoke($"Error starting recording: {ex.Message}");
            }
        }

        public void StopRecording()
        {
            if (!_isRecording) return;
            
            System.Diagnostics.Debug.WriteLine("Stopping key recording...");
            _isRecording = false;
            _timer.Stop();
            
            if (_parentWindow != null)
            {
                _parentWindow.PreviewKeyDown -= Window_PreviewKeyDown;
                _parentWindow = null;
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!_isRecording) return;

            System.Diagnostics.Debug.WriteLine($"Raw Key pressed: {e.Key}, System Key: {e.SystemKey}, Modifiers: {Keyboard.Modifiers}, Key Value: {(int)e.Key}");

            _currentModifiers = Keyboard.Modifiers;
            
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                e.Handled = true;
                StopRecording();
                RecordingCancelled?.Invoke();
                return;
            }
            
            var actualKey = e.Key == System.Windows.Input.Key.System ? e.SystemKey : e.Key;
            
            if (IsModifierKey(actualKey))
            {
                OnKeyStateChanged?.Invoke(Keys.None, _currentModifiers);
                e.Handled = true;
                return;
            }
            
            var xnaKey = ConvertToXNAKey(actualKey);
            System.Diagnostics.Debug.WriteLine($"Converted to XNA Key: {xnaKey}");

            if (xnaKey != Keys.None)
            {
                var binding = new GameKeyBinding(xnaKey, _currentModifiers);
                System.Diagnostics.Debug.WriteLine($"Created binding - Key: {binding.MainKey}, Modifiers: {binding.Modifiers}");
                
                if (ValidateBinding(binding))
                {
                    e.Handled = true;
                    StopRecording();
                    RecordingComplete?.Invoke(binding);
                }
            }

            OnKeyStateChanged?.Invoke(xnaKey, _currentModifiers);
            e.Handled = true;
        }

        private Keys ConvertToXNAKey(System.Windows.Input.Key wpfKey)
        {
            System.Diagnostics.Debug.WriteLine($"Converting WPF Key: {wpfKey}");
    
            // Direct function key mapping
            if (wpfKey >= System.Windows.Input.Key.F1 && wpfKey <= System.Windows.Input.Key.F24)
            {
                var fKeyNumber = (int)wpfKey - (int)System.Windows.Input.Key.F1 + 1;
                var xnaKey = (Keys)((int)Keys.F1 + (fKeyNumber - 1));
                System.Diagnostics.Debug.WriteLine($"Mapped F-key {fKeyNumber} to XNA Key: {xnaKey} (value: {(int)xnaKey})");
                return xnaKey;
            }

            return wpfKey switch
            {
                System.Windows.Input.Key.A => Keys.A,
                System.Windows.Input.Key.B => Keys.B,
                System.Windows.Input.Key.C => Keys.C,
                System.Windows.Input.Key.D => Keys.D,
                System.Windows.Input.Key.E => Keys.E,
                System.Windows.Input.Key.F => Keys.F,
                System.Windows.Input.Key.G => Keys.G,
                System.Windows.Input.Key.H => Keys.H,
                System.Windows.Input.Key.I => Keys.I,
                System.Windows.Input.Key.J => Keys.J,
                System.Windows.Input.Key.K => Keys.K,
                System.Windows.Input.Key.L => Keys.L,
                System.Windows.Input.Key.M => Keys.M,
                System.Windows.Input.Key.N => Keys.N,
                System.Windows.Input.Key.O => Keys.O,
                System.Windows.Input.Key.P => Keys.P,
                System.Windows.Input.Key.Q => Keys.Q,
                System.Windows.Input.Key.R => Keys.R,
                System.Windows.Input.Key.S => Keys.S,
                System.Windows.Input.Key.T => Keys.T,
                System.Windows.Input.Key.U => Keys.U,
                System.Windows.Input.Key.V => Keys.V,
                System.Windows.Input.Key.W => Keys.W,
                System.Windows.Input.Key.X => Keys.X,
                System.Windows.Input.Key.Y => Keys.Y,
                System.Windows.Input.Key.Z => Keys.Z,
                System.Windows.Input.Key.D0 => Keys.D0,
                System.Windows.Input.Key.D1 => Keys.D1,
                System.Windows.Input.Key.D2 => Keys.D2,
                System.Windows.Input.Key.D3 => Keys.D3,
                System.Windows.Input.Key.D4 => Keys.D4,
                System.Windows.Input.Key.D5 => Keys.D5,
                System.Windows.Input.Key.D6 => Keys.D6,
                System.Windows.Input.Key.D7 => Keys.D7,
                System.Windows.Input.Key.D8 => Keys.D8,
                System.Windows.Input.Key.D9 => Keys.D9,
                System.Windows.Input.Key.Space => Keys.Space,
                System.Windows.Input.Key.Enter => Keys.Enter,
                System.Windows.Input.Key.Tab => Keys.Tab,
                System.Windows.Input.Key.Up => Keys.Up,
                System.Windows.Input.Key.Down => Keys.Down,
                System.Windows.Input.Key.Left => Keys.Left,
                System.Windows.Input.Key.Right => Keys.Right,
                System.Windows.Input.Key.OemPlus => Keys.OemPlus,
                System.Windows.Input.Key.OemMinus => Keys.OemMinus,
                System.Windows.Input.Key.LeftShift => Keys.LeftShift,
                System.Windows.Input.Key.RightShift => Keys.RightShift,
                System.Windows.Input.Key.LeftCtrl => Keys.LeftControl,
                System.Windows.Input.Key.RightCtrl => Keys.RightControl,
                System.Windows.Input.Key.LeftAlt => Keys.LeftAlt,
                System.Windows.Input.Key.RightAlt => Keys.RightAlt,
                _ => Keys.None
            };
        }

        private bool IsModifierKey(System.Windows.Input.Key key)
        {
            return key == System.Windows.Input.Key.LeftShift || key == System.Windows.Input.Key.RightShift ||
                   key == System.Windows.Input.Key.LeftCtrl || key == System.Windows.Input.Key.RightCtrl ||
                   key == System.Windows.Input.Key.LeftAlt || key == System.Windows.Input.Key.RightAlt;
        }

        private bool ValidateBinding(GameKeyBinding binding)
        {
            if (binding.Modifiers.HasFlag(ModifierKeys.Alt) && binding.MainKey == Keys.F4)
            {
                RecordingError?.Invoke("Alt+F4 is reserved by the system");
                return false;
            }

            return true;
        }
    }
}