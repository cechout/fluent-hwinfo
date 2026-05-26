using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using FluentHwInfo.ViewModels;

namespace FluentHwInfo.Views
{
    public sealed partial class SensorsPage : Page
    {
        // here we define the ViewModel SensorViewModel as a property of the SensorsPage class, which is the DataContext for
        // the whole XAML page
        // so {x:Bind ViewModel.HardwareGroups} can find its target
        public SensorsViewModel ViewModel { get; }

        // Wir merken uns hier das aktuell offene Widget-Fenster (Spam-Schutz)
        private WidgetWindow _currentWidgetWindow = null;

        public SensorsPage()
        {
            this.InitializeComponent();

            // here we create the highest ViewModel
            // Once this happens, the HardwareMonitorService automatically starts its 500ms measurement loop in the background
            ViewModel = new SensorsViewModel();
        }

        private void PinToWidget_Click(object sender, RoutedEventArgs e)
        {
            // 1. Prüfen: Ist das Fenster schon offen?
            if (_currentWidgetWindow == null)
            {
                // 2. Nein? Dann bauen wir ein neues!
                _currentWidgetWindow = new WidgetWindow();

                // 3. Wenn der User das Widget später über das 'X' schließt, 
                // müssen wir unseren Spam-Schutz wieder zurücksetzen (auf null).
                _currentWidgetWindow.Closed += (s, args) =>
                {
                    _currentWidgetWindow = null;
                };

                // 4. Fenster auf den Bildschirm werfen
                _currentWidgetWindow.Activate();
            }
            else
            {
                // 5. Wenn es schon offen ist, holen wir es einfach in den Vordergrund
                _currentWidgetWindow.Activate();
            }

            // TODO für später: Hier holen wir uns dann noch das aktuell ausgewählte 
            // SensorRowViewModel aus der UI und übergeben es an das _currentWidgetWindow.
        }
    }
}