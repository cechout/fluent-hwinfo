using System.Collections.ObjectModel;

namespace FluentHwInfo.ViewModels
{
    /// <summary>
    /// Serves as the mid-level DataContext directly bound to a single UI Expander control, encapsulating the next smaller scope 
    /// in the ViewModel hierarchy
    /// 
    /// Responsibilities:
    /// - Visually groups a specific hardware component (e.g., "Intel Core i9-12900H") by binding its name to the Expander Header.
    /// - Maintains an ObservableCollection of nested SensorRowViewModels bound to the Expander Content.
    /// </summary>
    public class HardwareGroupViewModel
    {
        // binds to the text at the top of the Expander (Header)
        public string HardwareName { get; set; } = "Hardware Name not provided";

        // binds to the content of the Expander (the actual sensor rows)
        public ObservableCollection<SensorRowViewModel> Sensors { get; set; }

        public HardwareGroupViewModel()
        {
            // initializes the empty list for this specific hardware
            Sensors = new ObservableCollection<SensorRowViewModel>();
        }
    }
}