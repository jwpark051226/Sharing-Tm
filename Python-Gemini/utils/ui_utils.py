import sys
import datetime
from typing import List

class UIUtils:
    """UI 출력 관련 유틸리티 함수들"""

    @staticmethod
    def get_input_int() -> int:
        try:
            return int(input())
        except ValueError:
            return -1

    @staticmethod
    def cut_string(text: str, max_length: int) -> str:
        """문자열 자르기 헬퍼"""
        if not text:
            return "-"
        if len(text) > max_length:
            return text[:max_length-2] + ".."
        return text

    @staticmethod
    def print_user_list(user_list: List[dict]) -> None:
        print("----------------------------------------------------------------------")
        print(" ID    이름          잔액              이용권    대여 상태")
        print("----------------------------------------------------------------------")
        for user in user_list:
            balance_display = f"{user['balance']:,}원"
            has_ticket = "Y" if user['has_valid_ticket'] else "N"
            renting = "대여중" if user.get('is_renting', False) else "N"
            print(f" {user['userid']:<4}  {user['username']:<10}  {balance_display:>14}   {has_ticket:<6}   {renting}")
        print("----------------------------------------------------------------------\n")

    @staticmethod
    def print_user_list_only_id_and_name(user_list: List[dict]) -> None:
        print("-------------------------")
        print(" ID    이름            ")
        print("-------------------------")
        for user in user_list:
            print(f" {user['userid']:<3}   {user['username']}") 
        print("-------------------------\n")

    @staticmethod
    def print_station_list(station_list: List[dict]) -> None:
        print("\n------------------------------------------------------------------------------------------")
        print(" ID    대여소 이름        보관 타임머신  대여소 주소")
        print("------------------------------------------------------------------------------------------")
        for station in station_list:
            # station_service 쿼리에서 current_stock으로 가져옴
            stock = f"{station['current_stock']}대"
            print(f" {station['rsid']:<3}   {station['rs_name']:<12}   {stock:<11}   {station['rs_address']}")
        print("------------------------------------------------------------------------------------------")

    @staticmethod
    def print_time_machine_list(tm_list: List[dict]) -> None:
        print("\n----------------------------------------------------------------------")
        print(" ID     모델         현재 시간대             현재 대여소")
        print("----------------------------------------------------------------------")
        for tm in tm_list:
            display_time = tm['current_era_point']
            if display_time == "CurrentTime":
                # C# hh 포맷(12시간) 매칭 -> %I 사용
                display_time = datetime.datetime.now().strftime("%Y-%m-%d %I:%M")
            
            # None 처리
            st_name = tm['rs_name'] if tm['rs_name'] else "(대여중)"
            
            print(f" {tm['tmid']:<4}   {tm['model']:<10}   {display_time:<20}   {st_name}")
        print("----------------------------------------------------------------------")

    @staticmethod
    def print_rental_history(rental_list: List[dict]) -> None:
        print("\n-----------------------------------------------------------------------------------------------------------------------")
        print(" ID     사용자 이름    출발 대여소        대여 일시            도착 대여소        반납 일시            이동 시간대")
        print("-----------------------------------------------------------------------------------------------------------------------")
        for rental in rental_list:
            r_time = rental['rental_time'].strftime("%Y/%m/%d %H:%M")
            rt_time = "-"
            if rental['return_time']: # None이 아니면
                 rt_time = rental['return_time'].strftime("%Y/%m/%d %H:%M")
            
            arr_station = rental['arrive_name'] if rental['arrive_name'] else "-"
            # cut_string 활용
            u_name = UIUtils.cut_string(rental['username'], 10)
            d_name = UIUtils.cut_string(rental['depart_name'], 13)
            a_name = UIUtils.cut_string(arr_station, 13)

            print(f" {rental['rentalid']:<4}   {u_name:<10}   {d_name:<13}   {r_time:<16}   {a_name:<13}   {rt_time:<16}   {rental['destination_point']}")
        print("-----------------------------------------------------------------------------------------------------------------------\n")

    @staticmethod
    def print_ticket_list(ticket_list: List[dict]) -> None:
        print("\n---------------------------------------------------------------------------------------")
        print(" ID     사용자 이름      구매 일시            만료 일시            상태")
        print("---------------------------------------------------------------------------------------")
        for ticket in ticket_list:
            p_time = ticket['purchase_time'].strftime("%Y/%m/%d %H:%M")
            e_time = ticket['expire_time'].strftime("%Y/%m/%d %H:%M")
            print(f" {ticket['ticketid']:<4}   {ticket['username']:<11}   {p_time:<18}   {e_time:<18}   {ticket['status']}")
        print("---------------------------------------------------------------------------------------\n")