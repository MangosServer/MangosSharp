DROP TABLE IF EXISTS `spawns_creatures`;

--
-- Table structure for table `creature`
--

DROP TABLE IF EXISTS `creature`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `creature` (
  `guid` int(10) unsigned NOT NULL AUTO_INCREMENT COMMENT 'A unique identifier given to each creature to distinguish creatures.',
  `id` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'The id of the template that is used when instantiating this creature.',
  `map` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'The map id of the location of the creature (See map.dbc).',
  `modelid` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'The model id of the the creature. ',
  `equipment_id` mediumint(9) NOT NULL DEFAULT '0' COMMENT 'The ID of the equipment that the creature is using.',
  `position_x` float NOT NULL DEFAULT '0' COMMENT 'The x position of the creature.',
  `position_y` float NOT NULL DEFAULT '0' COMMENT 'The y position of the creature.',
  `position_z` float NOT NULL DEFAULT '0' COMMENT 'The z position of the creature.',
  `orientation` float NOT NULL DEFAULT '0' COMMENT 'The orientation of the creature. (North = 0.0; South = pi (3.14159))',
  `spawntimesecs` int(10) unsigned NOT NULL DEFAULT '120' COMMENT 'The respawn time of the creature in seconds. ',
  `spawndist` float NOT NULL DEFAULT '5' COMMENT 'The maximum distance that the creature should spawn from its spawn point.',
  `currentwaypoint` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'The current waypoint of the creature.',
  `curhealth` int(10) unsigned NOT NULL DEFAULT '1' COMMENT 'The current health of the creature.',
  `curmana` int(10) unsigned NOT NULL DEFAULT '0' COMMENT 'The current mana of the creature.',
  `DeathState` tinyint(3) unsigned NOT NULL DEFAULT '0' COMMENT 'The creature''s death state.',
  `MovementType` tinyint(3) unsigned NOT NULL DEFAULT '0' COMMENT 'The movement type associated with this creature.',
  PRIMARY KEY (`guid`),
  KEY `idx_map` (`map`),
  KEY `index_id` (`id`)
) ENGINE=MyISAM AUTO_INCREMENT=590016 DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC COMMENT='Creature System';
/*!40101 SET character_set_client = @saved_cs_client */;

INSERT INTO `db_version` VALUES (21, 03, 000, "creature_prep", "creature_prep");