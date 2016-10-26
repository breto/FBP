using Dapper.Contrib.Extensions;
using FBP.Models;
using System;

namespace FBP.Models
{
    public class Matchup
    {
        public static readonly string FINAL_STATUS = "F";
        public static readonly string FINAL_OT_STATUS = "FO";
        public static readonly int TIE_INDICATOR = -1;
        public static readonly int GAME_NOT_FINAL_INDICATOR = -2;

        public int id { get; set; }
        public int nfl_id { get; set; }
        public int week_number { get; set; }
        public DateTime game_date { get; set; }
        public string status { get; set; }
        public int home_team_score { get; set; }
        public int visit_team_score { get; set; }
        public int win_team_id { get; set; }
        public string season { get; set; }
        public int home_team_id { get; set; }
        public int visit_team_id { get; set; }

        [Write(false)]
        public Team homeTeam { get; set; }
        [Write(false)]
        public Team visitTeam { get; set; }

        public int getWinningTeamId()
        {
            int w = 0;
            
            if(home_team_score > visit_team_score)
            {
                w = home_team_id;
            } else if(visit_team_score > home_team_score)
            {
                w = visit_team_id;
            } else
            {
                w = TIE_INDICATOR;
            }

            return w;
        }
    }

    
}
