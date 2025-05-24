using System;
using System.ComponentModel;

namespace TimeTrackerApp.Models
{
    public class TaskItem : INotifyPropertyChanged
    {
        private string _name;
        private string _estimatedTime;
        private string _timeElapsed;
        private string _remainingTime;
        private int _priority;
        private bool _isCompleted;
        private DateTime? _completedDate;

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
            get => _estimatedTime;
            set
            {
                if (_estimatedTime != value)
                {
                    _estimatedTime = value;
                    OnPropertyChanged(nameof(ExpectedTime));
                }
            }
        }

        public string TimeElapsed
        {
            get => _timeElapsed;
            set
            {
                if (_timeElapsed != value)
                {
                    _timeElapsed = value;
                    OnPropertyChanged(nameof(TimeElapsed));
                }
            }
        }

        public string RemainingTime
        {
            get => _remainingTime;
            set
            {
                if (_remainingTime != value)
                {
                    _remainingTime = value;
                    OnPropertyChanged(nameof(RemainingTime));
                }
            }
        }

        public int Priority
        {
            get => _priority;
            set
            {
                if (_priority != value)
                {
                    _priority = value;
                    OnPropertyChanged(nameof(Priority));
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

                    if (value)
                    {
                        CompletedDate = DateTime.UtcNow;
                    }
                    else
                    {
                        CompletedDate = null;
                    }

                    OnPropertyChanged(nameof(IsCompleted));
                    OnPropertyChanged(nameof(CompletedDate));
                    OnPropertyChanged(nameof(CompletedDateFormatted));
                }
            }
        }

        public DateTime? CompletedDate
        {
            get => _completedDate;
            set
            {
                if (_completedDate != value)
                {
                    _completedDate = value;
                    OnPropertyChanged(nameof(CompletedDate));
                    OnPropertyChanged(nameof(CompletedDateFormatted));
                }
            }
        }

        public string CompletedDateFormatted =>
            CompletedDate?.ToString("MM-dd HH:mm") ?? string.Empty;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
