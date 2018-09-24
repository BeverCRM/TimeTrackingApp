using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using Task_Time_Tracker.Model;
using Task_Time_Tracker.Utility_Functions;

namespace Task_Time_Tracker
{
    /// <summary>
    /// Interaction logic for NewTaskWindow.xaml
    /// </summary>
    public partial class NewTaskWindow : Window
    {
        private readonly CrmConnector _crmConnector;

        private ObservableCollection<ComboBoxPairs> projectCBP;
        private ObservableCollection<ComboBoxPairs> userCBP;

        public NewTaskWindow(CrmConnector connector)
        {
            InitializeComponent();

            _crmConnector = connector;

            RetrieveProjects();
            RetrieveUsers();
        }

        private async void RetrieveProjects()
        {
            /// Set Project Combo Box Display Member and Value
            ProjectComboBox.DisplayMemberPath = "Text";
            ProjectComboBox.SelectedValuePath = "Value";

            /// Retrieve all the projects
            projectCBP = new ObservableCollection<ComboBoxPairs>();
            List<Project> projects = await _crmConnector.RetrieveAllProjectsAsync();

            foreach (Project project in projects)
            {
                projectCBP.Add(new ComboBoxPairs(project.ProjectName, project.ProjectId));
            }
            ProjectComboBox.ItemsSource = projectCBP;
        }

        private async void RetrieveUsers()
        {
            /// Set Responsible Combo Box Display Member and Value
            ResponsibleComboBox.DisplayMemberPath = "Text";
            ResponsibleComboBox.SelectedValuePath = "Value";

            /// Retrieve all the users
            userCBP = new ObservableCollection<ComboBoxPairs>();
            List<User> users = await _crmConnector.RetrieveAllUsersAsync();

            foreach (User user in users)
            {
                userCBP.Add(new ComboBoxPairs(user.UserName, user.UserId));
            }
            ResponsibleComboBox.ItemsSource = userCBP;
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsInformationProvided())
            {
                if (decimal.TryParse(EstimatedHoursBox.Text, out decimal estimatedHours))
                {
                    string priority = PriorityComboBox.Text;
                    _crmConnector.CreateTask(((ComboBoxPairs)ProjectComboBox.SelectedItem).Value, TaskNameBox.Text,
                        ((ComboBoxPairs)ResponsibleComboBox.SelectedItem).Value, PriorityComboBox.Text,
                        DueDatePicker.SelectedDate, estimatedHours, DescriptionBox.Text);

                    Close();
                }
                else
                {
                    EstimatedHoursBox.Text = "";
                    MessageBox.Show("Please enter a valid estimation of hours!");
                }
            }
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("^[.][0-9]+$|^[0-9]*[.]{0,1}[0-9]*$");
            e.Handled = !regex.IsMatch(e.Text);
        }

        private bool IsInformationProvided()
        {
            if (ProjectComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please choose a valid project!");
                return false;
            }

            if (TaskNameBox.Text == "")
            {
                MessageBox.Show("Please enter a task name!");
                return false;
            }

            return true;
        }
    }
}
