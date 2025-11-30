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
            /* 유저 정보 조회 시, 현재 유효한(ACTIVE 상태 + 만료 전) 이용권 보유 여부를 함께 확인
             IF(COUNT(t.ticketid) > 0, 1, 0) : COUNT(t.ticketid) > 0이 참이면 1로, 거짓이면 0으로*/
            string query = @"
                SELECT 
                    u.userid, 
                    u.username, 
                    u.balance, 
                    IF(COUNT(DISTINCT t.ticketid) > 0, 1, 0) AS has_valid_ticket,
                    IF(COUNT(DISTINCT r.rentalid) > 0, 1, 0) AS is_renting
                FROM User AS u
                LEFT JOIN Ticket t 
                    ON u.userid = t.userid 
                    AND t.status = 'ACTIVE' 
                    AND t.expire_time > NOW()
                LEFT JOIN Rental r 
                    ON u.userid = r.userid 
                    AND r.return_time IS NULL
                WHERE u.username LIKE @username
                GROUP BY u.userid, u.username, u.balance";

            return ExecuteSelect(query, reader => new User
            {
                userID = reader.GetInt32("userid"),
                userName = reader.GetString("username"),
                balance = reader.GetInt32("balance"),
                hasTicket = Convert.ToBoolean(reader["has_valid_ticket"]),
                isRenting = Convert.ToBoolean(reader["is_renting"])
            },
            cmd => cmd.Parameters.AddWithValue("@username", $"%{searchName}%"));
        }

        public static List<User> GetUsers()
        {
            // 전체 유저 조회 (이용권 보유 여부 포함 로직 동일)
            string query = @"
            SELECT 
                u.userid, 
                u.username, 
                u.balance, 
                -- 유효한 티켓이 있는지 (중복 방지 DISTINCT)
                IF(COUNT(DISTINCT t.ticketid) > 0, 1, 0) AS has_valid_ticket,
                -- 미반납 대여 기록이 있는지 (return_time이 NULL인 것)
                IF(COUNT(DISTINCT r.rentalid) > 0, 1, 0) AS is_renting
             FROM User AS u
            -- 1) 티켓 정보 조인 (유효한 것만)
            LEFT JOIN Ticket t 
                ON u.userid = t.userid 
                AND t.status = 'ACTIVE' 
                AND t.expire_time > NOW()
            -- 2) 대여 정보 조인 (미반납인 것만)
            LEFT JOIN Rental r 
                ON u.userid = r.userid 
                AND r.return_time IS NULL
            GROUP BY u.userid, u.username, u.balance";

            return ExecuteSelect(query, reader => new User
            {
                userID = reader.GetInt32("userid"),
                userName = reader.GetString("username"),
                balance = reader.GetInt32("balance"),
                hasTicket = Convert.ToBoolean(reader["has_valid_ticket"]),
                isRenting = Convert.ToBoolean(reader["is_renting"])
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
        
        public static List<TimeMachine> GetTimeMachines()
        {
            // 타임머신 테이블의 외래키인 rsid를 통해 타임머신의 이름도 가져옴
            string query = @"
                SELECT 
                    tm.tmid, 
                    tm.model, 
                    tm.current_era_point, 
                    tm.rsid, 
                    rs.rs_name 
                FROM TimeMachine AS tm
                LEFT JOIN RentalStation AS rs ON tm.rsid = rs.rsid";

            return ExecuteSelect(query, reader =>
            {
                int rsidIndex = reader.GetOrdinal("rsid");
                int rsNameIndex = reader.GetOrdinal("rs_name");

                return new TimeMachine
                {
                    timeMachineID = reader.GetInt32("tmid"),
                    timeMachineModel = reader.GetString("model"),
                    currentEraPoint = reader.GetString("current_era_point"),
                    currentStationID = reader.IsDBNull(rsidIndex) ? 0 : reader.GetInt32(rsidIndex),
                    currentStationName = reader.IsDBNull(rsNameIndex) ? null : reader.GetString(rsNameIndex)
                };
            });
        }

        public static List<TimeMachine> GetTimeMachinesWithStation(int searchStationID)
        {
            string query = @"
                SELECT 
                    tm.tmid, 
                    tm.model, 
                    tm.current_era_point, 
                    tm.rsid, 
                    rs.rs_name 
                FROM TimeMachine AS tm
                LEFT JOIN RentalStation AS rs ON tm.rsid = rs.rsid
                WHERE tm.rsid = @stationID";

            return ExecuteSelect(query, reader =>
            {
                int rsidIndex = reader.GetOrdinal("rsid");
                int rsNameIndex = reader.GetOrdinal("rs_name");

                return new TimeMachine
                {
                    timeMachineID = reader.GetInt32("tmid"),
                    timeMachineModel = reader.GetString("model"),
                    currentEraPoint = reader.GetString("current_era_point"),
                    currentStationID = reader.IsDBNull(rsidIndex) ? 0 : reader.GetInt32(rsidIndex),
                    currentStationName = reader.IsDBNull(rsNameIndex) ? "(대여중)"  : reader.GetString(rsNameIndex)
                };
            },
            cmd => cmd.Parameters.AddWithValue("@stationID", searchStationID));
        }

        public static bool AddTimeMachine(string timeMachineModel, int stationID)
        {
            string query = @"INSERT INTO TimeMachine(model, rsid) VALUES(@timeMachineModel ,@stationID)";
            return ExecuteNonQuery(query, cmd =>
            {
                cmd.Parameters.AddWithValue("@timeMachineModel", timeMachineModel);
                cmd.Parameters.AddWithValue("@stationID", stationID);
            });
        }

        public static bool DeleteTimeMachine(int timeMachineID)
        {
            string query = "DELETE FROM TimeMachine WHERE tmid = @timeMachineID";
            return ExecuteNonQuery(query, cmd => cmd.Parameters.AddWithValue("@timeMachineID", timeMachineID));
        }

        public static bool ChargeBalance(int userID, int chargeAmount)
        {
            string query = "UPDATE User SET balance = balance + @chargeAmount WHERE userid = @userID";
            return ExecuteNonQuery(query, cmd =>
            {
                cmd.Parameters.AddWithValue("@chargeAmount", chargeAmount);
                cmd.Parameters.AddWithValue("@userID", userID);
            });
        }

        // 유저 정보 새로고침용
        public static User GetUserByID(int userID)
        {
            string query = @"
                SELECT 
                    u.userid, 
                    u.username, 
                    u.balance, 
                    IF(COUNT(DISTINCT t.ticketid) > 0, 1, 0) AS has_valid_ticket,
                    IF(COUNT(DISTINCT r.rentalid) > 0, 1, 0) AS is_renting
                FROM User AS u
                LEFT JOIN Ticket t 
                    ON u.userid = t.userid 
                    AND t.status = 'ACTIVE' 
                    AND t.expire_time > NOW()
                LEFT JOIN Rental r 
                    ON u.userid = r.userid 
                    AND r.return_time IS NULL
                WHERE u.userid = @userID
                GROUP BY u.userid, u.username, u.balance";

            List<User> result = ExecuteSelect(query, reader => new User
            {
                userID = reader.GetInt32("userid"),
                userName = reader.GetString("username"),
                balance = reader.GetInt32("balance"),
                hasTicket = Convert.ToBoolean(reader["has_valid_ticket"]),
                isRenting = Convert.ToBoolean(reader["is_renting"])
            },
            cmd => cmd.Parameters.AddWithValue("@userID", userID));

            // 결과가 있으면 첫 번째 유저 반환, 없으면 null 반환
            return result.Count > 0 ? result[0] : null;
        }

        public static List<Ticket> GetTickets()
        {
            string query = @"
                SELECT 
                    t.ticketid, 
                    t.purchase_time, 
                    t.expire_time, 
                    t.status, 
                    t.userid, 
                    u.username 
                FROM Ticket AS t
                JOIN User AS u ON t.userid = u.userid
                -- 내림차순으로 정렬
                ORDER BY t.ticketid DESC";

            return ExecuteSelect(query, reader => new Ticket
            {
                ticketID = reader.GetInt32("ticketid"),
                purchaseTime = reader.GetDateTime("purchase_time"),
                expireTime = reader.GetDateTime("expire_time"),
                status = reader.GetString("status"),
                userID = reader.GetInt32("userid"),
                userName = reader.GetString("username")
            });
        }

        public static List<Ticket> GetTicketsWithUserID(int userID)
        {
            string query = @"
                SELECT 
                    t.ticketid, 
                    t.purchase_time, 
                    t.expire_time, 
                    t.status, 
                    t.userid, 
                    u.username 
                FROM Ticket AS t
                JOIN User AS u ON t.userid = u.userid
                WHERE u.userID = @userID
                ORDER BY t.ticketid DESC";

            return ExecuteSelect(query, reader => new Ticket
            {
                ticketID = reader.GetInt32("ticketid"),
                purchaseTime = reader.GetDateTime("purchase_time"),
                expireTime = reader.GetDateTime("expire_time"),
                status = reader.GetString("status"),
                userID = reader.GetInt32("userid"),
                userName = reader.GetString("username")
            },
            cmd => cmd.Parameters.AddWithValue("@userID", userID));
        }

        // 티켓 구매
        public static bool PurchaseTicket(int userID, int ticketDurationDays, int price)
        {
            using (MySqlConnection connection = new MySqlConnection(sqlSetting))
            {
                connection.Open();
                // 트랜잭션 시작 
                MySqlTransaction transaction = connection.BeginTransaction();
                try
                {
                    // 티켓 생성 부분
                    DateTime now = DateTime.Now;
                    string insertQuery = @"
                        INSERT INTO Ticket(userId, purchase_time, expire_time)
                        VALUES(@userID, @purchase_time, @expire_time)";

                    using (MySqlCommand cmd1 = new MySqlCommand(insertQuery, connection, transaction))
                    {
                        cmd1.Parameters.AddWithValue("@userID", userID);
                        cmd1.Parameters.AddWithValue("@purchase_time", now);
                        cmd1.Parameters.AddWithValue("@expire_time", now.AddDays(ticketDurationDays));

                        cmd1.ExecuteNonQuery();
                    }

                    // 잔액 차감 부분
                    string updateQuery = "UPDATE User SET balance = balance - @price WHERE userid = @userID";

                    using (MySqlCommand cmd2 = new MySqlCommand(updateQuery, connection, transaction))
                    {
                        cmd2.Parameters.AddWithValue("@price", price);
                        cmd2.Parameters.AddWithValue("@userID", userID);

                        int rowsAffected = cmd2.ExecuteNonQuery();

                        // 만약 업데이트된 행이 없다면 실패 처리
                        if (rowsAffected == 0)
                        {
                            throw new Exception();
                        }
                    }
                    transaction.Commit();
                    return true;
                }
                catch
                {
                    // 하나라도 실패하면 트랜잭션 롤백 
                    transaction.Rollback();
                    return false;
                }
            }
        }

        // 만료 티켓 비활성화
        public static void UpdateExpiredTickets()
        {
            string query = @"
                UPDATE Ticket
                SET status = 'INACTIVE'
                WHERE status = 'ACTIVE' 
                    AND expire_time < NOW()";
            try
            {
                using (MySqlConnection connection = new MySqlConnection(sqlSetting))
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch
            {

            }
        }

        public static List<Rental> GetRentalHistory()
        {
            string query = @"
                SELECT 
                    r.rentalid,
                    r.userid,
                    u.username,
                    r.tmid,
                    r.start_rsid,
                    StartS.rs_name AS depart_station_name,
                    r.rental_time,
                    r.end_rsid,
                    EndS.rs_name AS arrive_station_name,
                    r.return_time,
                    r.destination_point
                FROM Rental r
                JOIN User u ON r.userid = u.userid
                LEFT JOIN RentalStation StartS ON r.start_rsid = StartS.rsid
                LEFT JOIN RentalStation EndS ON r.end_rsid = EndS.rsid
                ORDER BY r.rentalid DESC";

            return ExecuteSelect(query, reader =>
            {
                int endRsidIndex = reader.GetOrdinal("end_rsid");
                int arriveNameIndex = reader.GetOrdinal("arrive_station_name");
                int returnTimeIndex = reader.GetOrdinal("return_time");
                int startRsidIndex = reader.GetOrdinal("start_rsid");
                int departNameIndex = reader.GetOrdinal("depart_station_name");

                return new Rental
                {
                    rentalID = reader.GetInt32("rentalid"),
                    userID = reader.GetInt32("userid"),
                    userName = reader.GetString("username"),
                    timeMachineID = reader.GetInt32("tmid"),

                    // 제거된 대여소 출력
                    departStationID = reader.IsDBNull(startRsidIndex) ? 0 : reader.GetInt32(startRsidIndex),
                    departStationName = reader.IsDBNull(departNameIndex) ? "(제거된 대여소)" : reader.GetString(departNameIndex),

                    rentalTime = reader.GetDateTime("rental_time"),

                    // 도착지 정보 (미반납 시 -)
                    arriveStationID = reader.IsDBNull(endRsidIndex) ? 0 : reader.GetInt32(endRsidIndex),
                    arriveStationName = reader.IsDBNull(arriveNameIndex) ? "-" : reader.GetString(arriveNameIndex),

                    // 반납 시간 (미반납 시 MinValue로 설정 -> 출력 때 체크해서 빈칸으로 표시)
                    returnTime = reader.IsDBNull(returnTimeIndex) ? DateTime.MinValue : reader.GetDateTime(returnTimeIndex),

                    destinationPoint = reader.GetString("destination_point")
                };
            });
        }

        public static List<Rental> GetRentalHistoryWithUserID(int userID)
        {
            string query = @"
                SELECT 
                    r.rentalid,
                    r.userid,
                    u.username,
                    r.tmid,
                    r.start_rsid,
                    StartS.rs_name AS depart_station_name,
                    r.rental_time,
                    r.end_rsid,
                    EndS.rs_name AS arrive_station_name,
                    r.return_time,
                    r.destination_point
                FROM Rental r
                JOIN User u ON r.userid = u.userid
                LEFT JOIN RentalStation StartS ON r.start_rsid = StartS.rsid
                LEFT JOIN RentalStation EndS ON r.end_rsid = EndS.rsid
                WHERE u.userID = @userID
                ORDER BY r.rentalid DESC";

            return ExecuteSelect(query, reader =>
            {
                int endRsidIndex = reader.GetOrdinal("end_rsid");
                int arriveNameIndex = reader.GetOrdinal("arrive_station_name");
                int returnTimeIndex = reader.GetOrdinal("return_time");
                int startRsidIndex = reader.GetOrdinal("start_rsid");
                int departNameIndex = reader.GetOrdinal("depart_station_name");

                return new Rental
                {
                    rentalID = reader.GetInt32("rentalid"),
                    userID = reader.GetInt32("userid"),
                    userName = reader.GetString("username"),
                    timeMachineID = reader.GetInt32("tmid"),

                    // 제거된 대여소 출력
                    departStationID = reader.IsDBNull(startRsidIndex) ? 0 : reader.GetInt32(startRsidIndex),
                    departStationName = reader.IsDBNull(departNameIndex) ? "(제거된 대여소)" : reader.GetString(departNameIndex),

                    rentalTime = reader.GetDateTime("rental_time"),

                    // 도착지 정보 (미반납 시 -)
                    arriveStationID = reader.IsDBNull(endRsidIndex) ? 0 : reader.GetInt32(endRsidIndex),
                    arriveStationName = reader.IsDBNull(arriveNameIndex) ? "-" : reader.GetString(arriveNameIndex),

                    // 반납 시간 (미반납 시 MinValue로 설정 -> 출력 때 체크해서 빈칸으로 표시)
                    returnTime = reader.IsDBNull(returnTimeIndex) ? DateTime.MinValue : reader.GetDateTime(returnTimeIndex),

                    destinationPoint = reader.GetString("destination_point")
                };
            },
            cmd => cmd.Parameters.AddWithValue("@userID", userID));
        }

        public static bool RentTimeMachine(int userId, int timeMachineId, string destination)
        {
            using (MySqlConnection connection = new MySqlConnection(sqlSetting))
            {
                connection.Open();
                // 트랜잭션 시작
                MySqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    // 출발 대여소 검색
                    int startStationId = 0;
                    string selectQuery = "SELECT rsid FROM TimeMachine WHERE tmid = @tmId";

                    using (MySqlCommand cmdSelect = new MySqlCommand(selectQuery, connection, transaction))
                    {
                        cmdSelect.Parameters.AddWithValue("@tmId", timeMachineId);
                        // ExecuteScalar는 첫 번째 컬럼의 값을 가져옴
                        object result = cmdSelect.ExecuteScalar();
                        startStationId = Convert.ToInt32(result);
                    }

                    // Rental 테이블에 기록
                    string insertQuery = @"
                        INSERT INTO Rental (userid, tmid, depart_rs, rental_time, destination_point)
                        VALUES (@userId, @tmId, @startStationId, NOW(), @destination)";

                    using (MySqlCommand cmdInsert = new MySqlCommand(insertQuery, connection, transaction))
                    {
                        cmdInsert.Parameters.AddWithValue("@userId", userId);
                        cmdInsert.Parameters.AddWithValue("@tmId", timeMachineId);
                        cmdInsert.Parameters.AddWithValue("@startStationId", startStationId);
                        cmdInsert.Parameters.AddWithValue("@destination", destination);
                        cmdInsert.ExecuteNonQuery();
                    }

                    // 타임머신 테이블 변경
                    string updateQuery = @"
                        UPDATE TimeMachine 
                        SET rsid = NULL, 
                            current_era_point = @dest 
                        WHERE tmid = @tmId";

                    using (MySqlCommand cmdUpdate = new MySqlCommand(updateQuery, connection, transaction))
                    {
                        cmdUpdate.Parameters.AddWithValue("@dest", destination);
                        cmdUpdate.Parameters.AddWithValue("@tmId", timeMachineId);
                        cmdUpdate.ExecuteNonQuery();
                    }

                    // 오류 없으면 저장
                    transaction.Commit();
                    return true;
                }
                catch
                {
                    // 오류 발생 시 모든 작업 취소
                    transaction.Rollback();
                    return false;
                }
            }
        }

        public static bool ReturnTimeMachine(User user, int returnStationId)
        {
            using (MySqlConnection connection = new MySqlConnection(sqlSetting))
            {
                connection.Open();
                MySqlTransaction transaction = connection.BeginTransaction(); // 트랜잭션 시작

                try
                {
                    // 사용자의 대여내역 조회
                    int rentalID = 0;
                    int timeMachineID = 0;

                    // 반납 시간이 NULL인(아직 빌리고 있는) 기록 검색.
                    string findQuery = "SELECT rentalid, tmid FROM Rental WHERE userid = @userID AND return_time IS NULL";

                    using (MySqlCommand cmdFind = new MySqlCommand(findQuery, connection, transaction))
                    {
                        cmdFind.Parameters.AddWithValue("@userID", user.userID);

                        using (MySqlDataReader reader = cmdFind.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                rentalID = reader.GetInt32("rentalid");
                                timeMachineID = reader.GetInt32("tmid");
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }

                    // Rental 테이블 업데이트
                    DateTime returnTime = DateTime.Now;

                    string updateRental = @"
                        UPDATE Rental 
                        SET return_time = @returnTime, 
                            arrive_rs = @stationId
                        WHERE rentalid = @rentalId";

                    using (MySqlCommand cmdRental = new MySqlCommand(updateRental, connection, transaction))
                    {
                        cmdRental.Parameters.AddWithValue("@returnTime", returnTime);
                        cmdRental.Parameters.AddWithValue("@stationId", returnStationId);
                        cmdRental.Parameters.AddWithValue("@rentalId", rentalID); 
                        cmdRental.ExecuteNonQuery();
                    }

                    // TimeMachine 테이블 업데이트
                    string updateTM = @"
                        UPDATE TimeMachine 
                        SET rsid = @stationId, 
                            current_era_point = 'CurrentTime' 
                        WHERE tmid = @tmId";

                    using (MySqlCommand cmdTM = new MySqlCommand(updateTM, connection, transaction))
                    {
                        cmdTM.Parameters.AddWithValue("@stationId", returnStationId);
                        cmdTM.Parameters.AddWithValue("@tmId", timeMachineID); // 
                        cmdTM.ExecuteNonQuery();
                    }

                    // 모든 쿼리 성공 시 커밋
                    transaction.Commit();
                    return true;
                }
                catch
                {
                    // 하나라도 실패하면 롤백
                    transaction.Rollback();
                    return false;
                }
            }
        }
    }
}