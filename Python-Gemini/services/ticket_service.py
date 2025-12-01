from database.connection import DatabaseManager

class TicketService:
    """이용권 관련 비지니스 로직을 담당하는 서비스 클래스"""

    @staticmethod
    def get_tickets_by_user(userid: int):
        query = """
            SELECT t.ticketid, t.purchase_time, t.expire_time, t.status, t.userid, u.username 
            FROM Ticket AS t
            JOIN User AS u ON t.userid = u.userid
            WHERE u.userid = %s
            ORDER BY t.ticketid DESC
        """
        return DatabaseManager.execute_select(query, (userid,))

    @staticmethod
    def get_all_tickets():
        query = """
             SELECT t.ticketid, t.purchase_time, t.expire_time, t.status, t.userid, u.username 
             FROM Ticket AS t
             JOIN User AS u ON t.userid = u.userid
             ORDER BY t.ticketid DESC
        """
        return DatabaseManager.execute_select(query)

    @staticmethod
    def update_expired_tickets():
        query = "UPDATE Ticket SET status = 'INACTIVE' WHERE status = 'ACTIVE' AND expire_time < NOW()"
        DatabaseManager.execute_non_query(query)

    @staticmethod
    def purchase_ticket(userid, duration, price):
        """이용권 구매 트랜잭션 (티켓 생성 + 잔액 차감)"""
        try:
            conn = DatabaseManager.get_connection()
            conn.autocommit(False)
            with conn.cursor() as cursor:
                # 1. 티켓 생성
                cursor.execute("INSERT INTO Ticket(userid, purchase_time, expire_time) VALUES(%s, NOW(), DATE_ADD(NOW(), INTERVAL %s DAY))", (userid, duration))
                # 2. 잔액 차감
                cursor.execute("UPDATE User SET balance = balance - %s WHERE userid = %s", (price, userid))
            conn.commit()
            return True
        except:
            if conn: conn.rollback()
            return False