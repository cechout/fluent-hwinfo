using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

using FluentSensors.Common;


namespace FluentSensors.Controls.SensorGraph
{
    public sealed partial class SensorPanelControl : UserControl
    {
        public SensorPanelControl()
        {
            InitializeComponent();
        }


        // === dependency properties ===

        public SensorGraphViewModel ViewModel
        {
            get => (SensorGraphViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                nameof(ViewModel),
                typeof(SensorGraphViewModel),
                typeof(SensorPanelControl),
                new PropertyMetadata(null));

        public SensorPanelMode Mode
        {
            get => (SensorPanelMode)GetValue(ModeProperty);
            set => SetValue(ModeProperty, value);
        }

        // no changed-callback needed: Mode is set once via a static XAML attribute per instance and never
        // changes at runtime in current usage
        public static readonly DependencyProperty ModeProperty =
            DependencyProperty.Register(
                nameof(Mode),
                typeof(SensorPanelMode),
                typeof(SensorPanelControl),
                new PropertyMetadata(SensorPanelMode.Standard));


        // === bindable helper surfaces ===

        // classic Binding + ElementName target (not x:Bind): RowDefinition.Height does not support x:Bind
        // reliably across WinUI versions, same reasoning as SensorGraphControl's own Series/XAxes/YAxes
        public GridLength TitleRowHeight => Mode == SensorPanelMode.Standard ? new GridLength(0) : new GridLength(20);
        public GridLength StatusHeaderRowHeight => Mode == SensorPanelMode.Standard ? new GridLength(20) : new GridLength(0);

        // only Widget shows the inline status header (toggle button, Y-Max value, current value, sensor name);
        // Performance already shows the sensor name in its own title row and toggles the panel by tapping the
        // graph instead, Minimal never shows it
        private Visibility GetStatusHeaderVisibility(SensorPanelMode mode)
        {
            return mode == SensorPanelMode.Standard ? Visibility.Visible : Visibility.Collapsed;
        }

        // Y-axis scaling is only adjustable in Widget mode
        private Visibility GetYAxisControlsVisibility(SensorPanelMode mode, Visibility controlPanelVisibility)
        {
            return mode == SensorPanelMode.Standard ? controlPanelVisibility : Visibility.Collapsed;
        }

        // threshold stays adjustable in both Widget and Performance mode, never in Minimal
        private Visibility GetThresholdControlsVisibility(SensorPanelMode mode, Visibility controlPanelVisibility)
        {
            return mode == SensorPanelMode.Minimal ? Visibility.Collapsed : controlPanelVisibility;
        }


        // === event handlers ===

        // in Performance mode, the toggle button and header are hidden entirely, so tapping the graph itself is
        // the only way left to open the threshold panel; Widget keeps its own dedicated toggle button and is
        // intentionally left untouched, Minimal has no interactive panel content regardless of this
        private void GraphControl_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (Mode == SensorPanelMode.Performance)
            {
                ViewModel?.ToggleControlPanel();
            }
        }
    }
}