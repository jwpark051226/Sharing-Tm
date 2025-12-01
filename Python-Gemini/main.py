from database.connection import DatabaseManager
from menus.client_menu import ClientMenu
from menus.admin_menu import AdminMenu  
from utils.ui_utils import UIUtils

class MainProgram:
    """메인 프로그램 클래스"""

    @staticmethod
    def run():
        if DatabaseManager.test_connection():
            main_quit = False
            while not main_quit:
                print("\n----------------------------")
                print("공유 타임머신 서비스입니다.")
                print("----------------------------")
                print("1. 사용자 메뉴")
                print("2. 관리자 메뉴")
                print("0. 종료\n")
                print("입력 : ", end="")

                choice = UIUtils.get_input_int()

                if choice == 1:
                    ClientMenu.user_login_menu() # 사용자 모드 진입
                elif choice == 2:
                    AdminMenu.show_main_menu()   # 관리자 모드 진입
                elif choice == 0:
                    main_quit = True
                else:
                    print("0부터 2 사이의 수를 입력해주세요.\n")

if __name__ == "__main__":
    MainProgram.run()