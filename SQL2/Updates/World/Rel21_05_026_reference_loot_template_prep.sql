
--
-- Table structure for table `reference_loot_template`
--

DROP TABLE IF EXISTS `reference_loot_template`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `reference_loot_template` (
  `entry` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'The ID of the loot definition (loot template).',
  `item` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'Template ID of the item which can be included into the loot.',
  `ChanceOrQuestChance` float NOT NULL DEFAULT '100' COMMENT 'Meaning of that field is a bit different depending on its sign.',
  `groupid` tinyint(3) unsigned NOT NULL DEFAULT '0' COMMENT 'A group is a set of loot definitions.',
  `mincountOrRef` mediumint(9) NOT NULL DEFAULT '1' COMMENT 'The total number of copies of an item or may reference another loot template',
  `maxcount` tinyint(3) unsigned NOT NULL DEFAULT '1' COMMENT 'For non-reference entries - the maximum number of copies of the item.',
  `condition_id` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'Value that represents a loot condition that must be filled.',
  PRIMARY KEY (`entry`,`item`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC COMMENT='Loot System';
/*!40101 SET character_set_client = @saved_cs_client */;

INSERT INTO `db_version` VALUES (21, 05, 026, "reference_loot_template_prep", "reference_loot_template_prep");