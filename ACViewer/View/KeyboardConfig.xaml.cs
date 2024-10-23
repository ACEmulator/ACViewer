// File: ACViewer/View/KeyboardConfig.xaml.cs
using System;
using System.Text;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using ACViewer.Config;
using ACViewer.Input;
using ACViewer.Entity;
using MessageBox = System.Windows.MessageBox;

namespace ACViewer.View
{
    public partial class KeyboardConfig : Window
    {
        private readonly Config.Config _config;
        private KeyBindingConfig _workingConfig;
        private readonly BindingValidator _validator;
        private List<CategoryViewModel> _allCategories;
        private BindingViewModel _currentlyRecordingBinding;

        public KeyboardConfig(Config.Config config)
        {
            InitializeComponent();
            
            _config = config;
            _workingConfig = new KeyBindingConfig();
            _validator = new BindingValidator();

            // Clone current config
            CopyConfig(_config.KeyBindingConfig, _workingConfig);

            // Initialize UI
            ProfileComboBox.SelectedIndex = 0;
            LoadBindings();
        }

        private void LoadBindings()
        {
            System.Diagnostics.Debug.WriteLine("Current Bindings:");
            System.Diagnostics.Debug.WriteLine($"MoveForward: {_workingConfig.MoveForward?.MainKey}");
            System.Diagnostics.Debug.WriteLine($"ToggleZLevel: {_workingConfig.ToggleZLevel?.MainKey}");
            
            // Group bindings by category
            _allCategories = new List<CategoryViewModel>();

            // Add camera controls
            var cameraBindings = new List<BindingViewModel>
            {
                new BindingViewModel("Move Forward", _workingConfig.MoveForward),
                new BindingViewModel("Move Backward", _workingConfig.MoveBackward),
                new BindingViewModel("Strafe Left", _workingConfig.StrafeLeft),
                new BindingViewModel("Strafe Right", _workingConfig.StrafeRight),
                new BindingViewModel("Move Up", _workingConfig.MoveUp),
                new BindingViewModel("Move Down", _workingConfig.MoveDown)
            };
            _allCategories.Add(new CategoryViewModel("Camera Controls", cameraBindings));

            // Add Z-level controls
            var zLevelBindings = new List<BindingViewModel>
            {
                new BindingViewModel("Toggle Z-Level", _workingConfig.ToggleZLevel),
                new BindingViewModel("Increase Z-Level", _workingConfig.IncreaseZLevel),
                new BindingViewModel("Decrease Z-Level", _workingConfig.DecreaseZLevel)
            };
            _allCategories.Add(new CategoryViewModel("Z-Level Controls", zLevelBindings));

            // Add custom bindings if any
            if (_workingConfig.CustomBindings.Any())
            {
                var customBindings = _workingConfig.CustomBindings
                    .Select(kvp => new BindingViewModel(kvp.Key, kvp.Value))
                    .ToList();
                _allCategories.Add(new CategoryViewModel("Custom", customBindings));
            }

            UpdateBindingList();
        }

        private void UpdateBindingList()
        {
            string searchText = SearchBox.Text?.ToLower() ?? "";

            if (string.IsNullOrWhiteSpace(searchText))
            {
                CategoryList.ItemsSource = _allCategories;
                return;
            }

            // Filter categories and bindings
            var filteredCategories = _allCategories
                .Select(category => new CategoryViewModel(
                    category.Category,
                    category.Bindings.Where(binding => 
                        binding.DisplayName.ToLower().Contains(searchText) ||
                        binding.CurrentBinding.ToLower().Contains(searchText))
                        .ToList()))
                .Where(category => category.Bindings.Any())
                .ToList();

            CategoryList.ItemsSource = filteredCategories;
        }

        private void ClearBinding_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is BindingViewModel binding)
            {
                _workingConfig.SetBindingForAction(binding.DisplayName, new GameKeyBinding());
                binding.SetBinding(new GameKeyBinding());
                LoadBindings();
            }
        }

        private void ProfileComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProfileComboBox.SelectedItem is ComboBoxItem item)
            {
                string profile = item.Content.ToString() switch
                {
                    "Default (WASD)" => "default",
                    "Alternative (ESDF)" => "alternative",
                    "Arrow Keys" => "arrows",
                    _ => "default"
                };

                try
                {
                    _workingConfig = KeyBindingConfig.CreateProfile(profile);
                    LoadBindings();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, $"Error loading profile: {ex.Message}", "Profile Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    ProfileComboBox.SelectedIndex = 0;
                }
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateBindingList();
        }

        private void ImportBindings_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Keybinding files (*.json)|*.json|All files (*.*)|*.*",
                Title = "Import Keybindings"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _workingConfig = KeyBindingConfig.ImportFromFile(dialog.FileName);
                    LoadBindings();
                    MessageBox.Show(this, "Keybindings imported successfully", "Import Complete",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, $"Error importing keybindings: {ex.Message}", "Import Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExportBindings_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Keybinding files (*.json)|*.json|All files (*.*)|*.*",
                Title = "Export Keybindings",
                DefaultExt = ".json"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _workingConfig.ExportToFile(dialog.FileName);
                    MessageBox.Show(this, "Keybindings exported successfully", "Export Complete",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, $"Error exporting keybindings: {ex.Message}", "Export Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ResetAll_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(this, 
                "Are you sure you want to reset all keyboard bindings to their defaults?",
                "Reset All Bindings",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _workingConfig.SetDefaults();
                LoadBindings();
            }
        }
        
        private void OK_Click(object sender, RoutedEventArgs e)
{
    try
    {
        System.Diagnostics.Debug.WriteLine("\n=== Starting Save Process ===");
        
        // 1. First validate our working config has the right values
        System.Diagnostics.Debug.WriteLine($"Current ToggleZLevel in working config: {_workingConfig.ToggleZLevel.MainKey}");
        
        // 2. Check for conflicts
        var conflicts = new List<string>();
        var bindingMap = new Dictionary<string, string>();
        
        void CheckConflict(string name, GameKeyBinding binding)
        {
            if (binding == null || binding.IsEmpty) return;
            
            var key = $"{binding.MainKey}|{binding.Modifiers}";
            if (bindingMap.ContainsKey(key))
            {
                conflicts.Add($"'{name}' conflicts with '{bindingMap[key]}'");
            }
            else
            {
                bindingMap[key] = name;
            }
        }

        // Check all bindings for conflicts
        CheckConflict("Move Forward", _workingConfig.MoveForward);
        CheckConflict("Move Backward", _workingConfig.MoveBackward);
        CheckConflict("Strafe Left", _workingConfig.StrafeLeft);
        CheckConflict("Strafe Right", _workingConfig.StrafeRight);
        CheckConflict("Move Up", _workingConfig.MoveUp);
        CheckConflict("Move Down", _workingConfig.MoveDown);
        CheckConflict("Toggle Z-Level", _workingConfig.ToggleZLevel);
        CheckConflict("Increase Z-Level", _workingConfig.IncreaseZLevel);
        CheckConflict("Decrease Z-Level", _workingConfig.DecreaseZLevel);
        
        foreach (var kvp in _workingConfig.CustomBindings)
        {
            CheckConflict(kvp.Key, kvp.Value);
        }

        if (conflicts.Any())
        {
            var message = new StringBuilder("Cannot save due to the following conflicts:\n\n");
            foreach (var conflict in conflicts)
            {
                message.AppendLine(conflict);
            }
            message.AppendLine("\nPlease resolve these conflicts before saving.");
            
            MessageBox.Show(this, message.ToString(), "Key Binding Conflicts",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // 3. Create a completely new config to avoid any reference issues
        KeyBindingConfig newConfig = new()
        {
            MoveForward = _workingConfig.MoveForward.Clone(),
            MoveBackward = _workingConfig.MoveBackward.Clone(),
            StrafeLeft = _workingConfig.StrafeLeft.Clone(),
            StrafeRight = _workingConfig.StrafeRight.Clone(),
            MoveUp = _workingConfig.MoveUp.Clone(),
            MoveDown = _workingConfig.MoveDown.Clone(),
            ToggleZLevel = _workingConfig.ToggleZLevel.Clone(),
            IncreaseZLevel = _workingConfig.IncreaseZLevel.Clone(),
            DecreaseZLevel = _workingConfig.DecreaseZLevel.Clone(),
            CustomBindings = new Dictionary<string, GameKeyBinding>()
        };

        // Copy custom bindings
        foreach (var kvp in _workingConfig.CustomBindings)
        {
            newConfig.CustomBindings[kvp.Key] = kvp.Value.Clone();
        }

        System.Diagnostics.Debug.WriteLine($"=== Verification ===");
        System.Diagnostics.Debug.WriteLine($"Original working config ToggleZLevel: {_workingConfig.ToggleZLevel.MainKey}");
        System.Diagnostics.Debug.WriteLine($"New config ToggleZLevel: {newConfig.ToggleZLevel.MainKey}");

        // 4. Verify the new config is valid
        if (newConfig.ToggleZLevel.MainKey != _workingConfig.ToggleZLevel.MainKey)
        {
            throw new Exception("Config validation failed - key mismatch after copy");
        }

        // 5. Replace the main config
        _config.KeyBindingConfig = newConfig;

        System.Diagnostics.Debug.WriteLine($"Main config ToggleZLevel after assignment: {_config.KeyBindingConfig.ToggleZLevel.MainKey}");

        // 6. Save to file
        var saveResult = ConfigManager.SaveKeyBindings();
        if (!saveResult)
        {
            MessageBox.Show(this, "Failed to save key bindings to file.", "Save Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        // 7. Update the input manager
        if (GameView.Instance?.InputManager != null)
        {
            System.Diagnostics.Debug.WriteLine("Updating input manager");
            GameView.Instance.InputManager.UpdateBindings(_config.KeyBindingConfig);
        }
        
        DialogResult = true;
        Close();
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"Error in OK_Click: {ex}");
        MessageBox.Show(this, $"Error saving keybindings: {ex.Message}", "Save Error",
            MessageBoxButton.OK, MessageBoxImage.Error);
    }
}

        // Add a method to validate binding conflicts early:
        private void ChangeBinding_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is BindingViewModel binding)
            {
                _currentlyRecordingBinding = binding;
                var dialog = new KeyBindingDialog();
                dialog.Owner = this;

                var result = dialog.ShowDialog();
                if (result == true && dialog.ResultBinding != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Got new binding - Key: {dialog.ResultBinding.MainKey}, Modifiers: {dialog.ResultBinding.Modifiers}");
                    System.Diagnostics.Debug.WriteLine($"Setting binding for action: {_currentlyRecordingBinding.DisplayName}");

                    // Keep the display name and category from the original binding
                    var originalBinding = _workingConfig.GetBindingForAction(_currentlyRecordingBinding.DisplayName);
                    dialog.ResultBinding.DisplayName = originalBinding?.DisplayName ?? _currentlyRecordingBinding.DisplayName;
                    dialog.ResultBinding.Category = originalBinding?.Category ?? GetCategoryForAction(_currentlyRecordingBinding.DisplayName);

                    var (isValid, error) = _validator.ValidateBinding(dialog.ResultBinding);
                    if (!isValid)
                    {
                        MessageBox.Show(this, error, "Invalid Binding", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Log before setting
                    System.Diagnostics.Debug.WriteLine($"Setting binding for {_currentlyRecordingBinding.DisplayName} to {dialog.ResultBinding.MainKey}");
                    
                    _workingConfig.SetBindingForAction(_currentlyRecordingBinding.DisplayName, dialog.ResultBinding);
                    _currentlyRecordingBinding.SetBinding(dialog.ResultBinding);

                    // Verify the binding was set
                    var verifyBinding = _workingConfig.GetBindingForAction(_currentlyRecordingBinding.DisplayName);
                    System.Diagnostics.Debug.WriteLine($"Verified binding: {verifyBinding?.MainKey}, {verifyBinding?.Modifiers}");

                    LoadBindings();
                }

                _currentlyRecordingBinding = null;
            }
        }
        
        private string GetCategoryForAction(string actionName)
        {
            if (actionName.StartsWith("Move") || actionName.Contains("Strafe"))
                return "Camera";
            if (actionName.Contains("Z-Level"))
                return "Z-Level";
            return "";
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private static void CopyConfig(KeyBindingConfig source, KeyBindingConfig target)
        {
            target.MoveForward = source.MoveForward.Clone();
            target.MoveBackward = source.MoveBackward.Clone();
            target.StrafeLeft = source.StrafeLeft.Clone();
            target.StrafeRight = source.StrafeRight.Clone();
            target.MoveUp = source.MoveUp.Clone();
            target.MoveDown = source.MoveDown.Clone();
            target.ToggleZLevel = source.ToggleZLevel.Clone();
            target.IncreaseZLevel = source.IncreaseZLevel.Clone();
            target.DecreaseZLevel = source.DecreaseZLevel.Clone();

            target.CustomBindings = new Dictionary<string, GameKeyBinding>();
            foreach (var kvp in source.CustomBindings)
            {
                target.CustomBindings[kvp.Key] = kvp.Value.Clone();
            }
        }
    }

    public class CategoryViewModel
    {
        public string Category { get; }
        public List<BindingViewModel> Bindings { get; }

        public CategoryViewModel(string category, List<BindingViewModel> bindings)
        {
            Category = category;
            Bindings = bindings;
        }
    }

    public class BindingViewModel : INotifyPropertyChanged
    {
        public string DisplayName { get; }
        private string _currentBinding;
        public string CurrentBinding
        {
            get => _currentBinding;
            private set
            {
                if (_currentBinding != value)
                {
                    _currentBinding = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentBinding)));
                }
            }
        }

        private GameKeyBinding _binding;

        public event PropertyChangedEventHandler PropertyChanged;

        public BindingViewModel(string displayName, GameKeyBinding binding)
        {
            DisplayName = displayName;
            _binding = binding;
            UpdateCurrentBinding();
        }

        public void SetBinding(GameKeyBinding newBinding)
        {
            _binding = newBinding;
            UpdateCurrentBinding();
        }

        public GameKeyBinding GetBinding()
        {
            return _binding;
        }

        private void UpdateCurrentBinding()
        {
            CurrentBinding = _binding?.GetDisplayString() ?? "None";
        }
    }
}