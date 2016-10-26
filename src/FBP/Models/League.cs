using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FBP.Models
{
    public class League
    {
        public int id { get; set; }
        public string name { get; set; }
        public string password { get; set; }
        public IEnumerable<LeagueMember> members { get; set; }
    }
}
