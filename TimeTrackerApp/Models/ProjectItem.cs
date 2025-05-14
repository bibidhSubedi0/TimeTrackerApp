using System.Collections.ObjectModel;
using System.ComponentModel;

namespace TimeTrackerApp.Models
{
    public class ProjectItem : INotifyPropertyChanged
    {
        // Project properties
        private string _name;
        private string _timeElapsed;
        private string _estimatedTime;
        
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public string TimeSpent
        {
            get => _timeElapsed;
            set
            {
                if (_timeElapsed != value)
                {
                    _timeElapsed = value;
                    OnPropertyChanged(nameof(TimeSpent));
                }
            }
        }

        public string EstimatedTime
        {
            get => _estimatedTime;
            set
            {
                if (_estimatedTime != value)
                {
                    _estimatedTime = value;
                    OnPropertyChanged(nameof(EstimatedTime));
                }
            }
        }

        public ObservableCollection<TaskItem> Tasks { get; set; } = new();
        public ObservableCollection<TaskItem> CompletedTasks { get; set; } = new();


        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
