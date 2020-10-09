--
-- Table structure for table `creature_template_spells`
--

DROP TABLE IF EXISTS `creature_template_spells`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `creature_template_spells` (
  `entry` mediumint(8) unsigned NOT NULL COMMENT 'This references the unique ID in table creature_template.',
  `spell1` mediumint(8) unsigned NOT NULL COMMENT 'The spell identifier. The value has to match with a defined spell identifier.',
  `spell2` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'The spell identifier. The value has to match with a defined spell identifier.',
  `spell3` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'The spell identifier. The value has to match with a defined spell identifier.',
  `spell4` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'The spell identifier. The value has to match with a defined spell identifier.',
  PRIMARY KEY (`entry`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC COMMENT='Creature System (Spells used by creature)';
/*!40101 SET character_set_client = @saved_cs_client */;

INSERT INTO `db_version` VALUES (21, 07, 000, "creature_template_prep", "creature_template_prep");