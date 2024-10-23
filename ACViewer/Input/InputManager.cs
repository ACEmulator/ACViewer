using System;
using System.Windows.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using ACViewer.Config;
using ACViewer.Entity;
using GameTime = Microsoft.Xna.Framework.GameTime;
using Keyboard = Microsoft.Xna.Framework.Input.Keyboard;


namespace ACViewer.Input
{
    public class InputManager
    {
        private KeyBindingConfig _config;
        private KeyboardState _currentState;
        private KeyboardState _previousState;
        private ModifierKeys _currentModifiers;
        
        public event Action<GameKeyBinding> BindingPressed;

        public InputManager(KeyBindingConfig config)
        {
            _config = config;
            _currentState = Keyboard.GetState();
            _previousState = _currentState;
        }

        public void Update(GameTime gameTime)
        {
            _previousState = _currentState;
            _currentState = Keyboard.GetState();
            UpdateModifiers();
            
            CheckBinding(_config.MoveForward);
            CheckBinding(_config.MoveBackward);
            CheckBinding(_config.StrafeLeft);
            CheckBinding(_config.StrafeRight);
            CheckBinding(_config.MoveUp);
            CheckBinding(_config.MoveDown);
            CheckBinding(_config.ToggleZLevel);
            CheckBinding(_config.IncreaseZLevel);
            CheckBinding(_config.DecreaseZLevel);

            foreach (var binding in _config.CustomBindings.Values)
                CheckBinding(binding);
        }

        private void CheckBinding(GameKeyBinding binding)
        {
            if (binding.Matches(_currentState, _currentModifiers))
                BindingPressed?.Invoke(binding);
        }

        private void UpdateModifiers()
        {
            _currentModifiers = ModifierKeys.None;
            
            if (_currentState.IsKeyDown(Keys.LeftShift) || _currentState.IsKeyDown(Keys.RightShift))
                _currentModifiers |= ModifierKeys.Shift;
                
            if (_currentState.IsKeyDown(Keys.LeftControl) || _currentState.IsKeyDown(Keys.RightControl))
                _currentModifiers |= ModifierKeys.Control;
                
            if (_currentState.IsKeyDown(Keys.LeftAlt) || _currentState.IsKeyDown(Keys.RightAlt))
                _currentModifiers |= ModifierKeys.Alt;
        }

        public bool IsBindingActive(GameKeyBinding binding)
        {
            return binding.Matches(_currentState, _currentModifiers);
        }

        public void UpdateBindings(KeyBindingConfig newConfig)
        {
            _config = newConfig;
        }

        public ModifierKeys GetCurrentModifiers() => _currentModifiers;
    }
}