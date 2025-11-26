using MySql.Data.MySqlClient;
using Mysqlx.Crud;
using SharingTm.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SharingTm
{
    public static class DatabaseManager
    {
        // DB 연결 문자열 설정
        private static readonly string sqlSetting = "Server=localhost;Port=3306;Database=sharingtmdb;Uid=root;Pwd=1111";

        public static bool TestConnection()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(sqlSetting))
                {
                    connection.Open();
                    return true;
                }
            }
            catch
            {
                Console.WriteLine("MySQL 연결에 실패했습니다.");
                return false;
            }
        }

        // INSERT, UPDATE, DELETE 등 반환 데이터가 없는 쿼리를 처리하는 공통 메서드
        private static bool ExecuteNonQuery(string query, Action<MySqlCommand> parameterizer = null)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(sqlSetting))
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        parameterizer?.Invoke(command);
                        return command.ExecuteNonQuery() == 1;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"오류가 발생했습니다.\n{ex.Message}");
                return false;
            }
        }

        // SELECT 쿼리 실행 후 결과를 객체 리스트로 매핑하여 반환하는 공통 메서드
        private static List<T> ExecuteSelect<T>(string query, Func<MySqlDataReader, T> mapper, Action<MySqlCommand> parameterizer = null)
        {
            List<T> list = new List<T>();
            try
            {
                using (MySqlConnection connection = new MySqlConnection(sqlSetting))
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        parameterizer?.Invoke(command);

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                list.Add(mapper(reader));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"오류가 발생했습니다.\n{ex.Message}");
            }
            return list;
        }

        public static bool AddNewUser(string newUserName)
        {
            string query = "INSERT INTO User(username) VALUES(@username)";
            return ExecuteNonQuery(query, cmd => cmd.Parameters.AddWithValue("@username", newUserName));
        }

        public static bool DeleteUser(int userid)
        {
            string query = "DELETE FROM User WHERE userid = @userid";
            return ExecuteNonQuery(query, cmd => cmd.Parameters.AddWithValue("@userid", userid));
        }

        public static List<User> GetUserByName(string searchName)
        {
            // 유저 정보 조회 시, 현재 유효한(ACTIVE 상태 + 만료 전) 이용권 보유 여부를 함께 확인
            string query = @"
                SELECT u.userid, u.username, u.balance, IF(COUNT(t.ticketid) > 0, 1, 0) AS has_valid_ticket 
                FROM User AS u
                LEFT JOIN Ticket t ON u.userid = t.userid AND t.status = 'ACTIVE' AND t.expire_time > NOW()
                WHERE u.username LIKE @username
                GROUP BY u.userid, u.username, u.balance";

            return ExecuteSelect(query, reader => new User
            {
                userid = reader.GetInt32("userid"),
                username = reader.GetString("username"),
                balance = reader.GetInt32("balance"),
                hasTicket = Convert.ToBoolean(reader["has_valid_ticket"])
            },
            cmd => cmd.Parameters.AddWithValue("@username", $"%{searchName}%"));
        }

        public static List<User> GetUsers()
        {
            // 전체 유저 조회 (이용권 보유 여부 포함 로직 동일)
            string query = @"
                SELECT u.userid, u.username, u.balance, IF(COUNT(t.ticketid) > 0, 1, 0) AS has_valid_ticket 
                FROM User AS u
                LEFT JOIN Ticket t ON u.userid = t.userid AND t.status = 'ACTIVE' AND t.expire_time > NOW()
                GROUP BY u.userid, u.username, u.balance";

            return ExecuteSelect(query, reader => new User
            {
                userid = reader.GetInt32("userid"),
                username = reader.GetString("username"),
                balance = reader.GetInt32("balance"),
                hasTicket = Convert.ToBoolean(reader["has_valid_ticket"])
            });
        }

        public static bool AddStation(string stationName, string stationAddress)
        {
            string query = "INSERT INTO RentalStation(rs_name, rs_address) VALUES(@stationName, @stationAddress)";
            return ExecuteNonQuery(query, cmd =>
            {
                cmd.Parameters.AddWithValue("@stationName", stationName);
                cmd.Parameters.AddWithValue("@stationAddress", stationAddress);
            });
        }

        public static bool DeleteStation(int stationID)
        {
            string query = "DELETE FROM RentalStation WHERE rsid = @stationID";
            return ExecuteNonQuery(query, cmd => cmd.Parameters.AddWithValue("@stationID", stationID));
        }

        public static List<Station> GetStations()
        {
            // 대여소 조회 시, 현재 해당 대여소에 보관된(주차된) 타임머신 개수(current_stock)를 카운트
            string query = @"
                SELECT rs.rsid, rs.rs_name, rs.rs_address, COUNT(tm.tmid) AS current_stock
                FROM RentalStation AS rs
                LEFT JOIN TimeMachine AS tm ON rs.rsid = tm.rsid
                GROUP BY rs.rsid, rs.rs_name, rs.rs_address";

            return ExecuteSelect(query, reader => new Station
            {
                stationID = reader.GetInt32("rsid"),
                stationName = reader.GetString("rs_name"),
                stationAddress = reader.GetString("rs_address"),
                parkedTimeMachine = Convert.ToInt32(reader["current_stock"])
            });
        }

        public static List<Station> GetStationsByName(string searchName)
        {
            // 이름으로 대여소 검색 (재고 확인 로직 포함)
            string query = @"
                SELECT rs.rsid, rs.rs_name, rs.rs_address, COUNT(tm.tmid) AS current_stock
                FROM RentalStation AS rs
                LEFT JOIN TimeMachine AS tm ON rs.rsid = tm.rsid
                WHERE rs.rs_name LIKE @station_name
                GROUP BY rs.rsid, rs.rs_name, rs.rs_address";

            return ExecuteSelect(query, reader => new Station
            {
                stationID = reader.GetInt32("rsid"),
                stationName = reader.GetString("rs_name"),
                stationAddress = reader.GetString("rs_address"),
                parkedTimeMachine = Convert.ToInt32(reader["current_stock"])
            },
            cmd => cmd.Parameters.AddWithValue("@station_name", $"%{searchName}%"));
        }

        public static List<Station> GetStationsByAddress(string searchAddress)
        {
            // 주소로 대여소 검색 (재고 확인 로직 포함)
            string query = @"
                SELECT rs.rsid, rs.rs_name, rs.rs_address, COUNT(tm.tmid) AS current_stock
                FROM RentalStation AS rs
                LEFT JOIN TimeMachine AS tm ON rs.rsid = tm.rsid
                WHERE rs.rs_address LIKE @station_address
                GROUP BY rs.rsid, rs.rs_name, rs.rs_address";

            return ExecuteSelect(query, reader => new Station
            {
                stationID = reader.GetInt32("rsid"),
                stationName = reader.GetString("rs_name"),
                stationAddress = reader.GetString("rs_address"),
                parkedTimeMachine = Convert.ToInt32(reader["current_stock"])
            },
            cmd => cmd.Parameters.AddWithValue("@station_address", $"%{searchAddress}%"));
        }
    }
}
