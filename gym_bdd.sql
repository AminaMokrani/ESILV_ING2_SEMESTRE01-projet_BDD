
DROP DATABASE IF EXISTS gymdb;
CREATE DATABASE gymdb;
USE gymdb;

-- ICI UNE FONCTION N'A PAS ETE UTILISEE EN COURS MAIS S'EST AVEREE IDEALE DANS NOTRE CAS
-- voici sa description 
-- Au niveau des id vous allez remarquer la fonction "INT AUTO_INCREMENT"
-- AUTO_INCREMENT est une propriété MySQL qui génère automatiquement
-- une valeur numérique unique et croissante pour chaque nouvel enregistrement.
-- Cela est utile dans notre cas afin d'éviter de chercher l'ID MAX a chaque fois



-- CREATION DES TABLES
CREATE TABLE users (
  id INT AUTO_INCREMENT PRIMARY KEY, 
  username VARCHAR(50) NOT NULL UNIQUE,
  password VARCHAR(100) NOT NULL,
  role ENUM('member','admin_principale','admin_secondaire') NOT NULL DEFAULT 'member',
  created_at DATETIME DEFAULT CURRENT_TIMESTAMP
);


CREATE TABLE members (
  id INT AUTO_INCREMENT PRIMARY KEY,
  user_id INT NOT NULL UNIQUE,
  first_name VARCHAR(50) NOT NULL,
  last_name VARCHAR(50) NOT NULL,
  email VARCHAR(100) NOT NULL UNIQUE,
  status ENUM('en attente','validée','annullée') DEFAULT 'en attente',
  FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE -- Ca signifie que si on supprime un utilisateur on supprime aussi le membre associé
);


CREATE TABLE coaches (
  id INT AUTO_INCREMENT PRIMARY KEY,
  first_name VARCHAR(50) NOT NULL,
  last_name VARCHAR(50) NOT NULL,
  specialties VARCHAR(255),
  phone VARCHAR(10),
  email VARCHAR(100)
);


CREATE TABLE rooms (
  id INT AUTO_INCREMENT PRIMARY KEY,
  name VARCHAR(50) NOT NULL UNIQUE,
  capacity INT NOT NULL CHECK (capacity > 0) -- Une contrainte : capacité supérieure à 0
);


CREATE TABLE courses (
  id INT AUTO_INCREMENT PRIMARY KEY,
  name VARCHAR(100) NOT NULL,
  description TEXT, -- UN TRES LONG VARCHAR FAIT POUR L'INDEXATION ET PERMET UN PERFORMANCE PLUS OPTIMALE QUE VARCHAR(10000) PAR EXEMPLE
  duration INT NOT NULL,
  coach_id INT,
  room_id INT,
  max_participants INT NOT NULL,
  schedule_date DATETIME NOT NULL,
  FOREIGN KEY (coach_id) REFERENCES coaches(id) ON DELETE SET NULL,
  FOREIGN KEY (room_id) REFERENCES rooms(id) ON DELETE SET NULL
);


CREATE TABLE reservations (
  id INT AUTO_INCREMENT PRIMARY KEY,
  course_id INT NOT NULL,
  member_id INT NOT NULL,
  reserved_at DATETIME DEFAULT CURRENT_TIMESTAMP,
  status ENUM('active','annullée') DEFAULT 'active',
  UNIQUE (course_id, member_id), -- ON DOIT AVEC UN UNIQUE COUPLE COURSE/MEMBRE. UN MEMBRE NE PEUT RESERVER AINSI QU'UNE SEULE FOIS A UNE MEME COURS
  FOREIGN KEY (course_id) REFERENCES courses(id) ON DELETE CASCADE,
  FOREIGN KEY (member_id) REFERENCES members(id) ON DELETE CASCADE
);

-- ETABLISSEMENT DES CONTRAINTES
ALTER TABLE courses ADD CONSTRAINT chk_duration_range CHECK (duration BETWEEN 10 AND 480);
ALTER TABLE members ADD CONSTRAINT chk_email_at CHECK (email LIKE '%@%');

-- CREATION DES UTILISATEURS
CREATE USER IF NOT EXISTS 'app_main'@'localhost' IDENTIFIED BY 'app_main_pwd';
GRANT ALL PRIVILEGES ON gymdb.* TO 'app_main'@'localhost';

CREATE USER IF NOT EXISTS 'app_secondary'@'localhost' IDENTIFIED BY 'app_secondary_pwd';
GRANT SELECT, INSERT, UPDATE, DELETE ON gymdb.* TO 'app_secondary'@'localhost';

CREATE USER IF NOT EXISTS 'report_user'@'localhost' IDENTIFIED BY 'report_pwd';
GRANT SELECT ON gymdb.* TO 'report_user'@'localhost';

FLUSH PRIVILEGES;

-- PEUPLEMENT DE LA BASE
INSERT INTO users (username, password, role) VALUES
('adminP', 'P123', 'admin_principale'),
('adminS', 'S123', 'admin_secondaire');


INSERT INTO rooms (name, capacity) VALUES
('Salle A', 2), -- Salle volontairement petite pour la remplir rapidement afin d'avoir un cas de salle pleine
('Salle B', 20),
('Salle C', 15);


INSERT INTO coaches (first_name, last_name, specialties, phone, email) VALUES
('Alice','Durand','Yoga','0612345678','alice@yahoo.com'),
('Marc','Petit','Musculation','0612345677','marc@gmail.com'),
('Sophie','Leroy','Zumba','0612345633','sophie@yahoo.com'),
('Youssef','Ben','HIIT','0662545078','youssef@yahoo.fr'),
('Emma','Legrand','Pilates','0631329485','emma@gmail.fr');


INSERT INTO courses (name, description, duration, coach_id, room_id, max_participants, schedule_date) VALUES
('Yoga Matin','Yoga doux',60,1,1,2, DATE_ADD(NOW(), INTERVAL 2 DAY)),
('Cross Training','HIIT',45,4,2,15, DATE_ADD(NOW(), INTERVAL 3 DAY)),
('Zumba Fun','Danse cardio',50,3,2,20, DATE_ADD(NOW(), INTERVAL 4 DAY)),
('Pilates','Renforcement',55,5,3,15, DATE_ADD(NOW(), INTERVAL 5 DAY));


INSERT INTO users (username, password, role) VALUES
('membre1','mdp1','member'),
('membre2','mdp2','member'),
('membre3','mdp3','member');

INSERT INTO members (user_id, first_name, last_name, email, status) VALUES
(3,'Jean','Martin','jean@gmail.com','validée'),
(4,'Sara','Dupont','sara@mail.com','validée'),
(5,'Simon','Dupond','simon@yahoo.com','en attente');


INSERT INTO reservations (course_id, member_id) VALUES
(1,1),
(1,2), -- Salle A maintenant pleine
(2,1),
(3,2),
(4,2);
