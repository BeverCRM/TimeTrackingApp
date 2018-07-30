using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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

        public string GetCurrentUserName()
        {
            Guid UserId = ((WhoAmIResponse)service.Execute(new WhoAmIRequest())).UserId;
            string username = service.Retrieve("systemuser", UserId, new ColumnSet("firstname")).GetAttributeValue<string>("firstname");
            return username;
        }

        public List<Project> RetrieveAllProjects()
        {
            List<Project> Projects = new List<Project>();
            
            string fetch = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' >" +
                           "<entity name='bvrcrm_project'>" +
                           "<attribute name='bvrcrm_name'/>" +
                           "<attribute name='bvrcrm_projectid'/>" +
                           "</entity>" +
                           "</fetch>";

            EntityCollection projects = service.RetrieveMultiple(new FetchExpression(fetch));

            foreach (Entity project in projects.Entities)
            {
                Project projectElement = new Project();
                projectElement.ProjectName = project["bvrcrm_name"].ToString();
                projectElement.ProjectId = (Guid)project["bvrcrm_projectid"];
                Projects.Add(projectElement);
            }

            return Projects;
        }

        public List<Project> RetrieveProjects()
        {
            List<Project> Projects = new List<Project>();
            Guid gUserId = ((WhoAmIResponse)service.Execute(new WhoAmIRequest())).UserId;

            string fetch = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' >";
            fetch += "<entity name='bvrcrm_project'>";
            fetch += "<attribute name='bvrcrm_name'/>";
            fetch += "<attribute name='bvrcrm_projectid'/>";
            fetch += "<link-entity name='bvrcrm_projecttask' from='bvrcrm_project' to='bvrcrm_projectid' alias = 'aa'>";
            fetch += "<filter type='and'>";
            fetch += "<condition attribute='ownerid' operator='eq' value='" + gUserId + "'/>";
            fetch += "<condition attribute='statuscode' operator='ne' value='2' />";
            fetch += "</filter>";
            fetch += "</link-entity>";
            fetch += "</entity>";
            fetch += "</fetch>";
            EntityCollection projects = service.RetrieveMultiple(new FetchExpression(fetch));
            foreach (Entity project in projects.Entities)
            {
                Project projectElement = new Project();
                projectElement.ProjectName = project["bvrcrm_name"].ToString();
                projectElement.ProjectId = (Guid)project["bvrcrm_projectid"];
                Projects.Add(projectElement);
            }

            return Projects;
        }

        public List<ProjectTask> RetrieveTasks(Guid projectId)
        {
            List<ProjectTask> Tasks = new List<ProjectTask>();
            Guid gUserId = ((WhoAmIResponse)service.Execute(new WhoAmIRequest())).UserId;

            string fetch = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' >";
            fetch += "<entity name='bvrcrm_projecttask'>";
            fetch += "<order attribute='bvrcrm_due_date' descending='false' />";
            fetch += "<attribute name='bvrcrm_projecttaskid'/>";
            fetch += "<attribute name='bvrcrm_name'/>";
            fetch += "<attribute name='statuscode'/>";
            fetch += "<attribute name='bvrcrm_priority'/>";
            fetch += "<attribute name='bvrcrm_due_date'/>";
            fetch += "<filter type='and'>";
            fetch += "<condition attribute='ownerid' operator='eq' value='" + gUserId + "'/>";
            fetch += "<condition attribute='statuscode' operator='ne' value='2' />";
            fetch += "<condition attribute='bvrcrm_project' operator='eq' value='" + projectId + "'/>";
            fetch += "</filter>";
            fetch += "</entity>";
            fetch += "</fetch>";

            EntityCollection tasks = service.RetrieveMultiple(new FetchExpression(fetch));

            foreach (Entity task in tasks.Entities)
            {
                ProjectTask projectTask = new ProjectTask();
                projectTask.TaskName = task["bvrcrm_name"].ToString();
                projectTask.TaskId = (Guid)task["bvrcrm_projecttaskid"];
                projectTask.Status = ((OptionSetValue)task["statuscode"]).Value;
                if (task.Contains("bvrcrm_due_date") && task["bvrcrm_due_date"] != null)
                    projectTask.DueDate = (DateTime)task["bvrcrm_due_date"];

                if (task.Contains("bvrcrm_priority") && task["bvrcrm_priority"] != null)
                {
                    int priorityValue = ((OptionSetValue)task["bvrcrm_priority"]).Value;
                    switch (priorityValue)
                    {
                        case 744240000:
                            projectTask.Priority = "High";
                            break;
                        case 744240001:
                            projectTask.Priority = "Medium";
                            break;
                        case 744240002:
                            projectTask.Priority = "Low";
                            break;
                    }
                }

                Tasks.Add(projectTask);
            }

            return Tasks;
        }

        public void AddMinutes(int minutes, Guid projectId, Guid taskId, string description)
        {
            Guid gUserId = ((WhoAmIResponse)service.Execute(new WhoAmIRequest())).UserId;
            DateTime thisDay = DateTime.Now;

            string fetch = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' >";
            fetch += "<entity name='bvrcrm_timetracking'>";
            fetch += "<attribute name='bvrcrm_minutes'/>";
            fetch += "<attribute name='bvrcrm_timetrackingid'/>";
            fetch += "<filter type='and'>";
            fetch += "<condition attribute='bvrcrm_tracking_date' operator='eq' value='" + thisDay.ToString("MM/dd/yyyy hh:mm:ss") + "'/>";
            fetch += "<condition attribute='ownerid' operator='eq' value='" + gUserId +"'/>";
            fetch += "<condition attribute='bvrcrm_project' operator='eq' value='" + projectId + "'/>";
            fetch += "<condition attribute='bvrcrm_task' operator='eq' value='" + taskId + "'/>";
            fetch += "<condition attribute='bvrcrm_name' operator='eq' value='" + description + "'/>";
            fetch += "</filter>";
            fetch += "</entity>";
            fetch += "</fetch>";

            EntityCollection timeTrackings = service.RetrieveMultiple(new FetchExpression(fetch));
            if (timeTrackings.Entities.Count() == 1)
            {
                foreach (Entity timeTracking in timeTrackings.Entities)
                {
                    timeTracking["bvrcrm_minutes"] = (Decimal)timeTracking["bvrcrm_minutes"] + Convert.ToDecimal(minutes);
                    service.Update(timeTracking);
                }
            }
            else
            {
                Entity timeTracking = new Entity("bvrcrm_timetracking");

                timeTracking["bvrcrm_tracking_date"] = thisDay;
                timeTracking["bvrcrm_minutes"] = Convert.ToDecimal(minutes);
                timeTracking["bvrcrm_name"] = description;
                timeTracking["bvrcrm_project"] = new EntityReference("bvrcrm_project", projectId);
                timeTracking["bvrcrm_task"] = new EntityReference("bvrcrm_projecttask", taskId);

                service.Create(timeTracking);
            }
        }

        public void CompleteTaskStatus(Guid taskId)
        {
            Entity Task = service.Retrieve("bvrcrm_projecttask", taskId, new ColumnSet(false));

            Task["bvrcrm_completed_date"] = DateTime.Now;

            service.Update(Task);

            SetStateRequest setTaskCompleted = new SetStateRequest()
            {
                EntityMoniker = new EntityReference
                {
                    Id = taskId,
                    LogicalName = "bvrcrm_projecttask"
                },
                State = new OptionSetValue(1),
                Status = new OptionSetValue(2)
            };

            service.Execute(setTaskCompleted);         
        }

        public void UpdateTaskStatus(Guid taskId)
        {
            Entity task = service.Retrieve("bvrcrm_projecttask", taskId, new ColumnSet(false));
            task["statuscode"] = new OptionSetValue(744240000);
            service.Update(task);
        }

        public int RetrieveTaskMinutes(Guid projectId,Guid taskId)
        {
            int minutes = 0;
            Guid gUserId = ((WhoAmIResponse)service.Execute(new WhoAmIRequest())).UserId;

            string fetch = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false' >";
            fetch += "<entity name='bvrcrm_timetracking'>";
            fetch += "<attribute name='bvrcrm_minutes'/>";
            fetch += "<filter type='and'>";
            fetch += "<condition attribute='ownerid' operator='eq' value='" + gUserId + "'/>";
            fetch += "<condition attribute='bvrcrm_project' operator='eq' value='" + projectId + "' />";
            fetch += "<condition attribute='bvrcrm_task' operator='eq' value='" + taskId + "'/>";
            fetch += "</filter>";
            fetch += "</entity>";
            fetch += "</fetch>";
            EntityCollection timeTrackings = service.RetrieveMultiple(new FetchExpression(fetch));

            foreach (Entity timeTracking in timeTrackings.Entities)
            {
                minutes += Convert.ToInt32((Decimal)timeTracking["bvrcrm_minutes"]);
            }

            return minutes;
        }

        public void CreateMeeting(Guid projectId, string description, DateTime? completedDate, Decimal duration)
        {
            try
            {
                /// Creates a project for the meeting
                Entity project = service.Retrieve("bvrcrm_project", projectId, new ColumnSet("bvrcrm_customer"));
                MessageBox.Show(duration.ToString());
                Entity task = new Entity("bvrcrm_projecttask");

                task["bvrcrm_name"] = description;
                task["bvrcrm_customer"] = project.GetAttributeValue<EntityReference>("bvrcrm_customer");
                task["bvrcrm_project"] = project.ToEntityReference();

                if (completedDate != null)
                    task["bvrcrm_completed_date"] = completedDate;

                Guid taskId = service.Create(task);

                /// Creates a time tracking record for the recently created project
                Entity timeTracking = new Entity("bvrcrm_timetracking");

                timeTracking["bvrcrm_minutes"] = duration;
                timeTracking["bvrcrm_name"] = description;
                timeTracking["bvrcrm_project"] = project.ToEntityReference();
                timeTracking["bvrcrm_task"] = new EntityReference("bvrcrm_projecttask", taskId);

                if (completedDate != null)
                    timeTracking["bvrcrm_tracking_date"] = completedDate;

                service.Create(timeTracking);

                /// Sets the status of recently created project to completed
                SetStateRequest setTaskCompleted = new SetStateRequest()
                {
                    EntityMoniker = new EntityReference
                    {
                        Id = taskId,
                        LogicalName = "bvrcrm_projecttask"
                    },
                    State = new OptionSetValue(1),
                    Status = new OptionSetValue(2)
                };

                service.Execute(setTaskCompleted);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
