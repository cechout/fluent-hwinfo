using System.Collections.ObjectModel;

namespace FluentHwInfo.ViewModels
{
    public class CPUViewModel
    {
        // ObservableCollection is a smart list that automatically notifies the UI
        // when you add, remove or change items in the list
        public ObservableCollection<SensorRowViewModel> SensorList { get; set; }

        public CPUViewModel()
        {
            SensorList = new ObservableCollection<SensorRowViewModel>();

            // here we simply create our 3 rows at the for the list
            // this replaces all the hardcoded <ListViewItem> from the XAML
            SensorList.Add(new SensorRowViewModel { Name = "CPU Package Power" });
            SensorList.Add(new SensorRowViewModel { Name = "IA Cores Power" });
            SensorList.Add(new SensorRowViewModel { Name = "GT Cores Power" });
        }
    }
}