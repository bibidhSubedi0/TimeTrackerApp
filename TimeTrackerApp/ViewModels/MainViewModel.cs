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
        private bool _showThisWeek = true;
        private bool _showLastWeek;
        private bool _showAllTime;

        // Add these properties at the start of the MainViewModel class
        private bool _showToday;
        private string _todayStudyTime = "00:00:00";
        private DateTime _lastResetDate = DateTime.UtcNow.Date;

        public bool ShowToday
        {
            get => _showToday;
            set
            {
                if (_showToday != value)
                {
                    _showToday = value;
                    if (value)
                    {
                        ShowThisWeek = false;
                        ShowLastWeek = false;
                        ShowAllTime = false;
                    }
                    OnPropertyChanged(nameof(ShowToday));
                    OnPropertyChanged(nameof(FilteredCompletedTasks));
                    OnPropertyChanged(nameof(FilteredCompletedTasksCount));
                    OnPropertyChanged(nameof(FilteredTotalTimeSpent));
                    OnPropertyChanged(nameof(FilteredCompletionRate));
                }
            }
        }

        public string TodayStudyTime
        {
            get => _todayStudyTime;
            set
            {
                if (_todayStudyTime != value)
                {
                    _todayStudyTime = value;
                    OnPropertyChanged(nameof(TodayStudyTime));
                }
            }
        }



        private ProjectItem _selectedProject;
        private ObservableCollection<ProjectItem> _projects = new();
        private ObservableCollection<TaskItem> _tasks = new();


        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<TaskItem> FilteredCompletedTasks
        {
            get
            {
                var now = DateTime.Now;
                var startOfWeek = now.Date.AddDays(-(int)now.DayOfWeek);
                var startOfLastWeek = startOfWeek.AddDays(-7);
                var endOfLastWeek = startOfWeek.AddSeconds(-1);
                var startOfToday = now.Date;

                var tasks = new ObservableCollection<TaskItem>();
                foreach (var project in Projects)
                {
                    foreach (var task in project.CompletedTasks)
                    {
                        if (ShowToday && task.CompletedDate >= startOfToday)
                        {
                            tasks.Add(task);
                        }
                        else if (ShowThisWeek && task.CompletedDate >= startOfWeek && task.CompletedDate <= now)
                        {
                            tasks.Add(task);
                        }
                        else if (ShowLastWeek && task.CompletedDate >= startOfLastWeek && task.CompletedDate <= endOfLastWeek)
                        {
                            tasks.Add(task);
                        }
                        else if (ShowAllTime)
                        {
                            tasks.Add(task);
                        }
                    }
                }
                return tasks;
            }
        }
        public int FilteredCompletedTasksCount => FilteredCompletedTasks.Count;

        public string FilteredTotalTimeSpent
        {
            get
            {
                TimeSpan total = TimeSpan.Zero;
                foreach (var task in FilteredCompletedTasks)
                {
                    // This is not working cuz my dumb ass did ElapsedTime = Eastimated time or some shit
                    if (TimeSpan.TryParse(task.TimeElapsed, out TimeSpan taskTime))
                    {
                        total += taskTime;
                    }

                    // Lets try to fix it for purano version as well
                    // Note! Cant fix cuz for some reason i was modifying the elapsed time T^T
                }
                return total.ToString(@"hh\:mm\:ss");
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
        public string FilteredCompletionRate
        {
            get
            {
                var now = DateTime.Now;
                var startOfWeek = now.Date.AddDays(-(int)now.DayOfWeek);
                var startOfLastWeek = startOfWeek.AddDays(-7);
                var endOfLastWeek = startOfWeek.AddSeconds(-1);

                int completedTasks = FilteredCompletedTasks.Count;
                int totalTasks = 0;

                foreach (var project in Projects)
                {
                    foreach (var task in project.Tasks)
                    {
                        if ((ShowThisWeek && task.CompletedDate >= startOfWeek) ||
                            (ShowLastWeek && task.CompletedDate >= startOfLastWeek && task.CompletedDate <= endOfLastWeek) ||
                            ShowAllTime)
                        {
                            totalTasks++;
                        }
                    }
                    foreach (var task in project.CompletedTasks)
                    {
                        if ((ShowThisWeek && task.CompletedDate >= startOfWeek) ||
                            (ShowLastWeek && task.CompletedDate >= startOfLastWeek && task.CompletedDate <= endOfLastWeek) ||
                            ShowAllTime)
                        {
                            totalTasks++;
                        }
                    }
                }

                if (totalTasks == 0) return "0%";
                return $"{(completedTasks * 100.0 / totalTasks):F0}%";
            }
        }
        public bool ShowThisWeek
        {
            get => _showThisWeek;
            set
            {
                if (_showThisWeek != value)
                {
                    _showThisWeek = value;
                    if (value)
                    {
                        ShowLastWeek = false;
                        ShowAllTime = false;
                    }
                    OnPropertyChanged(nameof(ShowThisWeek));
                    OnPropertyChanged(nameof(FilteredCompletedTasks));
                    OnPropertyChanged(nameof(FilteredCompletedTasksCount));
                    OnPropertyChanged(nameof(FilteredTotalTimeSpent));
                    OnPropertyChanged(nameof(FilteredCompletionRate));
                }
            }
        }
        public bool ShowLastWeek
        {
            get => _showLastWeek;
            set
            {
                if (_showLastWeek != value)
                {
                    _showLastWeek = value;
                    if (value)
                    {
                        ShowThisWeek = false;
                        ShowAllTime = false;
                    }
                    OnPropertyChanged(nameof(ShowLastWeek));
                    OnPropertyChanged(nameof(FilteredCompletedTasks));
                    OnPropertyChanged(nameof(FilteredCompletedTasksCount));
                    OnPropertyChanged(nameof(FilteredTotalTimeSpent));
                    OnPropertyChanged(nameof(FilteredCompletionRate));
                }
            }
        }
        public bool ShowAllTime
        {
            get => _showAllTime;
            set
            {
                if (_showAllTime != value)
                {
                    _showAllTime = value;
                    if (value)
                    {
                        ShowThisWeek = false;
                        ShowLastWeek = false;
                    }
                    OnPropertyChanged(nameof(ShowAllTime));
                    OnPropertyChanged(nameof(FilteredCompletedTasks));
                    OnPropertyChanged(nameof(FilteredCompletedTasksCount));
                    OnPropertyChanged(nameof(FilteredTotalTimeSpent));
                    OnPropertyChanged(nameof(FilteredCompletionRate));
                }
            }
        }
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
            var currentDate = DateTime.UtcNow.Date;
            if (currentDate != _lastResetDate)
            {
                _todayStudyTime = "00:00:00";
                _lastResetDate = currentDate;
            }


            _timeLeft = _timeLeft.Add(TimeSpan.FromSeconds(-1));

            if (TimeSpan.TryParse(_todayStudyTime, out TimeSpan dailyTime))
            {
                dailyTime = dailyTime.Add(TimeSpan.FromSeconds(1));
                TodayStudyTime = dailyTime.ToString(@"hh\:mm\:ss");
            }

            TimeSpan _temp = TimeSpan.Zero;
            TimeSpan.TryParse(SelectedProject.TimeSpent, out _temp);
            _temp = _temp.Add(TimeSpan.FromSeconds(1));
            SelectedProject.TimeSpent = _temp.ToString(@"hh\:mm\:ss");


            TimerCountdown = _timeLeft.ToString(@"hh\:mm\:ss");
            //_currentTask.ExpectedTime = TimerCountdown;
            _temp = TimeSpan.Zero;
            TimeSpan.TryParse(_currentTask.RemainingTime, out _temp);
            _temp = _temp.Add(TimeSpan.FromSeconds(-1));
            _currentTask.RemainingTime = _temp.ToString();

            TimeSpan.TryParse(_currentTask.TimeElapsed, out _temp);
            _temp = _temp.Add(TimeSpan.FromSeconds(1));
            _currentTask.TimeElapsed = _temp.ToString();





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
                TimeElapsed = "00:00:00",
                RemainingTime = NewTaskExpectedTime
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


            if (TimeSpan.TryParse(task.RemainingTime, out _timeLeft))
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
            _userData.TodayStudyTime = TodayStudyTime;
            _userData.LastResetDate = _lastResetDate;
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

            // Load daily study time
            if (_userData.LastResetDate.Date == DateTime.UtcNow.Date)
            {
                TodayStudyTime = _userData.TodayStudyTime ?? "00:00:00";
                _lastResetDate = _userData.LastResetDate;
            }
            else
            {
                TodayStudyTime = "00:00:00";
                _lastResetDate = DateTime.UtcNow.Date;
            }

            SelectedProject = Projects?.FirstOrDefault() ?? new ProjectItem();
        }
        private async Task AutoSaveAsync()
        {
            _userData.UserId = _userId;
            _userData.Projects = Projects.ToList();
            _userData.TodayStudyTime = TodayStudyTime;
            _userData.LastResetDate = _lastResetDate;
            await _dataService.SaveUserDataAsync(_userData);
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
                if (e.PropertyName == nameof(TaskItem.RemainingTime))
                {
                    RecalculateProjectTime();
                    await AutoSaveAsync();
                }
                if (e.PropertyName == nameof(TaskItem.ExpectedTime))
                {
                    RecalculateProjectTime();
                    await AutoSaveAsync();
                }
                if (e.PropertyName == nameof(TaskItem.IsCompleted))
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
