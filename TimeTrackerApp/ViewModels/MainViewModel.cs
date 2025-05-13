using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using TimeTrackerApp.Models;
using System.Windows;
using System.Windows.Threading;
using System.ComponentModel;

namespace TimeTrackerApp.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private bool _isTimerRunning = false;
        public bool IsTimerRunning
        {
            get => _isTimerRunning;
            set
            {
                _isTimerRunning = value;
                OnPropertyChanged(nameof(IsTimerRunning));
            }
        }


        private ProjectItem _selectedProject;
        private ObservableCollection<ProjectItem> _projects;
        private ObservableCollection<TaskItem> _tasks;
        private string _timerCountdown;
        private string _timerTaskName;
        private string _timerProjectName;
        private DispatcherTimer _timer;
        private TimeSpan _timeLeft;

        public ObservableCollection<ProjectItem> Projects
        {
            get => _projects;
            set
            {
                _projects = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<TaskItem> Tasks
        {
            get => _tasks;
            set
            {
                _tasks = value;
                OnPropertyChanged();
            }
        }

        public ProjectItem SelectedProject
        {
            get => _selectedProject;
            set
            {
                _selectedProject = value;
                Tasks = _selectedProject?.Tasks ?? new ObservableCollection<TaskItem>();
                OnPropertyChanged();
            }
        }

        public string TimerCountdown
        {
            get => _timerCountdown;
            set
            {
                _timerCountdown = value;
                OnPropertyChanged();
            }
        }

        public string TimerTaskName
        {
            get => _timerTaskName;
            set
            {
                _timerTaskName = value;
                OnPropertyChanged();
            }
        }

        public string TimerProjectName
        {
            get => _timerProjectName;
            set
            {
                _timerProjectName = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddProjectCommand { get; set; }
        public ICommand AddTaskCommand { get; set; }
        public ICommand StartTaskCommand { get; set; }
        public ICommand StopTimerCommand { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;


        public MainViewModel()
        {
            _projects = new ObservableCollection<ProjectItem>();
            _tasks = new ObservableCollection<TaskItem>();
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += Timer_Tick;

            // Relay the ICommand to the actual methods
            AddProjectCommand = new RelayCommand(AddProject);
            AddTaskCommand = new RelayCommand(AddTask);
            StartTaskCommand = new RelayCommand<TaskItem>(StartTask);
            StopTimerCommand = new RelayCommand(StopTimer);

            AddProject();
            SelectedProject = Projects.FirstOrDefault();
            AddTask();

        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _timeLeft = _timeLeft.Add(TimeSpan.FromSeconds(-1));
            TimerCountdown = _timeLeft.ToString(@"hh\:mm\:ss");

            if (_timeLeft <= TimeSpan.Zero)
            {
                _timer.Stop();
                TimerCountdown = "Time's up!";
            }
        }

        private void AddProject()
        {
            var newProject = new ProjectItem {
                Name = "Project " + (Projects.Count + 1),
                TimeSpent = "00:00:00"
            };
            Projects.Add(newProject);
        }

        private void AddTask()
        {
            if (SelectedProject == null)
            {
                MessageBox.Show("Please select a project first.");
                return;
            }

            var newTask = new TaskItem
            {
                Name = "Task " + (SelectedProject.Tasks.Count + 1),
                ExpectedTime = "00:25:00",
                IsCompleted = false,
                TimeSpent = "00:00:00"
            };

            SelectedProject.Tasks.Add(newTask);
        }

        private void StartTask(TaskItem task)
        {
            if (TimeSpan.TryParse(task.ExpectedTime, out _timeLeft))
            {
                TimerProjectName = "Project: " + SelectedProject.Name;
                TimerTaskName = task.Name;
                TimerCountdown = _timeLeft.ToString(@"hh\:mm\:ss");

                IsTimerRunning = true; // 
                _timer.Start();
            }
        }

        private void StopTimer()
        {

            _timer.Stop();
            TimerCountdown = "Timer Stopped";
            IsTimerRunning = false; 
        }


        protected void OnPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
