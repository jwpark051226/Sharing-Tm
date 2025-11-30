using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharingTm.Model
{
    public class User
    {
        public int userID { get; set; }
        public string userName { get; set; }
        public int balance {  get; set; }
        // 유효한 티켓 보유 여부
        public bool hasTicket { get; set; }
        // 현재 대여 중인지 여부
        public bool isRenting { get; set; }
    }
}
