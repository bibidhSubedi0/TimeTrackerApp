using System.ComponentModel;

namespace TimeTrackerApp.Models
{
    public class TaskItem : INotifyPropertyChanged
    {
        private string _name;
        private string _expectedTime;
        private bool _isCompleted;
        private string _timeSpentOnTask;

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
                    OnPropertyChanged();
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
                    OnPropertyChanged();
                }
            }
        }

        public string TimeSpent
        {
            get => _timeSpentOnTask;
            set
            {
                if (_timeSpentOnTask != value)
                {
                    _timeSpentOnTask = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
