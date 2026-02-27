
using System;
using GymAppConsole.Models;
using System.Collections.Generic;

namespace GymAppConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            DataAccess db = new DataAccess();


            while (true)
            {
                Console.Clear();
                Console.WriteLine();
                Console.WriteLine("Veuillez Lire de README avant d'exécuter le programme");
                Console.WriteLine("Menu principal:");
                Console.WriteLine("1) S'inscrire (membre seulement)");
                Console.WriteLine("2) Se connecter");
                Console.WriteLine("0) Quitter");
                Console.Write("Choix: ");
                string c = Console.ReadLine();
                if (c == "0") break;
                if (c == "1") Register(db);
                else if (c == "2") Login(db);
                else
                {
                    Console.WriteLine("Choix invalide."); Console.ReadKey();
                    Console.WriteLine("Appyuez sur une touche pour continuer");
                }

            } 
        }

        static void Register(DataAccess db)
        {
           
                Console.Clear();
                Console.WriteLine("\tInscription");
                Console.Write("Nom d'utilisateur: "); string username = Console.ReadLine();
                Console.Write("Mot de passe: "); string pwd = Console.ReadLine();
                Console.Write("Prénom: "); string fn = Console.ReadLine();
                Console.Write("Nom: "); string ln = Console.ReadLine();
                Console.Write("Email: "); string email = Console.ReadLine();

                int mid = db.RegisterUserAndMember(username, pwd, fn, ln, email);
                Console.WriteLine($"Inscription enregistrée (id membre {mid}). En attente de validation par administrateur.");
                
                Console.WriteLine("Appyuez sur une touche pour continuer");
                Console.ReadKey();


        }

        static void Login(DataAccess db)
        {
            
                Console.Clear();
                Console.WriteLine("\tConnexion");
                Console.Write("Nom d'utilisateur: "); string u = Console.ReadLine();
                Console.Write("Mot de passe: "); string p = Console.ReadLine();
                User user = db.Login(u, p);
                if (user == null) { 
                Console.WriteLine("Identifiants invalides.");
                Console.WriteLine("Appyuez sur une touche pour continuer");
                Console.ReadKey();
                return; }
                Console.WriteLine($"Connecté: {user.Username} (rôle: {user.Role})");
                Console.WriteLine("Appyuez sur une touche pour continuer");
                Console.ReadKey();
               


            if (user.Role == "member") MemberMenu(db, user);
                else if (user.Role == "admin_principale") AdminMenu(db, user, true);
                else if (user.Role == "admin_secondaire") AdminMenu(db, user, false);
                else
                {
                    Console.WriteLine("Rôle inconnu.");
       
                }
            Console.WriteLine("Appyuez sur une touche pour continuer");
            Console.ReadKey();
                



        }

        
        static void MemberMenu(DataAccess db, User user)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine();
                Console.WriteLine("\tMenu membre");
                Console.WriteLine("1) Voir les cours");
                Console.WriteLine("2) Réserver un cours");
                Console.WriteLine("3) Annuler une réservation");
                Console.WriteLine("4) Historique des inscriptions et gestion");
                Console.WriteLine("5) Voir mon profil");
                Console.WriteLine("0) Déconnexion");
                Console.Write("Choix: ");
                string ch = Console.ReadLine();
                if (ch == "0") break;

                
                    if (ch == "1")
                    {
                        List<Course> courses = db.ListCourses();
                        Console.WriteLine("Cours disponibles :");
                        foreach (Course c in courses) Console.WriteLine($"{c.Id} : {c.Name} - {c.Schedule} - capacité :{c.CurrentParticipants}/{c.MaxParticipants}");
                    }
                    else if (ch == "2")
                    {
                        Console.Write("Ton ID membre: "); int mid = int.Parse(Console.ReadLine());
                        Console.Write("ID du cours: "); int cid = int.Parse(Console.ReadLine());
                        bool ok = db.ReserveCourse(mid, cid);
                        Console.WriteLine(ok ? "Réservation effectuée." : "Impossible de réserver (plein).");
                    }
                    else if (ch == "3")
                    {
                        Console.Write("Ton ID membre: "); int mid = int.Parse(Console.ReadLine());
                        Console.Write("ID du cours: "); int cid = int.Parse(Console.ReadLine());
                        bool ok = db.CancelReservation(mid, cid);
                        Console.WriteLine(ok ? "Réservation annulée." : "Aucune réservation active trouvée.");
                    }
                    else if (ch == "4")
                    {
                        Console.Write("Ton ID membre: "); int mid = int.Parse(Console.ReadLine());
                        List<Reservation> list = db.GetMemberReservations(mid);
                        Console.WriteLine("Historique des inscriptions :");
                        foreach (Reservation r in list)
                        {
                            Console.WriteLine($"ID de reservation:{r.Id} - Cours:{r.CourseName} - Date cours:{r.CourseSchedule} - Statut:{r.Status} - Réservé le:{r.ReservedAt}");
                        }
                        Console.WriteLine("Souhaitez-vous annuler une inscription ? (o/n)");
                        string ans = Console.ReadLine();
                    do
                    {
                        if (ans == "o")
                        {
                            Console.Write("Entrez ID de réservation à annuler: "); int rid = int.Parse(Console.ReadLine());
                            bool ok = db.CancelReservationById(rid, mid);
                            Console.WriteLine(ok ? "Annulation effectuée." : "Échec annulation.");
                        }
                    }while(ans != "o" && ans !="n");

                    }
                    else if (ch == "5")
                    {
                        Member m = db.GetUserInfo(user.Id);
                        if (m == null)
                        {
                            Console.WriteLine("Profil introuvable.");
                        }
                        else
                        {
                            Console.WriteLine("\nmon Profil");
                            Console.WriteLine($"ID Membre : {m.Id}");
                            Console.WriteLine($"Nom : {m.FirstName} {m.LastName}");
                            Console.WriteLine($"Email: {m.Email}");
                            Console.WriteLine($"Statut : {m.Status}");

                            
                        }
                    }
                    else Console.WriteLine("Choix invalide.");
       

                
                Console.WriteLine("Appyuez sur une touche pour continuer");
                Console.ReadKey();
            }
        }

       
        static void AdminMenu(DataAccess db, User user, bool isPrimary)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine();
                Console.WriteLine("\tMenu admin");
                Console.WriteLine("1) Gestion des membres");
                Console.WriteLine("2) Gestion des coachs");
                Console.WriteLine("3) Gestion des cours");
                Console.WriteLine("4) Interface évaluative (admin principal seulement)");
                Console.WriteLine("0) Déconnexion");
                Console.Write("Choix: ");
                string ch = Console.ReadLine();
                if (ch == "0") break;

                if (ch == "1") GestionMembres(db, isPrimary);
                else if (ch == "2") GestionCoachs(db);
                else if (ch == "3") GestionCours(db);
                else if (ch == "4")
                {
                    if (!isPrimary) Console.WriteLine("Accès refusé (réservé à l'admin principal).");
                    else InterfaceEvaluative(db);
                }
                else Console.WriteLine("Choix invalide.");
                
                Console.WriteLine("Appyuez sur une touche pour continuer");
                Console.ReadKey();
            }
        }

        static void GestionMembres(DataAccess db, bool isPrimary)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine();
                Console.WriteLine("\tGestion des membres");
                Console.WriteLine("1) Valider une inscription");
                Console.WriteLine("2) Supprimer une adhésion (admin principal seulement)");
                Console.WriteLine("3) Afficher infos d'un membre");
                Console.WriteLine("4) Modifier infos d'un membre");
                Console.WriteLine("5) Afficher tous les membres");
                Console.WriteLine("0) Retour");
                Console.Write("Choix: ");
                string ch = Console.ReadLine();
                if (ch == "0") break;

                
                    if (ch == "1")
                    {
                        Console.WriteLine("\nMembres en attente de validation :");
                        List<string> pending = db.GetPendingMembers();

                        if (pending.Count == 0)
                        {
                            Console.WriteLine("Aucun membre en attente.");
                        }
                        else
                        {
                            foreach (string info in pending)
                            {
                                Console.WriteLine(info);
                            }

                            Console.Write("\nID membre à valider (ou 0 pour annuler): ");
                            if (int.TryParse(Console.ReadLine(), out int mid) && mid != 0)
                            {
                                bool ok = db.ValidateMember(mid);
                                Console.WriteLine(ok ? "Membre validé." : "ID introuvable ou déjà validé.");
                            }
                        }
                    }
                    else if (ch == "2")
                    {
                        if (!isPrimary) { 
                        Console.WriteLine("Droit refusé.");
                        Console.WriteLine("Appyuez sur une touche pour continuer");
                        Console.ReadKey();
                        continue; 
                    }
                        Console.Write("ID membre à supprimer: "); int mid = int.Parse(Console.ReadLine());
                        bool ok = db.DeleteMemberById(mid);
                        Console.WriteLine(ok ? "Membre supprimé." : "Suppression échouée.");
                    }
                    else if (ch == "3")
                    {
                        Console.Write("ID membre: "); int mid = int.Parse(Console.ReadLine());
                        Member m = db.GetMemberInfo(mid);
                        if (m == null) Console.WriteLine("Membre introuvable.");
                        else Console.WriteLine($"ID:{m.Id} - Nom:{m.FirstName} {m.LastName} - Email:{m.Email} - Statut:{m.Status}");
                    }
                    else if (ch == "4")
                    {
                        Console.Write("ID membre à modifier: "); int mid = int.Parse(Console.ReadLine());
                        Console.Write("Prénom (vide = inchangé): "); string fn = Console.ReadLine();
                        Console.Write("Nom (vide = inchangé): "); string ln = Console.ReadLine();
                        Console.Write("Email (vide = inchangé): "); string em = Console.ReadLine();
                        bool ok = db.UpdateMember(mid, fn, ln, em);
                        Console.WriteLine(ok ? "Membre mis à jour." : "Mise à jour échouée ou rien à modifier.");
                    }
                    else if (ch == "5")
                    {
                        Console.WriteLine("\n Liste de tous les membres :");
                        List<string> allMembers = db.GetAllMembers();

                        if (allMembers.Count == 0)
                        {
                            Console.WriteLine("Aucun membre enregistré.");
                        }
                        else
                        {
                            foreach (string info in allMembers)
                            {
                                Console.WriteLine(info);
                            }
                            Console.WriteLine($"Total : {allMembers.Count} membre(s).");
                        }
                    }


                Console.WriteLine("Appyuez sur une touche pour continuer");
                Console.ReadKey();
            }
        }

        static void GestionCoachs(DataAccess db)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine();
                Console.WriteLine("\tGestion des coachs");
                Console.WriteLine("1) Enregistrer un coach");
                Console.WriteLine("2) Afficher la liste des coachs");
                Console.WriteLine("3) Afficher coachs et leurs cours");
                Console.WriteLine("0) Retour");
                Console.Write("Choix: ");
                string ch = Console.ReadLine();
                if (ch == "0") break;

                
                    if (ch == "1")
                    {
                        Console.Write("Prénom: "); string fn = Console.ReadLine();
                        Console.Write("Nom: "); string ln = Console.ReadLine();
                        Console.Write("Spécialités: "); string spec = Console.ReadLine();
                        Console.Write("Téléphone: "); string ph = Console.ReadLine();
                        Console.Write("Email: "); string em = Console.ReadLine();
                        int id = db.AddCoach(fn, ln, spec, ph, em);
                        Console.WriteLine($"Coach ajouté (id {id}).");
                    }
                    else if (ch == "2")
                    {
                        List<Coach> list = db.ListCoaches();
                        Console.WriteLine("Coachs:");
                        foreach (Coach co in list) Console.WriteLine($"{co.Id} - {co.FirstName} {co.LastName} - {co.Specialties} - {co.Phone} - {co.Email}");
                    }
                    else if (ch == "3")
                    {
                        List<(Coach coach, Course course)> list = db.ListCoachesWithCoursesLeftJoin();
                        Console.WriteLine("Coachs et (peut etre) leurs cours:");
                        foreach ((Coach coach, Course course) item in list)
                        {
                            Coach co = item.coach;
                            Course c = item.course;
                            Console.WriteLine($"{co.Id} - {co.FirstName} {co.LastName} : {(c == null ? "Aucun cours" : c.Name + " le " + c.Schedule)}");
                        }
                    }


                Console.WriteLine("Appyuez sur une touche pour continuer");
                Console.ReadKey();
            }
        }

        static void GestionCours(DataAccess db)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine();
                Console.WriteLine("\tGestion des cours");
                Console.WriteLine("1) Enregistrer un cours");
                Console.WriteLine("2) Afficher la liste des cours");
                Console.WriteLine("3) RIGHT JOIN - Lister rooms avec leurs cours ");
                Console.WriteLine("0) Retour");
                Console.Write("Choix: ");
                string ch = Console.ReadLine();
                if (ch == "0") break;

                
                    if (ch == "1")
                    {
                        Console.Write("Nom: "); string name = Console.ReadLine();
                        Console.Write("Description: "); string desc = Console.ReadLine();
                        Console.Write("Durée (minutes): "); int dur = int.Parse(Console.ReadLine());
                        Console.Write("CoachId: "); int coachId = int.Parse(Console.ReadLine());
                        Console.Write("RoomId: "); int roomId = int.Parse(Console.ReadLine());
                        Console.Write("Capacité max: "); int maxp = int.Parse(Console.ReadLine());
                        Console.Write("Date/heure (yyyy-MM-dd HH:mm): "); DateTime sched = DateTime.Parse(Console.ReadLine());
                        int id = db.AddCourse(name, desc, dur, coachId, roomId, maxp, sched);
                        Console.WriteLine($"Cours ajouté (id {id}).");
                    }
                    else if (ch == "2")
                    {
                        List<Course> list = db.ListCourses();
                        foreach (Course c in list) Console.WriteLine($"{c.Id} : {c.Name} - {c.Schedule} - Id coach:{c.CoachId} - id salle :{c.RoomId} - capacité : {c.CurrentParticipants}/{c.MaxParticipants}");
                    }
                    else if (ch == "3")
                    {
                        List<(int roomId, string roomName, Course course)> list = db.RoomsRightJoinCourses();
                       
                        foreach ((int roomId, string roomName, Course course) t in list)
                        {
                            Console.WriteLine($"Id salle:{t.roomId} nom:{t.roomName} -- Cours:{t.course.Name} ({t.course.Schedule})");
                        }
                    }

                Console.WriteLine("Appyuez sur une touche pour continuer");
                Console.ReadKey();
            }
        }

        static void InterfaceEvaluative(DataAccess db)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine();
                Console.WriteLine("\tInterface évaluative");
                Console.WriteLine("1) Cours pleins (sous-requête)");
                Console.WriteLine("2) Membres avec plus de 2 réservations la semaine prochaine ");
                Console.WriteLine("3) Ensembliste - Panorama ");
                Console.WriteLine("4) Jointures-réservations avec infos membre et cours");
                Console.WriteLine("5) Jointures-cours et coach");
                Console.WriteLine("6) LEFT JOIN-réservations par cours");
                Console.WriteLine("7) Agrégations");
                Console.WriteLine("0) Retour");
                Console.Write("Choix: ");
                string ch = Console.ReadLine();
                if (ch == "0") break;

                
                    if (ch == "1")
                    {
                        List<Course> full = db.GetFullCourses();
                        Console.WriteLine("Cours pleins:");
                        foreach (Course c in full) Console.WriteLine($"{c.Id} - {c.Name} - {c.Schedule}");
                    }
                    else if (ch == "2")
                    {
                        List<Member> mems = db.GetMembersWithAtLeastTwoNextWeek();
                        Console.WriteLine("Membres concernés:");
                        foreach (Member m in mems) Console.WriteLine($"{m.Id} - {m.FirstName} {m.LastName} - {m.Email}");
                    }
                    else if (ch == "3")
                    {
                        List<Panorama> liste = db.GetPanorama();
                        Console.WriteLine("panorama/ vue sur l'ensemble des données :");
                        foreach (Panorama e in liste) Console.WriteLine(e);
                    }
                    else if (ch == "4")
                    {
                        List<(Reservation res, Member mem)> list = db.GetReservationsWithMemberAndCourse();
                        Console.WriteLine("Réservations actives avec infos:");
                        foreach ((Reservation res, Member mem) t in list)
                        {
                            Reservation r = t.res;
                            Member m = t.mem;
                            Console.WriteLine($"Id réservation:{r.Id} - Membre:{m.FirstName} {m.LastName} - Cours:{r.CourseName} - Date:{r.CourseSchedule}");
                        }
                    }
                    else if (ch == "5")
                    {
                        List<(Course course, Coach coach)> list = db.GetCoursesWithCoach();
                        Console.WriteLine("Cours et coach:");
                        foreach ((Course course, Coach coach) t in list) Console.WriteLine($"{t.course.Id} - {t.course.Name} -- Coach: {t.coach.FirstName} {t.coach.LastName}");
                    }
                    else if (ch == "6")
                    {
                        List<(string courseName, int reserved)> counts = db.GetCourseReservationCounts();
                        Console.WriteLine("Nombre réservations par cours:");
                        foreach ((string courseName, int reserved) t in counts) Console.WriteLine($"{t.courseName} : {t.reserved} réservation(s)");
                    }
                    else if (ch == "7")
                    {
                        StatsReport s = db.GetAggregatedStats();
                        Console.WriteLine($"Total réservations:{s.TotalReservations} - Total minutes cours:{s.TotalMinutesOfCourses} \n Moyenne capacité(s):{s.AvgCapacity}  Min durée:{s.MinDuration} \n Max durée:{s.MaxDuration} - Membres distincts:{s.DistinctMembersReserved}");
                    }



                Console.WriteLine("Appyuez sur une touche pour continuer");
                Console.ReadKey();
            }
        }
    }
}