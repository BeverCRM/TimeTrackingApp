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
using Task_Time_Tracker.Model;
using Task_Time_Tracker.Utility_Functions;

namespace Task_Time_Tracker
{
    /// <summary>
    /// Interaction logic for TimeTrackerWindow.xaml
    /// </summary>
    public partial class TimeTrackerWindow : Window
    {
        ProjectTask ProjectTask = new ProjectTask();
        CRM_Connector crmConnector;
        List<Tuple<string,Guid>> projects;
        List<Tuple<string, Guid>> tasks;

        public TimeTrackerWindow(CRM_Connector Connector)
        {
            InitializeComponent();

            crmConnector = Connector;
            string loginUserName = crmConnector.username;
            string userName = "";
            for (int i = 6; i < loginUserName.Length; i++)
            {
                userName += loginUserName[i];
            }
            CurrentUserBox.Text = userName;
            CurrentUserBox.IsEnabled = false;

            projects = ProjectTask.retrieveProjects(crmConnector);
            ProjectComboBox.DisplayMemberPath = "Text";
            ProjectComboBox.SelectedValuePath = "Value";
            List<ComboBoxPairs> projectCBP = new List<ComboBoxPairs>();
            foreach (Tuple<string,Guid> project in projects)
            {
                projectCBP.Add(new ComboBoxPairs(project.Item1, project.Item2));
            }
            ProjectComboBox.ItemsSource = projectCBP;
        }

        private void ProjectComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            tasks = ProjectTask.retrieveTasks(crmConnector, ((ComboBoxPairs)ProjectComboBox.SelectedItem).Value);
            TaskComboBox.DisplayMemberPath = "Text";
            TaskComboBox.SelectedValuePath = "Value";
            List<ComboBoxPairs> taskCBP = new List<ComboBoxPairs>();
            foreach (Tuple<string, Guid> task in tasks)
            {
                taskCBP.Add(new ComboBoxPairs(task.Item1, task.Item2));
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
    }
}
