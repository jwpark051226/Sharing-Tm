import sys
import datetime
from services.station_service import StationService
from services.tm_service import TimeMachineService
from services.rental_service import RentalService
from utils.ui_utils import UIUtils

class ClientRentMenu:
    """타임머신 대여/반납 메뉴 클래스"""

    # --- Helper Functions (Internal) ---
    @staticmethod
    def _rent_select_destination(model):
        while True:
            print("\n시간대 입력 예) 2025-05-21, BC 0333-10-03)")
            # input 사용 (버퍼 문제 해결됨)
            input_str = input("이동할 시간대를 입력해주세요.(0으로 돌아가기)  : ").strip()

            if input_str == "0":
                return None
            
            try:
                target_date = datetime.datetime.strptime(input_str, "%Y-%m-%d")
                if model == "past" and target_date >= datetime.datetime.now():
                    print("과거 모델입니다. 현재보다 이전의 시간대를 입력해주세요.")
                    continue
                if model == "future" and target_date <= datetime.datetime.now():
                    print("미래 모델입니다. 현재보다 미래의 시간대를 입력해주세요.")
                    continue
                return input_str
            except ValueError:
                pass
            
            if input_str.upper().startswith("BC"):
                if model == "future":
                    print("미래 모델입니다. 현재보다 미래의 시간대를 입력해주세요.")
                    continue
                date_part = input_str[2:].strip()
                try:
                    datetime.datetime.strptime(date_part, "%Y-%m-%d")
                    return "BC " + date_part
                except ValueError:
                    pass
            
            print("시간 형식이 잘못됐습니다.")

    @staticmethod
    def _rent_select_time_machine(station):
        result = None
        tm_list = TimeMachineService.get_time_machines(station['rsid'])
        if len(tm_list) > 0:
            UIUtils.print_time_machine_list(tm_list)
            selected = False
            while not selected:
                print("타임머신 모델 past : 과거로만 이동 가능.")
                print("타임머신 모델 future : 미래로만 이동 가능.")
                print("타임머신의 ID를 입력해주세요.(0으로 돌아가기) : ", end="")
                tm_id = UIUtils.get_input_int()

                if tm_id == 0:
                    selected = True
                else:
                    target = next((tm for tm in tm_list if tm['tmid'] == tm_id), None)
                    if target:
                        selected = True
                        result = target
                    else:
                        print("해당하는 ID의 타임머신이 없습니다.")
        else:
            print("주차된 타임머신이 없습니다.")
        return result

    @staticmethod
    def _rent_select_station_menu():
        # 내부 함수들: station select by id/name/address
        def select_by_id():
            st_list = StationService.get_stations()
            if not st_list: 
                print("등록된 대여소가 없습니다.")
                return None
            UIUtils.print_station_list(st_list)
            while True:
                print("대여소의 ID를 입력해주세요.(0으로 돌아가기) : ", end="")
                sid = UIUtils.get_input_int()
                if sid == 0: return None
                target = next((s for s in st_list if s['rsid'] == sid), None)
                if target: return target
                print("해당하는 ID의 대여소가 없습니다.")

        def select_by_keyword(k_type):
            print(f"검색할 대여소의 {'이름' if k_type=='name' else '주소'}을 입력해주세요 : ", end="")
            keyword = sys.stdin.readline().strip()
            st_list = StationService.get_stations(search_type=k_type, keyword=keyword)
            if not st_list:
                print("해당 대여소를 찾을 수 없습니다.")
                return None
            UIUtils.print_station_list(st_list)
            
            if len(st_list) == 1:
                while True:
                    print("해당 대여소를 선택하시겠습니까?(y/n) : ", end="")
                    ans = sys.stdin.readline().strip().lower()
                    if ans == 'y': return st_list[0]
                    elif ans == 'n': return None
                    print("y 혹은 n을 입력해주세요.")
            else:
                while True:
                    print("대여소의 ID를 입력해주세요.(0으로 돌아가기) : ", end="")
                    sid = UIUtils.get_input_int()
                    if sid == 0: return None
                    target = next((s for s in st_list if s['rsid'] == sid), None)
                    if target: return target
                    print("해당하는 ID의 대여소가 없습니다.")

        while True:
            print("\n1. 대여소 직접 선택")
            print("2. 대여소 이름으로 선택")
            print("3. 대여소 주소로 선택")
            print("0. 상위 메뉴로 돌아가기")
            print("입력 : ", end="")
            choice = UIUtils.get_input_int()

            if choice == 1: return select_by_id()
            elif choice == 2: return select_by_keyword("name")
            elif choice == 3: return select_by_keyword("address")
            elif choice == 0: return None
            else: print("0부터 3 사이의 수를 입력해주세요.")

    # --- Public Actions ---
    @staticmethod
    def rent_process(user):
        station = ClientRentMenu._rent_select_station_menu()
        if station is None: return False
        
        tm = ClientRentMenu._rent_select_time_machine(station)
        if tm is None: return False
        
        dest = ClientRentMenu._rent_select_destination(tm['model'])
        if dest is None: return False

        if RentalService.rent_time_machine(user['userid'], tm['tmid'], dest):
            print("타임머신을 대여했습니다.")
            return True
        else:
            print("타임머신 대여에 실패했습니다.")
            return False

    @staticmethod
    def return_process(user):
        result = False
        return_station = None
        station_list = StationService.get_stations()
        if len(station_list) > 0:
            UIUtils.print_station_list(station_list)
            selected = False
            while not selected:
                print("대여소의 ID를 입력해주세요.(0으로 돌아가기) : ", end="")
                sid = UIUtils.get_input_int()
                if sid == 0:
                    return result
                else:
                    target = next((s for s in station_list if s['rsid'] == sid), None)
                    if target:
                        selected = True
                        return_station = target
                    else:
                        print("해당하는 ID의 대여소가 없습니다.")
            
            if RentalService.return_time_machine(user['userid'], return_station['rsid']):
                print("타임머신을 반납했습니다.")
                result = True
            else:
                print("타임머신 반납에 실패했습니다.")
        else:
            print("등록된 대여소가 없습니다.")
        return result