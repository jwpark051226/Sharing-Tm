import pymysql
from dataclasses import dataclass
from typing import List, Optional

# ==========================================
# 1. Model 클래스 (데이터 구조)
# ==========================================
@dataclass
class User:
    userid: int
    username: str
    balance: int
    has_ticket: bool

@dataclass
class Station:
    station_id: int
    station_name: str
    station_address: str
    parked_time_machine: int

# ==========================================
# 2. Database Manager (DB 연결 및 로직)
# ==========================================
class DatabaseManager:
    # DB 접속 정보 설정
    db_config = {
        'host': 'localhost',
        'port': 3306,
        'user': 'root',
        'password': '1111',
        'db': 'sharingtmdb',
        'charset': 'utf8',
        'cursorclass': pymysql.cursors.DictCursor # 데이터를 딕셔너리로 받기 위함
    }

    @staticmethod
    def get_connection():
        return pymysql.connect(**DatabaseManager.db_config)

    @staticmethod
    def test_connection() -> bool:
        try:
            conn = DatabaseManager.get_connection()
            conn.close()
            return True
        except Exception as e:
            print(f"MySQL 연결 실패: {e}")
            return False

    @staticmethod
    def execute_non_query(query: str, params=None) -> bool:
        try:
            with DatabaseManager.get_connection() as conn:
                with conn.cursor() as cursor:
                    cursor.execute(query, params)
                conn.commit()
                return True # 파이썬은 행 수 확인보다 에러 없음을 성공으로 간주하는게 일반적
        except Exception as e:
            print(f"오류 발생: {e}")
            return False

    @staticmethod
    def execute_select(query: str, mapper, params=None) -> list:
        result_list = []
        try:
            with DatabaseManager.get_connection() as conn:
                with conn.cursor() as cursor:
                    cursor.execute(query, params)
                    rows = cursor.fetchall()
                    for row in rows:
                        result_list.append(mapper(row))
        except Exception as e:
            print(f"오류 발생: {e}")
        return result_list

    # --- 사용자 관련 메서드 ---
    @staticmethod
    def get_users(search_name: str = None) -> List[User]:
        query = """
            SELECT u.userid, u.username, u.balance, IF(COUNT(t.ticketid) > 0, 1, 0) AS has_valid_ticket 
            FROM User AS u
            LEFT JOIN Ticket t ON u.userid = t.userid AND t.status = 'ACTIVE' AND t.expire_time > NOW()
        """
        if search_name:
            query += " WHERE u.username LIKE %s"
            params = (f"%{search_name}%",)
        else:
            params = None
        
        query += " GROUP BY u.userid, u.username, u.balance"

        def user_mapper(row):
            return User(
                userid=row['userid'],
                username=row['username'],
                balance=row['balance'],
                has_ticket=bool(row['has_valid_ticket'])
            )

        return DatabaseManager.execute_select(query, user_mapper, params)

    @staticmethod
    def add_new_user(username: str) -> bool:
        query = "INSERT INTO User(username) VALUES(%s)"
        return DatabaseManager.execute_non_query(query, (username,))

    @staticmethod
    def delete_user(userid: int) -> bool:
        query = "DELETE FROM User WHERE userid = %s"
        return DatabaseManager.execute_non_query(query, (userid,))

    # --- 대여소 관련 메서드 ---
    @staticmethod
    def get_stations(search_type: str = None, keyword: str = None) -> List[Station]:
        query = """
            SELECT rs.rsid, rs.rs_name, rs.rs_address, COUNT(tm.tmid) AS current_stock
            FROM RentalStation AS rs
            LEFT JOIN TimeMachine AS tm ON rs.rsid = tm.rsid
        """
        params = None
        if search_type == "name":
            query += " WHERE rs.rs_name LIKE %s"
            params = (f"%{keyword}%",)
        elif search_type == "address":
            query += " WHERE rs.rs_address LIKE %s"
            params = (f"%{keyword}%",)
        
        query += " GROUP BY rs.rsid, rs.rs_name, rs.rs_address"

        def station_mapper(row):
            return Station(
                station_id=row['rsid'],
                station_name=row['rs_name'],
                station_address=row['rs_address'],
                parked_time_machine=row['current_stock']
            )

        return DatabaseManager.execute_select(query, station_mapper, params)

    @staticmethod
    def add_station(name: str, address: str) -> bool:
        query = "INSERT INTO RentalStation(rs_name, rs_address) VALUES(%s, %s)"
        return DatabaseManager.execute_non_query(query, (name, address))

    @staticmethod
    def delete_station(station_id: int) -> bool:
        query = "DELETE FROM RentalStation WHERE rsid = %s"
        return DatabaseManager.execute_non_query(query, (station_id,))


# ==========================================
# 3. UI 및 메인 로직
# ==========================================

def print_user_list(user_list: List[User]):
    print("-" * 55)
    print(f" {'ID':<3}   {'이름':<7}        {'잔액':<10}      {'이용권'}")
    print("-" * 55)
    for user in user_list:
        ticket_str = "Y" if user.has_ticket else "N"
        print(f" {user.userid:<3}   {user.username:<7}    {user.balance:>10,}원     {ticket_str}")
    print("-" * 55 + "\n")

def print_station_list(station_list: List[Station]):
    print("\n" + "-" * 90)
    print(f" {'ID':<3}   {'대여소 이름':<12}           {'보관 타임머신':<11}   {'대여소 주소'}")
    print("-" * 90)
    for station in station_list:
        print(f" {station.station_id:<3}   {station.station_name:<12}    {str(station.parked_time_machine) + '대':<11}   {station.station_address}")
    print("-" * 90)

# --- 관리자: 사용자 관리 ---
def admin_user_manage():
    while True:
        print("\n1. 사용자 조회\n2. 사용자 추가\n3. 사용자 삭제\n0. 상위 메뉴로 돌아가기\n")
        try:
            choice = int(input("입력 : "))
        except ValueError:
            print("숫자를 입력해주세요.")
            continue

        if choice == 1:
            users = DatabaseManager.get_users()
            if users:
                print_user_list(users)
            else:
                print("등록된 사용자가 없습니다.")
        
        elif choice == 2:
            new_name = input("\n추가할 사용자의 이름을 입력해주세요 : ")
            if len(new_name) <= 45:
                # 중복 이름 처리 로직
                users = DatabaseManager.get_users()
                if any(u.username == new_name for u in users):
                    i = 1
                    while True:
                        temp_name = f"{new_name}({i})"
                        if not any(u.username == temp_name for u in users):
                            new_name = temp_name
                            break
                        i += 1
                
                if DatabaseManager.add_new_user(new_name):
                    print(f"{new_name} 사용자가 추가되었습니다.")
                else:
                    print("사용자 추가 실패.")
            else:
                print("이름은 45자 이하여야 합니다.")

        elif choice == 3:
            search_type = input("\n1. 직접 선택(ID) / 2. 이름 검색 : ")
            if search_type == "1":
                users = DatabaseManager.get_users()
                if users:
                    print_user_list(users)
                    try:
                        del_id = int(input("제거할 ID (0:취소): "))
                        if del_id != 0:
                            if DatabaseManager.delete_user(del_id):
                                print("삭제 성공")
                            else:
                                print("삭제 실패")
                    except ValueError:
                        print("숫자만 입력하세요.")
            elif search_type == "2":
                name = input("검색할 이름: ")
                users = DatabaseManager.get_users(search_name=name)
                if not users:
                    print("사용자를 찾을 수 없습니다.")
                else:
                    print_user_list(users)
                    if len(users) == 1:
                        confirm = input("제거하시겠습니까? (y/n): ")
                        if confirm.lower() == 'y':
                            DatabaseManager.delete_user(users[0].userid)
                            print("삭제 성공")
                    else:
                        print("여러 명이 검색되었습니다. ID로 삭제해주세요.")
        
        elif choice == 0:
            break

# --- 관리자: 대여소 관리 ---
def admin_station_manage():
    while True:
        print("\n1. 대여소 조회\n2. 대여소 추가\n3. 대여소 삭제\n0. 상위 메뉴로 돌아가기\n")
        try:
            choice = int(input("입력 : "))
        except ValueError: continue

        if choice == 1:
            stations = DatabaseManager.get_stations()
            if stations:
                print_station_list(stations)
            else:
                print("등록된 대여소가 없습니다.")
        
        elif choice == 2:
            name = input("\n대여소 이름: ")
            address = input("대여소 주소: ")
            if DatabaseManager.add_station(name, address):
                print("추가되었습니다.")
            else:
                print("추가 실패")
        
        elif choice == 3:
            # (간소화를 위해 ID 삭제 로직만 구현, 필요시 확장 가능)
            stations = DatabaseManager.get_stations()
            print_station_list(stations)
            try:
                del_id = int(input("제거할 대여소 ID (0:취소): "))
                if del_id != 0:
                    if DatabaseManager.delete_station(del_id):
                        print("삭제 성공")
                    else:
                        print("삭제 실패")
            except ValueError: pass

        elif choice == 0:
            break

def admin_menu():
    while True:
        print("\n1. 사용자 관리\n2. 대여소 관리\n0. 메인으로\n")
        try:
            choice = int(input("입력 : "))
        except ValueError: continue

        if choice == 1:
            admin_user_manage()
        elif choice == 2:
            admin_station_manage()
        elif choice == 0:
            break

def user_menu():
    while True:
        print("1. 로그인\n2. 회원가입\n0. 뒤로가기\n")
        if input("입력: ") == "0": break

# ==========================================
# 4. 메인 실행부
# ==========================================
def main():
    if DatabaseManager.test_connection():
        while True:
            print("-" * 30)
            print("공유 타임머신 서비스입니다.")
            print("-" * 30)
            print("1. 사용자 메뉴")
            print("2. 관리자 메뉴")
            print("0. 종료\n")
            
            try:
                choice = int(input("입력 : "))
            except ValueError:
                print("숫자를 입력해주세요.")
                continue

            if choice == 1:
                user_menu()
            elif choice == 2:
                admin_menu()
            elif choice == 0:
                print("프로그램을 종료합니다.")
                break
            else:
                print("0~2 사이의 수를 입력해주세요.")

if __name__ == "__main__":
    main()