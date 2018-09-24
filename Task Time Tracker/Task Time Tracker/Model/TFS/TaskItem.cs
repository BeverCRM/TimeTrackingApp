using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Task_Time_Tracker.Model.TFS
{
    [DataContract]
    public class TaskItem
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "rev")]
        public int Rev { get; set; }

        [DataMember(Name = "fields")]
        public TaskAtrributes Fields { get; set; }

        [DataMember(Name = "url")]
        public string Url { get; set; }
    }
}
