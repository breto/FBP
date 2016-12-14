using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FBP.Models
{
    public class UserScore
    {
        public string userName { get; set; }
        public int league_id { get; set; }
        public int week { get; set; }
        public int weekScore { get; set; }
        public int seasonScore { get; set; }

        public UserScore()
        {

        }

        public UserScore(string userName, int weekScore, int seasonScore, int week, int league_id)
        {
            this.userName = userName;
            this.weekScore = weekScore;
            this.seasonScore = seasonScore;
            this.week = week;
            this.league_id = league_id;
        }

        public int CompareTo(UserScore u)
        {
            if (u == null)
            {
                return 1;
            }
            else
            {
                return this.weekScore.CompareTo(u.weekScore);
            }
        }
        public override int GetHashCode()
        {
            return userName.GetHashCode();
        }
        public bool Equals(UserScore other)
        {
            if (other == null) return false;
            if (other is UserScore)
            {
                return (this.userName.Equals(((UserScore)other).userName));
            }
            else
            {
                return false;
            }
        }
    }
}
