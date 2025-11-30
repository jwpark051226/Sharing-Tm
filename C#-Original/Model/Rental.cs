using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharingTm.Model
{
    public class Rental
    {
        public int rentalID { get; set; }
        public int userID { get; set; }
        public string userName { get; set; } // 출력용
        public int timeMachineID { get; set; }
        public int departStationID { get; set; }
        public string departStationName { get; set; } // 출력용
        public DateTime rentalTime { get; set; }
        public int arriveStationID { get; set; }
        public string arriveStationName { get; set; } // 출력용
        public DateTime returnTime { get; set; }
        public string destinationPoint { get; set; }
    }
}
