from typing import List, Optional
from database.connection import DatabaseManager

class TimeMachineService:
    """타임머신 관련 비지니스 로직을 담당하는 서비스 클래스"""

    @staticmethod
    def get_time_machines(station_id: Optional[int] = None) -> List[dict]:
        query = """
            SELECT tm.tmid, tm.model, tm.current_era_point, tm.rsid, rs.rs_name 
            FROM TimeMachine AS tm
            LEFT JOIN RentalStation AS rs ON tm.rsid = rs.rsid
        """
        params = None
        if station_id:
            query += " WHERE tm.rsid = %s"
            params = (station_id,)
        
        return DatabaseManager.execute_select(query, params)

    @staticmethod
    def add_time_machine(model: str, station_id: int) -> bool:
        query = "INSERT INTO TimeMachine(model, rsid) VALUES(%s, %s)"
        return DatabaseManager.execute_non_query(query, (model, station_id))

    @staticmethod
    def delete_time_machine(tm_id: int) -> bool:
        query = "DELETE FROM TimeMachine WHERE tmid = %s"
        return DatabaseManager.execute_non_query(query, (tm_id,))