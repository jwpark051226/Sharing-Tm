from database.connection import DatabaseManager

class RentalService:
    """대여 관련 비지니스 로직을 담당하는 서비스 클래스"""

    @staticmethod
    def get_rental_history_by_user(userid: int):
        query = """
            SELECT 
                r.rentalid, r.userid, u.username, r.tmid, r.depart_rs, 
                StartS.rs_name AS depart_name, r.rental_time, 
                r.arrive_rs, EndS.rs_name AS arrive_name, 
                r.return_time, r.destination_point
            FROM Rental r
            JOIN User u ON r.userid = u.userid
            LEFT JOIN RentalStation StartS ON r.depart_rs = StartS.rsid
            LEFT JOIN RentalStation EndS ON r.arrive_rs = EndS.rsid
            WHERE u.userid = %s
            ORDER BY r.rentalid DESC
        """
        return DatabaseManager.execute_select(query, (userid,))

    @staticmethod
    def get_all_rentals():
        query = """
            SELECT 
                r.rentalid, r.userid, u.username, r.tmid, r.depart_rs, 
                StartS.rs_name AS depart_name, r.rental_time, 
                r.arrive_rs, EndS.rs_name AS arrive_name, 
                r.return_time, r.destination_point
            FROM Rental r
            JOIN User u ON r.userid = u.userid
            LEFT JOIN RentalStation StartS ON r.depart_rs = StartS.rsid
            LEFT JOIN RentalStation EndS ON r.arrive_rs = EndS.rsid
            ORDER BY r.rentalid DESC
        """
        return DatabaseManager.execute_select(query)

    @staticmethod
    def rent_time_machine(userid, tm_id, destination):
        """대여 트랜잭션"""
        try:
            conn = DatabaseManager.get_connection()
            conn.autocommit(False)
            with conn.cursor() as cursor:
                cursor.execute("SELECT rsid FROM TimeMachine WHERE tmid = %s", (tm_id,))
                row = cursor.fetchone()
                start_station = row['rsid']
                cursor.execute("INSERT INTO Rental (userid, tmid, depart_rs, rental_time, destination_point) VALUES (%s, %s, %s, NOW(), %s)", (userid, tm_id, start_station, destination))
                cursor.execute("UPDATE TimeMachine SET rsid = NULL, current_era_point = %s WHERE tmid = %s", (destination, tm_id))
            conn.commit()
            return True
        except:
            if conn: conn.rollback()
            return False

    @staticmethod
    def return_time_machine(userid, station_id):
        """반납 트랜잭션"""
        try:
            conn = DatabaseManager.get_connection()
            conn.autocommit(False)
            with conn.cursor() as cursor:
                cursor.execute("SELECT rentalid, tmid FROM Rental WHERE userid = %s AND return_time IS NULL LIMIT 1", (userid,))
                row = cursor.fetchone()
                if not row:
                    conn.rollback()
                    return False
                rid, tmid = row['rentalid'], row['tmid']
                cursor.execute("UPDATE Rental SET return_time = NOW(), arrive_rs = %s WHERE rentalid = %s", (station_id, rid))
                cursor.execute("UPDATE TimeMachine SET rsid = %s, current_era_point = 'CurrentTime' WHERE tmid = %s", (station_id, tmid))
            conn.commit()
            return True
        except:
            if conn: conn.rollback()
            return False