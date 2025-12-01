import sys
from services.tm_service import TimeMachineService
from services.station_service import StationService
from utils.ui_utils import UIUtils

class TimeMachineManagementMenu:
    """타임머신 관리 메뉴 클래스"""

    @staticmethod
    def view_time_machines():
        tm_list = TimeMachineService.get_time_machines()
        if len(tm_list) > 0:
            UIUtils.print_time_machine_list(tm_list)
        else:
            print("등록된 타임머신이 없습니다.")

    @staticmethod
    def add_time_machine():
        selected_model = None
        s_list = StationService.get_stations()
        if len(s_list) > 0:
            has_model = False
            while not has_model:
                model_in = input("타임머신의 모델을 설정해주세요 (p(past)/f(future) 0:돌아가기) : ").strip()
                if model_in == "0":
                    return
                elif model_in.lower() == "p":
                    selected_model = "past"
                    has_model = True
                elif model_in.lower() == "f":
                    selected_model = "future"
                    has_model = True
                else:
                    print("p f 0 중 하나를 입력해주세요.")
            
            UIUtils.print_station_list(s_list)
            exists = False
            while not exists:
                print("타임머신을 보관할 대여소의 ID를 입력해주세요.(0으로 돌아가기) : ", end="")
                sid = UIUtils.get_input_int()
                
                if sid == 0:
                    exists = True
                elif any(s['rsid'] == sid for s in s_list):
                    exists = True
                    if TimeMachineService.add_time_machine(selected_model, sid):
                        print("타임머신이 추가되었습니다.")
                    else:
                        print("타임머신 추가에 실패했습니다.")
                else:
                    print("해당하는 ID의 대여소가 없습니다.")
        else:
            print("타임머신을 보관할 대여소가 없습니다. 우선 대여소를 추가해주세요.")

    @staticmethod
    def del_time_machine_with_select():
        tm_list = TimeMachineService.get_time_machines()
        if len(tm_list) > 0:
            UIUtils.print_time_machine_list(tm_list)
            exists = False
            while not exists:
                print("제거할 타임머신의 ID를 입력해주세요 (0으로 돌아가기) : ", end="")
                tid = UIUtils.get_input_int()
                
                if tid == 0:
                    exists = True
                else:
                    target = next((tm for tm in tm_list if tm['tmid'] == tid), None)
                    if target:
                        exists = True
                        if target['rsid'] is None or target['rsid'] == 0:
                            print("대여중인 타임머신은 삭제할 수 없습니다.")
                        else:
                            if TimeMachineService.delete_time_machine(tid):
                                print("타임머신을 성공적으로 제거했습니다.")
                            else:
                                print("타임머신을 제거할 수 없었습니다.")
                    else:
                        print("해당하는 ID의 타임머신이 없습니다.")
        else:
            print("등록된 타임머신이 없습니다.")

    @staticmethod
    def del_time_machine_with_station():
        s_list = StationService.get_stations()
        if len(s_list) > 0:
            UIUtils.print_station_list(s_list)
            exists = False
            while not exists:
                print("검색할 대여소의 ID를 입력해주세요.(0으로 돌아가기) : ", end="")
                sid = UIUtils.get_input_int()
                
                if sid == 0:
                    return
                elif any(s['rsid'] == sid for s in s_list):
                    exists = True
                    tm_list = TimeMachineService.get_time_machines(sid)
                    print(len(tm_list))
                    if len(tm_list) > 0:
                        UIUtils.print_time_machine_list(tm_list)
                        tm_exists = False
                        while not tm_exists:
                            print("제거할 타임머신의 ID를 입력해주세요 (0으로 돌아가기) : ", end="")
                            tid = UIUtils.get_input_int()
                            
                            if tid == 0:
                                tm_exists = True
                            elif any(tm['tmid'] == tid for tm in tm_list):
                                tm_exists = True
                                if TimeMachineService.delete_time_machine(tid):
                                    print("타임머신을 성공적으로 제거했습니다.")
                                else:
                                    print("타임머신을 제거할 수 없었습니다.")
                            else:
                                print("해당하는 ID의 타임머신이 없습니다.")
                    else:
                        print("해당 대여소에 보관되어 있는 타임머신이 없습니다.")
                else:
                    print("해당하는 ID의 대여소가 없습니다.")
        else:
            print("등록된 대여소가 없습니다.")

    @staticmethod
    def delete_menu():
        quit_flag = False
        while not quit_flag:
            print("\n1. 직접 선택")
            print("2. 보관되어 있는 대여소로 검색")
            print("0. 상위 메뉴로 돌아가기")
            print("입력 : ", end="")
            choice = UIUtils.get_input_int()
            
            if choice == 1:
                TimeMachineManagementMenu.del_time_machine_with_select()
            elif choice == 2:
                TimeMachineManagementMenu.del_time_machine_with_station()
            elif choice == 0:
                quit_flag = True
            else:
                print("0부터 2 사이의 수를 입력해주세요.")

    @staticmethod
    def show_menu():
        quit_flag = False
        while not quit_flag:
            print("\n1. 타임머신 조회")
            print("2. 타임머신 추가")
            print("3. 타임머신 삭제")
            print("0. 상위 메뉴로 돌아가기\n")
            print("입력 : ", end="")
            choice = UIUtils.get_input_int()
            
            if choice == 1:
                TimeMachineManagementMenu.view_time_machines()
            elif choice == 2:
                TimeMachineManagementMenu.add_time_machine()
            elif choice == 3:
                TimeMachineManagementMenu.delete_menu()
            elif choice == 0:
                quit_flag = True
            else:
                print("0부터 3 사이의 수를 입력해주세요.")