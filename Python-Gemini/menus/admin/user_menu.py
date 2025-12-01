import sys
from services.user_service import UserService
from utils.ui_utils import UIUtils

class UserManagementMenu:
    """사용자 관리 메뉴 클래스"""

    @staticmethod
    def view_users():
        user_list = UserService.get_users()
        if len(user_list) > 0:
            UIUtils.print_user_list(user_list)
        else:
            print("등록된 사용자가 없습니다.")

    @staticmethod
    def add_user():
        print("\n추가할 사용자의 이름을 입력해주세요 : ", end="")
        new_user_name = sys.stdin.readline().strip()
        if len(new_user_name) <= 45:
            user_list = UserService.get_users()
            unique_name = UserService.generate_unique_username(new_user_name, user_list)
            
            if UserService.add_new_user(unique_name):
                print(f"{unique_name} 사용자가 추가되었습니다.")
            else:
                print("사용자 추가에 실패했습니다.")
        else:
            print("사용자의 이름은 45글자 이하여야 합니다.")

    @staticmethod
    def delete_user_by_selection():
        user_list = UserService.get_users()
        if len(user_list) > 0:
            UIUtils.print_user_list_only_id_and_name(user_list)
            selected_exists = False
            while not selected_exists:
                print("제거할 사용자의 ID를 입력해주세요 (0으로 돌아가기) : ", end="")
                del_id = UIUtils.get_input_int()
                
                if del_id == 0:
                    selected_exists = True
                else:
                    target = next((u for u in user_list if u['userid'] == del_id), None)
                    if target:
                        if target.get('is_renting', False):
                            print("타임머신을 대여중인 사용자는 삭제할 수 없습니다.")
                        else:
                            selected_exists = True
                            if UserService.delete_user(del_id):
                                print("사용자를 성공적으로 제거했습니다.")
                            else:
                                print("사용자를 제거할 수 없었습니다.")
                    else:
                        print("해당하는 ID의 사용자가 없습니다.")
        else:
            print("등록된 사용자가 없습니다.")

    @staticmethod
    def delete_user_by_name():
        print("검색할 사용자의 이름을 입력해주세요 : ", end="")
        name = sys.stdin.readline().strip()
        user_list = UserService.get_users(search_name=name)
        
        if len(user_list) == 0:
            print("해당 사용자를 찾을 수 없습니다.")
        else:
            UIUtils.print_user_list_only_id_and_name(user_list)
            if len(user_list) == 1:
                has_confirmed = False
                while not has_confirmed:
                    print("해당 사용자를 제거하시겠습니까?(y/n) : ", end="")
                    confirm = sys.stdin.readline().strip()
                    if confirm.lower() == "y":
                        has_confirmed = True
                        if user_list[0].get('is_renting', False):
                            print("타임머신을 대여중인 사용자는 삭제할 수 없습니다.")
                        else:
                            if UserService.delete_user(user_list[0]['userid']):
                                print("사용자를 성공적으로 제거했습니다.")
                            else:
                                print("사용자를 제거할 수 없었습니다.")
                    elif confirm.lower() == "n":
                        has_confirmed = True
                    else:
                        print("y 혹은 n을 입력해주세요.")
            else:
                exists = False
                while not exists:
                    print("제거할 사용자의 ID를 입력해주세요.(0으로 돌아가기) : ", end="")
                    uid = UIUtils.get_input_int()
                    if uid == 0:
                        exists = True
                    
                    target = next((u for u in user_list if u['userid'] == uid), None)
                    if target:
                        if target.get('is_renting', False):
                            print("타임머신을 대여중인 사용자는 삭제할 수 없습니다.")
                        else:
                            exists = True
                            if UserService.delete_user(uid):
                                print("사용자를 성공적으로 제거했습니다.")
                            else:
                                print("사용자를 제거할 수 없었습니다.")
                    else:
                        if uid != 0:
                            print("해당하는 ID의 사용자가 없습니다.")

    @staticmethod
    def delete_menu():
        del_quit = False
        while not del_quit:
            print("\n1. 직접 선택")
            print("2. 사용자 이름으로 검색")
            print("0. 상위 메뉴로 돌아가기")
            print("입력 : ", end="")
            
            choice = UIUtils.get_input_int()
            if choice == 1:
                UserManagementMenu.delete_user_by_selection()
            elif choice == 2:
                UserManagementMenu.delete_user_by_name()
            elif choice == 0:
                del_quit = True
            else:
                print("0부터 2 사이의 수를 입력해주세요.")

    @staticmethod
    def show_menu():
        quit_menu = False
        while not quit_menu:
            print("\n1. 사용자 조회")
            print("2. 사용자 추가")
            print("3. 사용자 삭제")
            print("0. 상위 메뉴로 돌아가기\n")
            print("입력 : ", end="")
            
            choice = UIUtils.get_input_int()
            if choice == 1:
                UserManagementMenu.view_users()
            elif choice == 2:
                UserManagementMenu.add_user()
            elif choice == 3:
                UserManagementMenu.delete_menu()
            elif choice == 0:
                quit_menu = True
            else:
                print("0부터 3 사이의 수를 입력해주세요.")