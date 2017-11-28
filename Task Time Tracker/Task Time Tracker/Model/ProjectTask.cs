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
        public string taskName { get; set; }
        public Guid taskId { get; set; }
        public int status { get; set; }
        public string priority { get; set; }
        public DateTime dueDate { get; set; }
    }
}
