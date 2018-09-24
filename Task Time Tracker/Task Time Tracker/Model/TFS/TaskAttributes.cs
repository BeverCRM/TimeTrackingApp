using System.Runtime.Serialization;

namespace Task_Time_Tracker.Model.TFS
{
    [DataContract]
    public class TaskAtrributes
    {
        [DataMember(Name = "Microsoft.VSTS.Common.Priority")]
        public string Priority { get; set; }

        [DataMember(Name = "Microsoft.VSTS.Scheduling.DueDate")]
        public string DueDate { get; set; }

        [DataMember(Name = "Microsoft.VSTS.Scheduling.OriginalEstimate")]
        public string EstimatedHours { get; set; }

        [DataMember(Name = "System.AssignedTo")]
        public string Responsible { get; set; }

        [DataMember(Name = "System.Description")]
        public string Description { get; set; }

        [DataMember(Name = "System.State")]
        public string Status { get; set; }

        [DataMember(Name = "System.Title")]
        public string TaskName { get; set; }

        [DataMember(Name = "System.Id")]
        public string SystemId { get; set; }
    }
}
