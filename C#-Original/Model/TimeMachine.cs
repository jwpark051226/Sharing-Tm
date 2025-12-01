using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharingTm.Model
{
    public class TimeMachine
    {
        public int timeMachineID {  get; set; }
        public string timeMachineModel { get; set; }
        // 타임머신의 현재 시간대, 시간여행중이 아닐때의 표시 및 기원전 표시를 위해 string(C#), VARCHAR(MySQL)을 사용
        public string currentEraPoint { get; set; }
        // 현재 보관되어 있는 대여소의 ID를 가짐, 대여중이거나 지정되지 않았을 시 NULL
        public int currentStationID { get; set; } 
        // 출력용으로 보관되어 있는 대여소의 이름을 가짐
        public string currentStationName { get; set; } 
    }
}
