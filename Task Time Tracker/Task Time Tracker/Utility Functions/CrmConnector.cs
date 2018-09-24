using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Description;
using System.Threading.Tasks;
using System.Windows;
using Task_Time_Tracker.Model;
using Task_Time_Tracker.Model.TFS;

namespace Task_Time_Tracker.Utility_Functions
{
    public class CrmConnector
    {
        private readonly string _username;
        private readonly string _password;
        private readonly string _soapUri;

        private IOrganizationService service;

        public CrmConnector(string username, string password, string soapUri)
        {
            _username = username;
            _password = password;
            _soapUri = soapUri;
        }

        public async Task<Tuple<string, string>> ConnectToMSCRMAsync()
        {
            string connectionCode = "0";
            string errorMessage = "";

            try
            {
                ClientCredentials credentials = new ClientCredentials();
                credentials.UserName.UserName = _username;
                credentials.UserName.Password = _password;
                Uri serviceUri = new Uri(_soapUri);

                using (OrganizationServiceProxy proxy = new OrganizationServiceProxy(serviceUri, null, credentials, null))
                {
                    await Task.Run(() => proxy.Authenticate());
                    service = proxy;
                }

            }
            catch (Exception ex)
            {
                connectionCode = "-1";
                errorMessage = ex.Message;
            }

            return new Tuple<string, string>(connectionCode, errorMessage);
        }

        public async Task<string> GetCurrentUserNameAsync()
        {
            WhoAmIResponse whoAmIResponse = await Task.Run(() => (WhoAmIResponse)service.Execute(new WhoAmIRequest()));
            Guid UserId = whoAmIResponse.UserId;
            Entity user = await Task.Run(() => service.Retrieve("systemuser", UserId, new ColumnSet("firstname")));
            return user.GetAttributeValue<string>("firstname");
        }

        public async Task<List<Project>> RetrieveAllProjectsAsync()
        {
            List<Project> projects = new List<Project>();

            string fetch = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' >" +
                           "<entity name='bvrcrm_project'>" +
                           "<attribute name='bvrcrm_name'/>" +
                           "<attribute name='bvrcrm_projectid'/>" +
                           "</entity>" +
                           "</fetch>";

            EntityCollection projectCollection = await Task.Run(() => service.RetrieveMultiple(new FetchExpression(fetch)));

            foreach (Entity project in projectCollection.Entities)
            {
                Project projectElement = new Project
                {
                    ProjectName = project["bvrcrm_name"].ToString(),
                    ProjectId = (Guid)project["bvrcrm_projectid"]
                };
                projects.Add(projectElement);
            }

            return projects;
        }



        public async Task<List<User>> RetrieveAllUsersAsync()
        {
            List<User> users = new List<User>();

            string fetch = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' >" +
                           "<entity name='systemuser'>" +
                           "<attribute name='fullname'/>" +
                           "<attribute name='systemuserid'/>" +
                           "</entity>" +
                           "</fetch>";

            EntityCollection userCollection = await Task.Run(() => service.RetrieveMultiple(new FetchExpression(fetch)));

            foreach (Entity user in userCollection.Entities)
            {
                User userElement = new User
                {
                    UserName = user.GetAttributeValue<string>("fullname"),
                    UserId = (Guid)user["systemuserid"]
                };
                users.Add(userElement);
            }

            return users;
        }

        public async Task<List<Project>> RetrieveProjectsAsync()
        {
            List<Project> Projects = new List<Project>();
            Guid gUserId = await GetUserId();

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
            EntityCollection projects = await Task.Run(() => service.RetrieveMultiple(new FetchExpression(fetch)));
            foreach (Entity project in projects.Entities)
            {
                Project projectElement = new Project
                {
                    ProjectName = project["bvrcrm_name"].ToString(),
                    ProjectId = (Guid)project["bvrcrm_projectid"]
                };
                Projects.Add(projectElement);
            }

            return Projects;
        }

        public async Task<List<ProjectTask>> RetrieveTasksAsync(Guid projectId)
        {
            List<ProjectTask> Tasks = new List<ProjectTask>();
            Guid gUserId = await GetUserId();

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

            EntityCollection tasks = await Task.Run(() => service.RetrieveMultiple(new FetchExpression(fetch)));

            foreach (Entity task in tasks.Entities)
            {
                ProjectTask projectTask = new ProjectTask
                {
                    TaskName = task["bvrcrm_name"].ToString(),
                    TaskId = (Guid)task["bvrcrm_projecttaskid"],
                    Status = ((OptionSetValue)task["statuscode"]).Value
                };
                if (task.Contains("bvrcrm_due_date") && task["bvrcrm_due_date"] != null)
                {
                    projectTask.DueDate = (DateTime)task["bvrcrm_due_date"];
                }

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

        public async void AddMinutes(int minutes, Guid projectId, Guid taskId, string description)
        {
            Guid gUserId = await GetUserId();
            DateTime thisDay = DateTime.Now;

            string fetch = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' >";
            fetch += "<entity name='bvrcrm_timetracking'>";
            fetch += "<attribute name='bvrcrm_minutes'/>";
            fetch += "<attribute name='bvrcrm_timetrackingid'/>";
            fetch += "<filter type='and'>";
            fetch += "<condition attribute='bvrcrm_tracking_date' operator='eq' value='" + thisDay.ToString("MM/dd/yyyy hh:mm:ss") + "'/>";
            fetch += "<condition attribute='ownerid' operator='eq' value='" + gUserId + "'/>";
            fetch += "<condition attribute='bvrcrm_project' operator='eq' value='" + projectId + "'/>";
            fetch += "<condition attribute='bvrcrm_task' operator='eq' value='" + taskId + "'/>";
            fetch += "<condition attribute='bvrcrm_name' operator='eq' value='" + description + "'/>";
            fetch += "</filter>";
            fetch += "</entity>";
            fetch += "</fetch>";

            EntityCollection timeTrackings = await Task.Run(() => service.RetrieveMultiple(new FetchExpression(fetch)));
            if (timeTrackings.Entities.Count() == 1)
            {
                foreach (Entity timeTracking in timeTrackings.Entities)
                {
                    timeTracking["bvrcrm_minutes"] = (Decimal)timeTracking["bvrcrm_minutes"] + Convert.ToDecimal(minutes);
                    await Task.Run(() => service.Update(timeTracking));
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

                await Task.Run(() => service.Create(timeTracking));
            }
        }

        public async void CompleteTaskStatus(Guid taskId)
        {
            Entity task = await Task.Run(() => service.Retrieve("bvrcrm_projecttask", taskId, new ColumnSet(false)));

            task["bvrcrm_completed_date"] = DateTime.Now;

            service.Update(task);

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

            await Task.Run(() => service.Execute(setTaskCompleted));
        }

        public async void UpdateTaskStatus(Guid taskId)
        {
            Entity task = await Task.Run(() => service.Retrieve("bvrcrm_projecttask", taskId, new ColumnSet(false)));
            task["statuscode"] = new OptionSetValue(744240000);
            await Task.Run(() => service.Update(task));
        }

        public async Task<int> RetrieveTaskMinutesAsync(Guid projectId, Guid taskId)
        {
            int minutes = 0;
            Guid gUserId = await GetUserId();

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
            EntityCollection timeTrackings = await Task.Run(() => service.RetrieveMultiple(new FetchExpression(fetch)));

            foreach (Entity timeTracking in timeTrackings.Entities)
            {
                minutes += Convert.ToInt32((decimal)timeTracking["bvrcrm_minutes"]);
            }

            return minutes;
        }

        public async void CreateMeeting(Guid projectId, string description, DateTime? completedDate, decimal duration)
        {
            try
            {
                /// Get the owner of the project
                Entity project = await Task.Run(() => service.Retrieve("bvrcrm_project", projectId, new ColumnSet("bvrcrm_customer")));

                /// Creates a project for the meeting
                Entity task = new Entity("bvrcrm_projecttask");

                task["bvrcrm_name"] = description;
                task["bvrcrm_customer"] = project.GetAttributeValue<EntityReference>("bvrcrm_customer");
                task["bvrcrm_project"] = project.ToEntityReference();

                if (completedDate != null)
                {
                    task["bvrcrm_completed_date"] = completedDate;
                }

                Guid taskId = service.Create(task);

                /// Creates a time tracking record for the recently created project
                Entity timeTracking = new Entity("bvrcrm_timetracking");

                timeTracking["bvrcrm_minutes"] = duration;
                timeTracking["bvrcrm_name"] = description;
                timeTracking["bvrcrm_project"] = project.ToEntityReference();
                timeTracking["bvrcrm_task"] = new EntityReference("bvrcrm_projecttask", taskId);

                if (completedDate != null)
                {
                    timeTracking["bvrcrm_tracking_date"] = completedDate;
                }

                await Task.Run(() => service.Create(timeTracking));

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

                await Task.Run(() => service.Execute(setTaskCompleted));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public async void CreateTask(Guid projectId, string taskName, Guid ownerId, string priority,
            DateTime? dueDate, decimal estimatedHours, string description, int? tfsId = null)
        {
            /// Get the owner of the project
            Entity project = await Task.Run(() => service.Retrieve("bvrcrm_project", projectId, new ColumnSet("bvrcrm_customer")));

            /// Create a project for the meeting
            Entity task = new Entity("bvrcrm_projecttask");

            task["bvrcrm_name"] = taskName;
            task["bvrcrm_customer"] = project.GetAttributeValue<EntityReference>("bvrcrm_customer");
            task["bvrcrm_project"] = project.ToEntityReference();
            task["ownerid"] = new EntityReference("systemuser", ownerId);

            switch (priority)
            {
                case "High":
                    task["bvrcrm_priority"] = new OptionSetValue(744240000);
                    break;
                case "Meidum":
                    task["bvrcrm_priority"] = new OptionSetValue(744240001);
                    break;
                case "Low":
                    task["bvrcrm_priority"] = new OptionSetValue(744240002);
                    break;
            }

            if (dueDate != null)
            {
                //task["bvrcrm_due_date"] = dueDate;
            }

            task["bvrcrm_estimated_hours"] = estimatedHours;
            task["bvrcrm_description"] = description;

            if (tfsId != null)
            {
                task["bvrcrm_tfsid"] = tfsId;
            }

            await Task.Run(() => service.Create(task));
        }

        public async Task<Guid> GetUserId()
        {
            WhoAmIResponse whoAmIResponse = await Task.Run(() => (WhoAmIResponse)service.Execute(new WhoAmIRequest()));
            return whoAmIResponse.UserId;
        }

        public async Task<Guid> GetUserIdByName(string firstName, string secondName)
        {
            QueryExpression userQuery = new QueryExpression
            {
                EntityName = "systemuser",
                ColumnSet = new ColumnSet(),
                Criteria =
                {
                    FilterOperator = LogicalOperator.And,
                    Conditions =
                    {
                        new ConditionExpression("fullname", ConditionOperator.Equal, firstName + " " + secondName)
                    }
                }
            };

            EntityCollection userCollection = await Task.Run(() => service.RetrieveMultiple(userQuery));

            return (userCollection.Entities.Count > 0) ? userCollection.Entities.First().Id : Guid.Empty;
        }

        public async Task<bool> TfsInformationExists(Guid projectId)
        {
            Entity project = await Task.Run(() => service.Retrieve("bvrcrm_project", projectId,
                new ColumnSet("bvrcrm_tfs_url", "bvrcrm_customer", "bvrcrm_tfs_access_token")));

            string personalAccessToken = project.GetAttributeValue<string>("bvrcrm_tfs_access_token");
            string tfsUrl = project.GetAttributeValue<string>("bvrcrm_tfs_url");

            if (personalAccessToken == null || tfsUrl == null)
            {
                return false;
            }

            return true;
        }

        public async Task<List<ProjectTask>> GetTfsTasks(Guid projectId, string tfsTaskIds)
        {
            Entity project = await Task.Run(() => service.Retrieve("bvrcrm_project", projectId,
                new ColumnSet("bvrcrm_tfs_url", "bvrcrm_customer", "bvrcrm_tfs_access_token")));

            var personalAccessToken = project.GetAttributeValue<string>("bvrcrm_tfs_access_token");
            var tfsUrl = project.GetAttributeValue<string>("bvrcrm_tfs_url");

            TfsConnector tfsConnector = new TfsConnector(personalAccessToken, tfsUrl);

            TaskList tfsTasks = await tfsConnector.GetTasksFromTfs(tfsTaskIds);
            List<ProjectTask> tasks = new List<ProjectTask>();

            foreach (TaskItem tfsTask in tfsTasks.Value)
            {
                ProjectTask task = await GetTaskFromTfsTask(tfsTask);

                tasks.Add(task);
            }

            return tasks;
        }

        private async Task<ProjectTask> GetTaskFromTfsTask(TaskItem tfsTask)
        {
            int tfsTaskId = Convert.ToInt32(tfsTask.Fields.SystemId);
            
            ProjectTask task = new ProjectTask();
            task.TaskName = tfsTask.Fields.TaskName;
            task.Description = tfsTask.Fields.Description;
            task.EstimatedHours = Convert.ToInt32(Convert.ToDouble(tfsTask.Fields.EstimatedHours));
            task.TfsId = tfsTaskId;
            task.DueDate = Convert.ToDateTime(tfsTask.Fields.DueDate);

            switch (tfsTask.Fields.Priority)
            {
                case "1":
                    task.Priority = "Low";
                    break;

                case "2":
                    task.Priority = "Medium";
                    break;

                case "3":
                    task.Priority = "Medium";
                    break;

                case "4":
                    task.Priority = "High";
                    break;
            }

            string[] userName = tfsTask.Fields.Responsible.Split(' ');
            Guid taskOwner = await GetUserIdByName(userName[0], userName[1]);
                
            task.OwnerId = taskOwner;

            return task;
        }

        public async Task<bool> TfsTaskExists(int tfsTaskId)
        {
            QueryExpression projectTaskQuery = new QueryExpression
            {
                EntityName = "bvrcrm_projecttask",
                ColumnSet = new ColumnSet(false),
                Criteria =
                {
                    FilterOperator = LogicalOperator.And,
                    Conditions =
                    {
                        new ConditionExpression("bvrcrm_tfsid", ConditionOperator.Equal, tfsTaskId)
                    }
                }
            };

            EntityCollection projectTaskCollection = await Task.Run(() => service.RetrieveMultiple(projectTaskQuery));

            return projectTaskCollection.Entities.Count > 0;
        }
    }
}