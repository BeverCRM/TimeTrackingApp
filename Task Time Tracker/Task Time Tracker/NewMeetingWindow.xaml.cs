using System;
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
    /// Interaction logic for NewMeetingWindow.xaml
    /// </summary>
    public partial class NewMeetingWindow : Window
    {
        CrmConnector crmConnector;

        ObservableCollection<ComboBoxPairs> projectCBP;

        public NewMeetingWindow(CrmConnector Connector)
        {
            InitializeComponent();

            crmConnector = Connector;

            CompletedDatePicker.SelectedDate = DateTime.Now;

            RetrieveProjects();
        }

        private async void RetrieveProjects()
        {
            ProjectComboBox.DisplayMemberPath = "Text";
            ProjectComboBox.SelectedValuePath = "Value";

            projectCBP = new ObservableCollection<ComboBoxPairs>();
            List<Project> projects = await crmConnector.RetrieveAllProjectsAsync();

            foreach (Project project in projects)
            {
                projectCBP.Add(new ComboBoxPairs(project.ProjectName, project.ProjectId));
            }
            ProjectComboBox.ItemsSource = projectCBP;
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsInformationProvided())
            {
                if (decimal.TryParse(DurationBox.Text, out decimal duration))
                {
                    crmConnector.CreateMeeting(((ComboBoxPairs)ProjectComboBox.SelectedItem).Value, DescriptionBox.Text,
                        CompletedDatePicker.SelectedDate, duration * 60);

                    Close();
                }
                else
                {
                    DurationBox.Text = "";
                    MessageBox.Show("Please enter a valid duration!");
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

            return true;
        }
    }
}
