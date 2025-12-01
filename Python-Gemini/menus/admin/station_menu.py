import sys
from services.station_service import StationService
from utils.ui_utils import UIUtils

class StationManagementMenu:
    """대여소 관리 메뉴 클래스"""

    @staticmethod
    def view_stations():
        st_list = StationService.get_stations()
        if len(st_list) > 0:
            UIUtils.print_station_list(st_list)
        else:
            print("등록된 대여소가 없습니다.")

    @staticmethod
    def add_station():
        print("\n추가할 대여소의 이름을 입력해주세요 : ", end="")
        name = sys.stdin.readline().strip()
        if len(name) <= 45:
            print("추가할 대여소의 주소를 입력해주세요 : ", end="")
            addr = sys.stdin.readline().strip()
            if len(addr) <= 100:
                if StationService.add_station(name, addr):
                    print("대여소가 추가되었습니다.")
                else:
                    print("대여소를 추가할 수 없었습니다.")
            else:
                print("대여소의 주소은 100글자 이하여야 합니다.")
        else:
            print("대여소의 이름은 45자 이하여야 합니다.")

    @staticmethod
    def delete_station_by_selection():
        st_list = StationService.get_stations()
        if len(st_list) > 0:
            UIUtils.print_station_list(st_list)
            selected_exists = False
            while not selected_exists:
                print("제거할 대여소의 ID를 입력해주세요.(0으로 돌아가기) : ", end="")
                del_id = UIUtils.get_input_int()
                
                if del_id == 0:
                    selected_exists = True
                else:
                    target = next((s for s in st_list if s['rsid'] == del_id), None)
                    if target:
                        if target['current_stock'] > 0:
                            print("타임머신이 주차되어 있는 대여소는 삭제할 수 없습니다.")
                        else:
                            selected_exists = True
                            if StationService.delete_station(del_id):
                                print("대여소를 성공적으로 제거했습니다.")
                            else:
                                print("대여소를 제거할 수 없었습니다.")
                    else:
                        print("해당하는 ID의 대여소가 없습니다.")
        else:
            print("등록된 대여소가 없습니다.")

    @staticmethod
    def delete_station_by_name():
        print("검색할 대여소의 이름을 입력해주세요 : ", end="")
        name = sys.stdin.readline().strip()
        st_list = StationService.get_stations(search_type="name", keyword=name)
        
        if len(st_list) == 0:
            print("해당 대여소를 찾을 수 없습니다.")
        else:
            UIUtils.print_station_list(st_list)
            if len(st_list) == 1:
                has_confirmed = False
                while not has_confirmed:
                    print("해당 대여소를 제거하시겠습니까?(y/n) : ", end="")
                    confirm = sys.stdin.readline().strip()
                    if confirm.lower() == "y":
                        has_confirmed = True
                        if st_list[0]['current_stock'] > 0:
                            print("타임머신이 주차되어 있는 대여소는 삭제할 수 없습니다.")
                        else:
                            if StationService.delete_station(st_list[0]['rsid']):
                                print("대여소를 성공적으로 제거했습니다.")
                            else:
                                print("대여소를 제거할 수 없었습니다.")
                    elif confirm.lower() == "n":
                        has_confirmed = True
                    else:
                        print("y 혹은 n을 입력해주세요.")
            else:
                exists = False
                while not exists:
                    print("제거할 대여소의 ID를 입력해주세요.(0으로 돌아가기) : ", end="")
                    sid = UIUtils.get_input_int()
                    if sid == 0:
                        exists = True
                    else:
                        target = next((s for s in st_list if s['rsid'] == sid), None)
                        if target:
                            if target['current_stock'] > 0:
                                print("타임머신이 주차되어 있는 대여소는 삭제할 수 없습니다.")
                            else:
                                exists = True
                                if StationService.delete_station(sid):
                                    print("대여소를 성공적으로 제거했습니다.")
                                else:
                                    print("대여소를 제거할 수 없었습니다.")
                        else:
                            print("해당하는 ID의 대여소가 없습니다.")

    @staticmethod
    def delete_station_by_address():
        print("검색할 대여소의 주소를 입력해주세요 : ", end="")
        addr = sys.stdin.readline().strip()
        st_list = StationService.get_stations(search_type="address", keyword=addr)
        
        if len(st_list) == 0:
            print("해당 대여소를 찾을 수 없습니다.")
        else:
            UIUtils.print_station_list(st_list)
            # 로직은 이름 검색과 동일하므로 중복 최소화를 위해 복사하여 사용하거나 
            # 위 로직과 동일하게 단일/다중 처리
            if len(st_list) == 1:
                has_confirmed = False
                while not has_confirmed:
                    print("해당 대여소를 제거하시겠습니까?(y/n) : ", end="")
                    confirm = sys.stdin.readline().strip()
                    if confirm.lower() == "y":
                        has_confirmed = True
                        if st_list[0]['current_stock'] > 0:
                            print("타임머신이 주차되어 있는 대여소는 삭제할 수 없습니다.")
                        else:
                            if StationService.delete_station(st_list[0]['rsid']):
                                print("대여소를 성공적으로 제거했습니다.")
                            else:
                                print("대여소를 제거할 수 없었습니다.")
                    elif confirm.lower() == "n":
                        has_confirmed = True
                    else:
                        print("y 혹은 n을 입력해주세요.")
            else:
                exists = False
                while not exists:
                    print("제거할 대여소의 ID를 입력해주세요.(0으로 돌아가기) : ", end="")
                    sid = UIUtils.get_input_int()
                    if sid == 0:
                        exists = True
                    else:
                        target = next((s for s in st_list if s['rsid'] == sid), None)
                        if target:
                            if target['current_stock'] > 0:
                                print("타임머신이 주차되어 있는 대여소는 삭제할 수 없습니다.")
                            else:
                                exists = True
                                if StationService.delete_station(sid):
                                    print("대여소를 성공적으로 제거했습니다.")
                                else:
                                    print("대여소를 제거할 수 없었습니다.")
                        else:
                            print("해당하는 ID의 대여소가 없습니다.")

    @staticmethod
    def delete_menu():
        quit_menu = False
        while not quit_menu:
            print("\n1. 직접 선택")
            print("2. 대여소 이름으로 검색")
            print("3. 대여소 주소로 검색")
            print("0. 상위 메뉴로 돌아가기")
            print("입력 : ", end="")
            
            choice = UIUtils.get_input_int()
            if choice == 1:
                StationManagementMenu.delete_station_by_selection()
            elif choice == 2:
                StationManagementMenu.delete_station_by_name()
            elif choice == 3:
                StationManagementMenu.delete_station_by_address()
            elif choice == 0:
                quit_menu = True
            else:
                print("0부터 3 사이의 수를 입력해주세요.")

    @staticmethod
    def show_menu():
        quit_menu = False
        while not quit_menu:
            print("\n1. 대여소 조회")
            print("2. 대여소 추가")
            print("3. 대여소 삭제")
            print("0. 상위 메뉴로 돌아가기\n")
            print("입력 : ", end="")
            
            choice = UIUtils.get_input_int()
            if choice == 1:
                StationManagementMenu.view_stations()
            elif choice == 2:
                StationManagementMenu.add_station()
            elif choice == 3:
                StationManagementMenu.delete_menu()
            elif choice == 0:
                quit_menu = True
            else:
                print("0부터 3 사이의 수를 입력해주세요.")