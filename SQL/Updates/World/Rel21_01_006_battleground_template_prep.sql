--
-- Table structure for table `battleground_template`
--

DROP TABLE IF EXISTS `battleground_template`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `battleground_template` (
  `id` mediumint(8) unsigned NOT NULL COMMENT 'The battleground ID (See BattlemasterList.dbc).',
  `MinPlayersPerTeam` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'The minimum number of players that need to join the battleground.',
  `MaxPlayersPerTeam` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'Controls how many players from each team can join the battleground.',
  `MinLvl` tinyint(3) unsigned NOT NULL DEFAULT '0' COMMENT 'The minimum level that players need to be in order to join the battleground.',
  `MaxLvl` tinyint(3) unsigned NOT NULL DEFAULT '0' COMMENT 'The maximum level that players need to be in order to join the battleground.',
  `AllianceStartLoc` mediumint(8) unsigned NOT NULL COMMENT 'The location where the alliance players get teleported to in the battleground.',
  `AllianceStartO` float NOT NULL COMMENT 'The orientation of the alliance players upon teleport.',
  `HordeStartLoc` mediumint(8) unsigned NOT NULL COMMENT 'The location where the horde players get teleported to in the battleground.',
  `HordeStartO` float NOT NULL COMMENT 'The orientation of the horde players upon teleport into the battleground.',
  `StartMaxDist` float NOT NULL DEFAULT '0' COMMENT 'The maximium distance from the start location.',
  `Comment` varchar(200) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

INSERT INTO `db_version` VALUES (21, 01, 006, "battleground_template_prep", "battleground_template_prep");