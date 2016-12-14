using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FBP.ViewModels
{
    
    public class AdminViewModel
    {
        public string sqlCommand { get; set; }
        public string season { get; set; }
        public int week { get; set; }
        public IEnumerable<Object> results { get; set; }

    }
}
