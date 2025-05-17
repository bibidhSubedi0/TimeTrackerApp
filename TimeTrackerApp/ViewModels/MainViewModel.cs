using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
        private UserData _userData = new();
        private DispatcherTimer _timer;
        private TimeSpan _timeLeft;
        private bool _isTimerRunning;
        private string _timerCountdown;
        private string _timerTaskName;
        private string _timerProjectName;
        private TaskItem _currentTask; 
        private string _newProjectName;
        private string _newTaskName;
        private string _newTaskExpectedTime;
        private double _timerProgress;
        private TimeSpan _totalTime;
        private bool _showActiveTasks = true;
        private bool _showCompletedTasks;


        
        private ProjectItem _selectedProject;
        private ObservableCollection<ProjectItem> _projects = new();
        private ObservableCollection<TaskItem> _tasks = new();
        

        public event PropertyChangedEventHandler PropertyChanged;
        public double TimerProgress
        {
            get => _timerProgress;
            set
            {
                if (_timerProgress != value)
                {
                    _timerProgress = value;
                    OnPropertyChanged(nameof(TimerProgress));
                }
            }
        }
        public bool ShowActiveTasks
        {
            get => _showActiveTasks;
            set
            {
                if (_showActiveTasks != value)
                {
                    _showActiveTasks = value;
                    OnPropertyChanged(nameof(ShowActiveTasks));
                    if (value)
                    {
                        ShowCompletedTasks = false;
                    }
                }
            }
        }
        public bool ShowCompletedTasks
        {
            get => _showCompletedTasks;
            set
            {
                if (_showCompletedTasks != value)
                {
                    _showCompletedTasks = value;
                    OnPropertyChanged(nameof(ShowCompletedTasks));
                    if (value)
                    {
                        ShowActiveTasks = false;
                    }
                }
            }
        }
        public string TotalTimeSpent
        {
            get
            {
                TimeSpan total = TimeSpan.Zero;
                foreach (var project in Projects)
                {
                    if (TimeSpan.TryParse(project.TimeSpent, out TimeSpan projectTime))
                    {
                        total += projectTime;
                    }
                }
                return total.ToString(@"hh\:mm\:ss");
            }
        }
        public int TotalCompletedTasks
        {
            get
            {
                return Projects.Sum(p => p.CompletedTasks.Count);
            }
        }
        public string CompletionRate
        {
            get
            {
                int totalTasks = Projects.Sum(p => p.Tasks.Count + p.CompletedTasks.Count);
                int completedTasks = Projects.Sum(p => p.CompletedTasks.Count);

                if (totalTasks == 0) return "0%";
                return $"{(completedTasks * 100.0 / totalTasks):F0}%";
            }
        }
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
        public string NewProjectName
        {
            get => _newProjectName;
            set
            {
                _newProjectName = value;
                OnPropertyChanged(nameof(_newProjectName));
            }
        }
        public string NewTaskName
        {
            get => _newTaskName;
            set
            {
                _newTaskName = value;
                OnPropertyChanged(nameof(NewTaskName));
            }
        }
        public string NewTaskExpectedTime
        {
            get => _newTaskExpectedTime;
            set
            {
                _newTaskExpectedTime = value;
                OnPropertyChanged(nameof(NewTaskExpectedTime));
            }
        }
        public ObservableCollection<TaskItem> AllCompletedTasks
        {
            get
            {
                var completedTasks = new ObservableCollection<TaskItem>();
                foreach (var project in Projects)
                {
                    foreach (var task in project.CompletedTasks)
                    {
                        completedTasks.Add(task);
                    }
                }
                return completedTasks;
            }
        }



        public ICommand AddProjectCommand { get; }
        public ICommand AddTaskCommand { get; }
        public ICommand DeleteTaskCommand { get; }
        public ICommand StartTaskCommand { get; }
        public ICommand StopTimerCommand { get; }

        public ICommand EnterKeyCommand { get; }
        public IAsyncRelayCommand SaveTasksCommand { get; }
        public IAsyncRelayCommand LoadTasksCommand { get; }

        
        

        public MainViewModel()
        {
            EnterKeyCommand = new RelayCommand<string>(ExecuteEnterKey);
            NewTaskExpectedTime = "00:30:00";

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += Timer_Tick;

            AddProjectCommand = new RelayCommand(AddProject);
            AddTaskCommand = new RelayCommand(AddTask);
            DeleteTaskCommand = new RelayCommand<TaskItem>(DeleteTask);

            StartTaskCommand = new RelayCommand<TaskItem>(StartTask);
            StopTimerCommand = new RelayCommand(StopTimer);


            SaveTasksCommand = new AsyncRelayCommand(SaveAsync);
            LoadTasksCommand = new AsyncRelayCommand(LoadAsync);

            // Add property changed notifications for statistics
            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(Projects) ||
                    e.PropertyName == nameof(Tasks))
                {
                    OnPropertyChanged(nameof(TotalTimeSpent));
                    OnPropertyChanged(nameof(TotalCompletedTasks));
                    OnPropertyChanged(nameof(CompletionRate));
                    OnPropertyChanged(nameof(AllCompletedTasks));
                }
            };

            _ = LoadAsync();

        }


        private void ExecuteEnterKey(string parameter)
        {
            if (parameter == "project")
            {
                AddProject();
            }
            else if (parameter == "task")
            {
                AddTask();
            }
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            _timeLeft = _timeLeft.Add(TimeSpan.FromSeconds(-1));

            TimeSpan _temp = TimeSpan.Zero;
            TimeSpan.TryParse(SelectedProject.TimeSpent, out _temp);
            _temp = _temp.Add(TimeSpan.FromSeconds(1));
            SelectedProject.TimeSpent = _temp.ToString(@"hh\:mm\:ss");

            TimerCountdown = _timeLeft.ToString(@"hh\:mm\:ss");
            _currentTask.ExpectedTime = TimerCountdown;

            // Calculate and update progress
            double elapsedSeconds = (_totalTime - _timeLeft).TotalSeconds;
            double totalSeconds = _totalTime.TotalSeconds;
            TimerProgress = (elapsedSeconds / totalSeconds) * 100;

            if (_timeLeft <= TimeSpan.Zero)
            {
                _timer.Stop();
                TimerCountdown = "Time's up!";
                TimerProgress = 100; // Ensure progress is complete
                                     //IsTimerRunning = false;
            }
        }
        private void StopTimer()
        {
            _timer.Stop();
            if (TimerCountdown == "Time's up!" || TimerCountdown == "Timer Stopped")
            {
                IsTimerRunning = false;
            }
            TimerCountdown = "Timer Stopped";

            //IsTimerRunning = false;
        }
        private async void AddProject()
        {
            var newProject = new ProjectItem
            {
                Name = NewProjectName,
                TimeSpent = "00:00:00"
            };

            HookProjectEvents(newProject);
            Projects.Add(newProject);
            NewProjectName = string.Empty;
            OnPropertyChanged(nameof(NewProjectName));
            await AutoSaveAsync();
        }
        private async void AddTask()
        {
            if (SelectedProject == null)
            {
                MessageBox.Show("Please select a project first.");
                return;
            }

            // Validate input
            if (string.IsNullOrWhiteSpace(NewTaskName))
            {
                MessageBox.Show("Please enter a task name.");
                return;
            }

            if (string.IsNullOrWhiteSpace(NewTaskExpectedTime))
            {
                MessageBox.Show("Please enter an expected time.");
                return;
            }

            var newTask = new TaskItem
            {
                Name = NewTaskName,
                ExpectedTime = NewTaskExpectedTime,
                IsCompleted = false,
                TimeElapsed = "00:00:00"
            };

            HookTaskEvents(newTask);
            SelectedProject.Tasks.Add(newTask);

            // Clear input fields
            NewTaskName = string.Empty;
            NewTaskExpectedTime = "00:30:00";
            OnPropertyChanged(nameof(NewTaskName));
            OnPropertyChanged(nameof(NewTaskExpectedTime));

            RecalculateProjectTime();

            await AutoSaveAsync();
        }
        private async void DeleteTask(TaskItem task)
        {
            if (task == null || SelectedProject == null)
                return;

            

            foreach (var t in SelectedProject.Tasks)
            {
                if (t.Name == task.Name)
                {
                    SelectedProject.Tasks.Remove(t);
                    break;
                }
            }
            RecalculateProjectTime();

            await AutoSaveAsync();
        }
        private void StartTask(TaskItem task)
        {
            if (task == null || SelectedProject == null)
                return;

            if (TimeSpan.TryParse(task.ExpectedTime, out _timeLeft))
            {
                _totalTime = _timeLeft;
                TimerProjectName = $"Project: {SelectedProject.Name}";
                TimerTaskName = task.Name;
                TimerCountdown = _timeLeft.ToString(@"hh\:mm\:ss");
                IsTimerRunning = true;
                TimerProgress = 0;
                _currentTask = task;
                _timer.Start();
            }
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

            SelectedProject = Projects?.FirstOrDefault() ?? new ProjectItem();
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
                if (e.PropertyName == nameof(TaskItem.ExpectedTime))
                {
                    RecalculateProjectTime();
                    await AutoSaveAsync();
                }
                if(e.PropertyName == nameof(TaskItem.IsCompleted))
                {
                    UpdateCompletedTasks();
                    await AutoSaveAsync();
                }

            };
        }
        private void RecalculateProjectTime()
        {
            TimeSpan total = TimeSpan.Zero;
            foreach (var task in SelectedProject.Tasks)
            {
                if (TimeSpan.TryParse(task.ExpectedTime, out TimeSpan t))
                    total += t;
            }

            SelectedProject.EstimatedTime = total.ToString(@"hh\:mm\:ss");
        }
        private void UpdateCompletedTasks()
        {
            var completed = SelectedProject.Tasks
                .Where(t => t.IsCompleted)
                .ToList();

            

            foreach (var task in completed)
            {
                task.CompletedDate = DateTime.Now;
                SelectedProject.CompletedTasks.Add(task);
                SelectedProject.Tasks.Remove(task);
            }

            // Update statistics
            OnPropertyChanged(nameof(TotalCompletedTasks));
            OnPropertyChanged(nameof(CompletionRate));
            OnPropertyChanged(nameof(AllCompletedTasks));
        }

    }
}
