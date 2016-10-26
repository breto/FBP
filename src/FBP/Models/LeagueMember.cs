using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FBP.Models
{
    [Table("league_members")]
    public class LeagueMember
    {
        public int id { get; set; }
        public int league_id { get; set; }
        public string user_name { get; set; }

        public LeagueMember()
        {

        }

        public LeagueMember(int league_id, string user_name)
        {
            this.league_id = league_id;
            this.user_name = user_name;
        }
    }
}
