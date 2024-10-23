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
        private ModifierKeys _lastModifiers;
        private Window _parentWindow;
        private readonly DispatcherTimer _timer;
        private int _remainingTime;
        private const int TIMEOUT_SECONDS = 3;  // Reduced to 3 seconds
        
        public event Action<Keys, ModifierKeys> OnKeyStateChanged;
        public event Action<GameKeyBinding> RecordingComplete;
        public event Action RecordingCancelled;
        public event Action<string> RecordingError;

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
            _remainingTime--;
            System.Diagnostics.Debug.WriteLine($"Timer tick: {_remainingTime} seconds remaining");
            
            if (_remainingTime <= 0)
            {
                // If only modifiers are held, use them as the key
                if (_lastModifiers != ModifierKeys.None)
                {
                    System.Diagnostics.Debug.WriteLine($"Timeout with modifiers: {_lastModifiers}");
                    var modifierKey = GetModifierAsKey(_lastModifiers);
                    if (modifierKey != Keys.None)
                    {
                        var binding = new GameKeyBinding(modifierKey, ModifierKeys.None);
                        StopRecording();
                        RecordingComplete?.Invoke(binding);
                        return;
                    }
                }

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
                _lastModifiers = ModifierKeys.None;
                
                _parentWindow.PreviewKeyDown += Window_PreviewKeyDown;
                _parentWindow.PreviewKeyUp += Window_PreviewKeyUp;
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
                _parentWindow.PreviewKeyUp -= Window_PreviewKeyUp;
                _parentWindow = null;
            }
        }

        private void Window_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (!_isRecording) return;

            _lastModifiers = Keyboard.Modifiers;
            OnKeyStateChanged?.Invoke(Keys.None, _lastModifiers);
            e.Handled = true;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!_isRecording) return;

            System.Diagnostics.Debug.WriteLine($"Key pressed: {e.Key}, System Key: {e.SystemKey}, Modifiers: {Keyboard.Modifiers}");

            _lastModifiers = Keyboard.Modifiers;

            // Handle escape to cancel
            if (e.Key == Key.Escape)
            {
                e.Handled = true;
                StopRecording();
                RecordingCancelled?.Invoke();
                return;
            }

            // Handle system keys (like Alt+ combinations)
            var actualKey = e.Key == Key.System ? e.SystemKey : e.Key;

            // If it's a modifier key alone, just update the display
            if (IsModifierKey(actualKey))
            {
                OnKeyStateChanged?.Invoke(Keys.None, _lastModifiers);
                e.Handled = true;
                return;
            }

            // Convert WPF key to XNA key
            var xnaKey = ConvertToXNAKey(actualKey);
            if (xnaKey != Keys.None)
            {
                var binding = new GameKeyBinding(xnaKey, _lastModifiers);
                if (ValidateBinding(binding))
                {
                    e.Handled = true;
                    StopRecording();
                    RecordingComplete?.Invoke(binding);
                }
            }

            OnKeyStateChanged?.Invoke(xnaKey, _lastModifiers);
            e.Handled = true;
        }

        private Keys GetModifierAsKey(ModifierKeys modifier)
        {
            // Return the specific modifier key that was pressed
            switch (modifier)
            {
                case ModifierKeys.Shift:
                    return Keyboard.IsKeyDown(Key.LeftShift) ? Keys.LeftShift : Keys.RightShift;
                case ModifierKeys.Control:
                    return Keyboard.IsKeyDown(Key.LeftCtrl) ? Keys.LeftControl : Keys.RightControl;
                case ModifierKeys.Alt:
                    return Keyboard.IsKeyDown(Key.LeftAlt) ? Keys.LeftAlt : Keys.RightAlt;
                default:
                    return Keys.None;
            }
        }

        private bool IsModifierKey(Key key)
        {
            return key == Key.LeftShift || key == Key.RightShift ||
                   key == Key.LeftCtrl || key == Key.RightCtrl ||
                   key == Key.LeftAlt || key == Key.RightAlt;
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

        private Keys ConvertToXNAKey(Key wpfKey)
        {
            // Handle system keys (Alt+ combinations)
            if (wpfKey == Key.System)
                return Keys.None;

            // This is a basic conversion - add more cases as needed
            return wpfKey switch
            {
                Key.A => Keys.A,
                Key.B => Keys.B,
                Key.C => Keys.C,
                Key.D => Keys.D,
                Key.E => Keys.E,
                Key.F => Keys.F,
                Key.G => Keys.G,
                Key.H => Keys.H,
                Key.I => Keys.I,
                Key.J => Keys.J,
                Key.K => Keys.K,
                Key.L => Keys.L,
                Key.M => Keys.M,
                Key.N => Keys.N,
                Key.O => Keys.O,
                Key.P => Keys.P,
                Key.Q => Keys.Q,
                Key.R => Keys.R,
                Key.S => Keys.S,
                Key.T => Keys.T,
                Key.U => Keys.U,
                Key.V => Keys.V,
                Key.W => Keys.W,
                Key.X => Keys.X,
                Key.Y => Keys.Y,
                Key.Z => Keys.Z,
                Key.D0 => Keys.D0,
                Key.D1 => Keys.D1,
                Key.D2 => Keys.D2,
                Key.D3 => Keys.D3,
                Key.D4 => Keys.D4,
                Key.D5 => Keys.D5,
                Key.D6 => Keys.D6,
                Key.D7 => Keys.D7,
                Key.D8 => Keys.D8,
                Key.D9 => Keys.D9,
                Key.Space => Keys.Space,
                Key.Enter => Keys.Enter,
                Key.Tab => Keys.Tab,
                Key.Up => Keys.Up,
                Key.Down => Keys.Down,
                Key.Left => Keys.Left,
                Key.Right => Keys.Right,
                Key.F1 => Keys.F1,
                Key.F2 => Keys.F2,
                Key.F3 => Keys.F3,
                Key.F4 => Keys.F4,
                Key.F5 => Keys.F5,
                Key.F6 => Keys.F6,
                Key.F7 => Keys.F7,
                Key.F8 => Keys.F8,
                Key.F9 => Keys.F9,
                Key.F10 => Keys.F10,
                Key.F11 => Keys.F11,
                Key.F12 => Keys.F12,
                Key.OemPlus => Keys.OemPlus,
                Key.OemMinus => Keys.OemMinus,
                Key.LeftShift => Keys.LeftShift,
                Key.RightShift => Keys.RightShift,
                Key.LeftCtrl => Keys.LeftControl,
                Key.RightCtrl => Keys.RightControl,
                Key.LeftAlt => Keys.LeftAlt,
                Key.RightAlt => Keys.RightAlt,
                _ => Keys.None
            };
        }
    }
}