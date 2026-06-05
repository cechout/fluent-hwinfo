using FluentHwInfo.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace FluentHwInfo.Views
{
    public sealed partial class SensorCardControl : UserControl
    {
        // Die Brücke: Erlaubt das Binden von Daten an dieses Custom Control
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

        public SensorCardControl()
        {
            this.InitializeComponent();
        }
    }
}