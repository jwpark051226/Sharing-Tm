from typing import List, Optional
from database.connection import DatabaseManager

class StationService:
    """대여소 관련 비즈니스 로직을 담당하는 서비스 클래스"""

    @staticmethod
    def get_stations(search_type: Optional[str] = None, keyword: Optional[str] = None) -> List[dict]:
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
    def add_station(name: str, address: str) -> bool:
        query = "INSERT INTO RentalStation(rs_name, rs_address) VALUES(%s, %s)"
        return DatabaseManager.execute_non_query(query, (name, address))

    @staticmethod
    def delete_station(station_id: int) -> bool:
        query = "DELETE FROM RentalStation WHERE rsid = %s"
        return DatabaseManager.execute_non_query(query, (station_id,))