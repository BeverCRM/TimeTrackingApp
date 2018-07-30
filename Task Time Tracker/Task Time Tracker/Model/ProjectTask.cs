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
        public string TaskName { get; set; }
        public Guid TaskId { get; set; }
        public int Status { get; set; }
        public string Priority { get; set; }
        public DateTime DueDate { get; set; }
    }
}
