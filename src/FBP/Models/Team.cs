using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FBP.Models
{
    public class Team
    {
        public int id { get; set; }
        public string city { get; set; }
        public string name { get; set; }
        public string short_name { get; set; }
        public int conferenceId { get; set; }
        public int wins { get; set; }
        public int losses { get; set; }
        public int pointsFor { get; set; }
        public int pointsAgainst { get; set; }
    }
}
