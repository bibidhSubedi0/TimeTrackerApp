using System.ComponentModel;

namespace TimeTrackerApp.Models
{
    public class TaskItem : INotifyPropertyChanged
    {
        private string _name;
        private string _expectedTime;
        private bool _isCompleted;
        private string _timeSpent;

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

        public string ExpectedTime
        {
            get => _expectedTime;
            set
            {
                if (_expectedTime != value)
                {
                    _expectedTime = value;
                    OnPropertyChanged(nameof(ExpectedTime));
                }
            }
        }

        public bool IsCompleted
        {
            get => _isCompleted;
            set
            {
                if (_isCompleted != value)
                {
                    _isCompleted = value;
                    OnPropertyChanged(nameof(IsCompleted));
                }
            }
        }

        public string TimeSpent
        {
            get => _timeSpent;
            set
            {
                if (_timeSpent != value)
                {
                    _timeSpent = value;
                    OnPropertyChanged(nameof(TimeSpent));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
