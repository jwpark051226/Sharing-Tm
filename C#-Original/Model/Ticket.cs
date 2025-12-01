using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace SharingTm.Model
{
    public class Ticket 
    {
        public int ticketID { get; set; }
        public int userID { get; set; }
        public string userName { get; set; } // 출력용
        public DateTime purchaseTime { get; set; }
        public DateTime expireTime { get; set; }
        public string status { get; set; }
    }
}
