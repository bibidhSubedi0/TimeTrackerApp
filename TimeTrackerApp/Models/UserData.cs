using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace TimeTrackerApp.Models
{
    
    public class UserData
        {
            public string UserId { get; set; }
            public List<ProjectItem> Projects { get; set; } = new();
        }
}
