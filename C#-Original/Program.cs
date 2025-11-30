using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Common;
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
                    Console.WriteLine("\n----------------------------");
                    Console.WriteLine("공유 타임머신 서비스입니다.");
                    Console.WriteLine("----------------------------");
                    Console.WriteLine("1. 사용자 메뉴");
                    Console.WriteLine("2. 관리자 메뉴");
                    Console.WriteLine("0. 종료\n");
                    Console.Write("입력 : ");

                    if (!int.TryParse(Console.ReadLine(), out int mainMenuChoice))
                    {
                        mainMenuChoice = -1; // 숫자가 아닐 경우 default로 이동
                    }

                    switch (mainMenuChoice)
                    {
                        case 1:
                            UserMainMenu();
                            break;
                        case 2:
                            AdminMainMenu();
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
        private static void UserMainMenu()
        {
            bool userQuit = false;
            while (!userQuit)
            {
                Console.WriteLine("\n1. 로그인");
                Console.WriteLine("2. 회원가입");
                Console.WriteLine("0. 메뉴 선택으로 돌아가기\n");
                Console.Write("입력 : ");

                if (!int.TryParse(Console.ReadLine(), out int userMenuChoice))
                {
                    userMenuChoice = -1;
                }

                switch (userMenuChoice)
                {
                    case 1:
                        UserLoginMenu();
                        break;
                    case 2:
                        AddUser();
                        break;
                    case 0:
                        userQuit = true;
                        break;
                    default:
                        Console.WriteLine("0부터 2 사이의 수를 입력해주세요.\n");
                        break;
                }
            }
        }
        
        // 관리자 전용 메뉴 진입
        private static void AdminMainMenu()
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

                if (!int.TryParse(Console.ReadLine(), out int adminMenuChoice))
                {
                    adminMenuChoice = -1;
                }

                switch (adminMenuChoice)
                {
                    case 1:
                        AdminUserManage();
                        break;
                    case 2:
                        AdminStationManage();
                        break;
                    case 3:
                        AdminTimeMachineManage();
                        break;
                    case 5:
                        AdminViewTicket();
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

        private static void UserLoginMenu()
        {
            bool loginMenuQuit = false;
            User selectedUser = null;
            while (!loginMenuQuit)
            {
                Console.WriteLine("\n1. 직접 선택");
                Console.WriteLine("2. 사용자 이름으로 검색");
                Console.WriteLine("0. 상위 메뉴로 돌아가기");
                Console.Write("입력 : ");

                if (!int.TryParse(Console.ReadLine(), out int howToSearchUser))
                {
                    howToSearchUser = -1;
                }

                switch (howToSearchUser)
                {
                    case 1:
                        selectedUser = LoginWithSelect();
                        if (selectedUser != null)
                        {
                            UserMenu(selectedUser);
                            loginMenuQuit = true;
                        }
                        break;
                    case 2:
                        selectedUser = LoginWithName();
                        if (selectedUser != null)
                        {
                            UserMenu(selectedUser);
                            loginMenuQuit = true;
                        }
                        break;
                    case 0:
                        loginMenuQuit = true;
                        break;
                    default:
                        Console.WriteLine("0부터 2 사이의 수를 입력해주세요");
                        break;
                }
            }
        }

        private static User LoginWithSelect()
        {
            User selectedUser = null;
            List<User> userList = DatabaseManager.GetUsers();
            if (userList.Count > 0)
            {
                PrintUserListOnlyIDAndName(userList);
                bool selectedUserIdExists = false;
                while (!selectedUserIdExists)
                {
                    Console.Write("로그인할 사용자의 ID를 입력해주세요 (0으로 돌아가기) : ");

                    if (!int.TryParse(Console.ReadLine(), out int loginUserID))
                    {
                        loginUserID = -1;
                    }

                    if (loginUserID == 0)
                        selectedUserIdExists = true;

                    User targetUser = userList.FirstOrDefault(user => user.userID == loginUserID);
                    if (targetUser != null)
                    {
                        selectedUserIdExists = true;
                        Console.WriteLine($"{targetUser.userName} 사용자로 로그인했습니다.");
                        selectedUser = targetUser;
                    }
                    else
                        Console.WriteLine("해당하는 ID의 사용자가 없습니다.");
                }
            }
            else
                Console.WriteLine("\n등록된 사용자가 없습니다.");

            return selectedUser;
        }

        private static User LoginWithName()
        {
            User selectedUser = null;
            Console.Write("\n검색할 사용자의 이름을 입력해주세요 : ");
            string searchUserName = Console.ReadLine();
            List<User> userList = DatabaseManager.GetUserByName(searchUserName);
            if (userList.Count == 0)
                Console.WriteLine("해당 사용자를 찾을 수 없습니다.");
            else
            {
                PrintUserListOnlyIDAndName(userList);
                // 검색 결과가 1명일 경우 바로 로그인 의사 확인
                if (userList.Count == 1)
                {
                    bool hasConfirmed = false;
                    while (!hasConfirmed)
                    {
                        Console.Write("해당 사용자로 로그인하시겠습니까?(y/n) : ");
                        string confirmDel = Console.ReadLine();
                        if (confirmDel.ToLower().Equals("y"))
                        {
                            hasConfirmed = true;
                            selectedUser = userList[0];
                            Console.WriteLine($"{selectedUser.userName} 사용자로 로그인했습니다.");
                        }
                        else if (confirmDel.ToLower().Equals("n"))
                        {
                            hasConfirmed = true;
                        }
                        else
                        {
                            Console.WriteLine("y 혹은 n을 입력해주세요.");
                        }
                    }
                }
                // 검색 결과가 여러 명일 경우 ID로 특정하여 로그인
                else
                {
                    bool selectedUserIdExists = false;
                    while (!selectedUserIdExists)
                    {
                        Console.Write("로그인할 사용자의 ID를 입력해주세요 (0으로 돌아가기) : ");

                        if (!int.TryParse(Console.ReadLine(), out int loginUserID))
                        {
                            loginUserID = -1;
                        }

                        if (loginUserID == 0)
                            selectedUserIdExists = true;

                        User targetUser = userList.FirstOrDefault(user => user.userID == loginUserID);
                        if (targetUser != null)
                        {
                            selectedUserIdExists = true;
                            Console.WriteLine($"{targetUser.userName} 사용자로 로그인에 성공했습니다.");
                            selectedUser = targetUser;
                        }
                        else
                            Console.WriteLine("해당하는 ID의 타임머신이 없습니다.");
                    }
                }
            }
            return selectedUser;
        }

        private static void UserMenu(User user)
        {
            bool userMenuQuit = false;
            while (!userMenuQuit)
            {
                // 사용자의 이름, 잔액, 이용권 유무, 현재 대여중 여부의 인적사항 출력
                Console.WriteLine("\n---------사용자 정보---------");
                Console.WriteLine($"이름 : {user.userName}");
                Console.WriteLine($"소지금 : {user.balance}원");
                Console.WriteLine($"이용권 유무 : {(user.hasTicket ? "Y" : "N")}");
                Console.WriteLine($"현재 타임머신 대여중 : {(user.isRenting ? "Y" : "N")}");
                // TODO 현재 시간대 넣는것도 생각해보기?
                Console.WriteLine("-----------------------------");

                Console.WriteLine("\n1. 대여/반납");
                Console.WriteLine("2. 이용권 구매");
                Console.WriteLine("3. 소지금 충전");
                Console.WriteLine("4. 이용 내역 조회");
                Console.WriteLine("5. 회원 탈퇴");
                Console.WriteLine("0. 로그아웃");
                Console.Write("입럭 : ");

                if (!int.TryParse(Console.ReadLine(), out int menuChoice))
                {
                    menuChoice = -1;
                }

                switch (menuChoice)
                {
                    case 1:
                        if (!user.isRenting)
                        {
                            DatabaseManager.UpdateExpiredTickets();
                            RefreshUser(user);
                            if (!user.hasTicket)
                                Console.WriteLine("유효한 이용권이 없습니다.");
                            else
                            {
                                if (UserRentTimeMachine(user))
                                    RefreshUser(user);
                            }
                        }
                        else
                        {
                            if (UserReturnTimeMachine(user))
                                RefreshUser(user);
                        }
                        break;
                    case 2:
                        DatabaseManager.UpdateExpiredTickets();
                        RefreshUser(user);
                        if (user.hasTicket)
                            Console.WriteLine("이미 이용권을 소유하고 있습니다.");
                        else
                        {
                            if (UserBuyTicket(user))
                                RefreshUser(user);
                        }
                        break;
                    case 3:
                        if (UserChargeBalance(user))
                            RefreshUser(user);
                        break;
                    case 4:
                        UserViewUsageHistory(user);
                        break;
                    case 5:
                        if (UserDelUser(user))
                            userMenuQuit = true;
                        break;
                    case 0:
                        userMenuQuit = true;
                        break;
                    default:
                        Console.WriteLine("0부터 5 사이의 수를 입력해주세요.");
                        break;
                }
            }
        }

        private static bool UserRentTimeMachine(User user)
        {
            bool result = false;
            Station station = RentSelectStation();
            if (station == null) return false;
            TimeMachine timeMachine = RentSelectTimeMachine(station);
            if (timeMachine == null) return false;
            string destination = RentSelectDestination(timeMachine.timeMachineModel);
            if (destination == null) return false;
            if (DatabaseManager.RentTimeMachine(user.userID, timeMachine.timeMachineID, destination))
            {
                Console.WriteLine("타임머신을 대여했습니다.");
                result = true;
            }
            else
                Console.WriteLine("타임머신 대여에 실패했습니다.");
            return result;
        }

        // TODO 주소, 이름으로도 대여소 검색할 수 있게 하기
        private static Station RentSelectStation()
        {
            Station result = null;
            List<Station> stationList = DatabaseManager.GetStations();
            if (stationList.Count > 0)
            {
                PrintStationList(stationList);
                bool selectedStationIDExists = false;
                while (!selectedStationIDExists)
                {
                    Console.Write("대여소의 ID를 입력해주세요.(0으로 돌아가기) : ");

                    if (!int.TryParse(Console.ReadLine(), out int stationID))
                    {
                        stationID = -1;
                    }

                    if (stationID == 0)
                        selectedStationIDExists = true;
                    else
                    {
                        Station targetStation = stationList.FirstOrDefault(station => station.stationID == stationID);
                        if (targetStation != null)
                        {
                            selectedStationIDExists = true;
                            result = targetStation;
                        }
                        else
                            Console.WriteLine("해당하는 ID의 대여소가 없습니다.");
                    }
                }
            }
            else
                Console.WriteLine("등록된 대여소가 없습니다.");
            return result;
        }

        private static TimeMachine RentSelectTimeMachine(Station station)
        {
            TimeMachine result = null;
            List<TimeMachine> timeMachineList = DatabaseManager.GetTimeMachinesWithStation(station.stationID);
            if (timeMachineList.Count > 0)
            {
                PrintTimeMachineList(timeMachineList);
                bool selectedTimeMachineIDExists = false;
                while (!selectedTimeMachineIDExists)
                {
                    Console.WriteLine("타임머신 모델 past : 과거로만 이동 가능.");
                    Console.WriteLine("타임머신 모델 future : 미래로만 이동 가능.");
                    Console.Write("타임머신의 ID를 입력해주세요.(0으로 돌아가기) : ");

                    if (!int.TryParse(Console.ReadLine(), out int timeMachineID))
                    {
                        timeMachineID = -1;
                    }

                    if (timeMachineID == 0)
                        selectedTimeMachineIDExists = true;
                    else
                    {
                        TimeMachine targetTimeMachine = timeMachineList.FirstOrDefault(tm => tm.timeMachineID == timeMachineID);
                        if (targetTimeMachine != null)
                        {
                            selectedTimeMachineIDExists = true;
                            result = targetTimeMachine;
                        }
                        else
                            Console.WriteLine("해당하는 ID의 타임머신이 없습니다.");
                    }
                }
            }
            else
                Console.WriteLine("주차된 타임머신이 없습니다.");
            return result;
        }

        private static bool UserReturnTimeMachine(User user)
        {
            bool result = false;
            Station returnStation = null;
            List<Station> stationList = DatabaseManager.GetStations();
            if (stationList.Count > 0)
            {
                PrintStationList(stationList);
                bool selectedStationIDExists = false;
                while (!selectedStationIDExists)
                {
                    Console.Write("대여소의 ID를 입력해주세요.(0으로 돌아가기) : ");

                    if (!int.TryParse(Console.ReadLine(), out int stationID))
                    {
                        stationID = -1;
                    }

                    if (stationID == 0)
                        return result;
                    else
                    {
                        Station targetStation = stationList.FirstOrDefault(station => station.stationID == stationID);
                        if (targetStation != null)
                        {
                            selectedStationIDExists = true;
                            returnStation = targetStation;
                        }
                        else
                            Console.WriteLine("해당하는 ID의 대여소가 없습니다.");
                    }
                }
                if (DatabaseManager.ReturnTimeMachine(user, returnStation.stationID))
                {
                    Console.WriteLine("타임머신을 반납했습니다.");
                    result = true;
                }
                else
                    Console.WriteLine("타임머신 반납에 실패했습니다.");
            }
            else
                Console.WriteLine("등록된 대여소가 없습니다.");
            return result;
        }

        private static string RentSelectDestination(string model)
        {
            while (true)
            {
                Console.WriteLine("\n시간대 입력 예) 2025-05-21, BC 2333-10-03)");
                Console.Write("이동할 시간대를 입력해주세요.(0으로 돌아가기)  : ");
                string input = Console.ReadLine().Trim();

                if (input == "0")
                    return null;

                if (DateTime.TryParseExact(input, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime targetDate))
                {
                    if (model.Equals("past") && targetDate >= DateTime.Now)
                    {
                        Console.WriteLine("과거 모델입니다. 현재보다 이전의 시간대를 입력해주세요.");
                        continue;
                    }

                    if (model.Equals("future") && targetDate <= DateTime.Now)
                    {
                        Console.WriteLine("미래 모델입니다. 현재보다 미래의 시간대를 입력해주세요.");
                        continue;
                    }

                    return input;
                }

                if (input.ToUpper().StartsWith("BC"))
                {
                    if (model.Equals("future"))
                    {
                        Console.WriteLine("미래 모델입니다. 현재보다 미래의 시간대를 입력해주세요.");
                        continue;
                    }

                    string datePart = input.Substring(2).Trim();
                    if (DateTime.TryParseExact(datePart, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out _))
                    {
                        return "BC " + datePart;
                    }
                }
            }
        }

        private static bool UserDelUser(User targetUser)
        {
            bool result = false;
            // 현재 대여중인 사용자의 삭제 방지
            if (!targetUser.isRenting)
            {
                bool hasConfirmed = false;
                while (!hasConfirmed)
                {
                    Console.Write("\n정말로 탈퇴하시겠습니까?(y/n) : ");
                    string confirmDel = Console.ReadLine();
                    if (confirmDel.ToLower().Equals("y"))
                    {
                        hasConfirmed = true;
                        if (DatabaseManager.DeleteUser(targetUser.userID))
                        {
                            Console.WriteLine("회원 탈퇴에 성공했습니다.");
                            result = true;
                        }
                        else
                            Console.Write("회원 탈퇴에 실패했습니다.");
                    }
                    else if (confirmDel.ToLower().Equals("n"))
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
                Console.WriteLine("\n현재 타임머신을 대여중인 사용자는 탈퇴할 수 없습니다.");
                return result;
        }

        private static bool UserChargeBalance(User targetUser)
        {
            bool isValidAmount = false;
            bool result = false;
            while (!isValidAmount)
            {
                Console.Write("\n충전할 금액을 입력해주세요.(0으로 돌아가기) : ");
                
                if (!int.TryParse(Console.ReadLine(), out int amountInput))
                {
                    amountInput = -1;
                }

                if (amountInput == 0)
                    isValidAmount = true;
                else if (amountInput < 0)
                    Console.WriteLine("유효한 금액을 입력해주세요.");
                else
                {
                    isValidAmount = true;
                    if (DatabaseManager.ChargeBalance(targetUser.userID, amountInput))
                    {
                        Console.WriteLine($"{amountInput}원을 충전했습니다.");
                        result = true;
                    }  
                    else
                        Console.WriteLine("충전에 실패했습니다.");
                }
            }
            return result;
        }

        private static bool UserBuyTicket(User user)
        {
            bool result = false;
            bool flag = false;
            int ticketPrice = 0;
            int durationDays = 0;
            while (!flag)
            {
                Console.WriteLine("\n1. 일일권 구매 - 1,000원");
                Console.WriteLine("2. 7일권 구매 - 3,000원");
                Console.WriteLine("3. 30일권 구매 - 5,000원");
                Console.WriteLine("4. 180일권 구매 - 15,000원");
                Console.WriteLine("5. 365일권 구매 - 30,000원");
                Console.WriteLine("0. 상위 메뉴로 돌아가기");
                Console.Write("입력 : ");

                if (!int.TryParse(Console.ReadLine(), out int userInput))
                {
                    userInput = -1;
                }

                switch (userInput)
                {
                    case 1: ticketPrice = 1000; durationDays = 1; break;
                    case 2: ticketPrice = 3000; durationDays = 7; break;
                    case 3: ticketPrice = 5000; durationDays = 30; break;
                    case 4: ticketPrice = 15000; durationDays = 180; break;
                    case 5: ticketPrice = 30000; durationDays = 365; break;
                    case 0: return false;
                    default:
                        Console.WriteLine("0부터 5 사이의 수를 입력해주세요.");
                        continue;
                }

                if (user.balance >= ticketPrice)
                {
                    flag = true;
                    result = true;
                    if (DatabaseManager.PurchaseTicket(user.userID, durationDays, ticketPrice))
                        Console.WriteLine("이용권을 구매했습니다.");
                    else
                        Console.WriteLine("이용권 구매에 실패했습니다.");
                }
                else
                    Console.WriteLine("소지금이 부족합니다.");
            }
            return result;
        }

        private static void UserViewUsageHistory(User user)
        {
            bool viewHistoryQuit = false;
            while (!viewHistoryQuit)
            {
                Console.WriteLine("\n1. 사용자 대여내역 조회");
                Console.WriteLine("2. 사용자 이용권 구매내역 조회");
                Console.WriteLine("0. 상위 메뉴로 돌아가기");
                Console.Write("입력 : ");

                if (!int.TryParse(Console.ReadLine(), out int viewInput))
                {
                    viewInput = -1;
                }

                switch (viewInput)
                {
                    case 2:
                        List<Ticket> ticketList = DatabaseManager.GetTicketsWithUserID(user.userID);
                        PrintTicketList(ticketList);
                        break;
                    case 0:
                        viewHistoryQuit = true;
                        break;
                    default:
                        Console.WriteLine("0부터 2 사이의 수를 입력해주세요.");
                        break;
                }
            }
        }

        // 유저 정보 새로고침
        private static void RefreshUser(User targetUser)
        {
            User refreshedUser = DatabaseManager.GetUserByID(targetUser.userID);

            if (refreshedUser != null)
            {
                targetUser.balance = refreshedUser.balance;
                targetUser.hasTicket = refreshedUser.hasTicket;
                targetUser.isRenting = refreshedUser.isRenting;
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
                
                if (!int.TryParse(Console.ReadLine(), out int menuChoice))
                {
                    menuChoice = -1;
                }

                switch (menuChoice)
                {
                    case 1:
                        AdminViewUser();
                        break;
                    case 2:
                        AddUser();
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

                if (!int.TryParse(Console.ReadLine(), out int menuChoice))
                {
                    menuChoice = -1;
                }

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

        private static void AdminTimeMachineManage()
        {
            bool timeMachineMenuQuit = false;
            while (!timeMachineMenuQuit)
            {
                Console.WriteLine("\n1. 타임머신 조회");
                Console.WriteLine("2. 타임머신 추가");
                Console.WriteLine("3. 타임머신 삭제");
                Console.WriteLine("0. 상위 메뉴로 돌아가기\n");
                Console.Write("입력 : ");

                if (!int.TryParse(Console.ReadLine(), out int menuChoice))
                {
                    menuChoice = -1;
                }

                switch (menuChoice)
                {
                    case 1:
                        AdminViewTimeMachine();
                        break;
                    case 2:
                        AdminAddTimeMachine();
                        break;
                    case 3:
                        AdminDelTimeMachine();
                        break;
                    case 0:
                        timeMachineMenuQuit = true;
                        break;
                    default:
                        Console.WriteLine("0부터 3 사이의 수를 입력해주세요.");
                        break;
                }
            }
        }

        private static void AdminViewUser()
        {
            List<User> userListForView = DatabaseManager.GetUsers();
            if (userListForView.Count > 0)
            {
                PrintUserList(userListForView);
            }
            else
                Console.WriteLine("등록된 사용자가 없습니다.");
        }

        private static void AddUser()
        {
            Console.Write("\n추가할 사용자의 이름을 입력해주세요 : ");
            string newUserName = Console.ReadLine();
            if (newUserName.Length <= 45)
            {
                List<User> userList = DatabaseManager.GetUsers();
                
                // 이름 중복 시 구분을 위해 뒤에 숫자를 붙힘
                if (userList.Any(user => user.userName.Equals(newUserName)))
                {
                    int i = 1;
                    bool flag = false;
                    while (!flag)
                    {
                        if (userList.Any(user => user.userName.Equals($"{newUserName}({i})")))
                            i++;
                        else
                        {
                            flag = true;
                            newUserName = $"{newUserName}({i})";
                        }
                    }
                }
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

                if (!int.TryParse(Console.ReadLine(), out int howToSearchUser))
                {
                    howToSearchUser = -1;
                }

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

        // TODO 대여중인 유저 삭제 방지
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
                    Console.Write("제거할 사용자의 ID를 입력해주세요 (0으로 돌아가기) : ");
                    
                    // 실패 시 -1을 넣어 ID없음 로직으로 빠지게 함 (0은 뒤로가기이므로 겹치지 않게)
                    if (!int.TryParse(Console.ReadLine(), out int delUserID))
                    {
                        delUserID = -1; 
                    }

                    if (delUserID == 0)
                        selectedUserIdExists = true;
                    else if (userList.Any(user => user.userID == delUserID))
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
                        Console.Write("해당 사용자를 제거하시겠습니까?(y/n) : ");
                        string confirmDel = Console.ReadLine();
                        if (confirmDel.ToLower().Equals("y"))
                        {
                            hasConfirmed = true;
                            if (DatabaseManager.DeleteUser(userList[0].userID))
                                Console.WriteLine("사용자를 성공적으로 제거했습니다.");
                            else
                                Console.WriteLine("사용자를 제거할 수 없었습니다.");
                        }
                        else if (confirmDel.ToLower().Equals("n")) 
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
                        Console.Write("제거할 사용자의 ID를 입력해주세요.(0으로 돌아가기) : ");
                        
                        if (!int.TryParse(Console.ReadLine(), out int delUserID))
                        {
                            delUserID = -1;
                        }

                        if (delUserID == 0)
                            selectedUserIdExists_1 = true;
                        else if (userList.Any(user => user.userID == delUserID))
                        {
                            selectedUserIdExists_1 = true;
                            if (DatabaseManager.DeleteUser(delUserID))
                                Console.WriteLine("사용자를 성공적으로 제거했습니다.");
                            else
                                Console.WriteLine("사용자를 제거할 수 없었습니다.");
                        }
                        else
                            Console.WriteLine("해당하는 ID의 사용자가 없습니다.");
                    }
                }
            }
        }

        private static void PrintUserListOnlyIDAndName(List<User> userList)
        {
            Console.WriteLine("-------------------------");
            Console.WriteLine(" ID    이름           ");
            Console.WriteLine("-------------------------");
            foreach (User user in userList)
            {
                Console.WriteLine(" {0,-3}   {1}",
                    user.userID,
                    user.userName,
                    user.balance + "원",
                    (user.hasTicket == true) ? "Y" : "N"
                );
            }
            Console.WriteLine("-------------------------\n");
        }

        private static void PrintUserList(List<User> userList)
        {
            Console.WriteLine("----------------------------------------------------------------------");
            Console.WriteLine(" ID    이름          잔액             이용권    대여 상태");
            Console.WriteLine("----------------------------------------------------------------------");

            foreach (User user in userList)
            {
                string balanceDisplay = user.balance.ToString("N0") + "원";
                Console.WriteLine(" {0,-4}  {1,-10}  {2,14}   {3,-6}   {4}",
                    user.userID,  
                    user.userName,      
                    balanceDisplay,          
                    (user.hasTicket ? "Y" : "N"),  
                    (user.isRenting ? "대여중" : "N") 
                );
            }
            Console.WriteLine("----------------------------------------------------------------------\n");
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
                
                if (!int.TryParse(Console.ReadLine(), out int howToSelectStation))
                {
                    howToSelectStation = -1;
                }

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
                    
                    if (!int.TryParse(Console.ReadLine(), out int delStationID))
                    {
                        delStationID = -1;
                    }
                    if (delStationID == 0)
                        selectedStationIDExists = true;
                    else
                    {
                        Station targetStation = stationList.FirstOrDefault(station => station.stationID == delStationID);
                        if (targetStation != null)
                        {
                            if (targetStation.parkedTimeMachine > 0)
                                Console.WriteLine("타임머신이 주차되어 있는 대여소는 삭제할 수 없습니다.");
                            else
                            {
                                selectedStationIDExists = true;
                                if (DatabaseManager.DeleteStation(delStationID))
                                    Console.WriteLine("대여소를 성공적으로 제거했습니다.");
                                else
                                    Console.WriteLine("대여소를 제거할 수 없었습니다.");
                            }
                        }
                        else
                            Console.WriteLine("해당하는 ID의 대여소가 없습니다.");
                    }
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
                            if (stationList[0].parkedTimeMachine == 0)
                            {
                                if (DatabaseManager.DeleteStation(stationList[0].stationID))
                                    Console.WriteLine("대여소를 성공적으로 제거했습니다.");
                                else
                                    Console.WriteLine("대여소를 제거할 수 없었습니다.");
                            }
                            else
                                Console.WriteLine("타임머신이 주차되어 있는 대여소는 삭제할 수 없습니다.");
                        }
                        else if (confirmDel.ToLower().Equals("n"))
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

                        if (!int.TryParse(Console.ReadLine(), out int delStationID))
                        {
                            delStationID = -1;
                        }

                        if (delStationID == 0)
                            selectedStationIDExists = true;
                        else
                        {
                            Station targetStation = stationList.FirstOrDefault(station => station.stationID == delStationID);
                            if (targetStation != null)
                            {
                                if (targetStation.parkedTimeMachine > 0)
                                    Console.WriteLine("타임머신이 주차되어 있는 대여소는 삭제할 수 없습니다.");
                                else
                                {
                                    selectedStationIDExists = true;
                                    if (DatabaseManager.DeleteStation(delStationID))
                                        Console.WriteLine("대여소를 성공적으로 제거했습니다.");
                                    else
                                        Console.WriteLine("대여소를 제거할 수 없었습니다.");
                                }
                            }
                            else
                                Console.WriteLine("해당하는 ID의 대여소가 없습니다.");
                        }
                    }
                }
            }
        }

        private static void DelStationWithAddress()
        {
            Console.Write("검색할 대여소의 주소를 입력해주세요 : ");
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
                            if (stationList[0].parkedTimeMachine == 0)
                            {
                                if (DatabaseManager.DeleteStation(stationList[0].stationID))
                                    Console.WriteLine("대여소를 성공적으로 제거했습니다.");
                                else
                                    Console.WriteLine("대여소를 제거할 수 없었습니다.");
                            }
                            else
                                Console.WriteLine("타임머신이 주차되어 있는 대여소는 삭제할 수 없습니다.");
                        }
                        else if (confirmDel.ToLower().Equals("n"))
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
                        
                        if (!int.TryParse(Console.ReadLine(), out int delStationID))
                        {
                            delStationID = -1;
                        }

                        if (delStationID == 0)
                            selectedStationIDExists = true;
                        else
                        {
                            Station targetStation = stationList.FirstOrDefault(station => station.stationID == delStationID);
                            if (targetStation != null)
                            {
                                if (targetStation.parkedTimeMachine > 0)
                                    Console.WriteLine("타임머신이 주차되어 있는 대여소는 삭제할 수 없습니다.");
                                else
                                {
                                    selectedStationIDExists = true;
                                    if (DatabaseManager.DeleteStation(delStationID))
                                        Console.WriteLine("대여소를 성공적으로 제거했습니다.");
                                    else
                                        Console.WriteLine("대여소를 제거할 수 없었습니다.");
                                }
                            }
                            else
                                Console.WriteLine("해당하는 ID의 대여소가 없습니다.");
                        }
                    }
                }
            }
        }

        private static void PrintStationList(List<Station> stationList)
        {
            Console.WriteLine("\n------------------------------------------------------------------------------------------");
            Console.WriteLine(" ID    대여소 이름           보관 타임머신  대여소 주소");
            Console.WriteLine("------------------------------------------------------------------------------------------");
            foreach (Station station in stationList)
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

        private static void AdminViewTimeMachine()
        {
            List<TimeMachine> timeMachineList = DatabaseManager.GetTimeMachines();
            if (timeMachineList.Count > 0)
            {
                PrintTimeMachineList(timeMachineList);
            }
            else
                Console.WriteLine("등록된 타임머신이 없습니다.");
        }

        private static void AdminAddTimeMachine()
        {
            string selectedModel = null;
            List<Station> stationList = DatabaseManager.GetStations();
            if (stationList.Count > 0)
            {
                bool hasModelSelected = false;
                while (!hasModelSelected)
                {
                    Console.Write("타임머신의 모델을 설정해주세요 (p(past)/f(future) 0:돌아가기) : ");
                    string modelInput = Console.ReadLine();
                    if (modelInput.Equals("0"))
                        return;
                    else if (modelInput.ToLower().Equals("p"))
                    {
                        selectedModel = "past";
                        hasModelSelected = true;
                    }
                    else if (modelInput.ToLower().Equals("f"))
                    {
                        selectedModel = "future";
                        hasModelSelected = true;
                    }
                    else
                        Console.WriteLine("p f 0 중 하나를 입력해주세요.");
                }

                PrintStationList(stationList);
                bool selectedStationIDExists = false;
                while (!selectedStationIDExists)
                {
                    Console.Write("타임머신을 보관할 대여소의 ID를 입력해주세요.(0으로 돌아가기) : ");

                    if (!int.TryParse(Console.ReadLine(), out int locateStationID))
                    {
                        locateStationID = -1;
                    }

                    if (locateStationID == 0)
                        selectedStationIDExists = true;
                    else if (stationList.Any(station => station.stationID == locateStationID))
                    {
                        selectedStationIDExists = true;
                        if (DatabaseManager.AddTimeMachine(selectedModel, locateStationID))
                            Console.WriteLine("타임머신이 추가되었습니다.");
                        else
                            Console.WriteLine("타임머신 추가에 실패했습니다.");
                    }
                    else
                        Console.WriteLine("해당하는 ID의 대여소가 없습니다.");
                }
            }
            else
                Console.WriteLine("타임머신을 보관할 대여소가 없습니다. 우선 대여소를 추가해주세요.");
        }

        private static void AdminDelTimeMachine()
        {
            bool delMenuQuit = false;
            while (!delMenuQuit)
            {
                Console.WriteLine("\n1. 직접 선택");
                Console.WriteLine("2. 보관되어 있는 대여소로 검색");
                Console.WriteLine("0. 상위 메뉴로 돌아가기");
                Console.Write("입력 : ");

                if (!int.TryParse(Console.ReadLine(), out int howToSearchTimeMachine))
                {
                    howToSearchTimeMachine = -1;
                }

                switch (howToSearchTimeMachine)
                {
                    case 1:
                        DelTimeMachineWithSelect();
                        break;
                    case 2:
                        DelTimeMachineWithStation();
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

        private static void DelTimeMachineWithSelect()
        {
            List<TimeMachine> timeMachineList = DatabaseManager.GetTimeMachines();
            if (timeMachineList.Count > 0)
            {
                PrintTimeMachineList(timeMachineList);

                bool selectedTimeMachineIDExists = false;
                while (!selectedTimeMachineIDExists)
                {
                    Console.Write("제거할 타임머신의 ID를 입력해주세요 (0으로 돌아가기) : ");

                    if (!int.TryParse(Console.ReadLine(), out int delTimeMachineID))
                    {
                        delTimeMachineID = -1;
                    }

                    if (delTimeMachineID == 0)
                        selectedTimeMachineIDExists = true;

                    else
                    {
                        TimeMachine targetTimeMachine = timeMachineList.FirstOrDefault(tm => tm.timeMachineID == delTimeMachineID);
                        if (targetTimeMachine != null)
                        {
                            selectedTimeMachineIDExists = true;

                            // 대여중 상태인 타임머신 삭제 방지 
                            if (targetTimeMachine.currentStationID == 0)
                                Console.WriteLine("대여중인 타임머신은 삭제할 수 없습니다.");
                            else
                            {
                                if (DatabaseManager.DeleteTimeMachine(delTimeMachineID))
                                    Console.WriteLine("타임머신을 성공적으로 제거했습니다.");
                                else
                                    Console.WriteLine("타임머신을 제거할 수 없었습니다.");
                            }
                        }
                        else
                            Console.WriteLine("해당하는 ID의 타임머신이 없습니다.");
                    }
                }
            }
            else
                Console.WriteLine("등록된 타임머신이 없습니다.");
        }

        private static void DelTimeMachineWithStation()
        {
            List<Station> stationList = DatabaseManager.GetStations();
            List<TimeMachine> timeMachineList = null;
            if (stationList.Count > 0)
            {
                PrintStationList(stationList);
                bool selectedStationIDExists = false;
                while (!selectedStationIDExists)
                {
                    Console.Write("검색할 대여소의 ID를 입력해주세요.(0으로 돌아가기) : ");

                    if (!int.TryParse(Console.ReadLine(), out int locateStationID))
                    {
                        locateStationID = -1;
                    }

                    if (locateStationID == 0)
                       return;
                    else if (stationList.Any(station => station.stationID == locateStationID))
                    {
                        selectedStationIDExists = true;
                        timeMachineList = DatabaseManager.GetTimeMachinesWithStation(locateStationID);
                    }
                    else
                        Console.WriteLine("해당하는 ID의 대여소가 없습니다.");
                }
                Console.WriteLine(timeMachineList.Count());
                if (timeMachineList.Count > 0)
                {
                    PrintTimeMachineList(timeMachineList);
                    bool selectedTimeMachineIDExists = false;
                    while (!selectedTimeMachineIDExists)
                    {
                        Console.Write("제거할 타임머신의 ID를 입력해주세요 (0으로 돌아가기) : ");

                        if (!int.TryParse(Console.ReadLine(), out int delTimeMachineID))
                        {
                            delTimeMachineID = -1;
                        }

                        if (delTimeMachineID == 0)
                            selectedTimeMachineIDExists = true;
                        else if (timeMachineList.Any(tm => tm.timeMachineID == delTimeMachineID))
                        {
                            selectedTimeMachineIDExists = true;
                            if (DatabaseManager.DeleteTimeMachine(delTimeMachineID))
                                Console.WriteLine("타임머신을 성공적으로 제거했습니다.");
                            else
                                Console.WriteLine("타임머신을 제거할 수 없었습니다.");
                        }
                        else
                            Console.WriteLine("해당하는 ID의 타임머신이 없습니다.");
                    }
                }
                else
                    Console.WriteLine("해당 대여소에 보관되어 있는 타임머신이 없습니다.");
            }
            else
                Console.WriteLine("등록된 대여소가 없습니다.");
        }

        private static void AdminViewTicket()
        {
            List<Ticket> ticketList = DatabaseManager.GetTickets();
            if (ticketList.Count > 0)
                PrintTicketList(ticketList);
            else
                Console.WriteLine("조회할 이용권 구매내역이 없습니다.");
        }

        private static void PrintTimeMachineList(List<TimeMachine> timeMachineList)
        {
            Console.WriteLine("\n----------------------------------------------------------------------");
            Console.WriteLine(" ID     모델         현재 시간대            현재 대여소");
            Console.WriteLine("----------------------------------------------------------------------");

            foreach (TimeMachine tm in timeMachineList)
            {
                string displayTime = tm.currentEraPoint;
                if (displayTime.Equals("CurrentTime"))
                    displayTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm");

                Console.WriteLine(" {0,-4}   {1,-10}   {2,-20}   {3}",
                    tm.timeMachineID,      
                    tm.timeMachineModel,
                    displayTime,   
                    tm.currentStationName  
                );
            }
            Console.WriteLine("----------------------------------------------------------------------");
        }

        private static void PrintTicketList(List<Ticket> ticketList)
        {
            Console.WriteLine("\n---------------------------------------------------------------------------------------");
            Console.WriteLine(" ID     사용자 이름      구매 일시            만료 일시            상태");
            Console.WriteLine("---------------------------------------------------------------------------------------");
            foreach (Ticket ticket in ticketList)
            {
                string pTime = ticket.purchaseTime.ToString("yyyy/MM/dd HH:mm");
                string eTime = ticket.expireTime.ToString("yyyy/MM/dd HH:mm");
                Console.WriteLine(" {0,-4}   {1,-11}   {2,-18}   {3,-18}   {4}",
                    ticket.ticketID,
                    ticket.userName,
                    pTime,
                    eTime,
                    ticket.status
                );
            }
            Console.WriteLine("---------------------------------------------------------------------------------------\n");
        }
    }
}