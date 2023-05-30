using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cafe_Manager_API.Entities
{
    public class Employee
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string Gender { get; set; }
    }
}
