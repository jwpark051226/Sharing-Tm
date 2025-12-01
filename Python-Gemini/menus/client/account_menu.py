import sys
from services.rental_service import RentalService
from services.user_service import UserService
from services.ticket_service import TicketService
from utils.ui_utils import UIUtils

class ClientAccountMenu:
    """사용자 계정 관리 메뉴 클래스"""

    @staticmethod
    def buy_ticket(user):
        flag = False
        while not flag:
            print("\n1. 일일권 구매 - 1,000원")
            print("2. 7일권 구매 - 3,000원")
            print("3. 30일권 구매 - 5,000원")
            print("4. 180일권 구매 - 15,000원")
            print("5. 365일권 구매 - 30,000원")
            print("0. 상위 메뉴로 돌아가기")
            print("입력 : ", end="")
            
            choice = UIUtils.get_input_int()
            
            ticket_price = 0
            duration_days = 0

            if choice == 1: ticket_price, duration_days = 1000, 1
            elif choice == 2: ticket_price, duration_days = 3000, 7
            elif choice == 3: ticket_price, duration_days = 5000, 30
            elif choice == 4: ticket_price, duration_days = 15000, 180
            elif choice == 5: ticket_price, duration_days = 30000, 365
            elif choice == 0: return False
            else:
                print("0부터 5 사이의 수를 입력해주세요.")
                continue
            
            if user['balance'] >= ticket_price:
                if TicketService.purchase_ticket(user['userid'], duration_days, ticket_price):
                    print("이용권을 구매했습니다.")
                    return True # 상태 변경됨
                else:
                    print("이용권 구매에 실패했습니다.")
            else:
                print("소지금이 부족합니다.")
        return False

    @staticmethod
    def charge_balance(user):
        is_valid = False
        while not is_valid:
            print("\n충전할 금액을 입력해주세요.(0으로 돌아가기) : ", end="")
            amount = UIUtils.get_input_int()
            
            if amount == 0:
                is_valid = True
            elif amount < 0:
                print("유효한 금액을 입력해주세요.")
            else:
                is_valid = True
                if UserService.charge_balance(user['userid'], amount):
                    print(f"{amount}원을 충전했습니다.")
                    return True # 상태 변경됨
                else:
                    print("충전에 실패했습니다.")
        return False

    @staticmethod
    def view_history(user):
        sub_quit = False
        while not sub_quit:
            print("\n1. 사용자 대여내역 조회")
            print("2. 사용자 이용권 구매내역 조회")
            print("0. 상위 메뉴로 돌아가기")
            print("입력 : ", end="")
            sub_c = UIUtils.get_input_int()
            if sub_c == 1:
                rl = RentalService.get_rental_history_by_user(user['userid'])
                if rl: UIUtils.print_rental_history(rl)
                else: print("대여 내역이 없습니다.")
            elif sub_c == 2:
                tl = TicketService.get_tickets_by_user(user['userid'])
                if tl: UIUtils.print_ticket_list(tl)
                else: print("이용권 내역이 없습니다.")
            elif sub_c == 0:
                sub_quit = True
            else:
                print("0부터 2 사이의 수를 입력해주세요.")

    @staticmethod
    def delete_account(user):
        # is_renting 키가 없을 경우 False 기본값
        is_renting = user.get('is_renting', False) 
        if not is_renting:
            has_confirmed = False
            while not has_confirmed:
                print("\n정말로 탈퇴하시겠습니까?(y/n) : ", end="")
                confirm = sys.stdin.readline().strip()
                if confirm.lower() == "y":
                    has_confirmed = True
                    if UserService.delete_user(user['userid']):
                        print("회원 탈퇴에 성공했습니다.")
                        return True # 탈퇴 성공 (로그아웃 처리 필요)
                    else:
                        print("회원 탈퇴에 실패했습니다.", end="")
                elif confirm.lower() == "n":
                    has_confirmed = True
                else:
                    print("y 혹은 n을 입력해주세요.")
        else:
            print("\n현재 타임머신을 대여중인 사용자는 탈퇴할 수 없습니다.")
        return False