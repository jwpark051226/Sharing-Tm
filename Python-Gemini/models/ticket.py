from dataclasses import dataclass
from datetime import datetime

@dataclass
class Ticket:
    """이용권 정보를 담는 데이터 클래스"""
    ticketid: int
    purchase_time: datetime
    expire_time: datetime
    status: str
    userid: int
    username: str # 리스트 출력 시 JOIN된 사용자 이름을 담기 위해 포함