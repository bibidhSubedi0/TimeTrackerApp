using System.Collections.Generic;

namespace TimeTrackerApp.Models
{
    public class UserData
    {
        public string UserId { get; set; }
        public List<ProjectItem> Projects { get; set; } = new();
    }
}
