using Squirrel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Resources;
using System.Windows.Threading;
using Task_Time_Tracker.Model;
using Task_Time_Tracker.Utility_Functions;

namespace Task_Time_Tracker
{
    /// <summary>
    /// Interaction logic for TimeTrackerWindow.xaml
    /// </summary>
    public partial class TimeTrackerWindow : Window
    {
        private readonly CrmConnector _crmConnector;

        private ObservableCollection<ComboBoxPairs1> taskCBP;
        private ObservableCollection<ComboBoxPairs> projectCBP;

        private DispatcherTimer timer;
        private DispatcherTimer idleTimer;
        private int minutes = 0;
        private int hours = 0;
        private int currentMinutes = 0;

        private NotifyIcon ni;

        public TimeTrackerWindow(CrmConnector Connector)
        {
            InitializeComponent();

            SetNotifyIcon();

            timer = new DispatcherTimer();
            idleTimer = new DispatcherTimer();

            _crmConnector = Connector;

            RetrieveProjects();
            RetrieveUserName();

            SetIdleTimer();

            CheckForUpdates();
        }

        private async Task CheckForUpdates()
        {
            using (var updateManager = UpdateManager.GitHubUpdateManager("https://github.com/BeverCRM/TimeTrackingApp"))
            {
                await updateManager.Result.UpdateApp();
            }
        }

        private void SetIdleTimer()
        {
            idleTimer.Interval = new TimeSpan(0, 1, 0);
            idleTimer.Tick += IdleTimer_Tick;
            idleTimer.Start();
        }

        private void IdleTimer_Tick(object sender, EventArgs e)
        {
            var idleTime = IdleTimeDetector.GetIdleTimeInfo();

            if (idleTime.IdleTime.TotalMinutes >= 20)
            {
                MainWindow mainWindow = new MainWindow();
                Close();
                mainWindow.Show();
            }
        }

        private void SetNotifyIcon()
        {
            ni = new NotifyIcon();
            StreamResourceInfo sri = System.Windows.Application.GetResourceStream(new Uri("Task Time Tracker;component/Resources/SystemTray.ico", UriKind.Relative));
            ni.Icon = new Icon(sri.Stream);
            ni.Visible = false;
            ni.Click += delegate (object sender, EventArgs args)
            {
                Show();
                WindowState = WindowState.Normal;
                ni.Visible = false;
            };
        }

        private async void RetrieveProjects()
        {
            TaskComboBox.DisplayMemberPath = "Text";
            TaskComboBox.SelectedValuePath = "Value";

            ProjectComboBox.DisplayMemberPath = "Text";
            ProjectComboBox.SelectedValuePath = "Value";

            StartButton.IsEnabled = false;
            StopButton.IsEnabled = false;
            CompleteButton.IsEnabled = false;

            projectCBP = new ObservableCollection<ComboBoxPairs>();
            List<Project> projects = await _crmConnector.RetrieveProjectsAsync();

            foreach (Project project in projects)
            {
                projectCBP.Add(new ComboBoxPairs(project.ProjectName, project.ProjectId));
            }
            ProjectComboBox.ItemsSource = projectCBP;
        }

        private async void RetrieveUserName()
        {
            currentUserLabel.Content = "Welcome, " + await _crmConnector.GetCurrentUserNameAsync();
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                Hide();
            }

            ni.Visible = true;
            base.OnStateChanged(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            /*MessageBoxResult result = System.Windows.MessageBox.Show("Are you sure you want to exit?", "Exit", MessageBoxButton.YesNo);

            switch (result)
            {
                case MessageBoxResult.Yes:
                    timer.Stop();
                    if (currentMinutes != 0)
                    {
                        SendCollectedTime();
                    }
                    break;
                case MessageBoxResult.No:
                    e.Cancel = true;
                    break;
            }*/

            timer.Stop();
            idleTimer.Stop();

            if (currentMinutes != 0)
            {
                SendCollectedTime();
            }

            base.OnClosing(e);
        }

        private async void ProjectComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProjectComboBox.SelectedItem != null)
            {
                List<ProjectTask> tasks = await _crmConnector.RetrieveTasksAsync(((ComboBoxPairs)ProjectComboBox.SelectedItem).Value);
                taskCBP = new ObservableCollection<ComboBoxPairs1>();
                foreach (ProjectTask task in tasks)
                {
                    taskCBP.Add(new ComboBoxPairs1(task.TaskName, task));
                }
                TaskComboBox.ItemsSource = taskCBP;
            }
        }

        private void TaskComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TaskComboBox.SelectedItem != null)
            {
                PriorityLabel.Content = ((ComboBoxPairs1)TaskComboBox.SelectedItem).Value.Priority;
                if (((ComboBoxPairs1)TaskComboBox.SelectedItem).Value.DueDate != DateTime.MinValue)
                {
                    DueDateLabel.Content = ((ComboBoxPairs1)TaskComboBox.SelectedItem).Value.DueDate.AddDays(1).ToString("d");
                }
                else
                {
                    DueDateLabel.Content = "";
                }

                StartButton.IsEnabled = true;
            }
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            DescriptionBox.IsEnabled = false;
            StartButton.IsEnabled = false;
            StopButton.IsEnabled = true;
            CompleteButton.IsEnabled = false;

            RefreshButton.Visibility = Visibility.Hidden;

            TaskComboBox.IsEnabled = false;
            ProjectComboBox.IsEnabled = false;

            _crmConnector.UpdateTaskStatus(((ComboBoxPairs1)TaskComboBox.SelectedItem).Value.TaskId);

            await GetTime();
            SetTimer();
        }

        private void SetTimer()
        {
            timer.Interval = new TimeSpan(0, 1, 0);
            timer.Tick += TimerTick;
            timer.Start();
        }

        private async Task GetTime()
        {
            minutes = await _crmConnector.RetrieveTaskMinutesAsync(((ComboBoxPairs)ProjectComboBox.SelectedItem).Value, ((ComboBoxPairs1)TaskComboBox.SelectedItem).Value.TaskId);
            hours = minutes / 60;
            minutes = minutes % 60;
            if (hours < 10)
            {
                Time.Content = "0" + hours.ToString() + ":";
            }
            else
            {
                Time.Content = hours.ToString() + ":";
            }
            if (minutes < 10)
            {
                Time.Content += "0" + minutes.ToString();
            }
            else
            {
                Time.Content += minutes.ToString();
            }
        }

        void TimerTick(object sender, EventArgs e)
        {
            currentMinutes++;

            minutes++;
            if (minutes == 60)
            {
                minutes = 0;
                hours++;
            }
            if (hours < 10)
            {
                Time.Content = "0" + hours.ToString() + ":";
            }
            else
            {
                Time.Content = hours.ToString() + ":";
            }
            if (minutes < 10)
            {
                Time.Content += "0" + minutes.ToString();
            }
            else
            {
                Time.Content += minutes.ToString();
            }

            if (currentMinutes != 0 && currentMinutes % 20 == 0)
            {
                SendCollectedTime();
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;
            CompleteButton.IsEnabled = true;

            RefreshButton.Visibility = Visibility.Visible;

            TaskComboBox.IsEnabled = true;
            ProjectComboBox.IsEnabled = true;

            DescriptionBox.IsEnabled = true;

            timer.Tick -= TimerTick;
            timer.Stop();

            if(currentMinutes>0)
            {
                _crmConnector.AddMinutes(currentMinutes, ((ComboBoxPairs)ProjectComboBox.SelectedItem).Value, ((ComboBoxPairs1)TaskComboBox.SelectedItem).Value.TaskId, DescriptionBox.Text);
            }

            currentMinutes = 0;
            minutes = 0;
            hours = 0;
            Time.Content = "00:00";
        }

        private void CompleteButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = System.Windows.MessageBox.Show("Are you sure you want to complete the task?", "Complete Task", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                DescriptionBox.Text = "";
                _crmConnector.CompleteTaskStatus(((ComboBoxPairs1)TaskComboBox.SelectedItem).Value.TaskId);
                RefreshTasks();
            }
        }

        public void SendCollectedTime()
        {
            try
            {
                _crmConnector.AddMinutes(currentMinutes, ((ComboBoxPairs)ProjectComboBox.SelectedItem).Value, ((ComboBoxPairs1)TaskComboBox.SelectedItem).Value.TaskId, DescriptionBox.Text);
                currentMinutes = 0;
            }
            catch(Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
            }
        }

        public async void RefreshTasks()
        {
            PriorityLabel.Content = "";
            DueDateLabel.Content = "";

            StartButton.IsEnabled = false;
            StopButton.IsEnabled = false;
            CompleteButton.IsEnabled = false;
            //RefreshButton.IsEnabled = false;

            DescriptionBox.Text = "";

            if (taskCBP != null)
            {
                taskCBP.Clear();
            }

            if(projectCBP!=null)
            {
                projectCBP.Clear();
            }

            List<Project> projects = await _crmConnector.RetrieveProjectsAsync();
         
            foreach (Project project in projects)
            {
                projectCBP.Add(new ComboBoxPairs(project.ProjectName, project.ProjectId));
            }
            ProjectComboBox.ItemsSource = projectCBP;

            //RefreshButton.IsEnabled = true;
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshTasks();
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MenuSignOut_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            Close();
        }

        private void MenuNewMeeting_Click(object sender, RoutedEventArgs e)
        {
            NewMeetingWindow newMeetingWindow = new NewMeetingWindow(_crmConnector);
            newMeetingWindow.Owner = this;
            newMeetingWindow.ShowDialog();
        }

        private void MenuNewTask_Click(object sender, RoutedEventArgs e)
        {
            NewTaskWindow newTaskWindow = new NewTaskWindow(_crmConnector);
            newTaskWindow.Owner = this;
            newTaskWindow.ShowDialog();
        }

        private void MenuNewTfsTask_Click(object sender, RoutedEventArgs e)
        {
            NewTfsTaskWindow newTfsTaskWindow = new NewTfsTaskWindow(_crmConnector);
            newTfsTaskWindow.Owner = this;
            newTfsTaskWindow.ShowDialog();
        }
    }
}