using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FBP.Models
{
    public class Comment
    {
        public int id { get; set; }
        public string user_name { get; set; }
        public int league_id { get; set; }
        public string comment { get; set; }
        public DateTime date_posted { get; set; }
    }
}
