using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyProject.Models
{
    public class UserServiceModel
    {
        public int UsersServicesID { get; set; }
        public int UserID { get; set; }
        public int ServiceID { get; set; }
        public decimal InterestedValue { get; set; }
        public virtual Service Service { get; set; }
        public virtual User User { get; set; }
    }
}