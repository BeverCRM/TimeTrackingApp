using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task_Time_Tracker.Utility_Functions;

namespace Task_Time_Tracker.Model
{
    public class ProjectTask
    {
        public List<Tuple<String,Guid>> retrieveProjects(CRM_Connector crmConnector)
        {
            List<Tuple<String,Guid>> Projects = new List<Tuple<String, Guid>>();
            Guid gUserId = ((WhoAmIResponse)crmConnector.service.Execute(new WhoAmIRequest())).UserId;

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
            EntityCollection projects = crmConnector.service.RetrieveMultiple(new FetchExpression(fetch));
            foreach (Entity project in projects.Entities)
            {
                Projects.Add(new Tuple<string,Guid>(project["bvrcrm_name"].ToString(), (Guid)project["bvrcrm_projectid"]));
            }

            return Projects;
        }

        public List<Tuple<String, Guid>> retrieveTasks(CRM_Connector crmConnector, Guid projectId)
        {
            List<Tuple<String, Guid>> Tasks = new List<Tuple<String, Guid>>();
            Guid gUserId = ((WhoAmIResponse)crmConnector.service.Execute(new WhoAmIRequest())).UserId;

            string fetch1 = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' >";
            fetch1 += "<entity name='bvrcrm_projecttask'>";
            fetch1 += "<attribute name='bvrcrm_name'/>";
            fetch1 += "<filter type='and'>";
            fetch1 += "<condition attribute='ownerid' operator='eq' value='" + gUserId + "'/>";
            fetch1 += "<condition attribute='statuscode' operator='ne' value='2' />";
            fetch1 += "<condition attribute='bvrcrm_project' operator='eq' value='" + projectId + "'/>";
            fetch1 += "</filter>";
            fetch1 += "</entity>";
            fetch1 += "</fetch>";
            EntityCollection tasks = crmConnector.service.RetrieveMultiple(new FetchExpression(fetch1));
            foreach (Entity task in tasks.Entities)
            {
                Tasks.Add(new Tuple<string, Guid>(task["bvrcrm_name"].ToString(),task.Id));
            }

            return Tasks;
        }
    }
}
