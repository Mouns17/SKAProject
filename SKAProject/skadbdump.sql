-- MySQL dump 10.13  Distrib 8.0.44, for Win64 (x86_64)
--
-- Host: localhost    Database: skadatabase
-- ------------------------------------------------------
-- Server version	9.5.0

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;
SET @MYSQLDUMP_TEMP_LOG_BIN = @@SESSION.SQL_LOG_BIN;
SET @@SESSION.SQL_LOG_BIN= 0;

--
-- GTID state at the beginning of the backup 
--

SET @@GLOBAL.GTID_PURGED=/*!80000 '+'*/ '59a285ee-d3a5-11f0-99ef-02502824d7e4:1-136';

--
-- Table structure for table `company_info`
--

DROP TABLE IF EXISTS `company_info`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `company_info` (
  `CompanyID` tinyint NOT NULL DEFAULT '1',
  `Address` varchar(255) DEFAULT '',
  `Phone` varchar(20) DEFAULT '',
  `Email` varchar(100) DEFAULT '',
  PRIMARY KEY (`CompanyID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `company_info`
--

LOCK TABLES `company_info` WRITE;
/*!40000 ALTER TABLE `company_info` DISABLE KEYS */;
/*!40000 ALTER TABLE `company_info` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `departments`
--

DROP TABLE IF EXISTS `departments`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `departments` (
  `DepID` int NOT NULL AUTO_INCREMENT,
  `DepName` varchar(100) NOT NULL,
  PRIMARY KEY (`DepID`),
  UNIQUE KEY `DepName` (`DepName`)
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `departments`
--

LOCK TABLES `departments` WRITE;
/*!40000 ALTER TABLE `departments` DISABLE KEYS */;
INSERT INTO `departments` VALUES (4,'HR'),(3,'IT-Отдел'),(5,'Отдел кадров'),(6,'Отдел руководства');
/*!40000 ALTER TABLE `departments` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `logs`
--

DROP TABLE IF EXISTS `logs`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `logs` (
  `LogID` int NOT NULL AUTO_INCREMENT,
  `UserID` int DEFAULT NULL,
  `Action` varchar(255) NOT NULL,
  `EventType` varchar(50) DEFAULT NULL,
  `Description` text,
  `CreatedAt` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`LogID`),
  KEY `UserID` (`UserID`),
  CONSTRAINT `logs_ibfk_1` FOREIGN KEY (`UserID`) REFERENCES `users` (`UserID`) ON DELETE SET NULL
) ENGINE=InnoDB AUTO_INCREMENT=45 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `logs`
--

LOCK TABLES `logs` WRITE;
/*!40000 ALTER TABLE `logs` DISABLE KEYS */;
INSERT INTO `logs` VALUES (1,NULL,'Тестовое событие','Система','Проверка отображения логов','2026-04-24 22:51:13'),(2,1,'Вход в систему','Авторизация','Успешный вход','2026-04-26 21:52:44'),(3,1,'Создание должности','Справочники','Должность: Менеджер','2026-04-26 21:53:43'),(4,9,'Регистрация нового пользователя','Пользователи','Логин: Sliper67','2026-04-26 22:11:22'),(5,9,'Вход в систему','Авторизация','Успешный вход','2026-04-26 22:11:29'),(6,10,'Регистрация нового пользователя','Пользователи','Логин: AdminLogin2026','2026-04-26 22:15:59'),(7,1,'Вход в систему','Авторизация','Успешный вход','2026-04-26 22:27:39'),(8,10,'Вход в систему','Авторизация','Успешный вход','2026-04-26 22:28:00'),(9,10,'Вход в систему','Авторизация','Успешный вход','2026-04-26 22:30:34'),(10,10,'Добавление отдела','Управление персоналом','Добавлен отдел: Отдел кадров','2026-04-26 22:32:39'),(11,10,'Добавление отдела','Управление персоналом','Добавлен отдел: Отдел руководства','2026-04-26 22:32:53'),(12,10,'Создание должности','Справочники','Должность: Руководитель','2026-04-26 22:33:43'),(13,10,'Создание должности','Справочники','Должность: Смотритель','2026-04-26 22:33:48'),(14,10,'Вход в систему','Авторизация','Успешный вход','2026-04-26 22:35:37'),(15,10,'Вход в систему','Авторизация','Успешный вход','2026-04-26 22:37:57'),(16,10,'Вход в систему','Авторизация','Успешный вход','2026-04-26 22:43:29'),(17,10,'Вход в систему','Авторизация','Успешный вход','2026-04-26 22:46:13'),(18,10,'Вход в систему','Авторизация','Успешный вход','2026-04-26 23:12:44'),(19,10,'Вход в систему','Авторизация','Успешный вход','2026-04-26 23:14:49'),(20,10,'Вход в систему','Авторизация','Успешный вход','2026-04-26 23:18:17'),(21,10,'Вход в систему','Авторизация','Успешный вход','2026-04-26 23:19:20'),(22,10,'Вход в систему','Авторизация','Успешный вход','2026-04-26 23:46:11'),(23,10,'Вход в систему','Авторизация','Успешный вход','2026-04-27 00:02:54'),(24,10,'Вход в систему','Авторизация','Успешный вход','2026-04-27 00:25:18'),(25,10,'Вход в систему','Авторизация','Успешный вход','2026-04-27 00:33:26'),(26,10,'Вход в систему','Авторизация','Успешный вход','2026-04-27 00:36:51'),(27,10,'Вход в систему','Авторизация','Успешный вход','2026-04-27 00:40:13'),(28,10,'Вход в систему','Авторизация','Успешный вход','2026-04-27 00:42:14'),(29,10,'Вход в систему','Авторизация','Успешный вход','2026-04-27 00:43:35'),(30,10,'Вход в систему','Авторизация','Успешный вход','2026-04-27 01:04:54'),(31,10,'Вход в систему','Авторизация','Успешный вход','2026-04-27 01:13:24'),(32,10,'Вход в систему','Авторизация','Успешный вход','2026-04-27 01:21:19'),(33,10,'Вход в систему','Авторизация','Успешный вход','2026-04-27 01:24:54'),(34,10,'Вход в систему','Авторизация','Успешный вход','2026-04-27 01:27:55'),(35,10,'Вход в систему','Авторизация','Успешный вход','2026-04-27 01:34:58'),(36,10,'Вход в систему','Авторизация','Успешный вход','2026-04-27 01:40:01'),(37,10,'Вход в систему','Авторизация','Успешный вход','2026-04-27 01:46:45'),(38,10,'Вход в систему','Авторизация','Успешный вход','2026-04-27 01:50:12'),(39,10,'Вход в систему','Авторизация','Успешный вход','2026-04-27 01:51:02'),(40,10,'Вход в систему','Авторизация','Успешный вход','2026-04-27 01:53:59'),(41,10,'Вход в систему','Авторизация','Успешный вход','2026-04-27 01:55:46'),(42,10,'Вход в систему','Авторизация','Успешный вход','2026-04-27 01:58:54'),(43,10,'Вход в систему','Авторизация','Успешный вход','2026-04-27 02:04:25'),(44,10,'Вход в систему','Авторизация','Успешный вход','2026-04-27 02:11:05');
/*!40000 ALTER TABLE `logs` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `positions`
--

DROP TABLE IF EXISTS `positions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `positions` (
  `PosID` int NOT NULL AUTO_INCREMENT,
  `PosName` varchar(100) NOT NULL,
  PRIMARY KEY (`PosID`),
  UNIQUE KEY `PosName` (`PosName`)
) ENGINE=InnoDB AUTO_INCREMENT=9 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `positions`
--

LOCK TABLES `positions` WRITE;
/*!40000 ALTER TABLE `positions` DISABLE KEYS */;
INSERT INTO `positions` VALUES (3,'Директор'),(5,'Заместитель директора'),(6,'Менеджер'),(7,'Руководитель'),(8,'Смотритель'),(4,'Уборщик');
/*!40000 ALTER TABLE `positions` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `reports`
--

DROP TABLE IF EXISTS `reports`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `reports` (
  `ReportID` int NOT NULL AUTO_INCREMENT,
  `AuthorID` int NOT NULL,
  `RecipientID` int DEFAULT NULL,
  `ReportDate` date NOT NULL,
  `Description` text,
  `Status` enum('Принято','На проверке','Отклонено') DEFAULT 'На проверке',
  `CreatedAt` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`ReportID`),
  KEY `AuthorID` (`AuthorID`),
  KEY `RecipientID` (`RecipientID`),
  CONSTRAINT `reports_ibfk_1` FOREIGN KEY (`AuthorID`) REFERENCES `users` (`UserID`) ON DELETE CASCADE,
  CONSTRAINT `reports_ibfk_2` FOREIGN KEY (`RecipientID`) REFERENCES `users` (`UserID`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `reports`
--

LOCK TABLES `reports` WRITE;
/*!40000 ALTER TABLE `reports` DISABLE KEYS */;
/*!40000 ALTER TABLE `reports` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tasks`
--

DROP TABLE IF EXISTS `tasks`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `tasks` (
  `TaskID` int NOT NULL AUTO_INCREMENT,
  `Title` varchar(200) NOT NULL,
  `Description` text,
  `CreatedBy` int NOT NULL,
  `AssignedTo` int DEFAULT NULL,
  `Status` enum('Новая','В работе','Завершена') DEFAULT 'Новая',
  `Priority` enum('Низкий','Средний','Высокий') DEFAULT 'Средний',
  `Deadline` date DEFAULT NULL,
  `CreatedAt` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`TaskID`),
  KEY `CreatedBy` (`CreatedBy`),
  KEY `AssignedTo` (`AssignedTo`),
  CONSTRAINT `tasks_ibfk_1` FOREIGN KEY (`CreatedBy`) REFERENCES `users` (`UserID`) ON DELETE CASCADE,
  CONSTRAINT `tasks_ibfk_2` FOREIGN KEY (`AssignedTo`) REFERENCES `users` (`UserID`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tasks`
--

LOCK TABLES `tasks` WRITE;
/*!40000 ALTER TABLE `tasks` DISABLE KEYS */;
/*!40000 ALTER TABLE `tasks` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `users`
--

DROP TABLE IF EXISTS `users`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `users` (
  `UserID` int NOT NULL AUTO_INCREMENT,
  `Login` varchar(50) NOT NULL,
  `Password` varchar(255) NOT NULL,
  `FirstName` varchar(50) NOT NULL,
  `LastName` varchar(50) NOT NULL,
  `MiddleName` varchar(50) DEFAULT '',
  `Phone` varchar(20) DEFAULT '',
  `Email` varchar(100) DEFAULT '',
  `CreatedAt` datetime DEFAULT CURRENT_TIMESTAMP,
  `Role` enum('Owner','Admin','User') DEFAULT 'User',
  PRIMARY KEY (`UserID`),
  UNIQUE KEY `Login` (`Login`)
) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `users`
--

LOCK TABLES `users` WRITE;
/*!40000 ALTER TABLE `users` DISABLE KEYS */;
INSERT INTO `users` VALUES (1,'Mouns18','12345','Владислав','Туманов','Николаевич','','nozerit777@gmail.com','2026-04-24 22:01:17','User'),(2,'MeowXMango67','Mango6776','Даниил','Кириенко','Иванович','','meowmango2008@gmail.com','2026-04-24 22:46:29','User'),(9,'Sliper67','676767','Иван','Слюзко','Романович','','','2026-04-26 22:11:22','User'),(10,'AdminLogin2026','Admin2026Password','ADMIN','','','','','2026-04-26 22:15:59','Admin');
/*!40000 ALTER TABLE `users` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `workers`
--

DROP TABLE IF EXISTS `workers`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `workers` (
  `WrkID` int NOT NULL AUTO_INCREMENT,
  `UserID` int NOT NULL,
  `DepID` int DEFAULT NULL,
  `PosID` int DEFAULT NULL,
  `Status` enum('Работает','Уволен','В отпуске') DEFAULT 'Работает',
  PRIMARY KEY (`WrkID`),
  UNIQUE KEY `UserID` (`UserID`),
  KEY `DepID` (`DepID`),
  KEY `PosID` (`PosID`),
  CONSTRAINT `workers_ibfk_1` FOREIGN KEY (`UserID`) REFERENCES `users` (`UserID`) ON DELETE CASCADE,
  CONSTRAINT `workers_ibfk_2` FOREIGN KEY (`DepID`) REFERENCES `departments` (`DepID`) ON DELETE SET NULL,
  CONSTRAINT `workers_ibfk_3` FOREIGN KEY (`PosID`) REFERENCES `positions` (`PosID`) ON DELETE SET NULL
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `workers`
--

LOCK TABLES `workers` WRITE;
/*!40000 ALTER TABLE `workers` DISABLE KEYS */;
INSERT INTO `workers` VALUES (1,1,3,3,'Работает'),(2,2,4,5,'В отпуске');
/*!40000 ALTER TABLE `workers` ENABLE KEYS */;
UNLOCK TABLES;
SET @@SESSION.SQL_LOG_BIN = @MYSQLDUMP_TEMP_LOG_BIN;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-05-01  3:22:45
