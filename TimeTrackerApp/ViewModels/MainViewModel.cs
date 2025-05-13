using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.Input;
using TimeTrackerApp.Models;
using TimeTrackerApp.Utils;

namespace TimeTrackerApp.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly DataService _dataService = new();
        private readonly string _userId = "default_user";

        private DispatcherTimer _timer;
        private TimeSpan _timeLeft;

        private UserData _userData = new();
        private bool _isTimerRunning;
        private ProjectItem _selectedProject;
        private ObservableCollection<ProjectItem> _projects = new();
        private ObservableCollection<TaskItem> _tasks = new();
        private string _timerCountdown;
        private string _timerTaskName;
        private string _timerProjectName;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsTimerRunning
        {
            get => _isTimerRunning;
            set
            {
                if (_isTimerRunning != value)
                {
                    _isTimerRunning = value;
                    OnPropertyChanged(nameof(IsTimerRunning));
                }
            }
        }

        public ObservableCollection<ProjectItem> Projects
        {
            get => _projects;
            set
            {
                if (_projects != value)
                {
                    _projects = value;
                    OnPropertyChanged(nameof(Projects));
                }
            }
        }

        public ObservableCollection<TaskItem> Tasks
        {
            get => _tasks;
            set
            {
                if (_tasks != value)
                {
                    _tasks = value;
                    OnPropertyChanged(nameof(Tasks));
                }
            }
        }

        public ProjectItem SelectedProject
        {
            get => _selectedProject;
            set
            {
                if (_selectedProject != value)
                {
                    _selectedProject = value;
                    Tasks = _selectedProject?.Tasks ?? new ObservableCollection<TaskItem>();
                    OnPropertyChanged(nameof(SelectedProject));
                }
            }
        }

        public string TimerCountdown
        {
            get => _timerCountdown;
            set
            {
                if (_timerCountdown != value)
                {
                    _timerCountdown = value;
                    OnPropertyChanged(nameof(TimerCountdown));
                }
            }
        }

        public string TimerTaskName
        {
            get => _timerTaskName;
            set
            {
                if (_timerTaskName != value)
                {
                    _timerTaskName = value;
                    OnPropertyChanged(nameof(TimerTaskName));
                }
            }
        }

        public string TimerProjectName
        {
            get => _timerProjectName;
            set
            {
                if (_timerProjectName != value)
                {
                    _timerProjectName = value;
                    OnPropertyChanged(nameof(TimerProjectName));
                }
            }
        }

        public ICommand AddProjectCommand { get; }
        public ICommand AddTaskCommand { get; }
        public ICommand StartTaskCommand { get; }
        public ICommand StopTimerCommand { get; }

        public IAsyncRelayCommand SaveTasksCommand { get; }
        public IAsyncRelayCommand LoadTasksCommand { get; }

        public MainViewModel()
        {
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += Timer_Tick;

            AddProjectCommand = new RelayCommand(AddProject);
            AddTaskCommand = new RelayCommand(AddTask);
            StartTaskCommand = new RelayCommand<TaskItem>(StartTask);
            StopTimerCommand = new RelayCommand(StopTimer);

            SaveTasksCommand = new AsyncRelayCommand(SaveAsync);
            LoadTasksCommand = new AsyncRelayCommand(LoadAsync);

            _ = LoadAsync();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _timeLeft = _timeLeft.Add(TimeSpan.FromSeconds(-1));
            TimerCountdown = _timeLeft.ToString(@"hh\:mm\:ss");

            if (_timeLeft <= TimeSpan.Zero)
            {
                _timer.Stop();
                TimerCountdown = "Time's up!";
                IsTimerRunning = false;
            }
        }

        private async void AddProject()
        {
            var newProject = new ProjectItem
            {
                Name = $"Project {Projects.Count + 1}",
                TimeSpent = "00:00:00"
            };

            HookProjectEvents(newProject);
            Projects.Add(newProject);
            await AutoSaveAsync();
        }

        private async void AddTask()
        {
            if (SelectedProject == null)
            {
                MessageBox.Show("Please select a project first.");
                return;
            }

            var newTask = new TaskItem
            {
                Name = $"Task {SelectedProject.Tasks.Count + 1}",
                ExpectedTime = "00:25:00",
                IsCompleted = false,
                TimeSpent = "00:00:00"
            };

            HookTaskEvents(newTask);
            SelectedProject.Tasks.Add(newTask);
            await AutoSaveAsync();
        }

        private void StartTask(TaskItem task)
        {
            if (task == null || SelectedProject == null)
                return;

            if (TimeSpan.TryParse(task.ExpectedTime, out _timeLeft))
            {
                TimerProjectName = $"Project: {SelectedProject.Name}";
                TimerTaskName = task.Name;
                TimerCountdown = _timeLeft.ToString(@"hh\:mm\:ss");

                IsTimerRunning = true;
                _timer.Start();
            }
        }

        private void StopTimer()
        {
            _timer.Stop();
            TimerCountdown = "Timer Stopped";
            IsTimerRunning = false;
        }

        private async Task SaveAsync()
        {
            _userData.UserId = _userId;
            _userData.Projects = Projects.ToList();
            await _dataService.SaveUserDataAsync(_userData);
        }

        private async Task LoadAsync()
        {
            _userData = await _dataService.LoadUserDataAsync(_userId);
            Projects.Clear();

            foreach (var project in _userData.Projects)
            {
                HookProjectEvents(project);
                Projects.Add(project);
            }
        }

        private async Task AutoSaveAsync()
        {
            var data = new UserData
            {
                UserId = _userId,
                Projects = Projects.ToList()
            };

            await _dataService.SaveUserDataAsync(data);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void HookProjectEvents(ProjectItem project)
        {
            project.PropertyChanged += async (sender, e) =>
            {
                if (e.PropertyName == nameof(ProjectItem.Name))
                    await AutoSaveAsync();
            };

            foreach (var task in project.Tasks)
                HookTaskEvents(task);

            project.Tasks.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                {
                    foreach (TaskItem newTask in e.NewItems)
                        HookTaskEvents(newTask);
                }
            };
        }

        private void HookTaskEvents(TaskItem task)
        {
            task.PropertyChanged += async (sender, e) =>
            {
                if (e.PropertyName == nameof(TaskItem.Name))
                    await AutoSaveAsync();
            };
        }
    }
}
