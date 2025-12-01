from dataclasses import dataclass

@dataclass
class Station:
    """대여소 정보를 담는 데이터 클래스"""
    rsid: int
    rs_name: str
    rs_address: str
    current_stock: int = 0