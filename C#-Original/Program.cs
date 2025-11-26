using MySql.Data.MySqlClient;
using SharingTm.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace SharingTm
{
    public class Program
    {
        static void Main(string[] args)
        {
            // 프로그램 시작 시 DB 연결 상태 확인
            if (DatabaseManager.TestConnection())
            {
                bool mainQuit = false;
                while (!mainQuit)
                {
                    Console.WriteLine("----------------------------");
                    Console.WriteLine("공유 타임머신 서비스입니다.");
                    Console.WriteLine("----------------------------");
                    Console.WriteLine("1. 사용자 메뉴");
                    Console.WriteLine("2. 관리자 메뉴");
                    Console.WriteLine("0. 종료\n");
                    Console.Write("입력 : ");

                    int mainMenuChoice = int.Parse(Console.ReadLine());
                    switch (mainMenuChoice)
                    {
                        case 1:
                            UserMenu();
                            break;
                        case 2:
                            AdminMenu();
                            break;
                        case 0:
                            mainQuit = true;
                            break;
                        default:
                            Console.WriteLine("0부터 2 사이의 수를 입력해주세요.\n");
                            break;
                    }
                }
            }
        }

        // 미구현
        private static void UserMenu()
        {
            bool userQuit = false;
            while (!userQuit)
            {
                Console.WriteLine("1. 로그인");
                Console.WriteLine("2. 회원가입");
                Console.WriteLine("0. 메뉴 선택으로 돌아가기\n");
                userQuit = true; 
            }            
        }
        
        // 관리자 전용 메뉴 진입
        private static void AdminMenu()
        {
            bool adminQuit = false;
            while (!adminQuit)
            {
                Console.WriteLine("\n1. 사용자 관리");
                Console.WriteLine("2. 대여소 관리");
                Console.WriteLine("3. 타임머신 관리");
                Console.WriteLine("4. 대여목록 조회");
                Console.WriteLine("5. 이용권 구매내역 조회");
                Console.WriteLine("0. 메뉴 선택으로 돌아가기\n");
                Console.Write("입력 : ");

                int adminMenuChoice = int.Parse(Console.ReadLine());
                switch (adminMenuChoice)
                {
                    case 1:
                        AdminUserManage();
                        break;
                    case 2:
                        AdminStationManage();
                        break;
                    case 0:
                        adminQuit = true;
                        break;
                    default:
                        Console.WriteLine("0부터 5 사이의 수를 입력해주세요.");
                        break;
                }
            }
        }

        private static void AdminUserManage()
        {
            bool userMenuQuit = false;
            while (!userMenuQuit)
            {
                Console.WriteLine("\n1. 사용자 조회");
                Console.WriteLine("2. 사용자 추가");
                Console.WriteLine("3. 사용자 삭제");
                Console.WriteLine("0. 상위 메뉴로 돌아가기\n");
                Console.Write("입력 : ");

                int menuChoice = int.Parse(Console.ReadLine());
                switch (menuChoice)
                {
                    case 1:
                        AdminViewUser();
                        break;
                    case 2:
                        AdminAddUser();
                        break;
                    case 3:
                        AdminDelUserMenu();
                        break;
                    case 0:
                        userMenuQuit = true;
                        break;
                    default:
                        Console.WriteLine("0부터 2 사이의 수를 입력해주세요.");
                        break;
                }
            }
        }

        private static void AdminStationManage()
        {
            bool stationMenuQuit = false;
            while (!stationMenuQuit)
            {
                Console.WriteLine("\n1. 대여소 조회");
                Console.WriteLine("2. 대여소 추가");
                Console.WriteLine("3. 대여소 삭제");
                Console.WriteLine("0. 상위 메뉴로 돌아가기\n");
                Console.Write("입력 : ");

                int menuChoice = int.Parse(Console.ReadLine());
                switch (menuChoice)
                {
                    case 1:
                        AdminViewStation();
                        break;
                    case 2:
                        AdminAddStation();
                        break;
                    case 3:
                        AdminDelStationMenu();
                        break;
                    case 0:
                        stationMenuQuit = true;
                        break;
                    default:
                        Console.WriteLine("0부터 3 사이의 수를 입력해주세요.");
                        break;
                }
            }
        }

        private static void AdminViewUser()
        {
            // DB에서 전체 유저 목록 가져오기
            List<User> userListForView = DatabaseManager.GetUsers();
            if (userListForView.Count > 0)
            {
                PrintUserList(userListForView);
            }
            else
                Console.WriteLine("등록된 사용자가 없습니다.");
        }

        private static void AdminAddUser()
        {
            Console.Write("\n추가할 사용자의 이름을 입력해주세요 : ");
            string newUserName = Console.ReadLine();
            if (newUserName.Length <= 45)
            {
                List<User> userList = DatabaseManager.GetUsers();
                
                // 중복된 이름이 있는지 확인하여 처리하는 로직
                if (userList.Any(
                    user => user.username.Equals(newUserName)))
                {
                    int i = 1;
                    bool flag = false;
                    while (!flag)
                    {
                        // 중복 시 이름 뒤에 (n)을 붙여 유니크한 이름 생성
                        if (userList.Any(
                             user => user.username.Equals($"{newUserName}({i})")))
                            i++;
                        else
                        {
                            flag = true;
                            newUserName = $"{newUserName}({i})";
                        }
                    }
                }
                
                // 최종 결정된 이름으로 DB에 추가
                if (DatabaseManager.AddNewUser(newUserName))
                    Console.WriteLine($"{newUserName} 사용자가 추가되었습니다.");
                else Console.WriteLine("사용자 추가에 실패했습니다.");
                
            }
            else
                Console.WriteLine("사용자의 이름은 45글자 이하여야 합니다.");
        }

        private static void AdminDelUserMenu()
        {
            bool delMenuQuit = false;
            while (!delMenuQuit)
            {
                Console.WriteLine("\n1. 직접 선택");
                Console.WriteLine("2. 사용자 이름으로 검색");
                Console.WriteLine("0. 상위 메뉴로 돌아가기");
                Console.Write("입력 : ");

                int howToSearchUser = int.Parse(Console.ReadLine());
                switch (howToSearchUser)
                {
                    case 1:
                        DelUserWithSelect();
                        break;
                    case 2:
                        DelUserWithName();
                        break;
                    case 0:
                        delMenuQuit = true;
                        break;
                    default:
                        Console.WriteLine("0부터 2 사이의 수를 입력해주세요.");
                        break;
                }
            }
        }

        // 목록을 보고 ID를 직접 입력하여 삭제
        private static void DelUserWithSelect()
        {
            List<User> userList = DatabaseManager.GetUsers();
            if (userList.Count > 0)
            {
                PrintUserList(userList);

                bool selectedUserIdExists = false;
                while (!selectedUserIdExists)
                {
                    Console.Write("제거할 유저의 ID를 입력해주세요 (0으로 돌아가기) : ");
                    int delUserID = int.Parse(Console.ReadLine());
                    if (delUserID == 0)
                        selectedUserIdExists = true;
                    else if (userList.Any(user => user.userid == delUserID))
                    {
                        selectedUserIdExists = true;
                        if (DatabaseManager.DeleteUser(delUserID))
                            Console.WriteLine("사용자를 성공적으로 제거했습니다.");
                        else
                            Console.WriteLine("사용자를 제거할 수 없었습니다.");
                    }
                    else
                        Console.WriteLine("해당하는 ID의 사용자가 없습니다.");
                }
            }
            else
                Console.WriteLine("등록된 사용자가 없습니다.");
        }

        // 이름으로 검색하여 삭제 (동명이인 처리 포함)
        private static void DelUserWithName()
        {
            Console.Write("검색할 사용자의 이름을 입력해주세요 : ");
            string searchUserName = Console.ReadLine();
            List<User> userList = DatabaseManager.GetUserByName(searchUserName);
            if (userList.Count == 0)
                Console.WriteLine("해당 사용자를 찾을 수 없습니다.");
            else
            {
                PrintUserList(userList);

                // 검색 결과가 1명일 경우 바로 삭제 의사 확인
                if (userList.Count == 1)
                {
                    bool hasConfirmed = false;
                    while (!hasConfirmed)
                    {
                        Console.Write("해당 유저를 제거하시겠습니까?(y/n) : ");
                        string confirmDel = Console.ReadLine();
                        if (confirmDel.ToLower().Equals("y"))
                        {
                            hasConfirmed = true;
                            if (DatabaseManager.DeleteUser(userList[0].userid))
                                Console.WriteLine("사용자를 성공적으로 제거했습니다.");
                            else
                                Console.WriteLine("사용자를 제거할 수 없었습니다.");
                        }
                        else if (confirmDel.ToLower().Equals("y")) // (참고: 로직상 n에 대한 처리가 누락되어 있거나 중복 조건으로 보임)
                        {
                            hasConfirmed = true;
                        }
                        else
                        {
                            Console.WriteLine("y 혹은 n을 입력해주세요.");
                        }
                    }
                }
                // 검색 결과가 여러 명일 경우 ID로 특정하여 삭제
                else
                {
                    bool selectedUserIdExists_1 = false;
                    while (!selectedUserIdExists_1)
                    {
                        Console.Write("제거할 유저의 ID를 입력해주세요.(0으로 돌아가기) : ");
                        int delUserID = int.Parse(Console.ReadLine());
                        if (delUserID == 0)
                            selectedUserIdExists_1 = true;
                        else if (userList.Any(user => user.userid == delUserID))
                        {
                            selectedUserIdExists_1 = true;
                            if (DatabaseManager.DeleteUser(delUserID))
                                Console.WriteLine("사용자를 성공적으로 제거했습니다.");
                            else
                                Console.WriteLine("사용자를 제거할 수 없었습니다.");
                        }
                        else
                            Console.WriteLine("해당하는 ID의 유저가 없습니다.");
                    }
                }
            }
        }

        // 유저 리스트 출력 포맷팅
        private static void PrintUserList(List<User> userList)
        {
            Console.WriteLine("-----------------------------------------------------");
            Console.WriteLine(" ID    이름          잔액          이용권 유무");
            Console.WriteLine("-----------------------------------------------------");
            foreach (User user in userList)
            {
                Console.WriteLine(" {0,-3}   {1,-7}   {2,10:N0}   {3}",
                    user.userid,
                    user.username,
                    user.balance + "원",
                    (user.hasTicket == true) ? "Y" : "N"
                );
            }
            Console.WriteLine("-----------------------------------------------------\n");
        }

        private static void AdminViewStation()
        {
            List<Station> stationListForView = DatabaseManager.GetStations();
            if (stationListForView.Count > 0)
            {
                PrintStationList(stationListForView);
            }
            else
                Console.WriteLine("등록된 대여소가 없습니다.");
        }

        private static void AdminAddStation()
        {
            Console.Write("\n추가할 대여소의 이름을 입력해주세요 : ");
            string newStationName = Console.ReadLine();
            if (newStationName.Length <= 45)
            {
                Console.Write("추가할 대여소의 주소를 입력해주세요 : ");
                string newStationAddress = Console.ReadLine();
                if (newStationAddress.Length <= 100)
                {
                    if (DatabaseManager.AddStation(newStationName, newStationAddress))
                        Console.WriteLine("대여소가 추가되었습니다.");
                    else
                        Console.WriteLine("대여소를 추가할 수 없었습니다.");
                }
                else
                    Console.WriteLine("대여소의 주소은 100글자 이하여야 합니다.");
            }
            else
                Console.WriteLine("대여소의 이름은 45자 이하여야 합니다.");
        }

        private static void AdminDelStationMenu()
        {
            bool delMenuQuit = false;
            while (!delMenuQuit)
            {
                Console.WriteLine("\n1. 직접 선택");
                Console.WriteLine("2. 대여소 이름으로 검색");
                Console.WriteLine("3. 대여소 주소로 검색");
                Console.WriteLine("0. 상위 메뉴로 돌아가기\n");
                Console.Write("입력 : ");
                int howToSelectStation = int.Parse(Console.ReadLine());
                switch (howToSelectStation)
                {
                    case 1:
                        DelStationWithSelect();
                        break;
                    case 2:
                        DelStationWithName();
                        break;
                    case 3:
                        DelStationWithAddress();
                        break;
                    case 0:
                        delMenuQuit = true;
                        break;
                    default:
                        Console.WriteLine("0부터 3 사이의 수를 입력해주세요.");
                        break;
                }
            }
        }

        private static void DelStationWithSelect()
        {
            List<Station> stationList = DatabaseManager.GetStations();
            if (stationList.Count > 0)
            {
                PrintStationList(stationList);
                bool selectedStationIDExists = false;
                while (!selectedStationIDExists)
                {
                    Console.Write("제거할 대여소의 ID를 입력해주세요.(0으로 돌아가기) : ");
                    int delStationID = int.Parse(Console.ReadLine());
                    if (delStationID == 0)
                        selectedStationIDExists = true;
                    // 입력한 ID가 리스트에 존재하는지 검증
                    else if (stationList.Any(station => station.stationID == delStationID))
                    {
                        selectedStationIDExists = true;
                        if (DatabaseManager.DeleteStation(delStationID))
                            Console.WriteLine("대여소를 성공적으로 제거했습니다.");
                        else
                            Console.WriteLine("대여소를 제거할 수 없었습니다.");
                    }
                    else
                        Console.WriteLine("해당하는 ID의 대여소가 없습니다.");
                }
            }
            else
                Console.WriteLine("등록된 대여소가 없습니다.");
        }

        private static void DelStationWithName()
        {
            Console.Write("검색할 대여소의 이름을 입력해주세요 : ");
            string searchStationName = Console.ReadLine();
            List<Station> stationList = DatabaseManager.GetStationsByName(searchStationName);
            if (stationList.Count == 0)
                Console.WriteLine("해당 대여소를 찾을 수 없습니다.");
            else
            {
                PrintStationList(stationList);
                
                // 검색된 대여소가 1개일 때 처리
                if (stationList.Count == 1)
                {
                    bool hasConfirmed = false;
                    while (!hasConfirmed)
                    {
                        Console.Write("해당 대여소를 제거하시겠습니까?(y/n) : ");
                        string confirmDel = Console.ReadLine();
                        if (confirmDel.ToLower().Equals("y"))
                        {
                            hasConfirmed = true;
                            if (DatabaseManager.DeleteStation(stationList[0].stationID))
                                Console.WriteLine("대여소를 성공적으로 제거했습니다.");
                            else
                                Console.WriteLine("대여소를 제거할 수 없었습니다.");
                        }
                        else if (confirmDel.ToLower().Equals("y"))
                        {
                            hasConfirmed = true;
                        }
                        else
                        {
                            Console.WriteLine("y 혹은 n을 입력해주세요.");
                        }
                    }
                }
                // 검색된 대여소가 여러 개일 때 ID로 선택
                else
                {
                    bool selectedStationIDExists = false;
                    while (!selectedStationIDExists)
                    {
                        Console.Write("제거할 유저의 ID를 입력해주세요.(0으로 돌아가기) : ");
                        int delStationID = int.Parse(Console.ReadLine());
                        if (delStationID == 0)
                            selectedStationIDExists = true;
                        else if (stationList.Any(station => station.stationID == delStationID))
                        {
                            selectedStationIDExists = true;
                            if (DatabaseManager.DeleteStation(delStationID))
                                Console.WriteLine("대여소를 성공적으로 제거했습니다.");
                            else
                                Console.WriteLine("대여소를 제거할 수 없었습니다.");
                        }
                        else
                            Console.WriteLine("해당하는 ID의 대여소가 없습니다.");
                    }
                }
            }
        }

        private static void DelStationWithAddress()
        {
            Console.Write("검색할 대여소의 이름을 입력해주세요 : ");
            string searchStationAddress = Console.ReadLine();
            List<Station> stationList = DatabaseManager.GetStationsByAddress(searchStationAddress);
            if (stationList.Count == 0)
                Console.WriteLine("해당 대여소를 찾을 수 없습니다.");
            else
            {
                PrintStationList(stationList);

                if (stationList.Count == 1)
                {
                    bool hasConfirmed = false;
                    while (!hasConfirmed)
                    {
                        Console.Write("해당 대여소를 제거하시겠습니까?(y/n) : ");
                        string confirmDel = Console.ReadLine();
                        if (confirmDel.ToLower().Equals("y"))
                        {
                            hasConfirmed = true;
                            if (DatabaseManager.DeleteStation(stationList[0].stationID))
                                Console.WriteLine("대여소를 성공적으로 제거했습니다.");
                            else
                                Console.WriteLine("대여소를 제거할 수 없었습니다.");
                        }
                        else if (confirmDel.ToLower().Equals("y"))
                        {
                            hasConfirmed = true;
                        }
                        else
                        {
                            Console.WriteLine("y 혹은 n을 입력해주세요.");
                        }
                    }
                }
                else
                {
                    bool selectedStationIDExists = false;
                    while (!selectedStationIDExists)
                    {
                        Console.Write("제거할 대여소의 ID를 입력해주세요.(0으로 돌아가기) : ");
                        int delStationID = int.Parse(Console.ReadLine());
                        if (delStationID == 0)
                            selectedStationIDExists = true;
                        else if (stationList.Any(station => station.stationID == delStationID))
                        {
                            selectedStationIDExists = true;
                            if (DatabaseManager.DeleteStation(delStationID))
                                Console.WriteLine("대여소를 성공적으로 제거했습니다.");
                            else
                                Console.WriteLine("대여소를 제거할 수 없었습니다.");
                        }
                        else
                            Console.WriteLine("해당하는 ID의 대여소가 없습니다.");
                    }
                }
            }
        }

        // 대여소 리스트 출력 포맷팅
        private static void PrintStationList(List<Station> StationList)
        {
            Console.WriteLine("\n------------------------------------------------------------------------------------------");
            Console.WriteLine(" ID    대여소 이름           보관 타임머신  대여소 주소");
            Console.WriteLine("------------------------------------------------------------------------------------------");
            foreach (Station station in StationList)
            {
                Console.WriteLine(" {0,-3}   {1,-12}   {2,-11}   {3}",
                    station.stationID,
                    station.stationName,
                    station.parkedTimeMachine + "대",
                    station.stationAddress
                );
            }
            Console.WriteLine("------------------------------------------------------------------------------------------");
        }
    }
}
