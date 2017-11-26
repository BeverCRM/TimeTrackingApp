using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using Task_Time_Tracker.Model;

namespace Task_Time_Tracker.Utility_Functions
{
    public class CRM_Connector
    {
        public string username;
        private string password;
        private string soap_uri;
        
        public IOrganizationService service;

        public CRM_Connector(string Username, string Password, string Soap_Service_URI)
        {
            username = Username;
            password = Password;
            soap_uri = Soap_Service_URI;
        }

        public Tuple<string, string> Connect_To_MSCRM()
        {

            string connection_code = "0";
            string error_message = "";

            try
            {
                ClientCredentials credentials = new ClientCredentials();
                credentials.UserName.UserName = username;
                credentials.UserName.Password = password;
                Uri serviceUri = new Uri(soap_uri);
                using (OrganizationServiceProxy proxy = new OrganizationServiceProxy(serviceUri, null, credentials, null))
                {

                    proxy.EnableProxyTypes();
                    proxy.Authenticate();
                    proxy.Timeout = new TimeSpan(0, 30, 0);
                    service = (IOrganizationService)proxy;
                    //  _proxy=proxy;
                }

            }
            catch (Exception ex)
            {
                connection_code = "-1";
                error_message = ex.Message;
            }

            return new Tuple<string, string>(connection_code, error_message);
        }
    }
}
