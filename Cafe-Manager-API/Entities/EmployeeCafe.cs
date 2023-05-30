using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cafe_Manager_API.Entities
{
    public class EmployeeCafe
    {
        public string EmployeeId { get; set; }
        public string CafeId { get; set; }
        public DateTime StartDate { get; set; }
    }
}
