using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Task_Time_Tracker.Model;
using Task_Time_Tracker.Utility_Functions;

namespace Task_Time_Tracker
{
    /// <summary>
    /// Interaction logic for NewMeetingWindow.xaml
    /// </summary>
    public partial class NewMeetingWindow : Window
    {
        CRM_Connector crmConnector;

        ObservableCollection<ComboBoxPairs> projectCBP;

        public NewMeetingWindow(CRM_Connector Connector)
        {
            InitializeComponent();

            crmConnector = Connector;

            ProjectComboBox.DisplayMemberPath = "Text";
            ProjectComboBox.SelectedValuePath = "Value";

            projectCBP = new ObservableCollection<ComboBoxPairs>();
            List<Project> projects = crmConnector.RetrieveAllProjects();

            foreach (Project project in projects)
            {
                projectCBP.Add(new ComboBoxPairs(project.ProjectName, project.ProjectId));
            }
            ProjectComboBox.ItemsSource = projectCBP;

            CompletedDatePicker.DataContext = DateTime.Now;
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (Decimal.TryParse(Duration.Text, out Decimal duration))
            {
                crmConnector.CreateMeeting(((ComboBoxPairs)ProjectComboBox.SelectedItem).Value, DescriptionBox.Text,
                    CompletedDatePicker.SelectedDate, duration * 60);

                Close();
            }
            else
            {
                Duration.Text = "";
                MessageBox.Show("Please enter a valid duration!");
            }
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("^[.][0-9]+$|^[0-9]*[.]{0,1}[0-9]*$");
            e.Handled = !regex.IsMatch(e.Text);
        }
    }
}
