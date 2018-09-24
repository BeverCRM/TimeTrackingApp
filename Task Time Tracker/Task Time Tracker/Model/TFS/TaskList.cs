using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Task_Time_Tracker.Model.TFS
{
    [DataContract]
    public class TaskList
    {
        [DataMember(Name = "count")]
        public int Count { get; set; }

        [DataMember(Name = "value")]
        public List<TaskItem> Value { get; set; }
    }
}
