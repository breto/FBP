using FBP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FBP.ViewModels
{
    public class FootballPoolViewModel
    {
        public Bracket bracket { get; set; }
        public IEnumerable<Alert> errors { get; set; }
        public int weeksInSeason { get; set; }
        public int currentWeek { get; set; }
        public IEnumerable<Comment> comments { get; set; }

    }
}
