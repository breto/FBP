using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FBP.Models
{
    public class Alert
    {
        public static readonly string DANGER_TYPE = "danger";
        public static readonly string SUCCESS_TYPE= "success";
        public static readonly string INFO_TYPE = "info";
        public static readonly string WARNING_TYPE = "warning";

        public Alert()
        {

        }

        public Alert(string type, string message)
        {
            this.type = type;
            this.message = message;
        }

        public string type { get; set; }
        public string message { get; set; }

        public int CompareTo(Alert a)
        {
            if (a == null)
            {
                return 1;
            }
            else
            {
                return this.message.CompareTo(a.message);
            }
        }
        public override int GetHashCode()
        {
            return message.GetHashCode() ^ type.GetHashCode();
        }
        public override bool Equals(Object other)
        {
            if (other == null) return false;
            if (other is Alert) {
                return (this.message.Equals(((Alert)other).message) && (this.type.Equals(((Alert)other).type)));
            } else
            {
                return false;
            }
        }

    }
}
