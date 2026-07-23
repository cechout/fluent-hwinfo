using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;


namespace FluentSensors.Features.Performance
{
    public sealed partial class PerformancePage : Page
    {
        public PerformanceViewModel ViewModel => PerformanceViewModel.Instance;

        public PerformancePage()
        {
            InitializeComponent();
        }

        // x:Bind function bindings, used instead of a separate IValueConverter class for this simple case
        private Visibility ShowIfTrue(bool value) => value ? Visibility.Visible : Visibility.Collapsed;
        private Visibility ShowIfFalse(bool value) => value ? Visibility.Collapsed : Visibility.Visible;

        private void ShowOverall_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Cpu.IsShowingAllThreads = false;
        }

        private void ShowAllThreads_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Cpu.IsShowingAllThreads = true;
        }
    }
}