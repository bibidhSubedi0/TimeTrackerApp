using System.Windows;
using System.Windows.Input;
using TimeTrackerApp.ViewModels;

namespace TimeTrackerApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();  // This initializes the XAML components
        }

        private void TaskNameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var vm = DataContext as MainViewModel;
                if (vm?.AddTaskCommand.CanExecute(null) == true)
                {
                    vm.AddTaskCommand.Execute(null);
                }
            }
        }

        private void ProjectNameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var vm = DataContext as MainViewModel;
                if (vm?.AddProjectCommand.CanExecute(null) == true)
                {
                    vm.AddProjectCommand.Execute(null);
                }
            }
        }

    }
}
