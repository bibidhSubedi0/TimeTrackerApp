using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

namespace TimeTrackerApp.Models
{
    public class ProjectItem : INotifyPropertyChanged
    {
        private string _name;
        private string _timeSpentOnProject;

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
            get => _timeSpentOnProject;
            set
            {
                if (_timeSpentOnProject != value)
                {
                    _timeSpentOnProject = value;
                    OnPropertyChanged(nameof(TimeSpent));
                }
            }
        }

        public ObservableCollection<TaskItem> Tasks { get; set; } = new ObservableCollection<TaskItem>();

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
