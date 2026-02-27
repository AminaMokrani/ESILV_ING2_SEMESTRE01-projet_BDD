
using System;


// Code avec toutes les classes

namespace GymAppConsole.Models
{
    public class User
    {
        int id;
        public int Id { get { return id; } set {id = value; } }

        string username;
        public string Username { get { return username; } set { username = value; } }

        string role;
        public string Role { get { return role; } set { role = value; } }
    }

    public class Member
    {
        int id;
        public int Id { get { return id; } set { id = value; } }

        int userId;
        public int UserId { get { return userId; } set { userId = value; } }
        string firstName;
        public string FirstName { get { return firstName; } set { firstName = value; } }

        string lastName;
        public string LastName { get { return lastName; } set { lastName = value; } }

        string email;
        public string Email { get { return email; } set { email = value; } }
        string status;
        public string Status { get { return status; } set { status = value; } }
    }

    public class Coach
    {
        int id;
        public int Id { get { return id; } set { id = value; } }
        string firstName;
        public string FirstName { get { return firstName; } set { firstName = value; } }

        string lastName;
        public string LastName { get { return lastName; } set { lastName = value; } }
        string specialities;
        public string Specialties { get { return specialities; } set { specialities = value; } }

        string phone;
        public string Phone { get { return phone; } set { phone= value; } }

        string email;
        public string Email { get { return email; } set { email = value; } }
    }

    public class Course
    {
        int id;
        public int Id { get { return id; } set { id = value; } }
        string name;
        public string Name { get { return name; } set { name = value; } }

        DateTime schedule;
        public DateTime Schedule { get { return schedule; } set { schedule = value; } }
        int maxParticipants;
        public int MaxParticipants { get { return maxParticipants; } set { maxParticipants= value; } }

        int currentParticipants;
        public int CurrentParticipants { get { return currentParticipants; } set { currentParticipants = value; } }

        int coachId;
        public int CoachId { get { return coachId; } set { coachId = value; } }

        int roomId;
        public int RoomId { get { return roomId; } set { roomId = value; } }
    }

    public class Reservation
    {
        int id;
        public int Id { get { return id; } set { id = value; } }

        int courseId;
        public int CourseId { get { return courseId; } set { courseId = value; } }

        int memberId;
        public int MemberId { get { return memberId; } set { memberId = value; } }

        DateTime reserveAt;
        public DateTime ReservedAt { get { return reserveAt; } set { reserveAt = value; } }

        string status;
        public string Status { get { return status; } set { status = value; } }

        string courseName;
        public string CourseName { get { return courseName; } set { courseName = value; } }

        DateTime courseSchedule;
        public DateTime CourseSchedule { get { return courseSchedule; } set { courseSchedule = value; } }
    }


    // Classe créées dans le cadre du programme

    // classe permettant de regrouper l'ensemble des doonées issues de d'aggrégation
    public class StatsReport
    {
        int totalReservation;
        public int TotalReservations { get { return totalReservation; } set { totalReservation = value; } }

        int totalMinutesOfCourses;
        public int TotalMinutesOfCourses { get { return totalMinutesOfCourses; } set { totalMinutesOfCourses = value; } }

        decimal avgCapacity;
        public decimal AvgCapacity { get { return avgCapacity; } set { avgCapacity = value; } }

        int minDuration;
        public int MinDuration { get { return minDuration; } set { minDuration = value; } }
        int maxDuration;
        public int MaxDuration { get { return maxDuration; } set { maxDuration = value; } }

        int distinctMembersReserved;
        public int DistinctMembersReserved { get { return distinctMembersReserved; } set { distinctMembersReserved = value; } }
    }


    // Classe panorama permettant de regrouper l'ensemble des informations associant membre + cours + classe + coach
    public class Panorama
    {
        string memberFirstName;

        public string MemberFirstName { get { return memberFirstName; } set { memberFirstName= value; } }
    
        string memberLastName;
        public string MemberLastName { get { return memberLastName; } set { memberLastName = value; } }

        string courseName;
        public string CourseName { get { return courseName; } set { courseName = value; } }

        DateTime schedule;
        public DateTime Schedule { get { return schedule; } set { schedule = value; } }

        string coachLastName;
        public string CoachLastName { get { return coachLastName; } set { coachLastName = value; } }

        string roomName;
        public string RoomName { get { return roomName; } set { roomName = value; } }

        int roomCapacity;
        public int RoomCapacity { get { return roomCapacity; } set { roomCapacity = value; } }
        int maxParticipants;
        public int MaxParticipants { get { return maxParticipants; } set { maxParticipants = value; } }
    }
}
