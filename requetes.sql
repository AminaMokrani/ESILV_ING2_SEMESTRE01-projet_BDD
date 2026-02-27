-- 	Récapitulatif des requetes SQL utilisées
-- Cela n'englobe que les requêtes sql exécutable

USE gymdb;

-- Liste des membres en attente de validation
SELECT id, first_name, last_name 
FROM members 
WHERE status = 'en attente';

-- Liste de tous les membres
SELECT id, first_name, last_name, status 
FROM members 
ORDER BY id ASC;

-- Liste de tous les coachs
SELECT id, first_name, last_name, specialties, phone, email 
FROM coaches;

-- Liste de tous les cours
SELECT c.id, c.name,  c.description, c.duration, c.max_participants,  c.schedule_date, c.coach_id, c.room_id,COUNT(r.id) AS current_participants
FROM courses c
 LEFT JOIN reservations r ON c.id = r.course_id AND r.status = 'active'
GROUP BY c.id, c.name, c.description, c.duration, c.max_participants, c.schedule_date, c.coach_id, c.room_id
ORDER BY c.schedule_date;

-- Coachs avec leurs cours
SELECT co.id as coid, co.first_name, co.last_name, co.specialties, co.phone, co.email,
c.id as cid, c.name as cname, c.schedule_date, c.max_participants, c.coach_id
FROM coaches co
LEFT JOIN courses c ON c.coach_id = co.id;

-- Salles avec leurs cours
SELECT r.id as room_id, r.name as room_name, 
       c.id as course_id, c.name as course_name, 
       c.schedule_date, c.max_participants, c.coach_id
FROM rooms r
RIGHT JOIN courses c ON c.room_id = r.id;


-- Cours complets 
SELECT c.id, c.name, c.schedule_date, c.max_participants, c.coach_id
FROM courses c
WHERE (SELECT COUNT(*) 
FROM reservations r 
WHERE r.course_id = c.id 
AND r.status = 'active') >= c.max_participants;

-- Membres avec au moins 2 réservations actives dans la semaine
SELECT m.id, m.user_id, m.first_name, m.last_name, m.email, m.status
FROM members m
WHERE (
    SELECT COUNT(*)
    FROM reservations r
    JOIN courses c ON r.course_id = c.id
    WHERE r.member_id = m.id
      AND r.status = 'active'
      AND c.schedule_date BETWEEN CURRENT_DATE() AND DATE_ADD(CURRENT_DATE(), INTERVAL 7 DAY)
) >= 2;

-- Grand Panorama ( la requete ensembliste)
SELECT 
    m.first_name AS 'Prénom Membre',
    m.last_name AS 'Nom Membre',
    c.name AS 'Nom du Cours',
    c.schedule_date AS 'Date/Heure',
    co.last_name AS 'Coach',
    r.name AS 'Salle',
    r.capacity AS 'Capacité Salle',
    c.max_participants AS 'Max Participants'
FROM reservations res
JOIN members m ON res.member_id = m.id
JOIN courses c ON res.course_id = c.id
JOIN coaches co ON c.coach_id = co.id
JOIN rooms r ON c.room_id = r.id
WHERE res.status = 'active'
ORDER BY c.schedule_date ASC;

-- Réservations actives avec membres et cours
SELECT r.id as rid, r.reserved_at,
 r.status, r.member_id, r.course_id,
       m.id as mid, m.first_name, m.last_name, m.email,
       c.id as cid, c.name as cname, c.schedule_date
FROM reservations r
JOIN members m ON r.member_id = m.id
JOIN courses c ON r.course_id = c.id
WHERE r.status = 'active';

-- Cours avec leurs coachs 
SELECT c.id as cid, c.name as cname,
 c.schedule_date, c.max_participants, c.coach_id,
       co.id as coid, co.first_name, co.last_name, co.specialties
FROM courses c
JOIN coaches co ON c.coach_id = co.id;

-- Aggregation
SELECT
    COUNT(r.id) AS total_reservations,
    IFNULL(SUM(c.duration), 0) AS total_minutes_of_courses,
    IFNULL(AVG(c.max_participants), 0) AS avg_capacity,
    IFNULL(MIN(c.duration), 0) AS min_duration,
    IFNULL(MAX(c.duration), 0) AS max_duration,
    IFNULL(COUNT(DISTINCT r.member_id), 0) AS distinct_members_reserved
FROM reservations r
JOIN courses c ON r.course_id = c.id;

-- Nombre de réservations par cours 
SELECT c.name, COUNT(r.id) as nb 
FROM courses c 
LEFT JOIN reservations r ON r.course_id = c.id AND r.status = 'active' 
GROUP BY c.id;
