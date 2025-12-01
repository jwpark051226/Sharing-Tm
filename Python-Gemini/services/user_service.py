from typing import List, Optional
from database.connection import DatabaseManager

class UserService:
    """사용자 관련 비즈니스 로직을 담당하는 서비스 클래스"""

    @staticmethod
    def get_users(search_name: Optional[str] = None) -> List[dict]:
        query = """
            SELECT 
                u.userid, 
                u.username, 
                u.balance, 
                IF(COUNT(DISTINCT t.ticketid) > 0, 1, 0) AS has_valid_ticket,
                IF(COUNT(DISTINCT r.rentalid) > 0, 1, 0) AS is_renting,
                IFNULL(MAX(r.destination_point), 'CurrentTime') AS current_era
            FROM User AS u
            LEFT JOIN Ticket t 
                ON u.userid = t.userid 
                AND t.status = 'ACTIVE' 
                AND t.expire_time > NOW()
            LEFT JOIN Rental r 
                ON u.userid = r.userid 
                AND r.return_time IS NULL
        """
        params = None
        if search_name:
            query += " WHERE u.username LIKE %s"
            params = (f"%{search_name}%",)
        
        query += " GROUP BY u.userid, u.username, u.balance"
        return DatabaseManager.execute_select(query, params)

    @staticmethod
    def add_new_user(username: str) -> bool:
        query = "INSERT INTO User(username) VALUES(%s)"
        return DatabaseManager.execute_non_query(query, (username,))

    @staticmethod
    def delete_user(userid: int) -> bool:
        query = "DELETE FROM User WHERE userid = %s"
        return DatabaseManager.execute_non_query(query, (userid,))

    @staticmethod
    def generate_unique_username(base_name: str, existing_users: List[dict]) -> str:
        if not any(u['username'] == base_name for u in existing_users):
            return base_name
        
        i = 1
        while True:
            temp_name = f"{base_name}({i})"
            if not any(u['username'] == temp_name for u in existing_users):
                return temp_name
            i += 1

    @staticmethod
    def charge_balance(userid: int, amount: int) -> bool:
        """사용자 잔액 충전"""
        query = "UPDATE User SET balance = balance + %s WHERE userid = %s"
        return DatabaseManager.execute_non_query(query, (amount, userid))