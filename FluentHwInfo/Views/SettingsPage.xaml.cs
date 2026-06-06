using FluentHwInfo.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace FluentHwInfo.Views
{
    public sealed partial class SettingsPage : Page
    {
        // this is our "Türsteher" to prevent infinite loops when synchronizing the two color pickers
        private bool _isSyncingColor = false;
        private bool _isLoading = true;

        public SettingsPage()
        {
            this.InitializeComponent();

            // every time the page is created, call this new method to restore the last selected values in the combo boxes
            RestoreIntervalSelection();
            RestoreWidgetSettings();

            // Event Listener für den Widget Hintergrund Color Picker
            WidgetBackgroundColorPicker.RegisterPropertyChangedCallback(
                CommunityToolkit.WinUI.Controls.ColorPickerButton.SelectedColorProperty,
                WidgetBackgroundColorPicker_SelectedColorChanged);

            // NEU: Event Listener für den neuen Graph Color Picker
            GraphColorPicker.RegisterPropertyChangedCallback(
                CommunityToolkit.WinUI.Controls.ColorPickerButton.SelectedColorProperty,
                GraphColorPicker_SelectedColorChanged);

            _isLoading = false;
        }


        // theme combo box
        private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string themeTag = selectedItem.Tag?.ToString();

                SettingsService.Instance.AppTheme = themeTag;

                // we get the absolute root element of the current window
                if (this.XamlRoot?.Content is FrameworkElement rootElement)
                {
                    // Match-Mapping for the ElementTheme enum
                    rootElement.RequestedTheme = themeTag switch
                    {
                        "Light" => ElementTheme.Light,
                        "Dark" => ElementTheme.Dark,
                        _ => ElementTheme.Default // system default
                    };
                }
            }
        }


        // interval combo box
        private void IntervalComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                if (selectedItem.Tag != null && int.TryParse(selectedItem.Tag.ToString(), out int newIntervalMs))
                {
                    // we access the one HardwareMonitorService instance and change the interval at runtime
                    HardwareMonitorService.Instance.UpdateIntervalMs = newIntervalMs;

                    //System.Diagnostics.Debug.WriteLine($"Polling-Intervall changed to: {newIntervalMs} ms");
                }
            }
        }
        private void RestoreIntervalSelection()
        {
            // we read the current interval value from the HardwareMonitorService instance
            int currentInterval = HardwareMonitorService.Instance.UpdateIntervalMs;

            // we search through all the items in the IntervalComboBox and compare their Tag with the current interval value
            foreach (ComboBoxItem item in IntervalComboBox.Items)
            {
                if (item.Tag?.ToString() == currentInterval.ToString())
                {
                    // match found -> activate the item
                    IntervalComboBox.SelectedItem = item;
                    break;
                }
            }
        }


        // widget combo box
        private void BackdropComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BackdropComboBox.SelectedItem is ComboBoxItem item && item.Tag is string tag)
            {
                // just save the selected backdrop type
                SettingsService.Instance.BackdropType = tag;
            }
        }
        private void ColorSourceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ColorSourceComboBox.SelectedItem is ComboBoxItem item && item.Tag is string tag)
            {
                // just save the selected color source
                SettingsService.Instance.UseAccentColor = (tag == "Accent");
            }
        }
        private void TintSlider_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            SettingsService.Instance.TintOpacity = (float)e.NewValue;
        }

        private void LuminositySlider_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            SettingsService.Instance.LuminosityOpacity = (float)e.NewValue;
        }
        private void WidgetBackgroundColorPicker_SelectedColorChanged(DependencyObject sender, DependencyProperty dp)
        {
            if (_isLoading) return;

            if (sender is CommunityToolkit.WinUI.Controls.ColorPickerButton colorPicker)
            {
                // if user manually picks a color, we switch the source to "custom"
                SettingsService.Instance.UseAccentColor = false;
                ColorSourceComboBox.SelectedIndex = 1;

                SettingsService.Instance.CustomTintColor = colorPicker.SelectedColor;
            }
        }
        private void GraphColorSourceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GraphColorSourceComboBox.SelectedItem is ComboBoxItem item && item.Tag is string tag)
            {
                SettingsService.Instance.UseGraphAccentColor = (tag == "Accent");
            }
        }

        private void GraphColorPicker_SelectedColorChanged(DependencyObject sender, DependencyProperty dp)
        {
            if (_isLoading) return;

            if (sender is CommunityToolkit.WinUI.Controls.ColorPickerButton colorPicker)
            {
                // if user picks a color for the graph, we switch the source to "custom"
                SettingsService.Instance.UseGraphAccentColor = false;
                GraphColorSourceComboBox.SelectedIndex = 1;

                SettingsService.Instance.GraphCustomColor = colorPicker.SelectedColor;
            }
        }
        private void RestoreWidgetSettings()
        {
            ColorSourceComboBox.SelectedIndex = SettingsService.Instance.UseAccentColor ? 0 : 1;

            string currentBackdrop = SettingsService.Instance.BackdropType;
            foreach (ComboBoxItem item in BackdropComboBox.Items)
            {
                if (item.Tag?.ToString() == currentBackdrop)
                {
                    BackdropComboBox.SelectedItem = item;
                    break;
                }
            }

            TintSlider.Value = SettingsService.Instance.TintOpacity;
            LuminositySlider.Value = SettingsService.Instance.LuminosityOpacity;
            WidgetBackgroundColorPicker.SelectedColor = SettingsService.Instance.CustomTintColor;

            GraphColorSourceComboBox.SelectedIndex = SettingsService.Instance.UseGraphAccentColor ? 0 : 1;
            GraphColorPicker.SelectedColor = SettingsService.Instance.GraphCustomColor;
        }
    }
}