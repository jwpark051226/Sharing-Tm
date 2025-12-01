import sys
from services.user_service import UserService
from utils.ui_utils import UIUtils

class ClientAuthMenu:
    """사용자 인증(로그인/회원가입) 메뉴 클래스"""

    @staticmethod
    def login_with_select():
        selected_user = None
        user_list = UserService.get_users()
        if len(user_list) > 0:
            UIUtils.print_user_list_only_id_and_name(user_list)
            exists = False
            while not exists:
                print("로그인할 사용자의 ID를 입력해주세요 (0으로 돌아가기) : ", end="")
                uid = UIUtils.get_input_int()
                
                if uid == 0:
                    exists = True
                else:
                    target = next((u for u in user_list if u['userid'] == uid), None)
                    if target:
                        exists = True
                        print(f"{target['username']} 사용자로 로그인했습니다.")
                        selected_user = target
                    else:
                        print("해당하는 ID의 사용자가 없습니다.")
        else:
            print("\n등록된 사용자가 없습니다.")
        return selected_user

    @staticmethod
    def login_with_name():
        selected_user = None
        print("\n검색할 사용자의 이름을 입력해주세요 : ", end="")
        name = sys.stdin.readline().strip()
        user_list = UserService.get_users(search_name=name)
        if len(user_list) == 0:
            print("해당 사용자를 찾을 수 없습니다.")
        else:
            UIUtils.print_user_list_only_id_and_name(user_list)
            if len(user_list) == 1:
                has_confirmed = False
                while not has_confirmed:
                    print("해당 사용자로 로그인하시겠습니까?(y/n) : ", end="")
                    confirm = sys.stdin.readline().strip()
                    if confirm.lower() == "y":
                        has_confirmed = True
                        selected_user = user_list[0]
                        print(f"{selected_user['username']} 사용자로 로그인했습니다.")
                    elif confirm.lower() == "n":
                        has_confirmed = True
                    else:
                        print("y 혹은 n을 입력해주세요.")
            else:
                exists = False
                while not exists:
                    print("로그인할 사용자의 ID를 입력해주세요 (0으로 돌아가기) : ", end="")
                    uid = UIUtils.get_input_int()
                    if uid == 0:
                        exists = True
                    
                    target = next((u for u in user_list if u['userid'] == uid), None)
                    if target:
                        exists = True
                        print(f"{target['username']} 사용자로 로그인에 성공했습니다.")
                        selected_user = target
                    else:
                        if uid != 0:
                            print("해당하는 ID의 사용자가 없습니다.")
        return selected_user

    @staticmethod
    def add_user():
        print("\n추가할 사용자의 이름을 입력해주세요 : ", end="")
        name = sys.stdin.readline().strip()
        if len(name) <= 45:
            user_list = UserService.get_users()
            unique_name = UserService.generate_unique_username(name, user_list)
            if UserService.add_new_user(unique_name):
                print(f"{unique_name} 사용자가 추가되었습니다.")
            else:
                print("사용자 추가에 실패했습니다.")
        else:
            print("사용자의 이름은 45글자 이하여야 합니다.")

    @staticmethod
    def run_login_process():
        """로그인 메뉴 실행 후 로그인 성공 시 user dict 반환"""
        quit_flag = False
        while not quit_flag:
            print("\n1. 로그인")
            print("2. 회원가입")
            print("0. 메뉴 선택으로 돌아가기\n")
            print("입력 : ", end="")
            
            choice = UIUtils.get_input_int()
            
            if choice == 1:
                l_quit = False
                while not l_quit:
                    print("\n1. 직접 선택")
                    print("2. 사용자 이름으로 검색")
                    print("0. 상위 메뉴로 돌아가기")
                    print("입력 : ", end="")
                    c2 = UIUtils.get_input_int()
                    selected = None
                    if c2 == 1:
                        selected = ClientAuthMenu.login_with_select()
                    elif c2 == 2:
                        selected = ClientAuthMenu.login_with_name()
                    elif c2 == 0:
                        l_quit = True
                    else:
                        print("0부터 2 사이의 수를 입력해주세요")
                    
                    if selected:
                        return selected # 로그인 성공 시 사용자 객체 리턴
            elif choice == 2:
                ClientAuthMenu.add_user()
            elif choice == 0:
                quit_flag = True
            else:
                print("0부터 2 사이의 수를 입력해주세요.\n")
        return None