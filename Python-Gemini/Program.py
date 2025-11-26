import pymysql
import sys

# ==========================================
# 1. Database Manager (로직 담당)
# ==========================================
class DatabaseManager:
    # DB 설정 (C# 코드의 설정과 동일하게 맞춤)
    DB_CONFIG = {
        'host': 'localhost',
        'port': 3306,
        'user': 'root',
        'password': '1111',
        'db': 'sharingtmdb',
        'charset': 'utf8',
        'cursorclass': pymysql.cursors.DictCursor
    }

    @staticmethod
    def get_connection():
        return pymysql.connect(**DatabaseManager.DB_CONFIG)

    @staticmethod
    def test_connection():
        try:
            conn = DatabaseManager.get_connection()
            conn.close()
            return True
        except:
            print("MySQL 연결에 실패했습니다.")
            return False

    @staticmethod
    def execute_non_query(query, params=None):
        try:
            with DatabaseManager.get_connection() as conn:
                with conn.cursor() as cursor:
                    cursor.execute(query, params)
                conn.commit()
                return True
        except Exception as e:
            # C#에서는 예외 메시지 출력 후 false 반환
            print(f"오류가 발생했습니다.\n{e}")
            return False

    @staticmethod
    def execute_select(query, params=None):
        try:
            with DatabaseManager.get_connection() as conn:
                with conn.cursor() as cursor:
                    cursor.execute(query, params)
                    return cursor.fetchall()
        except Exception as e:
            print(f"오류가 발생했습니다.\n{e}")
            return []

    # --- User Functions ---
    @staticmethod
    def get_users(search_name=None):
        query = """
            SELECT u.userid, u.username, u.balance, IF(COUNT(t.ticketid) > 0, 1, 0) AS has_valid_ticket 
            FROM User AS u
            LEFT JOIN Ticket t ON u.userid = t.userid AND t.status = 'ACTIVE' AND t.expire_time > NOW()
        """
        params = None
        if search_name:
            query += " WHERE u.username LIKE %s"
            params = (f"%{search_name}%",)
        
        query += " GROUP BY u.userid, u.username, u.balance"
        return DatabaseManager.execute_select(query, params)

    @staticmethod
    def add_new_user(username):
        query = "INSERT INTO User(username) VALUES(%s)"
        return DatabaseManager.execute_non_query(query, (username,))

    @staticmethod
    def delete_user(userid):
        query = "DELETE FROM User WHERE userid = %s"
        return DatabaseManager.execute_non_query(query, (userid,))

    # --- Station Functions ---
    @staticmethod
    def get_stations(search_type=None, keyword=None):
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
        return DatabaseManager.execute_select(query, params)

    @staticmethod
    def add_station(name, address):
        query = "INSERT INTO RentalStation(rs_name, rs_address) VALUES(%s, %s)"
        return DatabaseManager.execute_non_query(query, (name, address))

    @staticmethod
    def delete_station(station_id):
        query = "DELETE FROM RentalStation WHERE rsid = %s"
        return DatabaseManager.execute_non_query(query, (station_id,))


# ==========================================
# 2. Program (화면 출력 및 메뉴 로직)
# ==========================================

def print_user_list(user_list):
    print("-----------------------------------------------------")
    print(" ID    이름         잔액         이용권 유무")
    print("-----------------------------------------------------")
    for user in user_list:
        # C# format: "{0,-3}   {1,-7}   {2,10:N0}   {3}"
        # {2,10:N0} -> user.balance + "원"을 문자열로 붙인 뒤 10칸 우측 정렬
        balance_str = f"{user['balance']}원"
        ticket_yn = "Y" if user['has_valid_ticket'] else "N"
        
        # 파이썬 f-string으로 C#의 정렬 규격(-3, -7, 10)을 정확히 모사
        print(f" {user['userid']:<3}   {user['username']:<7}   {balance_str:>10}   {ticket_yn}")
    print("-----------------------------------------------------\n")

def print_station_list(station_list):
    print("\n------------------------------------------------------------------------------------------")
    print(" ID    대여소 이름           보관 타임머신  대여소 주소")
    print("------------------------------------------------------------------------------------------")
    for station in station_list:
        # C# format: "{0,-3}   {1,-12}   {2,-11}   {3}"
        tm_count_str = f"{station['current_stock']}대"
        print(f" {station['rsid']:<3}   {station['rs_name']:<12}   {tm_count_str:<11}   {station['rs_address']}")
    print("------------------------------------------------------------------------------------------")

def get_input_int():
    try:
        return int(input())
    except ValueError:
        return -1 # 에러 상황

# ---------------------------------------------------------
# Admin Logic
# ---------------------------------------------------------

def admin_view_user():
    user_list = DatabaseManager.get_users()
    if len(user_list) > 0:
        print_user_list(user_list)
    else:
        print("등록된 사용자가 없습니다.")

def admin_add_user():
    print("\n추가할 사용자의 이름을 입력해주세요 : ", end="")
    new_user_name = input()
    if len(new_user_name) <= 45:
        user_list = DatabaseManager.get_users()
        
        # 중복 이름 검사 및 (n) 붙이기 로직
        if any(u['username'] == new_user_name for u in user_list):
            i = 1
            flag = False
            while not flag:
                temp_name = f"{new_user_name}({i})"
                if any(u['username'] == temp_name for u in user_list):
                    i += 1
                else:
                    flag = True
                    new_user_name = temp_name
        
        if DatabaseManager.add_new_user(new_user_name):
            print(f"{new_user_name} 사용자가 추가되었습니다.")
        else:
            print("사용자 추가에 실패했습니다.")
    else:
        print("사용자의 이름은 45글자 이하여야 합니다.")

def del_user_with_select():
    user_list = DatabaseManager.get_users()
    if len(user_list) > 0:
        print_user_list(user_list)
        selected_exists = False
        while not selected_exists:
            print("제거할 유저의 ID를 입력해주세요 (0으로 돌아가기) : ", end="")
            del_id = int(input()) # C# int.Parse
            
            if del_id == 0:
                selected_exists = True
            elif any(u['userid'] == del_id for u in user_list):
                selected_exists = True
                if DatabaseManager.delete_user(del_id):
                    print("사용자를 성공적으로 제거했습니다.")
                else:
                    print("사용자를 제거할 수 없었습니다.")
            else:
                print("해당하는 ID의 사용자가 없습니다.")
    else:
        print("등록된 사용자가 없습니다.")

def del_user_with_name():
    print("검색할 사용자의 이름을 입력해주세요 : ", end="")
    name = input()
    user_list = DatabaseManager.get_users(search_name=name)
    
    if len(user_list) == 0:
        print("해당 사용자를 찾을 수 없습니다.")
    else:
        print_user_list(user_list)
        
        if len(user_list) == 1:
            has_confirmed = False
            while not has_confirmed:
                print("해당 유저를 제거하시겠습니까?(y/n) : ", end="")
                confirm = input()
                if confirm.lower() == "y":
                    has_confirmed = True
                    if DatabaseManager.delete_user(user_list[0]['userid']):
                        print("사용자를 성공적으로 제거했습니다.")
                    else:
                        print("사용자를 제거할 수 없었습니다.")
                elif confirm.lower() == "n": 
                    has_confirmed = True
                else:
                    print("y 혹은 n을 입력해주세요.")
        else:
            # 여러명일 때 ID로 선택 삭제
            selected_exists = False
            while not selected_exists:
                print("제거할 유저의 ID를 입력해주세요.(0으로 돌아가기) : ", end="")
                del_id = int(input())
                if del_id == 0:
                    selected_exists = True
                elif any(u['userid'] == del_id for u in user_list):
                    selected_exists = True
                    if DatabaseManager.delete_user(del_id):
                        print("사용자를 성공적으로 제거했습니다.")
                    else:
                        print("사용자를 제거할 수 없었습니다.")
                else:
                    print("해당하는 ID의 유저가 없습니다.")

def admin_del_user_menu():
    del_quit = False
    while not del_quit:
        print("\n1. 직접 선택")
        print("2. 사용자 이름으로 검색")
        print("0. 상위 메뉴로 돌아가기")
        print("입력 : ", end="")
        
        choice = int(input())
        if choice == 1:
            del_user_with_select()
        elif choice == 2:
            del_user_with_name()
        elif choice == 0:
            del_quit = True
        else:
            print("0부터 2 사이의 수를 입력해주세요.")

def admin_user_manage():
    quit_menu = False
    while not quit_menu:
        print("\n1. 사용자 조회")
        print("2. 사용자 추가")
        print("3. 사용자 삭제")
        print("0. 상위 메뉴로 돌아가기\n")
        print("입력 : ", end="")
        
        choice = int(input())
        if choice == 1:
            admin_view_user()
        elif choice == 2:
            admin_add_user()
        elif choice == 3:
            admin_del_user_menu()
        elif choice == 0:
            quit_menu = True
        else:
            print("0부터 2 사이의 수를 입력해주세요.")

# --- Station Logic ---

def admin_view_station():
    st_list = DatabaseManager.get_stations()
    if len(st_list) > 0:
        print_station_list(st_list)
    else:
        print("등록된 대여소가 없습니다.")

def admin_add_station():
    print("\n추가할 대여소의 이름을 입력해주세요 : ", end="")
    name = input()
    if len(name) <= 45:
        print("추가할 대여소의 주소를 입력해주세요 : ", end="")
        addr = input()
        if len(addr) <= 100:
            if DatabaseManager.add_station(name, addr):
                print("대여소가 추가되었습니다.")
            else:
                print("대여소를 추가할 수 없었습니다.")
        else:
            print("대여소의 주소은 100글자 이하여야 합니다.")
    else:
        print("대여소의 이름은 45자 이하여야 합니다.")

def del_station_with_select():
    st_list = DatabaseManager.get_stations()
    if len(st_list) > 0:
        print_station_list(st_list)
        selected_exists = False
        while not selected_exists:
            print("제거할 대여소의 ID를 입력해주세요.(0으로 돌아가기) : ", end="")
            del_id = int(input())
            if del_id == 0:
                selected_exists = True
            elif any(s['rsid'] == del_id for s in st_list):
                selected_exists = True
                if DatabaseManager.delete_station(del_id):
                    print("대여소를 성공적으로 제거했습니다.")
                else:
                    print("대여소를 제거할 수 없었습니다.")
            else:
                print("해당하는 ID의 대여소가 없습니다.")
    else:
        print("등록된 대여소가 없습니다.")

def del_station_with_name():
    print("검색할 대여소의 이름을 입력해주세요 : ", end="")
    name = input()
    st_list = DatabaseManager.get_stations(search_type="name", keyword=name)
    if len(st_list) == 0:
        print("해당 대여소를 찾을 수 없습니다.")
    else:
        print_station_list(st_list)
        # (C# 로직과 동일하게 1명일 때와 여러명일 때 분기)
        if len(st_list) == 1:
            has_confirmed = False
            while not has_confirmed:
                print("해당 대여소를 제거하시겠습니까?(y/n) : ", end="")
                confirm = input()
                if confirm.lower() == "y":
                    has_confirmed = True
                    if DatabaseManager.delete_station(st_list[0]['rsid']):
                        print("대여소를 성공적으로 제거했습니다.")
                    else:
                        print("대여소를 제거할 수 없었습니다.")
                elif confirm.lower() == "n":
                    has_confirmed = True
                else:
                    print("y 혹은 n을 입력해주세요.")
        else:
            selected_exists = False
            while not selected_exists:
                print("제거할 대여소의 ID를 입력해주세요.(0으로 돌아가기) : ", end="")
                del_id = int(input())
                if del_id == 0:
                    selected_exists = True
                elif any(s['rsid'] == del_id for s in st_list):
                    selected_exists = True
                    if DatabaseManager.delete_station(del_id):
                        print("대여소를 성공적으로 제거했습니다.")
                    else:
                        print("대여소를 제거할 수 없었습니다.")
                else:
                    print("해당하는 ID의 대여소가 없습니다.")

def del_station_with_address():
    print("검색할 대여소의 주소을 입력해주세요 : ", end="") 
    addr = input()
    st_list = DatabaseManager.get_stations(search_type="address", keyword=addr)
    if len(st_list) == 0:
        print("해당 대여소를 찾을 수 없습니다.")
    else:
        print_station_list(st_list)
        if len(st_list) == 1:
            has_confirmed = False
            while not has_confirmed:
                print("해당 대여소를 제거하시겠습니까?(y/n) : ", end="")
                confirm = input()
                if confirm.lower() == "y":
                    has_confirmed = True
                    if DatabaseManager.delete_station(st_list[0]['rsid']):
                        print("대여소를 성공적으로 제거했습니다.")
                    else:
                        print("대여소를 제거할 수 없었습니다.")
                elif confirm.lower() == "n":
                    has_confirmed = True
                else:
                    print("y 혹은 n을 입력해주세요.")
        else:
            selected_exists = False
            while not selected_exists:
                print("제거할 대여소의 ID를 입력해주세요.(0으로 돌아가기) : ", end="")
                del_id = int(input())
                if del_id == 0:
                    selected_exists = True
                elif any(s['rsid'] == del_id for s in st_list):
                    selected_exists = True
                    if DatabaseManager.delete_station(del_id):
                        print("대여소를 성공적으로 제거했습니다.")
                    else:
                        print("대여소를 제거할 수 없었습니다.")
                else:
                    print("해당하는 ID의 대여소가 없습니다.")

def admin_del_station_menu():
    quit_menu = False
    while not quit_menu:
        print("\n1. 직접 선택")
        print("2. 대여소 이름으로 검색")
        print("3. 대여소 주소로 검색")
        print("0. 상위 메뉴로 돌아가기\n")
        print("입력 : ", end="")
        
        choice = int(input())
        if choice == 1:
            del_station_with_select()
        elif choice == 2:
            del_station_with_name()
        elif choice == 3:
            del_station_with_address()
        elif choice == 0:
            quit_menu = True
        else:
            print("0부터 3 사이의 수를 입력해주세요.")

def admin_station_manage():
    quit_menu = False
    while not quit_menu:
        print("\n1. 대여소 조회")
        print("2. 대여소 추가")
        print("3. 대여소 삭제")
        print("0. 상위 메뉴로 돌아가기\n")
        print("입력 : ", end="")
        
        choice = int(input())
        if choice == 1:
            admin_view_station()
        elif choice == 2:
            admin_add_station()
        elif choice == 3:
            admin_del_station_menu()
        elif choice == 0:
            quit_menu = True
        else:
            print("0부터 3 사이의 수를 입력해주세요.")

def admin_menu():
    quit_menu = False
    while not quit_menu:
        print("\n1. 사용자 관리")
        print("2. 대여소 관리")
        print("3. 타임머신 관리")
        print("4. 대여목록 조회")
        print("5. 이용권 구매내역 조회")
        print("0. 메뉴 선택으로 돌아가기\n")
        print("입력 : ", end="")

        choice = int(input())
        if choice == 1:
            admin_user_manage()
        elif choice == 2:
            admin_station_manage()
        elif choice == 0:
            quit_menu = True
        else:
            print("0부터 5 사이의 수를 입력해주세요.")

# 미구현
def user_menu():
    user_quit = False
    while not user_quit:
        print("0. 메뉴 선택으로 돌아가기\n")
        try:
            inp = input() 
            if inp == "0":
                user_quit = True
        except: pass

# ==========================================
# 3. Main Entry
# ==========================================
def main():
    if DatabaseManager.test_connection():
        main_quit = False
        while not main_quit:
            print("----------------------------")
            print("공유 타임머신 서비스입니다.")
            print("----------------------------")
            print("1. 사용자 메뉴")
            print("2. 관리자 메뉴")
            print("0. 종료\n")
            print("입력 : ", end="")

            try:
                main_menu_choice = int(input())
            except ValueError:
                main_menu_choice = -1 

            if main_menu_choice == 1:
                user_menu()
            elif main_menu_choice == 2:
                admin_menu()
            elif main_menu_choice == 0:
                main_quit = True
            else:
                print("0부터 2 사이의 수를 입력해주세요.\n")

if __name__ == "__main__":
    main()
