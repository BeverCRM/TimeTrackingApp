using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
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
       // ProjectTask ProjectTask = new ProjectTask();
        CRM_Connector crmConnector;
        List<Project> projects;
        List<ProjectTask> tasks;

        DispatcherTimer timer;

        int minutes = 0;
        int hours = 0;

        public TimeTrackerWindow(CRM_Connector Connector)
        {
            InitializeComponent();

            timer = new DispatcherTimer();

            crmConnector = Connector;

            string loginUserName = crmConnector.username;
            string userName = "";
            for (int i = 6; i < loginUserName.Length; i++)
            {
                userName += loginUserName[i];
            }
            CurrentUserBox.Text = userName;
            CurrentUserBox.IsEnabled = false;

            projects = crmConnector.retrieveProjects();
            ProjectComboBox.DisplayMemberPath = "Text";
            ProjectComboBox.SelectedValuePath = "Value";
            List<ComboBoxPairs> projectCBP = new List<ComboBoxPairs>();
            foreach (Project project in projects)
            {
                projectCBP.Add(new ComboBoxPairs(project.projectName, project.projectId));
            }
            ProjectComboBox.ItemsSource = projectCBP;

            StartButton.IsEnabled = false;
            StopButton.IsEnabled = false;
        }

        private void ProjectComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            tasks = crmConnector.retrieveTasks(((ComboBoxPairs)ProjectComboBox.SelectedItem).Value);
            TaskComboBox.DisplayMemberPath = "Text";
            TaskComboBox.SelectedValuePath = "Value";
            List<ComboBoxPairs> taskCBP = new List<ComboBoxPairs>();
            foreach (ProjectTask task in tasks)
            {
                taskCBP.Add(new ComboBoxPairs(task.taskName, task.taskId));
            }
            TaskComboBox.ItemsSource = taskCBP;
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

        private void TaskComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            StartButton.IsEnabled = true;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StartButton.IsEnabled = false;
            StopButton.IsEnabled = true;

            TaskComboBox.IsEnabled = false;
            ProjectComboBox.IsEnabled = false;

            timer.Interval = TimeSpan.FromMinutes(1);
            timer.Tick += timerTick;
            timer.Start();
        }

        void timerTick(object sender, EventArgs e)
        {
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
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;

            TaskComboBox.IsEnabled = true;
            ProjectComboBox.IsEnabled = true;

            timer.Stop();
            crmConnector.addMinutes(minutes + 60 * hours, ((ComboBoxPairs)ProjectComboBox.SelectedItem).Value, ((ComboBoxPairs)TaskComboBox.SelectedItem).Value, DescriptionBox.Text);
        }
    }
}
