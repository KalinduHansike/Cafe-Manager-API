using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cafe_Manager_API.Entities
{
    public class Cafe
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Logo { get; set; }
        public string Location { get; set; }
        //  public IFormFile file { get; set; }
    }
}
