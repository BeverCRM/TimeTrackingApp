using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Task_Time_Tracker.Model;
using Task_Time_Tracker.Utility_Functions;

namespace Task_Time_Tracker
{
    /// <summary>
    /// Interaction logic for NewTfsTaskWindow.xaml
    /// </summary>
    public partial class NewTfsTaskWindow : Window
    {
        private readonly CrmConnector _crmConnector;

        private ObservableCollection<ComboBoxPairs> projectCBP;

        public NewTfsTaskWindow(CrmConnector connector)
        {
            InitializeComponent();

            _crmConnector = connector;

            RetrieveProjects();
        }

        private async void RetrieveProjects()
        {
            ProjectComboBox.DisplayMemberPath = "Text";
            ProjectComboBox.SelectedValuePath = "Value";

            projectCBP = new ObservableCollection<ComboBoxPairs>();
            List<Project> projects = await _crmConnector.RetrieveAllProjectsAsync();

            foreach (Project project in projects)
            {
                projectCBP.Add(new ComboBoxPairs(project.ProjectName, project.ProjectId));
            }
            ProjectComboBox.ItemsSource = projectCBP;
        }

        private async void GetButton_Click(object sender, RoutedEventArgs e)
        {
            GetButton.IsEnabled = false;

            if (IsInformationProvided() && await IsTfsInformationProvided())
            {
                List<ProjectTask> tasks = await _crmConnector.GetTfsTasks(((ComboBoxPairs)ProjectComboBox.SelectedItem).Value, TaskIdsBox.Text);

                foreach (ProjectTask task in tasks)
                {
                    if (!await OwnerIsCurrentUser(task.OwnerId))
                    {
                        MessageBox.Show($"You're not the owner of a task with id: {task.TfsId}");
                        continue;
                    }

                    if (await TaskAlreadyExists(task.TfsId))
                    {
                        MessageBox.Show($"Task with id: {task.TfsId} already exists");
                        continue;
                    }

                    if (!OwnerExists(task.OwnerId))
                    {
                        MessageBox.Show($"The owner of task with id: {task.TfsId} does not exist in CRM");
                        continue;
                    }

                    _crmConnector.CreateTask(((ComboBoxPairs)ProjectComboBox.SelectedItem).Value, task.TaskName, task.OwnerId,
                        task.Priority, task.DueDate, task.EstimatedHours, task.Description, task.TfsId);
                }

                Close();
            }
            else
            {
                GetButton.IsEnabled = true;
            }
        }

        private bool OwnerExists(Guid ownerId)
        {
            return ownerId != Guid.Empty;
        }

        private async Task<bool> TaskAlreadyExists(int tfsId)
        {
            return await _crmConnector.TfsTaskExists(((ComboBoxPairs)ProjectComboBox.SelectedItem).Value, tfsId);
        }

        private async Task<bool> OwnerIsCurrentUser(Guid ownerId)
        {
            return await _crmConnector.GetUserId() == ownerId;
        }

        private async Task<bool> IsTfsInformationProvided()
        {
            if (!await _crmConnector.TfsInformationExists(((ComboBoxPairs)ProjectComboBox.SelectedItem).Value))
            {
                MessageBox.Show("The provided project has no tfs information provided");
                return false;
            }

            return true;
        }

        private bool IsInformationProvided()
        {
            if (ProjectComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please choose a valid project!");
                return false;
            }

            if (TaskIdsBox.Text == "")
            {
                MessageBox.Show("Please enter a task id!");
                return false;
            }

            return true;
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("^[,][0-9]+$|^[0-9]*[,]{0,1}[0-9]*$");
            e.Handled = !regex.IsMatch(e.Text);
        }
    }
}
