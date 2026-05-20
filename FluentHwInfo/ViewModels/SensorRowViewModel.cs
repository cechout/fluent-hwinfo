using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FluentHwInfo.ViewModels
{
    // this class represents a single row in the UI for a sensor metric, e.g. "CPU Package Power"
    // it contains the name of the metric and the values for current, minimum, maximum and average

    // SensorRowViewModel inherits from INotifyPropertyChanged
    // INotifyPropertyChanged is mandatory for the UI to react to changes in the data,
    // otherwise the UI would never know that it should update itself
    public class SensorRowViewModel : INotifyPropertyChanged
    {
        private string _currentValue = "0.0 W";
        private string _minimumValue = "0.0 W";
        private string _maximumValue = "0.0 W";
        private string _averageValue = "0.0 W";

        public string Name { get; set; } = "Unknown Sensor";

        public string CurrentValue
        {
            get => _currentValue;
            set
            {
                _currentValue = value;
                OnPropertyChanged();
            }
        }

        public string MinimumValue
        {
            get => _minimumValue;
            set
            {
                _minimumValue = value;
                OnPropertyChanged();
            }
        }

        public string MaximumValue
        {
            get => _maximumValue;
            set
            {
                _maximumValue = value;
                OnPropertyChanged();
            }
        }

        public string AverageValue
        {
            get => _averageValue;
            set
            {
                _averageValue = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}