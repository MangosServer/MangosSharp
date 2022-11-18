--
-- Table structure for table `game_event`
--

DROP TABLE IF EXISTS `game_event`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `game_event` (
  `entry` mediumint(8) unsigned NOT NULL COMMENT 'ID of the event.',
  `start_time` timestamp NOT NULL DEFAULT '0000-00-00 00:00:00' COMMENT 'Global starting date for the event.',
  `end_time` timestamp NOT NULL DEFAULT '0000-00-00 00:00:00' COMMENT 'Global ending date of the event.',
  `occurence` bigint(20) unsigned NOT NULL DEFAULT '86400' COMMENT 'Event periodicity (minutes).',
  `length` bigint(20) unsigned NOT NULL DEFAULT '43200' COMMENT 'Event duration (minutes).',
  `holiday` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'Holiday ID.',
  `description` varchar(255) DEFAULT NULL COMMENT 'Description of the event.',
  PRIMARY KEY (`entry`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

INSERT INTO `db_version` VALUES (21, 01, 001, "game_event_prep", "game_event_prep");