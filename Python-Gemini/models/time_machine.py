from dataclasses import dataclass
from typing import Optional

@dataclass
class TimeMachine:
    """타임머신 정보를 담는 데이터 클래스"""
    tmid: int
    model: str
    current_era_point: str
    rsid: Optional[int]
    current_station_name: Optional[str]