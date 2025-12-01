# 공유 타임머신 서비스 - Python 버전

Python으로 구현된 공유 타임머신 대여 서비스 관리 시스템입니다.

##  프로젝트 구조

```
Python-Gemini/
├── main.py                 # 프로그램 진입점
├── README.md               # 프로젝트 설명서
├── requirements.txt        # 의존성 패키지 목록
├── database/
│   ├── __init__.py
│   └── connection.py       # 데이터베이스 연결 관리 
├── models/
│   ├── __init__.py
│   ├── user.py             # 사용자 데이터 모델
│   ├── station.py          # 대여소 데이터 모델
│   ├── time_machine.py     # 타임머신 데이터 모델
│   ├── ticket.py           # 이용권 데이터 모델
│   └── rental.py           # 대여 이력 데이터 모델
├── services/
│   ├── __init__.py
│   ├── user_service.py     # 사용자 비즈니스 로직
│   ├── station_service.py  # 대여소 비즈니스 로직
│   ├── tm_service.py       # 타임머신 비즈니스 로직
│   └── rental_service.py   # 대여/반납 비지니스 로직
│   └── ticket_service.py   # 이용권 비지니스 로
├── utils/
│   ├── __init__.py
│   └── ui_utils.py         # UI 유틸리티 함수
└── menus/
    ├── __init__.py
    ├── admin_menu.py       # 관리자 메뉴 컨트롤러
    ├── client_menu.py      # 사용자 메뉴 컨트롤러
    ├── admin/              # [관리자] 하위 메뉴
    │   ├── __init__.py
    │   ├── user_menu.py	# 사용자 관리 메뉴
    │   ├── station_menu.py	# 대여소 관리 메뉴
    │   └── tm_menu.py	# 타임머신 관리 메뉴
    └── client/             # [사용자] 하위 메뉴
        ├── __init__.py
        ├── auth_menu.py	# 사용자 인증 메뉴
        ├── rent_menu.py	#타임머신 대여/반납 메뉴
        └── account_menu.py # 사용자 계정관리/결제 메뉴
```

##  설치 방법

### 1. 의존성 설치
```bash
pip install -r requirements.txt
```

### 2. 데이터베이스 설정
- **Host**: localhost
- **Port**: 3306
- **Username**: root
- **Password**: root
- **Database**: sharingtmdb

### 3. 프로그램 실행
```bash
python main.py
```

##  파일 설명

###  `main.py`
- **역할**: 애플리케이션의 진입점
- **기능**: 
  - 메인 메뉴 표시 (사용자 메뉴, 관리자 메뉴)
  - 데이터베이스 연결 테스트
  - 전체 프로그램 흐름 제어

### `database/connection.py`
```python
class DatabaseManager
```
- **역할**: 데이터베이스 연결 및 기본 쿼리 실행
- **주요 메서드**:
  - `get_connection()`: MySQL 연결 객체 반환
  - `test_connection()`: 데이터베이스 연결 테스트
  - `execute_non_query()`: INSERT, UPDATE, DELETE 쿼리 실행
  - `execute_select()`: SELECT 쿼리 실행

###  `models/user.py`
```python
@dataclass
class User
```
- **역할**: 사용자 데이터 구조 정의
- **속성**:
  - `userid`: 사용자 ID
  - `username`: 사용자 이름
  - `balance`: 소지금
  - `has_valid_ticket`: 유효한 이용권 보유 여부
  - `current_era`: 현재 소재 시간대

###  `models/station.py`
```python
@dataclass
class Station
```
- **역할**: 대여소 데이터 구조 정의
- **속성**:
  - `rsid`: 대여소 ID
  - `rs_name`: 대여소 이름
  - `rs_address`: 대여소 주소
  - `current_stock`: 현재 보관 중인 타임머신 수

###  `models/time_machine.py`
```python
@dataclass
class TimeMachine
```
- **역할**: 타임머신 데이터 구조 정의
- **속성**:
  - `tmid`: 타임머신 ID
  - `model`: 타임머신 모델
  - `current_era_point`: 현재 소재 시간대
  - `has_valid_ticket`: 유효한 이용권 보유 여부
  - `rsid`: 거치되어 있는 대여소 ID
  - `current_station_name`: 거치되어 있는 대여소 이름

###  `models/ticket.py`
```python
@dataclass
class Ticket
```
- **역할**: 이용권 데이터 구조 정의
- **속성**:
  - `ticketid`: 사용자 ID
  - `purchase_time`: 구매시간
  - `expire_time`: 만료시간
  - `status`: 상태
  - `userid`: 구매한 사용자의 ID
  - `username`: 구매한 사용자의 이름

###  `models/rental.py`
```python
@dataclass
class Rental
```
- **역할**: 대여 데이터 구조 정의
- **속성**:
  - `rentalid`: 대여 ID
  - `userid`: 대여한 사용자의 ID
  - `username`: 대여한 사용자의 이름
  - `tmid`: 대여한 타임머신의 ID
  - `depart_rs`: 대여한 대여소의 ID
  - `depart_station_name`: 대여한 대여소의 이름
  - `rental_time`: 대여 시간
  - `arrive_rs`: 반납한 대여소의 ID
  - `arrive_station_name`: 반납한 대여소의 이름 
  - `return_time`: 반납 시간
  - `destination_point`: 이동한 시간대

###  `services/user_service.py`
```python
class UserService
```
- **역할**: 사용자 관련 비즈니스 로직 처리
- **주요 메서드**:
  - `get_users()`: 사용자 목록 조회 (검색 기능 포함)
  - `add_new_user()`: 새 사용자 추가
  - `delete_user()`: 사용자 삭제
  - `generate_unique_username()`: 중복되지 않는 사용자명 생성
  - `charge_balance()`: 사용자 소지금 충전

###  `services/station_service.py`
```python
class StationService
```
- **역할**: 대여소 관련 비즈니스 로직 처리
- **주요 메서드**:
  - `get_stations()`: 대여소 목록 조회 (이름/주소 검색 가능)
  - `add_station()`: 새 대여소 추가
  - `delete_station()`: 대여소 삭제

###  `services/tm_service.py`
```python
class TimeMachineService
```
- **역할**: 대여소 관련 비즈니스 로직 처리
- **주요 메서드**:
  - `get_time_machines()`: 타임머신 목록 조회 (대여소 기준 검색 사능)
  - `add_time_machine()`: 새 타임머신 추가
  - `delete_time_machine()`: 타임머신 삭제

###  `services/ticket_service.py`
```python
class TicketService
```
- **역할**: 이용권 관련 비즈니스 로직 처리
- **주요 메서드**:
  - `purchase_ticket()`: 소지금 차감 및 신규 이용권 생성
  - `update_expired_tickets()`: 만료된 이용권 상태 일괄 업데이트
  - `get_all_tickets()`: 이용권 구매 이력 목록 조회
  - `get_tickets_by_user()`: 특정 사용자의 이용권 구매 이력 목록 조회

###  `services/rental_service.py`
```python
class RentalService
```
- **역할**: 이용권 관련 비즈니스 로직 처리
- **주요 메서드**:
  - `rent_time_machine()`: 대여 기록 생성 및 타임머신 상태 변경
  - `return_time_machine()`: 반납 시간/대여소 기록 및 타임머신 상태 변경
  - `get_all_rentals()`: 대여기록 목록 조회
  - `get_rental_history_by_user()`: 특정 사용자의 대여기록 목록 조회

###  `utils/ui_utils.py`
```python
class UIUtils
```
- **역할**: UI 출력 및 입력 처리 유틸리티
- **주요 메서드**:
  - `print_user_list()`: 사용자 목록 테이블 출력
  - `print_station_list()`: 대여소 목록 테이블 출력
  - `print_user_list_only_id_and_name()`: 사용자 목록 테이블 출력(ID와 이름만)
  - `print_time_machine_list()`: 타임머신 목록 테이블 출력
  - `print_rental_history()`: 대여기록 목록 테이블 출력
  - `print_ticket_list()`: 이용권 구매 이력 목록 테이블 출력
  - `get_input_int()`: 정수 입력 받기 (예외 처리 포함)
  - `get_yes_no_input()`: Y/N 입력 받기
  - `cut_string()`: 특정 길이 이상의 문자열 자르기

###  `menus/admin_menu.py`
```python
class AdminMenu
```
- **역할**: 관리자 메뉴 컨트롤
- **주요 기능**:
  - 관리자 메뉴 출력
  - 관리자 기능 흐름 제어
  - 이용권 구매 이력 목록 조회
  - 대여 기록 목록 조회

###  `menus/admin/user_menu.py`
```python
class UserManagementMenu
```
- **역할**: 관리자용 사용자 관리 메뉴 UI 및 로직
- **주요 기능**:
  - 사용자 조회, 추가, 삭제
  - 사용자 검색 (이름 기반)
  - 삭제 확인 프로세스

###  `menus/admin/station_menu.py`
```python
class StationManagementMenu
```
- **역할**: 관리자용 대여소 관리 메뉴 UI 및 로직
- **주요 기능**:
  - 대여소 조회, 추가, 삭제
  - 대여소 검색 (이름/주소 기반)
  - 삭제 확인 프로세스

###  `menus/admin/tm_menu.py`
```python
class TimeMachineManagementMenu
```
- **역할**: 관리자용 타임머신 관리 메뉴 UI 및 로직
- **주요 기능**:
  - 타임머신 조회, 추가, 삭제
  - 타임머신 검색 (대여소 기반)
  - 삭제 확인 프로세스

###  `menus/client_menu.py`
```python
class ClientMenu
```
- **역할**: 사용자 메뉴 컨트롤
- **주요 기능**:
  - 사용자 메뉴 출력
  - 사용자 기능 흐름 제어
  - 사용자 정보 새로고침

###  `menus/client/auth_menu.py`
```python
class ClientAuthMenu
```
- **역할**: 사용자용 사용자 인증 메뉴 UI 및 로직
- **주요 기능**:
  - 회원가입
  - 로그인 (이름 기반)

###  `menus/client/account_menu.py`
```python
class class ClientAccountMenu
```
- **역할**: 사용자용 계정 관리 메뉴 UI 및 로직
- **주요 기능**:
  - 소지금 충전
  - 이용권 구매
  - 이용권 구매 이력 조회
  - 타임머신 대여 내역 조회
  - 회원 탈퇴
  - 탈퇴 확인 프로세스

###  `menus/client/rent_menu.py`
```python
class ClientRentMenu
```
- **역할**: 사용자용 타임머신 대여/반납 메뉴 UI 및 로직
 **주요 기능**:
  - 타임머신 대여
  - 이동 시간대 확인
  - 타임머신 반납  

##  아키텍처

1. **Presentation Layer** (`menus/`): 사용자 인터페이스
2. **Business Logic Layer** (`services/`): 비즈니스 규칙 및 로직
3. **Data Access Layer** (`database/`): 데이터베이스 접근
4. **Domain Model Layer** (`models/`): 도메인 객체 정의
5. **Utility Layer** (`utils/`): 입출력 기능 보조
