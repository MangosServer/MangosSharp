DROP TABLE IF EXISTS `spawns_gameobjects`;

--
-- Table structure for table `gameobject`
--

DROP TABLE IF EXISTS `gameobject`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `gameobject` (
  `guid` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT COMMENT 'The unique identifier of the game object spawn.',
  `id` MEDIUMINT(8) UNSIGNED NOT NULL DEFAULT '0' COMMENT 'GameObject ID (See gameobject_template.entry).',
  `map` SMALLINT(5) UNSIGNED NOT NULL DEFAULT '0' COMMENT 'The map id that the game object is located on (See map.dbc).',
  `position_x` FLOAT NOT NULL DEFAULT '0' COMMENT 'The x location of the game object.',
  `position_y` FLOAT NOT NULL DEFAULT '0' COMMENT 'The y location of the game object.',
  `position_z` FLOAT NOT NULL DEFAULT '0' COMMENT 'The z location of the game object.',
  `orientation` FLOAT NOT NULL DEFAULT '0' COMMENT 'The orientation of the game object.',
  `rotation0` FLOAT NOT NULL DEFAULT '0' COMMENT 'The amount of rotation of an object along one of the axis.',
  `rotation1` FLOAT NOT NULL DEFAULT '0' COMMENT 'The amount of rotation of an object along one of the axis.',
  `rotation2` FLOAT NOT NULL DEFAULT '0' COMMENT 'The amount of rotation of an object along one of the axis.',
  `rotation3` FLOAT NOT NULL DEFAULT '0' COMMENT 'The amount of rotation of an object along one of the axis.',
  `spawntimesecs` INT(11) NOT NULL DEFAULT '0' COMMENT 'The respawn time for the game object, defined in seconds till respawn.',
  `animprogress` TINYINT(3) UNSIGNED NOT NULL DEFAULT '0' COMMENT 'Not really known what this is used for at this time (see description).',
  `state` TINYINT(3) UNSIGNED NOT NULL DEFAULT '0',
  PRIMARY KEY (`guid`),
  KEY `idx_map` (`map`),
  KEY `idx_id` (`id`)
) ENGINE=MYISAM AUTO_INCREMENT=632463 DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC COMMENT='Gameobject System';
/*!40101 SET character_set_client = @saved_cs_client */;

INSERT INTO `db_version` VALUES (21, 04, 000, "gameobject_prep", "gameobject_prep");