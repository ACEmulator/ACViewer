// File: ACViewer/View/KeyboardConfig.xaml.cs
using System;
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
                    var (isValid, error) = _validator.ValidateBinding(dialog.ResultBinding);
            
                    if (!isValid)
                    {
                        MessageBox.Show(this, error, "Invalid Binding", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    _workingConfig.SetBindingForAction(_currentlyRecordingBinding.DisplayName, dialog.ResultBinding);
                    _currentlyRecordingBinding.SetBinding(dialog.ResultBinding);
                    LoadBindings();
                }
        
                _currentlyRecordingBinding = null;
            }
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
            CopyConfig(_workingConfig, _config.KeyBindingConfig);
            DialogResult = true;
            Close();
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