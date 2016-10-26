using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FBP.Models
{
    public class Pick
    {
        [Key]
        public int pick_id { get; set; }
        public string user_name { get; set; }
        public int nfl_id { get; set; }
        public int weight { get; set; }
        public int winner_id { get; set; }
        public int week { get; set; }
        public int league_id { get; set; }

        [Write(false)]
        public Matchup matchup { get; set; }

        public Pick()
        {
        }
        public Pick(string user_name, int league_id, Matchup matchup)
        {
            this.user_name = user_name;
            if(matchup != null)
            {
                this.nfl_id = matchup.nfl_id;
                this.matchup = matchup;
                this.week = matchup.week_number;
                this.league_id = league_id;
            }
        }
    }
}
