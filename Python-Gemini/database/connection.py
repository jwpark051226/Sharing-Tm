import pymysql
from typing import Optional, List, Dict, Any

class DatabaseManager:
    """데이터베이스 연결 및 기본 쿼리 실행을 담당하는 클래스"""
    
    DB_CONFIG = {
        'host': 'localhost',
        'port': 3306,
        'user': 'root',
        'password': 'root',
        'db': 'sharingtmdb',
        'charset': 'utf8',
        'cursorclass': pymysql.cursors.DictCursor
    }

    @staticmethod
    def get_connection():
        return pymysql.connect(**DatabaseManager.DB_CONFIG)

    @staticmethod
    def test_connection() -> bool:
        try:
            conn = DatabaseManager.get_connection()
            conn.close()
            return True
        except Exception as e:
            print(f"MySQL 연결에 실패했습니다: {e}")
            return False

    @staticmethod
    def execute_non_query(query: str, params: Optional[tuple] = None) -> bool:
        try:
            with DatabaseManager.get_connection() as conn:
                with conn.cursor() as cursor:
                    cursor.execute(query, params)
                conn.commit()
                return True
        except Exception as e:
            print(f"오류가 발생했습니다.\n{e}")
            return False

    @staticmethod
    def execute_select(query: str, params: Optional[tuple] = None) -> List[Dict[str, Any]]:
        try:
            with DatabaseManager.get_connection() as conn:
                with conn.cursor() as cursor:
                    cursor.execute(query, params)
                    return cursor.fetchall()
        except Exception as e:
            print(f"오류가 발생했습니다.\n{e}")
            return []