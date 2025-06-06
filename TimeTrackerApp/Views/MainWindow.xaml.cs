﻿using System.Windows;
using System.Windows.Input;
using TimeTrackerApp.ViewModels;

namespace TimeTrackerApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TaskNameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (DataContext is MainViewModel vm && vm.AddTaskCommand.CanExecute(null))
                {
                    vm.AddTaskCommand.Execute(null);
                }
            }
        }

        private void ProjectNameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (DataContext is MainViewModel vm && vm.AddProjectCommand.CanExecute(null))
                {
                    vm.AddProjectCommand.Execute(null);
                }
            }
        }
    }
}
