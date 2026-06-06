using FluentHwInfo.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace FluentHwInfo.Views
{
    public sealed partial class SensorCardControl : UserControl
    {
        // mouse tracker fields
        private bool _isHovered = false;
        private bool _isPressed = false;

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                nameof(ViewModel),
                typeof(SensorRowViewModel),
                typeof(SensorCardControl),
                new PropertyMetadata(null));

        public SensorRowViewModel ViewModel
        {
            get => (SensorRowViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        // 1. Die Pointer-Events (Maus rein, raus, drücken, loslassen)
        private void RootGrid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            _isHovered = true;
            UpdateVisualState();
        }

        private void RootGrid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            _isHovered = false;
            _isPressed = false; // Sicherheits-Reset
            UpdateVisualState();
        }

        private void RootGrid_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _isPressed = true;
            UpdateVisualState();
        }

        private void RootGrid_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            _isPressed = false;
            UpdateVisualState();
        }

        // 2. Der eigentliche Klick (Ersetzt die ToggleButton-Logik!)
        private void RootGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                // Schaltet den Sensor ein/aus
                ViewModel.IsSelected = !ViewModel.IsSelected;
                UpdateVisualState();
            }
        }

        // 3. Unsere eigene, kugelsichere State-Machine
        private void UpdateVisualState()
        {
            if (ViewModel == null) return;

            // Welchen Status hat der Sensor gerade?
            bool isChecked = ViewModel.IsSelected;

            if (isChecked)
            {
                if (_isPressed) VisualStateManager.GoToState(this, "CheckedPressed", true);
                else if (_isHovered) VisualStateManager.GoToState(this, "CheckedHover", true);
                else VisualStateManager.GoToState(this, "Checked", true);
            }
            else
            {
                if (_isPressed) VisualStateManager.GoToState(this, "Pressed", true);
                else if (_isHovered) VisualStateManager.GoToState(this, "Hover", true);
                else VisualStateManager.GoToState(this, "Normal", true);
            }
        }

        public SensorCardControl()
        {
            this.InitializeComponent();

            // we force the card to start immediately in the correct visual state
            this.Loaded += (s, e) => UpdateVisualState();
        }
    }
}