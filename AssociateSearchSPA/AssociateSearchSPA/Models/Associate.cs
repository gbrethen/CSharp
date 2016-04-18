using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace AssociateSearchSPA.Models
{
    public class Associate
    {
        [Key]
        public string EmployeeId { get; set; }
        public string SupervisorId { get; set; }
        public string First { get; set; }
        public string Middle { get; set; }
        public string Last { get; set; }
        public string Title { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Department { get; set; }
        public string Location { get; set; }
        public string BU { get; set; }
        public string Company { get; set; }
    }
}