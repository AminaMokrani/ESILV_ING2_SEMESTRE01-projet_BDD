
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using GymAppConsole.Models;
using MySql.Data.MySqlClient;

namespace GymAppConsole
{
    public class DataAccess
    {
        // string pour la connexoin
        const string CONN_STR = "Server=localhost;Database=gymdb;Uid=app_main;Pwd=app_main_pwd;";



        // Méthode pour retourner la connexion sql
        MySqlConnection GetConnection() { return new MySqlConnection(CONN_STR); }

        
        // méthode servait à se connecter entant que user (membre ou admin)
        public User Login(string username, string password)
        {
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT id, password, role FROM users WHERE username=@u", conn);
                cmd.Parameters.AddWithValue("@u", username);
                using (MySqlDataReader rdr = cmd.ExecuteReader())
                {
                    if (!rdr.Read()) return null;
                    int id = rdr.GetInt32("id");
                    string storedPassword = rdr.GetString("password");
                    string role = rdr.GetString("role");
                    rdr.Close();
                    if (password == storedPassword) return new User { Id = id, Username = username, Role = role };
                    return null;
                }
            }
        }

        // méthode qui permet d'enregister/d'inscrire un nouveau membre qui est associé à un user
        public int RegisterUserAndMember(string username, string password, string firstName, string lastName, string email)
        {
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                using (MySqlTransaction tran = conn.BeginTransaction())
                {
                    try
                    {
                        MySqlCommand cmdUser = new MySqlCommand("INSERT INTO users (username,password,role) VALUES (@u,@p,'member'); SELECT LAST_INSERT_ID()", conn, tran);
                        cmdUser.Parameters.AddWithValue("@u", username);
                        cmdUser.Parameters.AddWithValue("@p", password);
                        int userId = Convert.ToInt32(cmdUser.ExecuteScalar());

                        MySqlCommand cmdMember = new MySqlCommand("INSERT INTO members (user_id,first_name,last_name,email) VALUES (@uid,@fn,@ln,@em); SELECT LAST_INSERT_ID()", conn, tran);
                        cmdMember.Parameters.AddWithValue("@uid", userId);
                        cmdMember.Parameters.AddWithValue("@fn", firstName);
                        cmdMember.Parameters.AddWithValue("@ln", lastName);
                        cmdMember.Parameters.AddWithValue("@em", email);
                        int memberId = Convert.ToInt32(cmdMember.ExecuteScalar());

                        tran.Commit();
                        return memberId;
                    }
                    catch (Exception ex)
                    {
                        try { tran.Rollback(); } catch { }
                        throw new Exception("Erreur enregistrement: " + ex.Message);
                    }
                }
            }
        }

        // PARTIE SUR LA GESTION DES MEMBRES

        //Fonction qui permet de valider un membre
        public bool ValidateMember(int memberId)
        {
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("UPDATE members SET status='validée' WHERE id=@id AND status='en attente'", conn);
                cmd.Parameters.AddWithValue("@id", memberId);
                return cmd.ExecuteNonQuery() > 0;
            }
        }


        // Fonction qui permet de renvoyer une liste de membres encore en attente de validation
        public List<string> GetPendingMembers()
        {
            List<string> list = new List<string>();
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                string sql = "SELECT id, first_name, last_name FROM members WHERE status = 'en attente'";
                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add($"ID: {reader["id"]} | Nom: {reader["first_name"]} {reader["last_name"]}");
                        }
                    }
                }
            }
            return list;
        }

        // Méthode qui permet de supprimer un membre en utilisant un id
        public bool DeleteMemberById(int memberId)
        {
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmdGet = new MySqlCommand("SELECT user_id FROM members WHERE id=@id", conn);
                cmdGet.Parameters.AddWithValue("@id", memberId);
                object o = cmdGet.ExecuteScalar();
                if (o == null) return false;
                int uid = Convert.ToInt32(o);
                MySqlCommand cmdDel = new MySqlCommand("DELETE FROM users WHERE id=@uid", conn);
                cmdDel.Parameters.AddWithValue("@uid", uid);
                return cmdDel.ExecuteNonQuery() > 0;
            }
        }

        // Méthode qui permet d'avoir les infos d'un membre selon son id
        public Member GetMemberInfo(int memberId)
        {
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT id,user_id,first_name,last_name,email,status FROM members WHERE id=@id", conn);
                cmd.Parameters.AddWithValue("@id", memberId);
                using (MySqlDataReader rdr = cmd.ExecuteReader())
                {
                    if (!rdr.Read()) return null;
                    return new Member
                    {
                        Id = rdr.GetInt32("id"),
                        UserId = rdr.GetInt32("user_id"),
                        FirstName = rdr.GetString("first_name"),
                        LastName = rdr.GetString("last_name"),
                        Email = rdr.GetString("email"),
                        Status = rdr.GetString("status")
                    };
                }
            }
        }


        // Méthode qui permet de retourner une liste avec tous les membres
        public List<string> GetAllMembers()
        {
            List<string> list = new List<string>();
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                string sql = "SELECT id, first_name, last_name, status FROM members ORDER BY id ASC";
                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add($"ID: {reader["id"]}: {reader["first_name"]} {reader["last_name"]} - Statut: {reader["status"]}");
                        }
                    }
                }
            }
            return list;
        }

        // Méthode qui permet de retourner les infos d'un user selon son id
        public Member GetUserInfo(int userId)
        {
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                string sql = "SELECT * FROM members WHERE user_id = @uid";
                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@uid", userId);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Member
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                FirstName = reader["first_name"].ToString(),
                                LastName = reader["last_name"].ToString(),
                                Email = reader["email"].ToString(),
                                Status = reader["status"].ToString()
                            };
                        }
                    }
                }
            }
            return null;
        }


        // Fonction qui permet de mettre à jour les infos d'un membre
        public bool UpdateMember(int memberId, string firstName, string lastName, string email)
        {
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                List<string> updates = new List<string>();
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;
                if (!string.IsNullOrEmpty(firstName)) { updates.Add("first_name=@fn"); cmd.Parameters.AddWithValue("@fn", firstName); }
                if (!string.IsNullOrEmpty(lastName)) { updates.Add("last_name=@ln"); cmd.Parameters.AddWithValue("@ln", lastName); }
                if (!string.IsNullOrEmpty(email)) { updates.Add("email=@em"); cmd.Parameters.AddWithValue("@em", email); }
                if (updates.Count == 0) return false;
                cmd.CommandText = $"UPDATE members SET {string.Join(",", updates)} WHERE id=@id";
                cmd.Parameters.AddWithValue("@id", memberId);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // PARTIE SUR LA GESTION DES COACHS

        // Fonction qui permet d'afficher un coach
        public int AddCoach(string firstName, string lastName, string specialties, string phone, string email)
        {
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("INSERT INTO coaches (first_name,last_name,specialties,phone,email) VALUES (@fn,@ln,@spec,@ph,@em); SELECT LAST_INSERT_ID()", conn);
                cmd.Parameters.AddWithValue("@fn", firstName);
                cmd.Parameters.AddWithValue("@ln", lastName);
                cmd.Parameters.AddWithValue("@spec", specialties);
                cmd.Parameters.AddWithValue("@ph", phone);
                cmd.Parameters.AddWithValue("@em", email);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        // Fonction qui permet de retourner une liste de coachs, autrement dit la liste de l'ensemble des coachs
        public List<Coach> ListCoaches()
        {
            List<Coach> list = new List<Coach>();
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT id,first_name,last_name,specialties,phone,email FROM coaches", conn);
                using (MySqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        list.Add(new Coach
                        {
                            Id = rdr.GetInt32("id"),
                            FirstName = rdr.GetString("first_name"),
                            LastName = rdr.GetString("last_name"),
                            Specialties = rdr.IsDBNull(rdr.GetOrdinal("specialties")) ? "" : rdr.GetString("specialties"),
                            Phone = rdr.IsDBNull(rdr.GetOrdinal("phone")) ? "" : rdr.GetString("phone"),
                            Email = rdr.IsDBNull(rdr.GetOrdinal("email")) ? "" : rdr.GetString("email")
                        });
                    }
                }
            }
            return list;
        }


        // Methode qui permet de retourner une liste couple coach/courses, afin de retourner les coachs associés à leurs cours
        public List<(Coach coach, Course course)> ListCoachesWithCoursesLeftJoin()
        {
            List<(Coach, Course)> list = new List<(Coach, Course)>();
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                string sql = @"
                    SELECT co.id as coid, co.first_name, co.last_name, co.specialties, co.phone, co.email,
                           c.id as cid, c.name as cname, c.schedule_date, c.max_participants, c.coach_id
                    FROM coaches co
                    LEFT JOIN courses c ON c.coach_id = co.id
                ";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                using (MySqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        Coach co = new Coach
                        {
                            Id = rdr.GetInt32("coid"),
                            FirstName = rdr.GetString("first_name"),
                            LastName = rdr.GetString("last_name"),
                            Specialties = rdr.IsDBNull(rdr.GetOrdinal("specialties")) ? "" : rdr.GetString("specialties"),
                            Phone = rdr.IsDBNull(rdr.GetOrdinal("phone")) ? "" : rdr.GetString("phone"),
                            Email = rdr.IsDBNull(rdr.GetOrdinal("email")) ? "" : rdr.GetString("email")
                        };
                        Course cour = null;
                        if (!rdr.IsDBNull(rdr.GetOrdinal("cid")))
                        {
                            cour = new Course
                            {
                                Id = rdr.GetInt32("cid"),
                                Name = rdr.GetString("cname"),
                                Schedule = rdr.GetDateTime("schedule_date"),
                                MaxParticipants = rdr.GetInt32("max_participants"),
                                CoachId = rdr.IsDBNull(rdr.GetOrdinal("coach_id")) ? 0 : rdr.GetInt32("coach_id")
                            };
                        }
                        list.Add((co, cour));
                    }
                }
            }
            return list;
        }


        // Méthode qui permet de returner une liste associant id de salle/nom de la salle/cours, afin de renvoyer les salles et leurs cours associés
        public List<(int roomId, string roomName, Course course)> RoomsRightJoinCourses()
        {
            List<(int, string, Course)> list = new List<(int, string, Course)>();
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                string sql = @"
                    SELECT r.id as room_id, r.name as room_name, c.id as course_id, c.name as course_name, c.schedule_date, c.max_participants, c.coach_id
                    FROM rooms r
                    RIGHT JOIN courses c ON c.room_id = r.id
                ";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                using (MySqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        int roomId = rdr.IsDBNull(rdr.GetOrdinal("room_id")) ? 0 : rdr.GetInt32("room_id");
                        string roomName = rdr.IsDBNull(rdr.GetOrdinal("room_name")) ? null : rdr.GetString("room_name");
                        Course course = new Course
                        {
                            Id = rdr.GetInt32("course_id"),
                            Name = rdr.GetString("course_name"),
                            Schedule = rdr.GetDateTime("schedule_date"),
                            MaxParticipants = rdr.GetInt32("max_participants"),
                            CoachId = rdr.IsDBNull(rdr.GetOrdinal("coach_id")) ? 0 : rdr.GetInt32("coach_id")
                        };
                        list.Add((roomId, roomName, course));
                    }
                }
            }
            return list;
        }

        // PARTIE DE GESTION DES COURS

        // Méthode permettant d'ajouter un cours
        public int AddCourse(string name, string description, int durationMinutes, int coachId, int roomId, int maxParticipants, DateTime schedule)
        {
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(@"
                    INSERT INTO courses (name,description,duration,coach_id,room_id,max_participants,schedule_date)
                    VALUES (@n,@d,@dur,@coach,@room,@maxp,@sched); SELECT LAST_INSERT_ID()", conn);
                cmd.Parameters.AddWithValue("@n", name);
                cmd.Parameters.AddWithValue("@d", description);
                cmd.Parameters.AddWithValue("@dur", durationMinutes);
                cmd.Parameters.AddWithValue("@coach", coachId);
                cmd.Parameters.AddWithValue("@room", roomId);
                cmd.Parameters.AddWithValue("@maxp", maxParticipants);
                cmd.Parameters.AddWithValue("@sched", schedule);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        // Méthode permettant de renvoyer l'ensemble des cours sous forme de liste
        
   
        public List<Course> ListCourses() { 

    List<Course> list = new List<Course>();
    using (MySqlConnection conn = GetConnection())
    {
        conn.Open();
        
    
        string sql = @"
           SELECT c.id, c.name,  c.description, c.duration, c.max_participants,  c.schedule_date, c.coach_id, c.room_id,COUNT(r.id) AS current_participants
        FROM courses c
         LEFT JOIN reservations r ON c.id = r.course_id AND r.status = 'active'
        GROUP BY c.id, c.name, c.description, c.duration, c.max_participants, c.schedule_date, c.coach_id, c.room_id
        ORDER BY c.schedule_date";

        using (MySqlCommand cmd = new MySqlCommand(sql, conn))
        {
            using (MySqlDataReader rdr = cmd.ExecuteReader())
            {
                while (rdr.Read())
                {
                    list.Add(new Course
                    {
                        Id = rdr.GetInt32("id"),
                        Name = rdr.GetString("name"), 
                        Schedule = rdr.GetDateTime("schedule_date"), 
                        MaxParticipants = rdr.GetInt32("max_participants"), 
   
                        CurrentParticipants = rdr.GetInt32("current_participants"),
                        CoachId = rdr.IsDBNull(rdr.GetOrdinal("coach_id")) ? 0 : rdr.GetInt32("coach_id"), 
                        RoomId = rdr.IsDBNull(rdr.GetOrdinal("room_id")) ? 0 : rdr.GetInt32("room_id")
                    });
                }
            }
        }
    }
    return list;
}

        // PARTIE CONCERNANT LES RESERVATIONS

        // Méthode permettant de reserver un cours
        public bool ReserveCourse(int memberId, int courseId)
        {
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                using (MySqlTransaction tran = conn.BeginTransaction())
                {
                    try
                    {
                        MySqlCommand cmdStatus = new MySqlCommand("SELECT status FROM members WHERE id=@mid", conn, tran);
                        cmdStatus.Parameters.AddWithValue("@mid", memberId);
                        string status = Convert.ToString(cmdStatus.ExecuteScalar() ?? "");
                        if (status != "validée") throw new Exception("Membre non validé.");

                        MySqlCommand cmdCheckRoom = new MySqlCommand(@"
                            SELECT c.max_participants, 
                            (SELECT COUNT(*) FROM reservations r WHERE r.course_id = c.id AND r.status = 'active') as current_count
                            FROM courses c WHERE c.id = @cid FOR UPDATE", conn, tran);
                        cmdCheckRoom.Parameters.AddWithValue("@cid", courseId);

                        using (MySqlDataReader reader = cmdCheckRoom.ExecuteReader())
                        {
                            if (!reader.Read()) throw new Exception("Cours introuvable.");
                            int maxp = reader.GetInt32("max_participants");
                            int current = reader.GetInt32("current_count");
                            reader.Close();
                            if (current >= maxp) throw new Exception("Capacité atteinte.");
                        }

                        MySqlCommand cmdUnique = new MySqlCommand("SELECT COUNT(*) FROM reservations WHERE course_id=@cid AND member_id=@mid AND status='active'", conn, tran);
                        cmdUnique.Parameters.AddWithValue("@cid", courseId);
                        cmdUnique.Parameters.AddWithValue("@mid", memberId);
                        if (Convert.ToInt32(cmdUnique.ExecuteScalar()) > 0) throw new Exception("Déjà inscrit.");

                        MySqlCommand cmdIns = new MySqlCommand("INSERT INTO reservations (course_id, member_id) VALUES (@cid, @mid)", conn, tran);
                        cmdIns.Parameters.AddWithValue("@cid", courseId);
                        cmdIns.Parameters.AddWithValue("@mid", memberId);
                        cmdIns.ExecuteNonQuery();

                        tran.Commit();
                        return true;
                    }
                    catch (Exception)
                    {
                        try { tran.Rollback(); } catch { }
                        return false;
                    }
                }
            }
        }


        // Méthode permettant de supprimer une reservation 
        public bool CancelReservation(int memberId, int courseId)
        {
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("UPDATE reservations SET status='annullée' WHERE member_id=@mid AND course_id=@cid AND status='active'", conn);
                cmd.Parameters.AddWithValue("@mid", memberId);
                cmd.Parameters.AddWithValue("@cid", courseId);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // Méthode permettant de supprimer une reservation selon son id
        public bool CancelReservationById(int reservationId, int memberId)
        {
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("UPDATE reservations SET status='annullée' WHERE id=@rid AND member_id=@mid AND status='active'", conn);
                cmd.Parameters.AddWithValue("@rid", reservationId);
                cmd.Parameters.AddWithValue("@mid", memberId);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // Méthode permettant de retourner l'ensemble des reservations d'un membre
        public List<Reservation> GetMemberReservations(int memberId)
        {
            List<Reservation> list = new List<Reservation>();
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                string sql = @"
                    SELECT r.id, r.course_id, r.member_id, r.reserved_at, r.status,
                           c.name as course_name, c.schedule_date
                    FROM reservations r
                    JOIN courses c ON r.course_id = c.id
                    WHERE r.member_id = @mid
                    ORDER BY c.schedule_date DESC";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@mid", memberId);
                using (MySqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        list.Add(new Reservation
                        {
                            Id = rdr.GetInt32("id"),
                            CourseId = rdr.GetInt32("course_id"),
                            MemberId = rdr.GetInt32("member_id"),
                            ReservedAt = rdr.GetDateTime("reserved_at"),
                            Status = rdr.GetString("status"),
                            CourseName = rdr.GetString("course_name"),
                            CourseSchedule = rdr.GetDateTime("schedule_date")
                        });
                    }
                }
            }
            return list;
        }

        // PARTIE EVALUATIVES


        // Méthode permettant de retourner l'ensemble des cours pleins
        public List<Course> GetFullCourses()
        {
            List<Course> list = new List<Course>();
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                string sql = @"
                    SELECT c.id, c.name, c.schedule_date, c.max_participants, c.coach_id
                    FROM courses c
                    WHERE (SELECT COUNT(*) FROM reservations r WHERE r.course_id = c.id AND r.status='active') >= c.max_participants";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                using (MySqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        list.Add(new Course
                        {
                            Id = rdr.GetInt32("id"),
                            Name = rdr.GetString("name"),
                            Schedule = rdr.GetDateTime("schedule_date"),
                            MaxParticipants = rdr.GetInt32("max_participants"),
                            CoachId = rdr.IsDBNull(rdr.GetOrdinal("coach_id")) ? 0 : rdr.GetInt32("coach_id")
                        });
                    }
                }
            }
            return list;
        }


        // Méthode permettant de retourner l'ensemble des membres ayant au moins 2 reservations dans la semaine qui viens
        public List<Member> GetMembersWithAtLeastTwoNextWeek()
        {
            List<Member> list = new List<Member>();
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                string sql = @"
                    SELECT m.id, m.user_id, m.first_name, m.last_name, m.email, m.status
                    FROM members m
                    WHERE (
                      SELECT COUNT(*)
                      FROM reservations r
                      JOIN courses c ON r.course_id = c.id
                      WHERE r.member_id = m.id
                        AND r.status='active'
                        AND c.schedule_date BETWEEN CURRENT_DATE() AND DATE_ADD(CURRENT_DATE(), INTERVAL 7 DAY)
                    ) >= 2";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                using (MySqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        list.Add(new Member
                        {
                            Id = rdr.GetInt32("id"),
                            UserId = rdr.GetInt32("user_id"),
                            FirstName = rdr.GetString("first_name"),
                            LastName = rdr.GetString("last_name"),
                            Email = rdr.GetString("email"),
                            Status = rdr.GetString("status")
                        });
                    }
                }
            }
            return list;
        }

        // Méthode permettant de retourner une sorte de panorama sur l'ensemble de la base de données
        // liant cours/membres/coachs/salles/reservations
        public List<Panorama> GetPanorama()
        {
            List<Panorama> list = new List<Panorama>();

            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                string sql = @"
            SELECT 
                m.first_name,
                m.last_name,
                c.name AS course_name,
                c.schedule_date,
                co.last_name AS coach_name,
                r.name AS room_name,
                r.capacity,
                c.max_participants
            FROM reservations res
            JOIN members m ON res.member_id = m.id
            JOIN courses c ON res.course_id = c.id
            JOIN coaches co ON c.coach_id = co.id
            JOIN rooms r ON c.room_id = r.id
            WHERE res.status = 'active'
            ORDER BY c.schedule_date ASC";

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                using (MySqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        list.Add(new Panorama
                        {
                            MemberFirstName = rdr.GetString("first_name"),
                            MemberLastName = rdr.GetString("last_name"),
                            CourseName = rdr.GetString("course_name"),
                            Schedule = rdr.GetDateTime("schedule_date"),
                            CoachLastName = rdr.GetString("coach_name"),
                            RoomName = rdr.GetString("room_name"),
                            RoomCapacity = rdr.GetInt32("capacity"),
                            MaxParticipants = rdr.GetInt32("max_participants")
                        });
                    }
                }
            }
            return list;
        }

        // Méthode permettant de retouner une liste couplant reservation et membre
        // Elle renvoie les réservations actives avec membres et cours
        public List<(Reservation res, Member mem)> GetReservationsWithMemberAndCourse()
        {
            List<(Reservation, Member)> list = new List<(Reservation, Member)>();
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                string sql = @"
                    SELECT r.id as rid, r.reserved_at, r.status, r.member_id, r.course_id,
                           m.id as mid, m.first_name, m.last_name, m.email,
                           c.id as cid, c.name as cname, c.schedule_date
                    FROM reservations r
                    JOIN members m ON r.member_id = m.id
                    JOIN courses c ON r.course_id = c.id
                    WHERE r.status='active'";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                using (MySqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        Reservation res = new Reservation
                        {
                            Id = rdr.GetInt32("rid"),
                            MemberId = rdr.GetInt32("member_id"),
                            CourseId = rdr.GetInt32("course_id"),
                            ReservedAt = rdr.GetDateTime("reserved_at"),
                            Status = rdr.GetString("status"),
                            CourseName = rdr.GetString("cname"),
                            CourseSchedule = rdr.GetDateTime("schedule_date")
                        };
                        Member mem = new Member
                        {
                            Id = rdr.GetInt32("mid"),
                            FirstName = rdr.GetString("first_name"),
                            LastName = rdr.GetString("last_name"),
                            Email = rdr.GetString("email")
                        };
                        list.Add((res, mem));
                    }
                }
            }
            return list;
        }


        // Méthode qui permet de renvoyer une liste couplant cours/coachs
        // Elle renvoie les cours avec leurs coachs 
        public List<(Course course, Coach coach)> GetCoursesWithCoach()
        {
            List<(Course, Coach)> list = new List<(Course, Coach)>();
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                string sql = @"
                    SELECT c.id as cid, c.name as cname, c.schedule_date, c.max_participants, c.coach_id,
                           co.id as coid, co.first_name, co.last_name, co.specialties
                    FROM courses c
                    JOIN coaches co ON c.coach_id = co.id";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                using (MySqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        Course course = new Course
                        {
                            Id = rdr.GetInt32("cid"),
                            Name = rdr.GetString("cname"),
                            Schedule = rdr.GetDateTime("schedule_date"),
                            MaxParticipants = rdr.GetInt32("max_participants"),
                            CoachId = rdr.IsDBNull(rdr.GetOrdinal("coach_id")) ? 0 : rdr.GetInt32("coach_id")
                        };
                        Coach coach = new Coach
                        {
                            Id = rdr.GetInt32("coid"),
                            FirstName = rdr.GetString("first_name"),
                            LastName = rdr.GetString("last_name"),
                            Specialties = rdr.IsDBNull(rdr.GetOrdinal("specialties")) ? "" : rdr.GetString("specialties")
                        };
                        list.Add((course, coach));
                    }
                }
            }
            return list;
        }


        // Méthode regroupant toutes les fonctions d'aggregation ( count - sum - avg - min - max -
        // count distinct (pas vraiment une fonction d'aggregation à part entière mais reste une bonne addition)
        // Cette méthode permet en soit d'avoir un rapport statistique de la base
        public StatsReport GetAggregatedStats()
        {
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                string sql = @"
                    SELECT
                      COUNT(r.id) AS total_reservations,
                      IFNULL(SUM(c.duration),0) AS total_minutes_of_courses,
                      IFNULL(AVG(c.max_participants),0) AS avg_capacity,
                      IFNULL(MIN(c.duration),0) AS min_duration,
                      IFNULL(MAX(c.duration),0) AS max_duration,
                      IFNULL(COUNT(DISTINCT r.member_id),0) AS distinct_members_reserved
                    FROM reservations r
                    JOIN courses c ON r.course_id = c.id";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                using (MySqlDataReader rdr = cmd.ExecuteReader())
                {
                    if (rdr.Read())
                    {
                        return new StatsReport
                        {
                            TotalReservations = rdr.IsDBNull(0) ? 0 : rdr.GetInt32("total_reservations"),
                            TotalMinutesOfCourses = rdr.IsDBNull(1) ? 0 : rdr.GetInt32("total_minutes_of_courses"),
                            AvgCapacity = rdr.IsDBNull(2) ? 0 : rdr.GetDecimal("avg_capacity"),
                            MinDuration = rdr.IsDBNull(3) ? 0 : rdr.GetInt32("min_duration"),
                            MaxDuration = rdr.IsDBNull(4) ? 0 : rdr.GetInt32("max_duration"),
                            DistinctMembersReserved = rdr.IsDBNull(5) ? 0 : rdr.GetInt32("distinct_members_reserved")
                        };
                    }
                }
            }
            return new StatsReport();
        }


        // Méthode permettant de renvoyer le nombre de reservations par cours
        public List<(string courseName, int reserved)> GetCourseReservationCounts()
        {
            List<(string, int)> list = new List<(string, int)>();
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                string sql = @"SELECT c.name, COUNT(r.id) as nb FROM courses c LEFT JOIN reservations r ON r.course_id = c.id AND r.status='active' GROUP BY c.id";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                using (MySqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        list.Add((rdr.GetString("name"), rdr.GetInt32("nb")));
                    }
                }
            }
            return list;
        }

    
    }
}