using FluentHwInfo.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace FluentHwInfo.Views
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();

            // every time the page is created, call this new method to restore the last selected values in the combo boxes
            RestoreIntervalSelection();
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

        private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string themeTag = selectedItem.Tag?.ToString();

                // we get the absolute root element of the current window
                if (this.XamlRoot?.Content is FrameworkElement rootElement)
                {
                    // Match-Mapping for the ElementTheme enum
                    rootElement.RequestedTheme = themeTag switch
                    {
                        "Light" => ElementTheme.Light,
                        "Dark" => ElementTheme.Dark,
                        _ => ElementTheme.Default // System Default
                    };
                }
            }
        }

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
    }
}