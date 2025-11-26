using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharingTm.Model
{
    public class User
    {
        public int userid { get; set; }
        public string username { get; set; }
        public int balance {  get; set; }
        public bool hasTicket { get; set; }
    }
}
