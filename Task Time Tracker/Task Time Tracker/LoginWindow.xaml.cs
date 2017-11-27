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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public CRM_Connector crmConnector;
        
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Login();
        }

        private void onPasswordBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                Login();
        }

        private void Login()
        {
            string userName = LoginBox.Text;
            string password = PasswordBox.Password;

            if (userName == "" || password == "")
                MessageBox.Show("Please provide your CRM credentials.");
            else
            {
                crmConnector = new CRM_Connector(userName, password, "https://bever.bever.am/XRMServices/2011/Organization.svc");

                Tuple<string, string> connectionStatus = crmConnector.Connect_To_MSCRM();

                if (connectionStatus.Item1 != "0")
                    MessageBox.Show(connectionStatus.Item2);
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
