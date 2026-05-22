-- MySQL dump 10.13  Distrib 8.0.46, for Win64 (x86_64)
--
-- Host: 127.0.0.1    Database: market_erp
-- ------------------------------------------------------
-- Server version	8.0.46

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

--
-- Table structure for table `categories`
--

DROP TABLE IF EXISTS `categories`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `categories` (
  `id` int NOT NULL AUTO_INCREMENT,
  `name` varchar(100) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `categories`
--

LOCK TABLES `categories` WRITE;
/*!40000 ALTER TABLE `categories` DISABLE KEYS */;
INSERT INTO `categories` VALUES (1,'İçecek'),(2,'Temel Gıda'),(3,'Atıştırmalık'),(4,'Temizlik'),(5,'Kişisel Bakım'),(6,'Manav'),(7,'Süt Ürünleri'),(8,'Bakliyat'),(9,'Et ve Şarküteri'),(10,'Donuk Ürün');
/*!40000 ALTER TABLE `categories` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `customers`
--

DROP TABLE IF EXISTS `customers`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `customers` (
  `id` int NOT NULL AUTO_INCREMENT,
  `full_name` varchar(150) NOT NULL,
  `phone` varchar(20) DEFAULT NULL,
  `email` varchar(100) DEFAULT NULL,
  `address` text,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=101 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `customers`
--

LOCK TABLES `customers` WRITE;
/*!40000 ALTER TABLE `customers` DISABLE KEYS */;
INSERT INTO `customers` VALUES (1,'Selin Arslan','05515428914','musteri1@mail.com','Samsun'),(2,'Veli Öztürk','05581852264','musteri2@mail.com','Ankara'),(3,'Ece Öztürk','05311059486','musteri3@mail.com','Konya'),(4,'Zeynep Öztürk','05358412769','musteri4@mail.com','Samsun'),(5,'Ece Güneş','05302876932','musteri5@mail.com','Ankara'),(6,'Zeynep Güneş','05317194416','musteri6@mail.com','Adana'),(7,'Merve Çelik','05433138181','musteri7@mail.com','İstanbul'),(8,'Deniz Polat','05581669324','musteri8@mail.com','Konya'),(9,'Murat Aydın','05512724590','musteri9@mail.com','Konya'),(10,'Merve Aslan','05493592974','musteri10@mail.com','Kayseri'),(11,'Elif Yıldız','05587917152','musteri11@mail.com','İstanbul'),(12,'Elif Kurt','05557907400','musteri12@mail.com','Kayseri'),(13,'Can Yıldız','05552813543','musteri13@mail.com','Antalya'),(14,'Mehmet Aksoy','05374348276','musteri14@mail.com','Erzurum'),(15,'Burak Koç','05564818418','musteri15@mail.com','Kayseri'),(16,'Ahmet Arslan','05426507321','musteri16@mail.com','Bursa'),(17,'Ayşe Öztürk','05419546409','musteri17@mail.com','Antalya'),(18,'Merve Kurt','05302935113','musteri18@mail.com','Bursa'),(19,'Elif Taş','05381641849','musteri19@mail.com','Ankara'),(20,'Hakan Aslan','05416262632','musteri20@mail.com','Antalya'),(21,'Hakan Eren','05337462506','musteri21@mail.com','Adana'),(22,'Murat Öztürk','05318315831','musteri22@mail.com','İstanbul'),(23,'Seda Güneş','05514305700','musteri23@mail.com','Konya'),(24,'Ece Kaya','05516539837','musteri24@mail.com','Adana'),(25,'Selin Şahin','05536038571','musteri25@mail.com','Samsun'),(26,'Deniz Aslan','05407751158','musteri26@mail.com','Bursa'),(27,'Merve Çelik','05368053918','musteri27@mail.com','Antalya'),(28,'Elif Bozkurt','05486049101','musteri28@mail.com','Antalya'),(29,'Merve Yılmaz','05395813613','musteri29@mail.com','Kayseri'),(30,'Ece Taş','05496406444','musteri30@mail.com','Erzurum'),(31,'Ali Doğan','05514585314','musteri31@mail.com','Samsun'),(32,'Veli Yıldız','05512422633','musteri32@mail.com','Bursa'),(33,'Seda Bozkurt','05402566777','musteri33@mail.com','Kayseri'),(34,'Deniz Aydın','05554340845','musteri34@mail.com','İzmir'),(35,'Ahmet Demir','05378971465','musteri35@mail.com','Adana'),(36,'Ayşe Doğan','05434262081','musteri36@mail.com','Antalya'),(37,'Veli Kılıç','05373475836','musteri37@mail.com','İstanbul'),(38,'Fatma Aslan','05373950888','musteri38@mail.com','Samsun'),(39,'Ali Demir','05475180853','musteri39@mail.com','Ankara'),(40,'Ali Çelik','05558795419','musteri40@mail.com','Samsun'),(41,'Merve Bozkurt','05408425149','musteri41@mail.com','Adana'),(42,'Seda Aslan','05568481188','musteri42@mail.com','İzmir'),(43,'Veli Doğan','05385147994','musteri43@mail.com','Bursa'),(44,'Seda Aksoy','05505013879','musteri44@mail.com','Bursa'),(45,'Ali Kaya','05525793722','musteri45@mail.com','Kayseri'),(46,'Can Kurt','05402351868','musteri46@mail.com','İzmir'),(47,'Zeynep Aydın','05423563646','musteri47@mail.com','Kayseri'),(48,'Ayşe Aslan','05436551268','musteri48@mail.com','Samsun'),(49,'Ali Aslan','05314470105','musteri49@mail.com','Antalya'),(50,'Buse Taş','05521327691','musteri50@mail.com','Adana'),(51,'Buse Aksoy','05306901533','musteri51@mail.com','Bursa'),(52,'Buse Aslan','05474700025','musteri52@mail.com','Erzurum'),(53,'Emre Öztürk','05439147720','musteri53@mail.com','İstanbul'),(54,'Buse Kurt','05517783308','musteri54@mail.com','İzmir'),(55,'Ali Çelik','05499961107','musteri55@mail.com','İstanbul'),(56,'Buse Taş','05481454696','musteri56@mail.com','Ankara'),(57,'Ece Çelik','05578746014','musteri57@mail.com','İzmir'),(58,'Mehmet Öztürk','05426492066','musteri58@mail.com','Kayseri'),(59,'Ali Kurt','05407360682','musteri59@mail.com','Bursa'),(60,'Ece Öztürk','05562374158','musteri60@mail.com','Erzurum'),(61,'Ahmet Güneş','05316871360','musteri61@mail.com','Kayseri'),(62,'Ayşe Demir','05541520596','musteri62@mail.com','Kayseri'),(63,'Murat Yılmaz','05493556551','musteri63@mail.com','Kayseri'),(64,'Zeynep Aksoy','05512919136','musteri64@mail.com','Adana'),(65,'Murat Doğan','05525299055','musteri65@mail.com','Konya'),(66,'Elif Bozkurt','05492921542','musteri66@mail.com','İzmir'),(67,'Deniz Şahin','05481430812','musteri67@mail.com','Bursa'),(68,'Kerem Kılıç','05424327421','musteri68@mail.com','Ankara'),(69,'Kerem Aydın','05336059717','musteri69@mail.com','Adana'),(70,'Fatma Taş','05551689025','musteri70@mail.com','Konya'),(71,'Merve Aslan','05517217147','musteri71@mail.com','Ankara'),(72,'Seda Kurt','05308047416','musteri72@mail.com','Erzurum'),(73,'Fatma Aslan','05418713146','musteri73@mail.com','İzmir'),(74,'Ece Yıldız','05539753457','musteri74@mail.com','Bursa'),(75,'Hakan Güneş','05549111742','musteri75@mail.com','Erzurum'),(76,'Ece Taş','05386407373','musteri76@mail.com','Kayseri'),(77,'Ayşe Öztürk','05588563202','musteri77@mail.com','Kayseri'),(78,'Ali Taş','05497358385','musteri78@mail.com','Konya'),(79,'Ahmet Aksoy','05576452832','musteri79@mail.com','İzmir'),(80,'Veli Arslan','05415334434','musteri80@mail.com','Konya'),(81,'Can Bozkurt','05525635017','musteri81@mail.com','Samsun'),(82,'Ahmet Eren','05362436347','musteri82@mail.com','Kayseri'),(83,'Ece Aksoy','05475031966','musteri83@mail.com','Erzurum'),(84,'Veli Doğan','05551289293','musteri84@mail.com','Ankara'),(85,'Deniz Aydın','05425081983','musteri85@mail.com','Bursa'),(86,'Kerem Polat','05459907391','musteri86@mail.com','Konya'),(87,'Ece Güneş','05406902401','musteri87@mail.com','Erzurum'),(88,'Can Koç','05384867786','musteri88@mail.com','Ankara'),(89,'Murat Kurt','05339990305','musteri89@mail.com','İzmir'),(90,'Murat Arslan','05539123504','musteri90@mail.com','Bursa'),(91,'Kerem Eren','05495747930','musteri91@mail.com','Ankara'),(92,'Murat Koç','05377054579','musteri92@mail.com','İzmir'),(93,'Deniz Yılmaz','05529961299','musteri93@mail.com','İzmir'),(94,'Can Demir','05315901239','musteri94@mail.com','İzmir'),(95,'Veli Şahin','05571205779','musteri95@mail.com','Adana'),(96,'Deniz Aksoy','05458389840','musteri96@mail.com','Konya'),(97,'Elif Demir','05389014965','musteri97@mail.com','Ankara'),(98,'Ayşe Kılıç','05452242892','musteri98@mail.com','Adana'),(99,'Mehmet Çelik','05346097514','musteri99@mail.com','Ankara'),(100,'Emre Şahin','05477982241','musteri100@mail.com','Adana');
/*!40000 ALTER TABLE `customers` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `employee_bonuses`
--

DROP TABLE IF EXISTS `employee_bonuses`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `employee_bonuses` (
  `id` int NOT NULL AUTO_INCREMENT,
  `employee_id` int NOT NULL,
  `bonus_amount` decimal(10,2) NOT NULL,
  `bonus_date` date NOT NULL,
  `description` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `employee_id` (`employee_id`),
  CONSTRAINT `employee_bonuses_ibfk_1` FOREIGN KEY (`employee_id`) REFERENCES `employees` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=16 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `employee_bonuses`
--

LOCK TABLES `employee_bonuses` WRITE;
/*!40000 ALTER TABLE `employee_bonuses` DISABLE KEYS */;
INSERT INTO `employee_bonuses` VALUES (1,4,2000.00,'2026-03-10','Başarılı satış performansı'),(2,15,1000.00,'2026-05-08','Başarılı satış performansı'),(3,15,1500.00,'2026-04-06','Başarılı satış performansı'),(4,14,2500.00,'2026-04-06','Hedef primi'),(5,15,1000.00,'2026-01-10','Başarılı satış performansı'),(6,15,2000.00,'2026-03-15','Müşteri memnuniyeti primi'),(7,15,1500.00,'2026-05-15','Hedef primi'),(8,4,1000.00,'2026-03-06','Müşteri memnuniyeti primi'),(9,11,2000.00,'2026-02-01','Hedef primi'),(10,13,2000.00,'2026-05-22','Fazla mesai primi'),(11,3,3000.00,'2026-04-14','Hedef primi'),(12,2,2500.00,'2026-01-06','Fazla mesai primi'),(13,8,2000.00,'2026-02-01','Hedef primi'),(14,7,1500.00,'2026-04-09','Hedef primi'),(15,5,3000.00,'2026-05-01','Hedef primi');
/*!40000 ALTER TABLE `employee_bonuses` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `employee_leaves`
--

DROP TABLE IF EXISTS `employee_leaves`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `employee_leaves` (
  `id` int NOT NULL AUTO_INCREMENT,
  `employee_id` int NOT NULL,
  `start_date` date NOT NULL,
  `end_date` date NOT NULL,
  `leave_reason` varchar(255) DEFAULT NULL,
  `status` varchar(50) DEFAULT 'Beklemede',
  PRIMARY KEY (`id`),
  KEY `employee_id` (`employee_id`),
  CONSTRAINT `employee_leaves_ibfk_1` FOREIGN KEY (`employee_id`) REFERENCES `employees` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `employee_leaves`
--

LOCK TABLES `employee_leaves` WRITE;
/*!40000 ALTER TABLE `employee_leaves` DISABLE KEYS */;
INSERT INTO `employee_leaves` VALUES (1,4,'2026-03-10','2026-03-12','Mazeret izni','Onaylandı'),(2,8,'2026-03-18','2026-03-21','Yıllık izin','Beklemede'),(3,10,'2026-05-13','2026-05-17','Yıllık izin','Beklemede'),(4,5,'2026-01-10','2026-01-11','Yıllık izin','Onaylandı'),(5,11,'2026-03-16','2026-03-18','Sağlık izni','Reddedildi'),(6,5,'2026-05-09','2026-05-11','Ailevi nedenler','Reddedildi'),(7,12,'2026-05-08','2026-05-10','Ailevi nedenler','Reddedildi'),(8,15,'2026-05-08','2026-05-10','Ailevi nedenler','Beklemede'),(9,7,'2026-03-16','2026-03-17','Sağlık izni','Beklemede'),(10,9,'2026-02-14','2026-02-18','Mazeret izni','Reddedildi');
/*!40000 ALTER TABLE `employee_leaves` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `employee_shifts`
--

DROP TABLE IF EXISTS `employee_shifts`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `employee_shifts` (
  `id` int NOT NULL AUTO_INCREMENT,
  `employee_id` int NOT NULL,
  `shift_date` date NOT NULL,
  `start_time` time NOT NULL,
  `end_time` time NOT NULL,
  `description` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `employee_id` (`employee_id`),
  CONSTRAINT `employee_shifts_ibfk_1` FOREIGN KEY (`employee_id`) REFERENCES `employees` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=41 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `employee_shifts`
--

LOCK TABLES `employee_shifts` WRITE;
/*!40000 ALTER TABLE `employee_shifts` DISABLE KEYS */;
INSERT INTO `employee_shifts` VALUES (1,12,'2026-05-12','16:00:00','23:00:00','Akşam vardiyası'),(2,13,'2026-05-12','08:00:00','16:00:00','Hafta sonu vardiyası'),(3,4,'2026-05-15','08:00:00','16:00:00','Hafta sonu vardiyası'),(4,8,'2026-05-08','09:00:00','17:00:00','Gündüz vardiyası'),(5,1,'2026-05-10','16:00:00','23:00:00','Depo desteği'),(6,4,'2026-05-06','12:00:00','20:00:00','Hafta sonu vardiyası'),(7,4,'2026-05-06','16:00:00','23:00:00','Depo desteği'),(8,8,'2026-05-10','16:00:00','23:00:00','Gündüz vardiyası'),(9,2,'2026-05-13','16:00:00','23:00:00','Akşam vardiyası'),(10,4,'2026-05-19','12:00:00','20:00:00','Gündüz vardiyası'),(11,1,'2026-05-10','16:00:00','23:00:00','Depo desteği'),(12,5,'2026-05-18','08:00:00','16:00:00','Gündüz vardiyası'),(13,7,'2026-05-05','12:00:00','20:00:00','Hafta sonu vardiyası'),(14,13,'2026-05-13','12:00:00','20:00:00','Gündüz vardiyası'),(15,7,'2026-05-02','09:00:00','17:00:00','Hafta sonu vardiyası'),(16,9,'2026-05-10','08:00:00','16:00:00','Depo desteği'),(17,9,'2026-05-15','12:00:00','20:00:00','Gündüz vardiyası'),(18,3,'2026-05-04','16:00:00','23:00:00','Hafta sonu vardiyası'),(19,13,'2026-05-11','12:00:00','20:00:00','Akşam vardiyası'),(20,4,'2026-05-20','16:00:00','23:00:00','Gündüz vardiyası'),(21,1,'2026-05-02','09:00:00','17:00:00','Hafta sonu vardiyası'),(22,13,'2026-05-16','16:00:00','23:00:00','Akşam vardiyası'),(23,10,'2026-05-17','09:00:00','17:00:00','Hafta sonu vardiyası'),(24,15,'2026-05-20','12:00:00','20:00:00','Akşam vardiyası'),(25,7,'2026-05-20','12:00:00','20:00:00','Hafta sonu vardiyası'),(26,9,'2026-05-17','16:00:00','23:00:00','Hafta sonu vardiyası'),(27,8,'2026-05-01','12:00:00','20:00:00','Hafta sonu vardiyası'),(28,11,'2026-05-04','16:00:00','23:00:00','Hafta sonu vardiyası'),(29,13,'2026-05-21','08:00:00','16:00:00','Depo desteği'),(30,5,'2026-05-21','09:00:00','17:00:00','Gündüz vardiyası'),(31,10,'2026-05-16','09:00:00','17:00:00','Depo desteği'),(32,3,'2026-05-22','09:00:00','17:00:00','Gündüz vardiyası'),(33,10,'2026-05-04','09:00:00','17:00:00','Gündüz vardiyası'),(34,8,'2026-05-11','16:00:00','23:00:00','Akşam vardiyası'),(35,7,'2026-05-07','16:00:00','23:00:00','Depo desteği'),(36,14,'2026-05-02','09:00:00','17:00:00','Akşam vardiyası'),(37,9,'2026-05-11','16:00:00','23:00:00','Depo desteği'),(38,6,'2026-05-06','16:00:00','23:00:00','Hafta sonu vardiyası'),(39,9,'2026-05-12','12:00:00','20:00:00','Depo desteği'),(40,4,'2026-05-08','12:00:00','20:00:00','Hafta sonu vardiyası');
/*!40000 ALTER TABLE `employee_shifts` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `employees`
--

DROP TABLE IF EXISTS `employees`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `employees` (
  `id` int NOT NULL AUTO_INCREMENT,
  `full_name` varchar(150) NOT NULL,
  `phone` varchar(20) DEFAULT NULL,
  `position` varchar(100) DEFAULT NULL,
  `salary` decimal(10,2) DEFAULT NULL,
  `hire_date` date DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=16 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `employees`
--

LOCK TABLES `employees` WRITE;
/*!40000 ALTER TABLE `employees` DISABLE KEYS */;
INSERT INTO `employees` VALUES (1,'Hakan Bozkurt','05379767312','Mağaza Müdürü',28000.00,'2024-08-10'),(2,'Kerem Aslan','05392010496','Muhasebe',38000.00,'2024-02-25'),(3,'Murat Arslan','05382362099','Depo Görevlisi',24000.00,'2024-03-18'),(4,'Ayşe Yıldız','05307853787','Mağaza Müdürü',38000.00,'2024-10-16'),(5,'Deniz Demir','05375833597','Müşteri Hizmetleri',26000.00,'2024-12-28'),(6,'Ali Kaya','05514916201','Reyon Görevlisi',38000.00,'2024-10-22'),(7,'Murat Aslan','05334771676','Müşteri Hizmetleri',24000.00,'2024-05-27'),(8,'Zeynep Kaya','05313783722','Satış Danışmanı',26000.00,'2024-10-24'),(9,'Kerem Koç','05443086308','Mağaza Müdürü',38000.00,'2024-05-23'),(10,'Buse Öztürk','05469284509','Mağaza Müdürü',22000.00,'2024-10-02'),(11,'Ece Kurt','05495200476','Kasiyer',22000.00,'2024-04-22'),(12,'Kerem Taş','05305521644','Muhasebe',22000.00,'2024-03-16'),(13,'Seda Doğan','05595666692','Depo Görevlisi',32000.00,'2024-07-21'),(14,'Veli Kaya','05456837817','Mağaza Müdürü',26000.00,'2024-06-22'),(15,'Fatma Yıldız','05407906258','Müşteri Hizmetleri',28000.00,'2024-05-22');
/*!40000 ALTER TABLE `employees` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `expenses`
--

DROP TABLE IF EXISTS `expenses`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `expenses` (
  `id` int NOT NULL AUTO_INCREMENT,
  `title` varchar(150) NOT NULL,
  `amount` decimal(10,2) NOT NULL,
  `expense_date` date NOT NULL,
  `description` text,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `expenses`
--

LOCK TABLES `expenses` WRITE;
/*!40000 ALTER TABLE `expenses` DISABLE KEYS */;
INSERT INTO `expenses` VALUES (1,'Kira',15000.00,'2026-05-22','Aylık mağaza kirası');
/*!40000 ALTER TABLE `expenses` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `products`
--

DROP TABLE IF EXISTS `products`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `products` (
  `id` int NOT NULL AUTO_INCREMENT,
  `category_id` int DEFAULT NULL,
  `supplier_id` int DEFAULT NULL,
  `barcode` varchar(50) DEFAULT NULL,
  `name` varchar(150) NOT NULL,
  `purchase_price` decimal(10,2) NOT NULL,
  `sale_price` decimal(10,2) NOT NULL,
  `stock_quantity` int NOT NULL DEFAULT '0',
  `critical_stock` int NOT NULL DEFAULT '5',
  `created_at` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `barcode` (`barcode`),
  KEY `category_id` (`category_id`),
  KEY `supplier_id` (`supplier_id`),
  CONSTRAINT `products_ibfk_1` FOREIGN KEY (`category_id`) REFERENCES `categories` (`id`),
  CONSTRAINT `products_ibfk_2` FOREIGN KEY (`supplier_id`) REFERENCES `suppliers` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=101 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `products`
--

LOCK TABLES `products` WRITE;
/*!40000 ALTER TABLE `products` DISABLE KEYS */;
INSERT INTO `products` VALUES (1,1,1,'100001','Coca Cola 1L',22.00,35.00,91,5,'2026-05-22 04:31:09'),(2,1,1,'100002','Fanta 1L',21.00,34.00,114,5,'2026-05-22 04:31:09'),(3,1,1,'100003','Sprite 1L',21.00,34.00,79,10,'2026-05-22 04:31:09'),(4,1,6,'100004','Su 1.5L',5.00,9.00,77,5,'2026-05-22 04:31:09'),(5,1,4,'100005','Ayran 300ml',7.00,12.00,96,5,'2026-05-22 04:31:09'),(6,2,6,'100006','Ekmek',6.00,10.00,132,15,'2026-05-22 04:31:09'),(7,2,5,'100007','Pirinç 1kg',38.00,55.00,98,5,'2026-05-22 04:31:09'),(8,2,2,'100008','Makarna 500g',12.00,20.00,41,10,'2026-05-22 04:31:09'),(9,2,5,'100009','Un 1kg',18.00,29.00,115,15,'2026-05-22 04:31:09'),(10,2,5,'100010','Şeker 1kg',28.00,42.00,0,8,'2026-05-22 04:31:09'),(11,3,2,'100011','Çikolata',11.00,20.00,65,8,'2026-05-22 04:31:09'),(12,3,3,'100012','Bisküvi',9.00,16.00,66,15,'2026-05-22 04:31:09'),(13,3,2,'100013','Cips',18.00,30.00,127,5,'2026-05-22 04:31:09'),(14,4,7,'100014','Deterjan 3kg',95.00,140.00,3,8,'2026-05-22 04:31:09'),(15,4,7,'100015','Bulaşık Deterjanı',35.00,55.00,23,10,'2026-05-22 04:31:09'),(16,4,7,'100016','Çamaşır Suyu',25.00,40.00,139,5,'2026-05-22 04:31:09'),(17,5,7,'100017','Şampuan',50.00,80.00,134,15,'2026-05-22 04:31:09'),(18,5,7,'100018','Diş Macunu',28.00,45.00,19,8,'2026-05-22 04:31:09'),(19,5,7,'100019','Sabun',12.00,22.00,21,15,'2026-05-22 04:31:09'),(20,6,6,'100020','Elma 1kg',20.00,35.00,27,8,'2026-05-22 04:31:09'),(21,6,6,'100021','Domates 1kg',18.00,32.00,130,10,'2026-05-22 04:31:09'),(22,6,6,'100022','Patates 1kg',14.00,25.00,126,10,'2026-05-22 04:31:09'),(23,7,4,'100023','Süt 1L',20.00,32.00,103,15,'2026-05-22 04:31:09'),(24,7,4,'100024','Yoğurt 1kg',35.00,55.00,115,8,'2026-05-22 04:31:09'),(25,7,4,'100025','Peynir 500g',75.00,110.00,119,8,'2026-05-22 04:31:09'),(26,8,8,'100026','Mercimek 1kg',35.00,52.00,100,8,'2026-05-22 04:31:09'),(27,8,8,'100027','Nohut 1kg',40.00,60.00,126,8,'2026-05-22 04:31:09'),(28,8,8,'100028','Fasulye 1kg',48.00,70.00,16,10,'2026-05-22 04:31:09'),(29,9,9,'100029','Sucuk 250g',90.00,135.00,103,10,'2026-05-22 04:31:09'),(30,9,9,'100030','Salam 250g',55.00,85.00,129,10,'2026-05-22 04:31:09'),(31,10,10,'100031','Donuk Pizza',60.00,95.00,0,10,'2026-05-22 04:31:09'),(32,10,10,'100032','Donuk Patates',45.00,70.00,74,15,'2026-05-22 04:31:09'),(33,2,1,'100033','Kırmızı Mercimek 33',38.80,51.96,35,15,'2026-05-22 04:31:09'),(34,2,9,'100034','Maden Suyu 34',74.14,93.62,137,15,'2026-05-22 04:31:09'),(35,2,4,'100035','Reçel 35',64.60,81.44,84,10,'2026-05-22 04:31:09'),(36,4,9,'100036','Deodorant 36',32.69,48.57,138,15,'2026-05-22 04:31:09'),(37,1,3,'100037','Bulgur 37',55.33,75.31,106,10,'2026-05-22 04:31:09'),(38,4,6,'100038','Kraker 38',18.39,23.70,45,8,'2026-05-22 04:31:09'),(39,6,10,'100039','Bal 39',98.40,151.72,143,15,'2026-05-22 04:31:09'),(40,9,2,'100040','Islak Mendil 40',16.83,23.01,57,15,'2026-05-22 04:31:09'),(41,10,6,'100041','Havuç 41',29.54,37.75,11,10,'2026-05-22 04:31:09'),(42,4,5,'100042','Maden Suyu 42',103.80,165.73,118,15,'2026-05-22 04:31:09'),(43,7,5,'100043','Tıraş Köpüğü 43',79.20,110.56,93,8,'2026-05-22 04:31:09'),(44,6,6,'100044','Zeytin 44',83.06,127.14,123,5,'2026-05-22 04:31:09'),(45,2,10,'100045','Tereyağı 45',27.17,41.89,17,10,'2026-05-22 04:31:09'),(46,3,8,'100046','Islak Mendil 46',38.23,61.94,21,15,'2026-05-22 04:31:09'),(47,9,4,'100047','Yumurta 10lu 47',44.32,69.02,22,15,'2026-05-22 04:31:09'),(48,1,4,'100048','Dondurma 48',11.60,15.96,0,8,'2026-05-22 04:31:09'),(49,5,2,'100049','Zeytin 49',110.27,162.85,103,8,'2026-05-22 04:31:09'),(50,6,4,'100050','Tereyağı 50',63.91,102.50,10,15,'2026-05-22 04:31:09'),(51,8,3,'100051','Bal 51',23.64,36.59,67,10,'2026-05-22 04:31:09'),(52,9,5,'100052','Kırmızı Mercimek 52',73.47,118.22,103,5,'2026-05-22 04:31:09'),(53,7,6,'100053','Reçel 53',119.70,156.25,83,15,'2026-05-22 04:31:09'),(54,8,2,'100054','Tavuk Göğsü 54',13.28,17.18,79,15,'2026-05-22 04:31:09'),(55,3,7,'100055','Kaşar 55',15.12,21.21,132,5,'2026-05-22 04:31:09'),(56,8,9,'100056','Bal 56',116.76,186.15,17,8,'2026-05-22 04:31:09'),(57,1,2,'100057','Yumurta 10lu 57',107.10,166.04,72,8,'2026-05-22 04:31:09'),(58,6,2,'100058','Tahin 58',56.69,81.15,21,15,'2026-05-22 04:31:09'),(59,5,9,'100059','Tavuk Göğsü 59',28.01,45.24,13,5,'2026-05-22 04:31:09'),(60,5,9,'100060','Kaşar 60',30.28,42.38,113,8,'2026-05-22 04:31:09'),(61,3,9,'100061','Tavuk Göğsü 61',111.31,180.03,76,5,'2026-05-22 04:31:09'),(62,10,6,'100062','Muz 62',10.18,16.51,14,10,'2026-05-22 04:31:09'),(63,5,4,'100063','Meyve Suyu 63',34.98,51.66,13,10,'2026-05-22 04:31:09'),(64,2,2,'100064','Kırmızı Mercimek 64',62.43,79.77,85,10,'2026-05-22 04:31:09'),(65,9,3,'100065','Gofret 65',81.89,133.38,99,8,'2026-05-22 04:31:09'),(66,3,5,'100066','Portakal 66',105.71,150.03,64,15,'2026-05-22 04:31:09'),(67,4,9,'100067','Tavuk Göğsü 67',89.75,119.41,143,8,'2026-05-22 04:31:09'),(68,5,7,'100068','Yumurta 10lu 68',80.79,115.15,34,8,'2026-05-22 04:31:09'),(69,9,8,'100069','Kraker 69',35.77,45.63,14,15,'2026-05-22 04:31:09'),(70,1,10,'100070','Soğan 70',33.77,45.19,63,15,'2026-05-22 04:31:09'),(71,2,1,'100071','Reçel 71',15.55,19.63,31,8,'2026-05-22 04:31:09'),(72,6,2,'100072','Portakal 72',34.66,52.60,115,10,'2026-05-22 04:31:09'),(73,4,9,'100073','Gofret 73',89.02,142.69,116,10,'2026-05-22 04:31:09'),(74,10,8,'100074','Reçel 74',95.88,150.82,1,15,'2026-05-22 04:31:09'),(75,4,2,'100075','Kraker 75',81.81,113.86,73,8,'2026-05-22 04:31:09'),(76,7,8,'100076','Bezelye Donuk 76',89.66,136.22,14,15,'2026-05-22 04:31:09'),(77,2,1,'100077','Islak Mendil 77',89.56,140.63,86,10,'2026-05-22 04:31:09'),(78,2,4,'100078','Zeytin 78',29.30,41.88,101,10,'2026-05-22 04:31:09'),(79,7,3,'100079','Bal 79',59.81,95.68,111,10,'2026-05-22 04:31:09'),(80,2,8,'100080','Köfte 80',104.47,153.58,46,15,'2026-05-22 04:31:09'),(81,1,9,'100081','Dondurma 81',9.65,12.42,124,5,'2026-05-22 04:31:09'),(82,4,3,'100082','Deodorant 82',62.39,83.32,50,15,'2026-05-22 04:31:09'),(83,7,1,'100083','Kek 83',50.44,82.93,138,10,'2026-05-22 04:31:09'),(84,5,8,'100084','Tahin 84',55.38,90.43,148,10,'2026-05-22 04:31:09'),(85,9,8,'100085','Gofret 85',29.27,39.14,71,5,'2026-05-22 04:31:09'),(86,1,10,'100086','Kırmızı Mercimek 86',68.73,106.48,85,10,'2026-05-22 04:31:09'),(87,1,1,'100087','Havuç 87',61.40,99.33,3,5,'2026-05-22 04:31:09'),(88,9,3,'100088','Meyve Suyu 88',115.61,148.22,130,10,'2026-05-22 04:31:09'),(89,3,2,'100089','Kaşar 89',15.61,24.89,52,10,'2026-05-22 04:31:09'),(90,7,2,'100090','Havuç 90',35.58,52.94,55,8,'2026-05-22 04:31:09'),(91,10,2,'100091','Deodorant 91',81.63,120.49,67,8,'2026-05-22 04:31:09'),(92,6,5,'100092','Zeytin 92',83.01,114.19,22,5,'2026-05-22 04:31:09'),(93,5,7,'100093','Gofret 93',83.23,114.03,82,15,'2026-05-22 04:31:09'),(94,6,2,'100094','Limonata 94',59.33,97.86,11,10,'2026-05-22 04:31:09'),(95,2,2,'100095','Soğan 95',31.87,43.22,36,5,'2026-05-22 04:31:09'),(96,6,2,'100096','Reçel 96',49.39,64.85,76,10,'2026-05-22 04:31:09'),(97,9,5,'100097','Kaşar 97',118.19,185.89,109,8,'2026-05-22 04:31:09'),(98,9,1,'100098','Yumurta 10lu 98',99.52,136.32,49,8,'2026-05-22 04:31:09'),(99,2,3,'100099','Bal 99',20.93,27.06,123,10,'2026-05-22 04:31:09'),(100,9,3,'100100','Bal 100',39.56,52.78,132,10,'2026-05-22 04:31:09');
/*!40000 ALTER TABLE `products` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `sale_details`
--

DROP TABLE IF EXISTS `sale_details`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `sale_details` (
  `id` int NOT NULL AUTO_INCREMENT,
  `sale_id` int NOT NULL,
  `product_id` int NOT NULL,
  `quantity` int NOT NULL,
  `unit_price` decimal(10,2) NOT NULL,
  `subtotal` decimal(10,2) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `sale_id` (`sale_id`),
  KEY `product_id` (`product_id`),
  CONSTRAINT `sale_details_ibfk_1` FOREIGN KEY (`sale_id`) REFERENCES `sales` (`id`),
  CONSTRAINT `sale_details_ibfk_2` FOREIGN KEY (`product_id`) REFERENCES `products` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=216 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `sale_details`
--

LOCK TABLES `sale_details` WRITE;
/*!40000 ALTER TABLE `sale_details` DISABLE KEYS */;
INSERT INTO `sale_details` VALUES (1,1,44,1,127.14,127.14),(2,1,15,2,55.00,110.00),(3,1,60,2,42.38,84.76),(4,2,2,5,34.00,170.00),(5,2,34,1,93.62,93.62),(6,3,75,3,113.86,341.58),(7,3,49,1,162.85,162.85),(8,4,42,1,165.73,165.73),(9,4,79,4,95.68,382.72),(10,4,29,3,135.00,405.00),(11,5,39,4,151.72,606.88),(12,6,59,5,45.24,226.20),(13,6,48,4,15.96,63.84),(14,7,79,3,95.68,287.04),(15,7,53,1,156.25,156.25),(16,8,47,1,69.02,69.02),(17,9,51,3,36.59,109.77),(18,10,83,5,82.93,414.65),(19,11,27,5,60.00,300.00),(20,11,9,2,29.00,58.00),(21,11,98,5,136.32,681.60),(22,12,1,2,35.00,70.00),(23,12,36,5,48.57,242.85),(24,12,19,3,22.00,66.00),(25,13,46,2,61.94,123.88),(26,14,17,4,80.00,320.00),(27,15,66,1,150.03,150.03),(28,15,76,4,136.22,544.88),(29,16,67,1,119.41,119.41),(30,16,39,1,151.72,151.72),(31,16,59,4,45.24,180.96),(32,17,57,3,166.04,498.12),(33,17,10,5,42.00,210.00),(34,17,11,2,20.00,40.00),(35,18,71,4,19.63,78.52),(36,18,92,5,114.19,570.95),(37,18,42,5,165.73,828.65),(38,19,90,1,52.94,52.94),(39,20,53,3,156.25,468.75),(40,21,55,3,21.21,63.63),(41,21,41,3,37.75,113.25),(42,22,12,4,16.00,64.00),(43,23,72,1,52.60,52.60),(44,23,43,4,110.56,442.24),(45,23,86,3,106.48,319.44),(46,24,46,5,61.94,309.70),(47,24,14,5,140.00,700.00),(48,25,45,5,41.89,209.45),(49,26,72,5,52.60,263.00),(50,26,99,5,27.06,135.30),(51,27,89,2,24.89,49.78),(52,27,35,3,81.44,244.32),(53,27,4,3,9.00,27.00),(54,28,73,4,142.69,570.76),(55,29,68,4,115.15,460.60),(56,29,28,4,70.00,280.00),(57,29,49,3,162.85,488.55),(58,30,77,2,140.63,281.26),(59,30,11,2,20.00,40.00),(60,30,7,5,55.00,275.00),(61,31,63,4,51.66,206.64),(62,31,78,4,41.88,167.52),(63,32,15,5,55.00,275.00),(64,32,37,4,75.31,301.24),(65,33,8,3,20.00,60.00),(66,33,1,2,35.00,70.00),(67,33,27,2,60.00,120.00),(68,34,64,4,79.77,319.08),(69,35,65,5,133.38,666.90),(70,36,6,4,10.00,40.00),(71,36,56,1,186.15,186.15),(72,36,3,3,34.00,102.00),(73,37,54,4,17.18,68.72),(74,37,38,1,23.70,23.70),(75,37,15,3,55.00,165.00),(76,38,12,1,16.00,16.00),(77,38,56,2,186.15,372.30),(78,39,40,3,23.01,69.03),(79,39,96,2,64.85,129.70),(80,40,68,5,115.15,575.75),(81,41,19,2,22.00,44.00),(82,41,31,3,95.00,285.00),(83,41,14,2,140.00,280.00),(84,42,10,4,42.00,168.00),(85,42,23,4,32.00,128.00),(86,42,99,5,27.06,135.30),(87,43,83,3,82.93,248.79),(88,43,82,3,83.32,249.96),(89,43,80,2,153.58,307.16),(90,44,36,1,48.57,48.57),(91,44,76,3,136.22,408.66),(92,45,8,3,20.00,60.00),(93,46,65,5,133.38,666.90),(94,46,50,5,102.50,512.50),(95,46,60,1,42.38,42.38),(96,47,61,1,180.03,180.03),(97,47,65,4,133.38,533.52),(98,47,20,1,35.00,35.00),(99,48,32,4,70.00,280.00),(100,49,48,4,15.96,63.84),(101,49,37,4,75.31,301.24),(102,50,83,3,82.93,248.79),(103,50,43,1,110.56,110.56),(104,50,9,5,29.00,145.00),(105,51,78,1,41.88,41.88),(106,51,20,5,35.00,175.00),(107,51,43,2,110.56,221.12),(108,52,17,1,80.00,80.00),(109,52,77,3,140.63,421.89),(110,53,86,5,106.48,532.40),(111,54,40,2,23.01,46.02),(112,54,24,3,55.00,165.00),(113,55,10,3,42.00,126.00),(114,56,5,5,12.00,60.00),(115,56,85,2,39.14,78.28),(116,56,44,5,127.14,635.70),(117,57,99,4,27.06,108.24),(118,57,80,1,153.58,153.58),(119,57,22,4,25.00,100.00),(120,58,58,2,81.15,162.30),(121,58,30,3,85.00,255.00),(122,58,69,4,45.63,182.52),(123,59,99,4,27.06,108.24),(124,59,37,5,75.31,376.55),(125,60,18,4,45.00,180.00),(126,60,33,3,51.96,155.88),(127,60,7,5,55.00,275.00),(128,61,35,4,81.44,325.76),(129,62,58,3,81.15,243.45),(130,63,31,1,95.00,95.00),(131,63,50,3,102.50,307.50),(132,64,84,2,90.43,180.86),(133,64,43,1,110.56,110.56),(134,64,19,3,22.00,66.00),(135,65,11,1,20.00,20.00),(136,66,68,3,115.15,345.45),(137,66,55,2,21.21,42.42),(138,66,15,3,55.00,165.00),(139,67,59,4,45.24,180.96),(140,67,9,5,29.00,145.00),(141,67,15,5,55.00,275.00),(142,68,38,4,23.70,94.80),(143,69,24,1,55.00,55.00),(144,69,86,5,106.48,532.40),(145,70,71,4,19.63,78.52),(146,70,3,1,34.00,34.00),(147,70,50,4,102.50,410.00),(148,71,45,2,41.89,83.78),(149,72,18,3,45.00,135.00),(150,72,6,5,10.00,50.00),(151,73,81,2,12.42,24.84),(152,73,24,1,55.00,55.00),(153,74,26,1,52.00,52.00),(154,75,61,2,180.03,360.06),(155,75,33,3,51.96,155.88),(156,75,5,3,12.00,36.00),(157,76,48,4,15.96,63.84),(158,77,89,3,24.89,74.67),(159,77,64,5,79.77,398.85),(160,78,24,3,55.00,165.00),(161,78,70,1,45.19,45.19),(162,78,38,1,23.70,23.70),(163,79,55,4,21.21,84.84),(164,79,22,3,25.00,75.00),(165,79,89,4,24.89,99.56),(166,80,8,4,20.00,80.00),(167,80,10,3,42.00,126.00),(168,80,86,5,106.48,532.40),(169,81,87,2,99.33,198.66),(170,81,57,1,166.04,166.04),(171,81,5,2,12.00,24.00),(172,82,78,2,41.88,83.76),(173,83,74,2,150.82,301.64),(174,84,32,1,70.00,70.00),(175,84,15,2,55.00,110.00),(176,85,100,4,52.78,211.12),(177,85,34,2,93.62,187.24),(178,86,10,4,42.00,168.00),(179,87,86,1,106.48,106.48),(180,87,45,5,41.89,209.45),(181,88,90,3,52.94,158.82),(182,88,82,5,83.32,416.60),(183,88,23,2,32.00,64.00),(184,89,4,5,9.00,45.00),(185,89,47,5,69.02,345.10),(186,90,9,3,29.00,87.00),(187,91,74,2,150.82,301.64),(188,91,10,3,42.00,126.00),(189,92,45,5,41.89,209.45),(190,92,17,5,80.00,400.00),(191,92,100,2,52.78,105.56),(192,93,16,5,40.00,200.00),(193,94,29,3,135.00,405.00),(194,94,45,1,41.89,41.89),(195,94,67,3,119.41,358.23),(196,95,79,1,95.68,95.68),(197,95,69,5,45.63,228.15),(198,96,85,5,39.14,195.70),(199,97,11,1,20.00,20.00),(200,98,54,1,17.18,17.18),(201,98,80,4,153.58,614.32),(202,98,82,5,83.32,416.60),(203,99,52,1,118.22,118.22),(204,99,39,1,151.72,151.72),(205,99,59,2,45.24,90.48),(206,100,78,3,41.88,125.64),(207,100,19,3,22.00,66.00),(208,101,12,3,16.00,48.00),(214,104,1,10,35.00,350.00),(215,104,12,2,16.00,32.00);
/*!40000 ALTER TABLE `sale_details` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `sales`
--

DROP TABLE IF EXISTS `sales`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `sales` (
  `id` int NOT NULL AUTO_INCREMENT,
  `customer_id` int DEFAULT NULL,
  `sale_date` datetime DEFAULT CURRENT_TIMESTAMP,
  `total_amount` decimal(10,2) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `customer_id` (`customer_id`),
  CONSTRAINT `sales_ibfk_1` FOREIGN KEY (`customer_id`) REFERENCES `customers` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=105 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `sales`
--

LOCK TABLES `sales` WRITE;
/*!40000 ALTER TABLE `sales` DISABLE KEYS */;
INSERT INTO `sales` VALUES (1,22,'2026-03-16 21:18:00',321.90),(2,87,'2026-04-18 14:05:00',263.62),(3,59,'2026-03-22 20:43:00',504.43),(4,87,'2026-02-16 09:39:00',953.45),(5,84,'2026-04-04 11:02:00',606.88),(6,15,'2026-01-08 17:08:00',290.04),(7,76,'2026-02-14 19:06:00',443.29),(8,89,'2026-03-07 16:28:00',69.02),(9,88,'2026-03-18 19:22:00',109.77),(10,25,'2026-01-15 10:42:00',414.65),(11,3,'2026-01-11 12:08:00',1039.60),(12,28,'2026-02-11 21:09:00',378.85),(13,23,'2026-01-22 09:08:00',123.88),(14,76,'2026-03-01 11:16:00',320.00),(15,68,'2026-01-03 16:28:00',694.91),(16,65,'2026-02-20 09:46:00',452.09),(17,52,'2026-04-22 10:31:00',748.12),(18,9,'2026-02-09 18:40:00',1478.12),(19,38,'2026-04-17 18:27:00',52.94),(20,84,'2026-05-07 15:28:00',468.75),(21,59,'2026-04-14 20:06:00',176.88),(22,20,'2026-04-03 10:53:00',64.00),(23,13,'2026-03-05 17:03:00',814.28),(24,86,'2026-04-02 13:38:00',1009.70),(25,28,'2026-02-22 16:14:00',209.45),(26,48,'2026-01-09 18:14:00',398.30),(27,87,'2026-05-01 18:59:00',321.10),(28,44,'2026-03-01 11:55:00',570.76),(29,9,'2026-02-21 09:05:00',1229.15),(30,21,'2026-03-10 20:20:00',596.26),(31,7,'2026-01-09 16:42:00',374.16),(32,35,'2026-02-17 10:22:00',576.24),(33,68,'2026-03-02 12:25:00',250.00),(34,98,'2026-03-10 14:07:00',319.08),(35,23,'2026-02-13 17:45:00',666.90),(36,86,'2026-03-03 15:55:00',328.15),(37,74,'2026-04-19 15:45:00',257.42),(38,22,'2026-05-15 20:58:00',388.30),(39,56,'2026-05-13 17:05:00',198.73),(40,43,'2026-02-03 17:40:00',575.75),(41,25,'2026-03-12 20:52:00',609.00),(42,23,'2026-05-05 21:48:00',431.30),(43,98,'2026-05-15 19:59:00',805.91),(44,57,'2026-01-16 16:40:00',457.23),(45,65,'2026-01-10 16:28:00',60.00),(46,37,'2026-01-21 10:39:00',1221.78),(47,58,'2026-05-21 12:20:00',748.55),(48,44,'2026-01-17 19:11:00',280.00),(49,57,'2026-05-17 18:10:00',365.08),(50,100,'2026-03-22 18:03:00',504.35),(51,87,'2026-04-10 13:46:00',438.00),(52,45,'2026-03-21 20:42:00',501.89),(53,72,'2026-04-21 21:21:00',532.40),(54,12,'2026-04-17 14:01:00',211.02),(55,99,'2026-04-07 12:08:00',126.00),(56,13,'2026-05-18 20:56:00',773.98),(57,49,'2026-02-06 11:53:00',361.82),(58,47,'2026-02-15 18:18:00',599.82),(59,25,'2026-03-22 18:28:00',484.79),(60,68,'2026-04-06 12:51:00',610.88),(61,14,'2026-05-04 13:05:00',325.76),(62,66,'2026-02-14 10:58:00',243.45),(63,4,'2026-04-02 15:32:00',402.50),(64,29,'2026-01-11 10:53:00',357.42),(65,61,'2026-02-16 16:39:00',20.00),(66,33,'2026-02-05 17:46:00',552.87),(67,16,'2026-01-08 15:40:00',600.96),(68,3,'2026-05-19 12:45:00',94.80),(69,1,'2026-05-12 12:36:00',587.40),(70,47,'2026-01-17 17:32:00',522.52),(71,48,'2026-03-01 14:50:00',83.78),(72,94,'2026-01-19 20:48:00',185.00),(73,44,'2026-02-22 16:44:00',79.84),(74,92,'2026-04-02 13:12:00',52.00),(75,41,'2026-03-17 15:52:00',551.94),(76,100,'2026-01-21 14:17:00',63.84),(77,52,'2026-04-13 14:11:00',473.52),(78,35,'2026-01-14 10:27:00',233.89),(79,42,'2026-03-10 16:38:00',259.40),(80,6,'2026-03-20 15:17:00',738.40),(81,96,'2026-02-01 11:54:00',388.70),(82,100,'2026-03-12 15:36:00',83.76),(83,87,'2026-04-12 14:28:00',301.64),(84,68,'2026-03-13 14:41:00',180.00),(85,64,'2026-05-13 17:07:00',398.36),(86,79,'2026-03-16 12:07:00',168.00),(87,23,'2026-04-03 21:43:00',315.93),(88,70,'2026-03-10 11:45:00',639.42),(89,16,'2026-02-05 12:50:00',390.10),(90,48,'2026-04-18 11:39:00',87.00),(91,51,'2026-04-17 15:49:00',427.64),(92,83,'2026-01-15 16:43:00',715.01),(93,99,'2026-02-14 17:58:00',200.00),(94,20,'2026-03-06 11:20:00',805.12),(95,26,'2026-05-09 11:40:00',323.83),(96,83,'2026-02-19 18:09:00',195.70),(97,93,'2026-05-11 18:02:00',20.00),(98,83,'2026-05-09 19:13:00',1048.10),(99,38,'2026-03-16 12:51:00',360.42),(100,57,'2026-04-16 16:13:00',191.64),(101,11,'2026-05-22 05:15:23',48.00),(104,18,'2026-05-22 06:00:02',382.00);
/*!40000 ALTER TABLE `sales` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `saved_queries`
--

DROP TABLE IF EXISTS `saved_queries`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `saved_queries` (
  `id` int NOT NULL AUTO_INCREMENT,
  `title` varchar(150) DEFAULT NULL,
  `sql_query` text,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `saved_queries`
--

LOCK TABLES `saved_queries` WRITE;
/*!40000 ALTER TABLE `saved_queries` DISABLE KEYS */;
INSERT INTO `saved_queries` VALUES (1,'Ürünleri Listele','SELECT * FROM products;'),(2,'Kritik Stok Ürünleri','SELECT name, stock_quantity, critical_stock FROM products WHERE stock_quantity <= critical_stock;'),(3,'Toplam Ciro','SELECT SUM(total_amount) AS toplam_ciro FROM sales;'),(4,'Müşteri Listesi','SELECT full_name, phone, email FROM customers;');
/*!40000 ALTER TABLE `saved_queries` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `stock_movements`
--

DROP TABLE IF EXISTS `stock_movements`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `stock_movements` (
  `id` int NOT NULL AUTO_INCREMENT,
  `product_id` int NOT NULL,
  `movement_type` varchar(20) NOT NULL,
  `quantity` int NOT NULL,
  `description` text,
  `movement_date` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `product_id` (`product_id`),
  CONSTRAINT `stock_movements_ibfk_1` FOREIGN KEY (`product_id`) REFERENCES `products` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=208 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `stock_movements`
--

LOCK TABLES `stock_movements` WRITE;
/*!40000 ALTER TABLE `stock_movements` DISABLE KEYS */;
INSERT INTO `stock_movements` VALUES (1,44,'Çıkış',1,'Satış işlemi','2026-05-22 04:31:09'),(2,15,'Çıkış',2,'Satış işlemi','2026-05-22 04:31:09'),(3,60,'Çıkış',2,'Satış işlemi','2026-05-22 04:31:09'),(4,2,'Çıkış',5,'Satış işlemi','2026-05-22 04:31:09'),(5,34,'Çıkış',1,'Satış işlemi','2026-05-22 04:31:09'),(6,75,'Çıkış',3,'Satış işlemi','2026-05-22 04:31:09'),(7,49,'Çıkış',1,'Satış işlemi','2026-05-22 04:31:09'),(8,42,'Çıkış',1,'Satış işlemi','2026-05-22 04:31:09'),(9,79,'Çıkış',4,'Satış işlemi','2026-05-22 04:31:09'),(10,29,'Çıkış',3,'Satış işlemi','2026-05-22 04:31:09'),(11,39,'Çıkış',4,'Satış işlemi','2026-05-22 04:31:09'),(12,59,'Çıkış',5,'Satış işlemi','2026-05-22 04:31:09'),(13,48,'Çıkış',4,'Satış işlemi','2026-05-22 04:31:09'),(14,79,'Çıkış',3,'Satış işlemi','2026-05-22 04:31:09'),(15,53,'Çıkış',1,'Satış işlemi','2026-05-22 04:31:09'),(16,47,'Çıkış',1,'Satış işlemi','2026-05-22 04:31:09'),(17,51,'Çıkış',3,'Satış işlemi','2026-05-22 04:31:09'),(18,83,'Çıkış',5,'Satış işlemi','2026-05-22 04:31:09'),(19,27,'Çıkış',5,'Satış işlemi','2026-05-22 04:31:09'),(20,9,'Çıkış',2,'Satış işlemi','2026-05-22 04:31:09'),(21,98,'Çıkış',5,'Satış işlemi','2026-05-22 04:31:09'),(22,1,'Çıkış',2,'Satış işlemi','2026-05-22 04:31:09'),(23,36,'Çıkış',5,'Satış işlemi','2026-05-22 04:31:09'),(24,19,'Çıkış',3,'Satış işlemi','2026-05-22 04:31:09'),(25,46,'Çıkış',2,'Satış işlemi','2026-05-22 04:31:09'),(26,17,'Çıkış',4,'Satış işlemi','2026-05-22 04:31:09'),(27,66,'Çıkış',1,'Satış işlemi','2026-05-22 04:31:09'),(28,76,'Çıkış',4,'Satış işlemi','2026-05-22 04:31:09'),(29,67,'Çıkış',1,'Satış işlemi','2026-05-22 04:31:09'),(30,39,'Çıkış',1,'Satış işlemi','2026-05-22 04:31:09'),(31,59,'Çıkış',4,'Satış işlemi','2026-05-22 04:31:09'),(32,57,'Çıkış',3,'Satış işlemi','2026-05-22 04:31:09'),(33,10,'Çıkış',5,'Satış işlemi','2026-05-22 04:31:09'),(34,11,'Çıkış',2,'Satış işlemi','2026-05-22 04:31:09'),(35,71,'Çıkış',4,'Satış işlemi','2026-05-22 04:31:09'),(36,92,'Çıkış',5,'Satış işlemi','2026-05-22 04:31:09'),(37,42,'Çıkış',5,'Satış işlemi','2026-05-22 04:31:09'),(38,90,'Çıkış',1,'Satış işlemi','2026-05-22 04:31:09'),(39,53,'Çıkış',3,'Satış işlemi','2026-05-22 04:31:09'),(40,55,'Çıkış',3,'Satış işlemi','2026-05-22 04:31:09'),(41,41,'Çıkış',3,'Satış işlemi','2026-05-22 04:31:10'),(42,12,'Çıkış',4,'Satış işlemi','2026-05-22 04:31:10'),(43,72,'Çıkış',1,'Satış işlemi','2026-05-22 04:31:10'),(44,43,'Çıkış',4,'Satış işlemi','2026-05-22 04:31:10'),(45,86,'Çıkış',3,'Satış işlemi','2026-05-22 04:31:10'),(46,46,'Çıkış',5,'Satış işlemi','2026-05-22 04:31:10'),(47,14,'Çıkış',5,'Satış işlemi','2026-05-22 04:31:10'),(48,45,'Çıkış',5,'Satış işlemi','2026-05-22 04:31:10'),(49,72,'Çıkış',5,'Satış işlemi','2026-05-22 04:31:10'),(50,99,'Çıkış',5,'Satış işlemi','2026-05-22 04:31:10'),(51,89,'Çıkış',2,'Satış işlemi','2026-05-22 04:31:10'),(52,35,'Çıkış',3,'Satış işlemi','2026-05-22 04:31:10'),(53,4,'Çıkış',3,'Satış işlemi','2026-05-22 04:31:10'),(54,73,'Çıkış',4,'Satış işlemi','2026-05-22 04:31:10'),(55,68,'Çıkış',4,'Satış işlemi','2026-05-22 04:31:10'),(56,28,'Çıkış',4,'Satış işlemi','2026-05-22 04:31:10'),(57,49,'Çıkış',3,'Satış işlemi','2026-05-22 04:31:10'),(58,77,'Çıkış',2,'Satış işlemi','2026-05-22 04:31:10'),(59,11,'Çıkış',2,'Satış işlemi','2026-05-22 04:31:10'),(60,7,'Çıkış',5,'Satış işlemi','2026-05-22 04:31:10'),(61,63,'Çıkış',4,'Satış işlemi','2026-05-22 04:31:10'),(62,78,'Çıkış',4,'Satış işlemi','2026-05-22 04:31:10'),(63,15,'Çıkış',5,'Satış işlemi','2026-05-22 04:31:10'),(64,37,'Çıkış',4,'Satış işlemi','2026-05-22 04:31:10'),(65,8,'Çıkış',3,'Satış işlemi','2026-05-22 04:31:10'),(66,1,'Çıkış',2,'Satış işlemi','2026-05-22 04:31:10'),(67,27,'Çıkış',2,'Satış işlemi','2026-05-22 04:31:10'),(68,64,'Çıkış',4,'Satış işlemi','2026-05-22 04:31:10'),(69,65,'Çıkış',5,'Satış işlemi','2026-05-22 04:31:10'),(70,6,'Çıkış',4,'Satış işlemi','2026-05-22 04:31:10'),(71,56,'Çıkış',1,'Satış işlemi','2026-05-22 04:31:10'),(72,3,'Çıkış',3,'Satış işlemi','2026-05-22 04:31:10'),(73,54,'Çıkış',4,'Satış işlemi','2026-05-22 04:31:10'),(74,38,'Çıkış',1,'Satış işlemi','2026-05-22 04:31:10'),(75,15,'Çıkış',3,'Satış işlemi','2026-05-22 04:31:10'),(76,12,'Çıkış',1,'Satış işlemi','2026-05-22 04:31:10'),(77,56,'Çıkış',2,'Satış işlemi','2026-05-22 04:31:10'),(78,40,'Çıkış',3,'Satış işlemi','2026-05-22 04:31:10'),(79,96,'Çıkış',2,'Satış işlemi','2026-05-22 04:31:10'),(80,68,'Çıkış',5,'Satış işlemi','2026-05-22 04:31:10'),(81,19,'Çıkış',2,'Satış işlemi','2026-05-22 04:31:10'),(82,31,'Çıkış',3,'Satış işlemi','2026-05-22 04:31:10'),(83,14,'Çıkış',2,'Satış işlemi','2026-05-22 04:31:10'),(84,10,'Çıkış',4,'Satış işlemi','2026-05-22 04:31:10'),(85,23,'Çıkış',4,'Satış işlemi','2026-05-22 04:31:10'),(86,99,'Çıkış',5,'Satış işlemi','2026-05-22 04:31:10'),(87,83,'Çıkış',3,'Satış işlemi','2026-05-22 04:31:10'),(88,82,'Çıkış',3,'Satış işlemi','2026-05-22 04:31:10'),(89,80,'Çıkış',2,'Satış işlemi','2026-05-22 04:31:10'),(90,36,'Çıkış',1,'Satış işlemi','2026-05-22 04:31:10'),(91,76,'Çıkış',3,'Satış işlemi','2026-05-22 04:31:10'),(92,8,'Çıkış',3,'Satış işlemi','2026-05-22 04:31:10'),(93,65,'Çıkış',5,'Satış işlemi','2026-05-22 04:31:10'),(94,50,'Çıkış',5,'Satış işlemi','2026-05-22 04:31:10'),(95,60,'Çıkış',1,'Satış işlemi','2026-05-22 04:31:10'),(96,61,'Çıkış',1,'Satış işlemi','2026-05-22 04:31:10'),(97,65,'Çıkış',4,'Satış işlemi','2026-05-22 04:31:10'),(98,20,'Çıkış',1,'Satış işlemi','2026-05-22 04:31:10'),(99,32,'Çıkış',4,'Satış işlemi','2026-05-22 04:31:10'),(100,48,'Çıkış',4,'Satış işlemi','2026-05-22 04:31:10'),(101,37,'Çıkış',4,'Satış işlemi','2026-05-22 04:31:10'),(102,83,'Çıkış',3,'Satış işlemi','2026-05-22 04:31:10'),(103,43,'Çıkış',1,'Satış işlemi','2026-05-22 04:31:10'),(104,9,'Çıkış',5,'Satış işlemi','2026-05-22 04:31:10'),(105,78,'Çıkış',1,'Satış işlemi','2026-05-22 04:31:10'),(106,20,'Çıkış',5,'Satış işlemi','2026-05-22 04:31:10'),(107,43,'Çıkış',2,'Satış işlemi','2026-05-22 04:31:10'),(108,17,'Çıkış',1,'Satış işlemi','2026-05-22 04:31:10'),(109,77,'Çıkış',3,'Satış işlemi','2026-05-22 04:31:10'),(110,86,'Çıkış',5,'Satış işlemi','2026-05-22 04:31:10'),(111,40,'Çıkış',2,'Satış işlemi','2026-05-22 04:31:10'),(112,24,'Çıkış',3,'Satış işlemi','2026-05-22 04:31:10'),(113,10,'Çıkış',3,'Satış işlemi','2026-05-22 04:31:10'),(114,5,'Çıkış',5,'Satış işlemi','2026-05-22 04:31:10'),(115,85,'Çıkış',2,'Satış işlemi','2026-05-22 04:31:10'),(116,44,'Çıkış',5,'Satış işlemi','2026-05-22 04:31:10'),(117,99,'Çıkış',4,'Satış işlemi','2026-05-22 04:31:10'),(118,80,'Çıkış',1,'Satış işlemi','2026-05-22 04:31:10'),(119,22,'Çıkış',4,'Satış işlemi','2026-05-22 04:31:10'),(120,58,'Çıkış',2,'Satış işlemi','2026-05-22 04:31:10'),(121,30,'Çıkış',3,'Satış işlemi','2026-05-22 04:31:10'),(122,69,'Çıkış',4,'Satış işlemi','2026-05-22 04:31:10'),(123,99,'Çıkış',4,'Satış işlemi','2026-05-22 04:31:10'),(124,37,'Çıkış',5,'Satış işlemi','2026-05-22 04:31:10'),(125,18,'Çıkış',4,'Satış işlemi','2026-05-22 04:31:10'),(126,33,'Çıkış',3,'Satış işlemi','2026-05-22 04:31:10'),(127,7,'Çıkış',5,'Satış işlemi','2026-05-22 04:31:10'),(128,35,'Çıkış',4,'Satış işlemi','2026-05-22 04:31:10'),(129,58,'Çıkış',3,'Satış işlemi','2026-05-22 04:31:10'),(130,31,'Çıkış',1,'Satış işlemi','2026-05-22 04:31:10'),(131,50,'Çıkış',3,'Satış işlemi','2026-05-22 04:31:10'),(132,84,'Çıkış',2,'Satış işlemi','2026-05-22 04:31:10'),(133,43,'Çıkış',1,'Satış işlemi','2026-05-22 04:31:10'),(134,19,'Çıkış',3,'Satış işlemi','2026-05-22 04:31:10'),(135,11,'Çıkış',1,'Satış işlemi','2026-05-22 04:31:10'),(136,68,'Çıkış',3,'Satış işlemi','2026-05-22 04:31:10'),(137,55,'Çıkış',2,'Satış işlemi','2026-05-22 04:31:10'),(138,15,'Çıkış',3,'Satış işlemi','2026-05-22 04:31:10'),(139,59,'Çıkış',4,'Satış işlemi','2026-05-22 04:31:11'),(140,9,'Çıkış',5,'Satış işlemi','2026-05-22 04:31:11'),(141,15,'Çıkış',5,'Satış işlemi','2026-05-22 04:31:11'),(142,38,'Çıkış',4,'Satış işlemi','2026-05-22 04:31:11'),(143,24,'Çıkış',1,'Satış işlemi','2026-05-22 04:31:11'),(144,86,'Çıkış',5,'Satış işlemi','2026-05-22 04:31:11'),(145,71,'Çıkış',4,'Satış işlemi','2026-05-22 04:31:11'),(146,3,'Çıkış',1,'Satış işlemi','2026-05-22 04:31:11'),(147,50,'Çıkış',4,'Satış işlemi','2026-05-22 04:31:11'),(148,45,'Çıkış',2,'Satış işlemi','2026-05-22 04:31:11'),(149,18,'Çıkış',3,'Satış işlemi','2026-05-22 04:31:11'),(150,6,'Çıkış',5,'Satış işlemi','2026-05-22 04:31:11'),(151,81,'Çıkış',2,'Satış işlemi','2026-05-22 04:31:11'),(152,24,'Çıkış',1,'Satış işlemi','2026-05-22 04:31:11'),(153,26,'Çıkış',1,'Satış işlemi','2026-05-22 04:31:11'),(154,61,'Çıkış',2,'Satış işlemi','2026-05-22 04:31:11'),(155,33,'Çıkış',3,'Satış işlemi','2026-05-22 04:31:11'),(156,5,'Çıkış',3,'Satış işlemi','2026-05-22 04:31:11'),(157,48,'Çıkış',4,'Satış işlemi','2026-05-22 04:31:11'),(158,89,'Çıkış',3,'Satış işlemi','2026-05-22 04:31:11'),(159,64,'Çıkış',5,'Satış işlemi','2026-05-22 04:31:11'),(160,24,'Çıkış',3,'Satış işlemi','2026-05-22 04:31:11'),(161,70,'Çıkış',1,'Satış işlemi','2026-05-22 04:31:11'),(162,38,'Çıkış',1,'Satış işlemi','2026-05-22 04:31:11'),(163,55,'Çıkış',4,'Satış işlemi','2026-05-22 04:31:11'),(164,22,'Çıkış',3,'Satış işlemi','2026-05-22 04:31:11'),(165,89,'Çıkış',4,'Satış işlemi','2026-05-22 04:31:11'),(166,8,'Çıkış',4,'Satış işlemi','2026-05-22 04:31:11'),(167,10,'Çıkış',3,'Satış işlemi','2026-05-22 04:31:11'),(168,86,'Çıkış',5,'Satış işlemi','2026-05-22 04:31:11'),(169,87,'Çıkış',2,'Satış işlemi','2026-05-22 04:31:11'),(170,57,'Çıkış',1,'Satış işlemi','2026-05-22 04:31:11'),(171,5,'Çıkış',2,'Satış işlemi','2026-05-22 04:31:11'),(172,78,'Çıkış',2,'Satış işlemi','2026-05-22 04:31:11'),(173,74,'Çıkış',2,'Satış işlemi','2026-05-22 04:31:11'),(174,32,'Çıkış',1,'Satış işlemi','2026-05-22 04:31:11'),(175,15,'Çıkış',2,'Satış işlemi','2026-05-22 04:31:11'),(176,100,'Çıkış',4,'Satış işlemi','2026-05-22 04:31:11'),(177,34,'Çıkış',2,'Satış işlemi','2026-05-22 04:31:11'),(178,10,'Çıkış',4,'Satış işlemi','2026-05-22 04:31:11'),(179,86,'Çıkış',1,'Satış işlemi','2026-05-22 04:31:11'),(180,45,'Çıkış',5,'Satış işlemi','2026-05-22 04:31:11'),(181,90,'Çıkış',3,'Satış işlemi','2026-05-22 04:31:11'),(182,82,'Çıkış',5,'Satış işlemi','2026-05-22 04:31:11'),(183,23,'Çıkış',2,'Satış işlemi','2026-05-22 04:31:11'),(184,4,'Çıkış',5,'Satış işlemi','2026-05-22 04:31:11'),(185,47,'Çıkış',5,'Satış işlemi','2026-05-22 04:31:11'),(186,9,'Çıkış',3,'Satış işlemi','2026-05-22 04:31:11'),(187,74,'Çıkış',2,'Satış işlemi','2026-05-22 04:31:11'),(188,10,'Çıkış',3,'Satış işlemi','2026-05-22 04:31:11'),(189,45,'Çıkış',5,'Satış işlemi','2026-05-22 04:31:11'),(190,17,'Çıkış',5,'Satış işlemi','2026-05-22 04:31:11'),(191,100,'Çıkış',2,'Satış işlemi','2026-05-22 04:31:11'),(192,16,'Çıkış',5,'Satış işlemi','2026-05-22 04:31:11'),(193,29,'Çıkış',3,'Satış işlemi','2026-05-22 04:31:11'),(194,45,'Çıkış',1,'Satış işlemi','2026-05-22 04:31:11'),(195,67,'Çıkış',3,'Satış işlemi','2026-05-22 04:31:11'),(196,79,'Çıkış',1,'Satış işlemi','2026-05-22 04:31:11'),(197,69,'Çıkış',5,'Satış işlemi','2026-05-22 04:31:11'),(198,85,'Çıkış',5,'Satış işlemi','2026-05-22 04:31:11'),(199,11,'Çıkış',1,'Satış işlemi','2026-05-22 04:31:11'),(200,54,'Çıkış',1,'Satış işlemi','2026-05-22 04:31:11'),(201,80,'Çıkış',4,'Satış işlemi','2026-05-22 04:31:11'),(202,82,'Çıkış',5,'Satış işlemi','2026-05-22 04:31:11'),(203,52,'Çıkış',1,'Satış işlemi','2026-05-22 04:31:11'),(204,39,'Çıkış',1,'Satış işlemi','2026-05-22 04:31:11'),(205,59,'Çıkış',2,'Satış işlemi','2026-05-22 04:31:11'),(206,78,'Çıkış',3,'Satış işlemi','2026-05-22 04:31:11'),(207,19,'Çıkış',3,'Satış işlemi','2026-05-22 04:31:11');
/*!40000 ALTER TABLE `stock_movements` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `suppliers`
--

DROP TABLE IF EXISTS `suppliers`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `suppliers` (
  `id` int NOT NULL AUTO_INCREMENT,
  `company_name` varchar(150) NOT NULL,
  `phone` varchar(20) DEFAULT NULL,
  `email` varchar(100) DEFAULT NULL,
  `address` text,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `suppliers`
--

LOCK TABLES `suppliers` WRITE;
/*!40000 ALTER TABLE `suppliers` DISABLE KEYS */;
INSERT INTO `suppliers` VALUES (1,'Coca Cola İçecek','02121234567','info@ccicecek.com','İstanbul'),(2,'Ülker Gıda','02162345678','tedarik@ulker.com','İstanbul'),(3,'Eti Gıda','02223334455','satis@eti.com','Eskişehir'),(4,'Pınar Süt','02324567890','info@pinar.com','İzmir'),(5,'Torku','03325556677','tedarik@torku.com','Konya'),(6,'Migros Toptan','02165557788','info@migrostoptan.com','İstanbul'),(7,'Eczacıbaşı Tüketim','02126667788','iletisim@eczacibasi.com','İstanbul'),(8,'Duru Bulgur','03384445566','satis@duru.com','Karaman'),(9,'Banvit','02665443322','info@banvit.com','Balıkesir'),(10,'SuperFresh','02164443322','info@superfresh.com','İstanbul');
/*!40000 ALTER TABLE `suppliers` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `support_tickets`
--

DROP TABLE IF EXISTS `support_tickets`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `support_tickets` (
  `id` int NOT NULL AUTO_INCREMENT,
  `customer_id` int DEFAULT NULL,
  `subject` varchar(150) NOT NULL,
  `message` text NOT NULL,
  `status` varchar(30) DEFAULT 'Açık',
  `created_at` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `customer_id` (`customer_id`),
  CONSTRAINT `support_tickets_ibfk_1` FOREIGN KEY (`customer_id`) REFERENCES `customers` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=31 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `support_tickets`
--

LOCK TABLES `support_tickets` WRITE;
/*!40000 ALTER TABLE `support_tickets` DISABLE KEYS */;
INSERT INTO `support_tickets` VALUES (1,84,'İade Talebi','Ürünü iade etmek istiyorum','Açık','2026-01-22 10:29:00'),(2,40,'Fiyat Şikayeti','Raftaki fiyat ile kasa fiyatı farklıydı','Çözüldü','2026-05-10 20:07:00'),(3,82,'İade Talebi','Ürünü iade etmek istiyorum','Çözüldü','2026-05-08 12:08:00'),(4,62,'Fiyat Şikayeti','Raftaki fiyat ile kasa fiyatı farklıydı','Çözüldü','2026-05-12 15:44:00'),(5,71,'Kasiyer Şikayeti','Kasada uzun süre bekledim','Açık','2026-02-22 21:38:00'),(6,11,'Ürün Önerisi','Bu ürünü mağazada görmek istiyorum','Çözüldü','2026-05-12 10:58:00'),(7,73,'Bozuk Ürün','Aldığım ürün hasarlı çıktı','Açık','2026-05-17 12:36:00'),(8,69,'Fiyat Şikayeti','Raftaki fiyat ile kasa fiyatı farklıydı','Açık','2026-03-17 16:07:00'),(9,88,'Fiyat Şikayeti','Raftaki fiyat ile kasa fiyatı farklıydı','Çözüldü','2026-01-17 16:51:00'),(10,8,'Kasiyer Şikayeti','Kasada uzun süre bekledim','Açık','2026-05-14 16:36:00'),(11,8,'Ürün Önerisi','Bu ürünü mağazada görmek istiyorum','Çözüldü','2026-03-01 15:16:00'),(12,1,'Eksik Ürün','Siparişimde eksik ürün vardı','Açık','2026-05-03 09:27:00'),(13,45,'Eksik Ürün','Siparişimde eksik ürün vardı','Açık','2026-05-02 10:59:00'),(14,61,'Bozuk Ürün','Aldığım ürün hasarlı çıktı','Çözüldü','2026-04-06 21:08:00'),(15,99,'Eksik Ürün','Siparişimde eksik ürün vardı','Çözüldü','2026-03-13 16:55:00'),(16,49,'Kasiyer Şikayeti','Kasada uzun süre bekledim','Açık','2026-05-05 19:55:00'),(17,45,'Bozuk Ürün','Aldığım ürün hasarlı çıktı','Açık','2026-05-13 17:08:00'),(18,94,'Fiyat Şikayeti','Raftaki fiyat ile kasa fiyatı farklıydı','Açık','2026-01-10 16:43:00'),(19,93,'Ürün Önerisi','Bu ürünü mağazada görmek istiyorum','Çözüldü','2026-05-13 12:15:00'),(20,59,'İade Talebi','Ürünü iade etmek istiyorum','Açık','2026-03-07 20:48:00'),(21,15,'Bozuk Ürün','Aldığım ürün hasarlı çıktı','Çözüldü','2026-05-01 12:13:00'),(22,9,'Bozuk Ürün','Aldığım ürün hasarlı çıktı','Açık','2026-04-20 19:45:00'),(23,7,'Fiyat Şikayeti','Raftaki fiyat ile kasa fiyatı farklıydı','Açık','2026-04-15 12:34:00'),(24,28,'Bozuk Ürün','Aldığım ürün hasarlı çıktı','Açık','2026-05-10 12:52:00'),(25,94,'Ürün Önerisi','Bu ürünü mağazada görmek istiyorum','Çözüldü','2026-05-20 21:43:00'),(26,42,'Fiyat Şikayeti','Raftaki fiyat ile kasa fiyatı farklıydı','Çözüldü','2026-02-22 17:14:00'),(27,53,'İade Talebi','Ürünü iade etmek istiyorum','Çözüldü','2026-01-18 18:56:00'),(28,95,'Fiyat Şikayeti','Raftaki fiyat ile kasa fiyatı farklıydı','Çözüldü','2026-05-16 09:22:00'),(29,83,'Eksik Ürün','Siparişimde eksik ürün vardı','Çözüldü','2026-05-11 20:26:00'),(30,53,'Fiyat Şikayeti','Raftaki fiyat ile kasa fiyatı farklıydı','Çözüldü','2026-04-06 21:34:00');
/*!40000 ALTER TABLE `support_tickets` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `users`
--

DROP TABLE IF EXISTS `users`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `users` (
  `id` int NOT NULL AUTO_INCREMENT,
  `username` varchar(50) NOT NULL,
  `password` varchar(100) NOT NULL,
  `role` varchar(50) DEFAULT 'Admin',
  PRIMARY KEY (`id`),
  UNIQUE KEY `username` (`username`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `users`
--

LOCK TABLES `users` WRITE;
/*!40000 ALTER TABLE `users` DISABLE KEYS */;
/*!40000 ALTER TABLE `users` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-05-22  8:06:00
