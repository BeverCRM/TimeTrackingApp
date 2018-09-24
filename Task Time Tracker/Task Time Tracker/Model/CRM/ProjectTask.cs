using System;

namespace Task_Time_Tracker.Model
{
    public class ProjectTask
    {
        public string TaskName { get; set; }
        public Guid TaskId { get; set; }
        public int Status { get; set; }
        public string Priority { get; set; }
        public DateTime DueDate { get; set; }
        public string Description { get; set; }
        public int EstimatedHours { get; set; }
        public Guid OwnerId { get; set; }
        public int TfsId { get; set; }
    }
}
