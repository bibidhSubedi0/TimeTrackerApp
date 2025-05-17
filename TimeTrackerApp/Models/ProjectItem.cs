using System.Collections.ObjectModel;
using System.ComponentModel;

namespace TimeTrackerApp.Models
{
    public class ProjectItem : INotifyPropertyChanged
    {
        private string _name;
        private string _timeElapsed;
        private string _estimatedTime;
        private float _completionPercentage = 25;

        
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

        public float CompletionPercentage
        {
            get
            {
                if ((Tasks?.Count ?? 0) + (CompletedTasks?.Count ?? 0) == 0)
                    return 0;

                return CompletedTasks.Count * 100f / (Tasks.Count + CompletedTasks.Count);
            }
            set
            {
                if (_completionPercentage != value)
                {
                    _completionPercentage = value;
                    OnPropertyChanged(nameof(CompletionPercentage));
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
