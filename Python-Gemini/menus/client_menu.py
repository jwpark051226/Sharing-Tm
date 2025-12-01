import datetime
from services.user_service import UserService
from services.rental_service import RentalService
from utils.ui_utils import UIUtils
from menus.client.auth_menu import ClientAuthMenu
from menus.client.rent_menu import ClientRentMenu
from menus.client.account_menu import ClientAccountMenu

class ClientMenu:
    """일반 사용자 모드 메인 컨트롤러 클래스"""
    
    @staticmethod
    def _refresh_user(user_dict):
        users = UserService.get_users()
        target = next((u for u in users if u['userid'] == user_dict['userid']), None)
        return target if target else user_dict

    @staticmethod
    def _user_dashboard(user):
        quit_flag = False
        while not quit_flag:
            current_era = user.get('current_era', 'CurrentTime')
            if current_era == "CurrentTime":
                current_era = datetime.datetime.now().strftime("%Y-%m-%d %I:%M")
            
            print("\n-------------사용자 정보-------------")
            print(f"이름 : {user['username']}")
            print(f"소지금 : {user['balance']}원")
            yn = "Y" if user['has_valid_ticket'] else "N"
            print(f"이용권 유무 : {yn}")
            rent_yn = "Y" if user.get('is_renting', False) else "N"
            print(f"현재 타임머신 대여중 : {rent_yn}")
            print(f"현재 체류 시간대 : {current_era}")
            print("-------------------------------------")
            
            print("\n1. 대여/반납")
            print("2. 이용권 구매")
            print("3. 소지금 충전")
            print("4. 이용 내역 조회")
            print("5. 회원 탈퇴")
            print("0. 로그아웃")
            print("입럭 : ", end="") 
            
            choice = UIUtils.get_input_int()

            if choice == 1:
                if not user.get('is_renting', False):
                    # 대여 시도 전 만료 티켓 정리
                    RentalService.update_expired_tickets()
                    user = ClientMenu._refresh_user(user)
                    
                    if not user['has_valid_ticket']:
                        print("유효한 이용권이 없습니다.")
                    else:
                        if ClientRentMenu.rent_process(user):
                            user = ClientMenu._refresh_user(user)
                else:
                    # 반납 시도
                    if ClientRentMenu.return_process(user):
                        user = ClientMenu._refresh_user(user)

            elif choice == 2:
                # 이용권 구매
                RentalService.update_expired_tickets()
                user = ClientMenu._refresh_user(user)
                if user['has_valid_ticket']:
                    print("이미 이용권을 소유하고 있습니다.")
                else:
                    if ClientAccountMenu.buy_ticket(user):
                        user = ClientMenu._refresh_user(user)

            elif choice == 3:
                # 소지금 충전
                if ClientAccountMenu.charge_balance(user):
                    user = ClientMenu._refresh_user(user)

            elif choice == 4:
                # 내역 조회
                ClientAccountMenu.view_history(user)

            elif choice == 5:
                # 탈퇴 (성공 시 루프 종료)
                if ClientAccountMenu.delete_account(user):
                    quit_flag = True

            elif choice == 0:
                quit_flag = True

            else:
                print("0부터 5 사이의 수를 입력해주세요.")

    @staticmethod
    def user_login_menu():
        logged_in_user = ClientAuthMenu.run_login_process()
        
        if logged_in_user:
            ClientMenu._user_dashboard(logged_in_user)