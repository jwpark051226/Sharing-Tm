from utils.ui_utils import UIUtils
from menus.admin.user_menu import UserManagementMenu       
from menus.admin.station_menu import StationManagementMenu 
from menus.admin.tm_menu import TimeMachineManagementMenu 
from services.rental_service import RentalService
from services.ticket_service import TicketService
class AdminMenu:
    """관리자 모드 메인 컨트롤러 클래스"""

    @staticmethod
    def show_main_menu():
        quit_menu = False
        while not quit_menu:
            print("\n=== 관리자 메뉴 ===")
            print("1. 사용자 관리")
            print("2. 대여소 관리")
            print("3. 타임머신 관리")
            print("4. 대여목록 조회")
            print("5. 이용권 구매내역 조회")
            print("0. 메뉴 선택으로 돌아가기\n")
            print("입력 : ", end="")

            choice = UIUtils.get_input_int()
            
            if choice == 1:
                UserManagementMenu.show_menu()
            elif choice == 2:
                StationManagementMenu.show_menu()
            elif choice == 3:
                TimeMachineManagementMenu.show_menu()
            elif choice == 4:
                rl = RentalService.get_all_rentals()
                if len(rl) > 0:
                    UIUtils.print_rental_history(rl)
                else:
                    print("조회할 대여내역이 없습니다.")
            elif choice == 5:
                tl = TicketService.get_all_tickets()
                if len(tl) > 0:
                    UIUtils.print_ticket_list(tl)
                else:
                    print("조회할 이용권 구매내역이 없습니다.")
            elif choice == 0:
                quit_menu = True
            else:
                print("0부터 5 사이의 수를 입력해주세요.")