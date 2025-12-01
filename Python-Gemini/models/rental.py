from dataclasses import dataclass
from datetime import datetime
from typing import Optional

@dataclass
class Rental:
    """대여 정보를 담는 데이터 클래스"""
    rentalid: int
    userid: int
    username: str
    tmid: int
    depart_rs: int
    depart_station_name: str
    rental_time: datetime
    arrive_rs: int
    arrive_station_name: str
    return_time: Optional[datetime]
    destination_point: str