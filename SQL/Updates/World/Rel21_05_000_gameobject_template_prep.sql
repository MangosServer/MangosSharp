DROP TABLE IF EXISTS `gameobjects`;

--
-- Table structure for table `gameobject_template`
--

DROP TABLE IF EXISTS `gameobject_template`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `gameobject_template` (
  `entry` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'Id of the gameobject template',
  `type` tinyint(3) unsigned NOT NULL DEFAULT '0' COMMENT 'GameObject Type',
  `displayId` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'A display model identifier for the Item.',
  `name` varchar(100) NOT NULL DEFAULT '' COMMENT 'Object''s Name',
  `faction` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'Object''s faction, if any. (See FactionTemplate.dbc)',
  `flags` int(10) unsigned NOT NULL DEFAULT '0' COMMENT 'GameObject Flag',
  `size` float NOT NULL DEFAULT '1' COMMENT 'Object''s size must be set because graphic models can be resample.',
  `data0` int(10) unsigned NOT NULL DEFAULT '0' COMMENT 'The content of the data fields depends on the gameobject type',
  `data1` int(10) unsigned NOT NULL DEFAULT '0' COMMENT 'The content of the data fields depends on the gameobject type',
  `data2` int(10) unsigned NOT NULL DEFAULT '0' COMMENT 'The content of the data fields depends on the gameobject type',
  `data3` int(10) unsigned NOT NULL DEFAULT '0' COMMENT 'The content of the data fields depends on the gameobject type',
  `data4` int(10) unsigned NOT NULL DEFAULT '0' COMMENT 'The content of the data fields depends on the gameobject type',
  `data5` int(10) unsigned NOT NULL DEFAULT '0' COMMENT 'The content of the data fields depends on the gameobject type',
  `data6` int(10) unsigned NOT NULL DEFAULT '0' COMMENT 'The content of the data fields depends on the gameobject type',
  `data7` int(10) unsigned NOT NULL DEFAULT '0' COMMENT 'The content of the data fields depends on the gameobject type',
  `data8` int(10) unsigned NOT NULL DEFAULT '0' COMMENT 'The content of the data fields depends on the gameobject type',
  `data9` int(10) unsigned NOT NULL DEFAULT '0' COMMENT 'The content of the data fields depends on the gameobject type',
  `data10` int(10) unsigned NOT NULL DEFAULT '0' COMMENT 'The content of the data fields depends on the gameobject type',
  `data11` int(10) unsigned NOT NULL DEFAULT '0' COMMENT 'The content of the data fields depends on the gameobject type',
  `data12` int(10) unsigned NOT NULL DEFAULT '0' COMMENT 'The content of the data fields depends on the gameobject type',
  `data13` int(10) unsigned NOT NULL DEFAULT '0' COMMENT 'The content of the data fields depends on the gameobject type',
  `data14` int(10) unsigned NOT NULL DEFAULT '0' COMMENT 'The content of the data fields depends on the gameobject type',
  `data15` int(10) unsigned NOT NULL DEFAULT '0' COMMENT 'The content of the data fields depends on the gameobject type',
  `data16` int(10) unsigned NOT NULL DEFAULT '0' COMMENT 'The content of the data fields depends on the gameobject type',
  `data17` int(10) unsigned NOT NULL DEFAULT '0' COMMENT 'The content of the data fields depends on the gameobject type',
  `data18` int(10) unsigned NOT NULL DEFAULT '0' COMMENT 'The content of the data fields depends on the gameobject type',
  `data19` int(10) unsigned NOT NULL DEFAULT '0' COMMENT 'The content of the data fields depends on the gameobject type',
  `data20` int(10) unsigned NOT NULL DEFAULT '0' COMMENT 'The content of the data fields depends on the gameobject type',
  `data21` int(10) unsigned NOT NULL DEFAULT '0' COMMENT 'The content of the data fields depends on the gameobject type',
  `data22` int(10) unsigned NOT NULL DEFAULT '0' COMMENT 'The content of the data fields depends on the gameobject type',
  `data23` int(10) unsigned NOT NULL DEFAULT '0' COMMENT 'The content of the data fields depends on the gameobject type',
  `mingold` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'DEPRECATED: Defines money looted from the game object.',
  `maxgold` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'DEPRECATED: Defines money looted from the game object.',
  PRIMARY KEY (`entry`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC COMMENT='Gameobject System';
/*!40101 SET character_set_client = @saved_cs_client */;

INSERT INTO `db_version` VALUES (21, 05, 000, "gameobject_template_prep", "gameobject_template_prep");