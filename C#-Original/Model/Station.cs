using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharingTm.Model
{
    public class Station
    {
        public int stationID {  get; set; }
        public string stationName { get; set; }
        public string stationAddress { get; set; }
        public int parkedTimeMachine { get; set; }
    }
}
