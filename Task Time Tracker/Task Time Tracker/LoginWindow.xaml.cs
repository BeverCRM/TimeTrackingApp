using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Task_Time_Tracker.Utility_Functions;

namespace Task_Time_Tracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CrmConnector crmConnector;
        
        public MainWindow()
        {
            string thisprocessname = Process.GetCurrentProcess().ProcessName;

            if (Process.GetProcesses().Count(p => p.ProcessName == thisprocessname) > 1)
            {
                MessageBox.Show("Instance already running");
                Close();
            }
            else
            {
                InitializeComponent();
                LoginBox.Focus();
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            Login();
        }

        private void OnPasswordBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                Login();
        }

        private async void Login()
        {
            string userName = LoginBox.Text;
            string password = PasswordBox.Password;

            if (userName == "" || password == "")
            {
                MessageBox.Show("Please provide your CRM credentials.");
            }
            else
            {
                crmConnector = new CrmConnector(userName, password, "https://bever.bever.am/XRMServices/2011/Organization.svc");

                Tuple<string, string> connectionStatus = await crmConnector.ConnectToMSCRMAsync();

                if (connectionStatus.Item1 != "0")
                {
                    MessageBox.Show(connectionStatus.Item2);
                }
                else
                {
                    TimeTrackerWindow timeTrackerWindow = new TimeTrackerWindow(crmConnector);
                    timeTrackerWindow.Show();
                    Close();
                }
            }
        }
    }
}
