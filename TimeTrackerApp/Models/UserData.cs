using System.Collections.Generic;

namespace TimeTrackerApp.Models
{
    public class UserData
    {
        public string UserId { get; set; }

        // As projects contain task, this should be enough!
        public List<ProjectItem> Projects { get; set; } = new();
    }
}
