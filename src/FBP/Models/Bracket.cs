using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FBP.Models
{
    public class Bracket
    {
        public string user_name { get; set; }
        public int week { get; set; }
        public int league_id { get; set; }
        public bool hasWeekStarted { get; set; }
        public bool allGamesStarted { get; set; }
        public IEnumerable<Pick> picks { get; set; }
        public League league { get; set; }

        public Bracket()
        {
            
        }
        public Bracket(string user_name, int week, int league_id)
        {
            this.user_name = user_name;
            this.week = week;
            this.league_id = league_id;
        }
    }
}
