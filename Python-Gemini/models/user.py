from dataclasses import dataclass

@dataclass
class User:
    """사용자 정보를 담는 데이터 클래스"""
    userid: int
    username: str
    balance: int
    has_valid_ticket: bool = False
    is_renting: bool = False        
    current_era: str = "CurrentTime" 