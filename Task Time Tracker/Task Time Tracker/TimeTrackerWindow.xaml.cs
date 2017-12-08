using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Resources;
using System.Windows.Shapes;
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
        CRM_Connector crmConnector;

       // List<Project> projects;
      //  List<ProjectTask> tasks;

        ObservableCollection<ComboBoxPairs1> taskCBP;
        ObservableCollection<ComboBoxPairs> projectCBP;

        DispatcherTimer timer;
        int minutes = 0;
        int hours = 0;
        int currentMinutes = 0;

        NotifyIcon ni;

        public TimeTrackerWindow(CRM_Connector Connector)
        {
            InitializeComponent();

            ni = new NotifyIcon();
            StreamResourceInfo sri = System.Windows.Application.GetResourceStream(new Uri("Task Time Tracker;component/Resources/SystemTray.ico", UriKind.Relative));
            ni.Icon = new Icon(sri.Stream);
            ni.Visible = false;
            ni.Click +=
                delegate (object sender, EventArgs args)
                {
                    Show();
                    WindowState = WindowState.Normal;
                    ni.Visible = false;
                };

            timer = new DispatcherTimer();

            crmConnector = Connector;

            currentUserLabel.Content = "Welcome, " + crmConnector.getCurrentUserName();

            TaskComboBox.DisplayMemberPath = "Text";
            TaskComboBox.SelectedValuePath = "Value";
            
            ProjectComboBox.DisplayMemberPath = "Text";
            ProjectComboBox.SelectedValuePath = "Value";

            List<Project> projects = crmConnector.retrieveProjects();
            projectCBP = new ObservableCollection<ComboBoxPairs>();
            foreach (Project project in projects)
            {
                projectCBP.Add(new ComboBoxPairs(project.projectName, project.projectId));
            }
            ProjectComboBox.ItemsSource = projectCBP;

            StartButton.IsEnabled = false;
            StopButton.IsEnabled = false;
            CompleteButton.IsEnabled = false;
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
                Hide();

            ni.Visible = true;
            base.OnStateChanged(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            MessageBoxResult result = System.Windows.MessageBox.Show("Are you sure you want to exit?", "Exit", MessageBoxButton.YesNo);

            switch (result)
            {
                case MessageBoxResult.Yes:
                    timer.Stop();
                    if (currentMinutes != 0 && currentMinutes % 20 == 0)
                        SendCollectedTime();
                    break;
                case MessageBoxResult.No:
                    e.Cancel = true;
                    break;
            }

            base.OnClosing(e);
        }

        private void ProjectComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProjectComboBox.SelectedItem != null)
            {
                List<ProjectTask> tasks = crmConnector.retrieveTasks(((ComboBoxPairs)ProjectComboBox.SelectedItem).Value);
                taskCBP = new ObservableCollection<ComboBoxPairs1>();
                foreach (ProjectTask task in tasks)
                {
                    taskCBP.Add(new ComboBoxPairs1(task.taskName, task));
                }
                TaskComboBox.ItemsSource = taskCBP;
            }
        }

        public class ComboBoxPairs
        {
            public string Text { get; set; }
            public Guid Value { get; set; }

            public ComboBoxPairs(string key, Guid value)
            {
                Text = key;
                Value = value;
            }
        }

        public class ComboBoxPairs1
        {
            public string Text { get; set; }
            public ProjectTask Value { get; set; }

            public ComboBoxPairs1(string key, ProjectTask value)
            {
                Text = key;
                Value = value;
            }
        }

        private void TaskComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TaskComboBox.SelectedItem != null)
            {
                PriorityLabel.Content = ((ComboBoxPairs1)TaskComboBox.SelectedItem).Value.priority;
                DueDateLabel.Content = ((ComboBoxPairs1)TaskComboBox.SelectedItem).Value.dueDate.ToString("d");
                StartButton.IsEnabled = true;
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (DescriptionBox.Text != "")
            {
                DescriptionBox.IsEnabled = false;
                StartButton.IsEnabled = false;
                StopButton.IsEnabled = true;
                CompleteButton.IsEnabled = false;

                RefreshButton.Visibility = Visibility.Hidden;

                TaskComboBox.IsEnabled = false;
                ProjectComboBox.IsEnabled = false;

                minutes = crmConnector.retrieveTaskMinutes(((ComboBoxPairs)ProjectComboBox.SelectedItem).Value, ((ComboBoxPairs1)TaskComboBox.SelectedItem).Value.taskId);
                hours = minutes / 60;
                minutes = minutes % 60;
                if (hours < 10)
                    Time.Content = "0" + hours.ToString() + ":";
                else
                    Time.Content = hours.ToString() + ":";
                if (minutes < 10)
                    Time.Content += "0" + minutes.ToString();
                else
                    Time.Content += minutes.ToString();

                timer.Interval = new TimeSpan(0, 1, 0);
                timer.Tick += timerTick;
                timer.Start();

                crmConnector.updateTaskStatus(((ComboBoxPairs1)TaskComboBox.SelectedItem).Value.taskId);
            }
            else
            {
                System.Windows.MessageBox.Show("Please provide description.");
            }
        }

        void timerTick(object sender, EventArgs e)
        {
            currentMinutes++;

            minutes++;
            if (minutes == 60)
            {
                minutes = 0;
                hours++;
            }
            if (hours < 10)
                Time.Content = "0" + hours.ToString() + ":";
            else
                Time.Content = hours.ToString() + ":";
            if (minutes < 10)
                Time.Content += "0" + minutes.ToString();
            else
                Time.Content += minutes.ToString();

            if (currentMinutes != 0 && currentMinutes % 20==0)
                SendCollectedTime();
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

            timer.Tick -= timerTick;
            timer.Stop();

            if(currentMinutes>0)
                crmConnector.addMinutes(currentMinutes, ((ComboBoxPairs)ProjectComboBox.SelectedItem).Value, ((ComboBoxPairs1)TaskComboBox.SelectedItem).Value.taskId, DescriptionBox.Text);

            currentMinutes = 0;
            minutes = 0;
            hours = 0;
            Time.Content = "00:00";
        }

        private void onCompletebuttonclick(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = System.Windows.MessageBox.Show("Are you sure you want to complete the task?", "Complete Task", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                DescriptionBox.Text = "";
                crmConnector.completeTaskStatus(((ComboBoxPairs1)TaskComboBox.SelectedItem).Value.taskId);
                refreshTasks();
            }
        }


        public void SendCollectedTime()
        {
            try
            {
                crmConnector.addMinutes(currentMinutes, ((ComboBoxPairs)ProjectComboBox.SelectedItem).Value, ((ComboBoxPairs1)TaskComboBox.SelectedItem).Value.taskId, DescriptionBox.Text);
                currentMinutes = 0;
            }
            catch(Exception ex)
            {

            }
        }

        public void refreshTasks()
        {
            PriorityLabel.Content = "";
            DueDateLabel.Content = "";

            StartButton.IsEnabled = false;
            StopButton.IsEnabled = false;
            CompleteButton.IsEnabled = false;

            DescriptionBox.Text = "";

            if(taskCBP!=null)
            taskCBP.Clear();

            if(projectCBP!=null)
            projectCBP.Clear();

            List<Project> projects = crmConnector.retrieveProjects();
         
            foreach (Project project in projects)
            {
                projectCBP.Add(new ComboBoxPairs(project.projectName, project.projectId));
            }
            ProjectComboBox.ItemsSource = projectCBP;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            refreshTasks();
        }
    }
}