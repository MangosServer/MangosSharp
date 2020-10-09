--
-- Table structure for table `command`
--

DROP TABLE IF EXISTS `command`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `command` (
  `name` varchar(50) NOT NULL DEFAULT '' COMMENT 'The Command Name.',
  `security` tinyint(3) unsigned NOT NULL DEFAULT '0' COMMENT 'The minimum security level to use the command (See account.gmlevel) in the realm',
  `help` longtext COMMENT 'The help text for the command which explains it''s use and parameters.',
  PRIMARY KEY (`name`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC COMMENT='Chat System';
/*!40101 SET character_set_client = @saved_cs_client */;


--
-- Table structure for table `conditions`
--

DROP TABLE IF EXISTS `conditions`;

CREATE TABLE `conditions` (
  `condition_entry` mediumint(8) unsigned NOT NULL AUTO_INCREMENT COMMENT 'Identifier',
  `type` tinyint(3) NOT NULL DEFAULT '0' COMMENT 'Type of the condition.',
  `value1` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'Data field One for the condition.',
  `value2` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'Data field Two for the condition.',
  `comments` varchar(200) DEFAULT NULL,
  PRIMARY KEY (`condition_entry`),
  UNIQUE KEY `unique_conditions` (`type`,`value1`,`value2`)
) ENGINE=MyISAM AUTO_INCREMENT=1791 DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC COMMENT='Condition System';
/*!40101 SET character_set_client = @saved_cs_client */;


--
-- Table structure for table `creature_addon`
--

DROP TABLE IF EXISTS `creature_addon`;

CREATE TABLE `creature_addon` (
  `guid` int(10) unsigned NOT NULL DEFAULT '0' COMMENT 'Signifies a unique creature guid (See creature.guid).',
  `mount` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'The model ID of the mount to be used to make the creature appear mounted.',
  `bytes1` int(10) unsigned NOT NULL DEFAULT '0' COMMENT 'The value here overrides the value for the creature''s unit field UNIT_FIELD_BYTE',
  `b2_0_sheath` tinyint(3) unsigned NOT NULL DEFAULT '0' COMMENT 'SheathState.',
  `b2_1_flags` tinyint(3) unsigned NOT NULL DEFAULT '0' COMMENT 'The value here overrides the value for the creature''s unit field UNIT_FIELD_BYTE',
  `emote` int(10) unsigned NOT NULL DEFAULT '0' COMMENT 'Emote ID that the creature should continually perform.',
  `moveflags` int(10) unsigned NOT NULL DEFAULT '0' COMMENT 'Flags controlling how the creature will behave animation-wise while moving.',
  `auras` text COMMENT 'This field controls any auras to be applied on the creature.',
  PRIMARY KEY (`guid`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `creature_ai_scripts`
--

DROP TABLE IF EXISTS `creature_ai_scripts`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `creature_ai_scripts` (
  `id` int(11) unsigned NOT NULL AUTO_INCREMENT COMMENT 'The unique identifier for the AI script entry.',
  `creature_id` int(11) unsigned NOT NULL DEFAULT '0' COMMENT 'This references the Unique ID in the Creature Template table.',
  `event_type` tinyint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'Event Type ID',
  `event_inverse_phase_mask` int(11) NOT NULL DEFAULT '0' COMMENT 'Mask for the event.',
  `event_chance` int(3) unsigned NOT NULL DEFAULT '100' COMMENT 'The percentage chance for this event to happen.',
  `event_flags` int(3) unsigned NOT NULL DEFAULT '0' COMMENT 'Event flags allow you to modify how events are executed.',
  `event_param1` int(11) NOT NULL DEFAULT '0' COMMENT 'Parameter Value 1 for the eventtype (See creature_ai_scripts.event_type).',
  `event_param2` int(11) NOT NULL DEFAULT '0' COMMENT 'Parameter Value 2 for the eventtype (See creature_ai_scripts.event_type).',
  `event_param3` int(11) NOT NULL DEFAULT '0' COMMENT 'Parameter Value 3 for the eventtype (See creature_ai_scripts.event_type).',
  `event_param4` int(11) NOT NULL DEFAULT '0' COMMENT 'Parameter Value 4 for the eventtype (See creature_ai_scripts.event_type).',
  `action1_type` tinyint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'The first actiontype.',
  `action1_param1` int(11) NOT NULL DEFAULT '0' COMMENT 'Parameter 1 of the action1_type (See creature_ai_scripts.action1_type)',
  `action1_param2` int(11) NOT NULL DEFAULT '0' COMMENT 'Parameter 2 of the action1_type (See creature_ai_scripts.action1_type)',
  `action1_param3` int(11) NOT NULL DEFAULT '0' COMMENT 'Parameter 3 of the action1_type (See creature_ai_scripts.action1_type)',
  `action2_type` tinyint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'The Second actiontype (See creature_ai_scripts.action2_type)',
  `action2_param1` int(11) NOT NULL DEFAULT '0' COMMENT 'Parameter 1 of action2_type (See creature_ai_scripts.action2_type)',
  `action2_param2` int(11) NOT NULL DEFAULT '0' COMMENT 'Parameter 2 of action2_type (See creature_ai_scripts.action2_type)',
  `action2_param3` int(11) NOT NULL DEFAULT '0' COMMENT 'Parameter 3 of action2_type (See creature_ai_scripts.action2_type)',
  `action3_type` tinyint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'The Third actiontype (See creature_ai_scripts.action3_type)',
  `action3_param1` int(11) NOT NULL DEFAULT '0' COMMENT 'Parameter 1 of action3_type (See creature_ai_scripts.action3_type)',
  `action3_param2` int(11) NOT NULL DEFAULT '0' COMMENT 'Parameter 2 of action3_type (See creature_ai_scripts.action3_type)',
  `action3_param3` int(11) NOT NULL DEFAULT '0' COMMENT 'Parameter 3 of action3_type (See creature_ai_scripts.action3_type)',
  `comment` varchar(255) NOT NULL DEFAULT '' COMMENT 'Documents what an event script is supposed to do.',
  PRIMARY KEY (`id`)
) ENGINE=MyISAM AUTO_INCREMENT=4334033 DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC COMMENT='EventAI Scripts';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `creature_ai_summons`
--

DROP TABLE IF EXISTS `creature_ai_summons`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `creature_ai_summons` (
  `id` int(11) unsigned NOT NULL AUTO_INCREMENT COMMENT 'This references the third action parameter in the Creature_ai_scripts.',
  `position_x` float NOT NULL DEFAULT '0' COMMENT 'The X position for the creature to be spawned.',
  `position_y` float NOT NULL DEFAULT '0' COMMENT 'The Y position for the creature to be spawned.',
  `position_z` float NOT NULL DEFAULT '0' COMMENT 'The Z position for the creature to be spawned.',
  `orientation` float NOT NULL DEFAULT '0' COMMENT 'The orientation for the creature to be spawned.',
  `spawntimesecs` int(11) unsigned NOT NULL DEFAULT '120' COMMENT 'The despawn timer for the summoned creature.',
  `comment` varchar(255) NOT NULL DEFAULT '' COMMENT 'Documents what kind of creature will be summoned.',
  PRIMARY KEY (`id`)
) ENGINE=MyISAM AUTO_INCREMENT=23 DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC COMMENT='EventAI Summoning Locations';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `creature_ai_texts`
--

DROP TABLE IF EXISTS `creature_ai_texts`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `creature_ai_texts` (
  `entry` mediumint(8) NOT NULL COMMENT 'This references a script using an action of the type ACTION_T_TEXT.',
  `content_default` text NOT NULL COMMENT 'Contains the text presented in the default language English.',
  `content_loc1` text COMMENT 'Korean localization of content_default.',
  `content_loc2` text COMMENT 'French localization of content_default.',
  `content_loc3` text COMMENT 'German localization of content_default.',
  `content_loc4` text COMMENT 'Chinese localization of content_default.',
  `content_loc5` text COMMENT 'Taiwanese localization of content_default.',
  `content_loc6` text COMMENT 'Spanish (Spain) localization of content_default',
  `content_loc7` text COMMENT 'Spanish (Latin America) localization of content_default',
  `content_loc8` text COMMENT 'Russian localization of content_default',
  `sound` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'A sound identifier.',
  `type` tinyint(3) unsigned NOT NULL DEFAULT '0' COMMENT 'The type of message to display.',
  `language` tinyint(3) unsigned NOT NULL DEFAULT '0' COMMENT 'A language identifier.',
  `emote` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'Emote ID that the creature should continually perform.',
  `comment` text COMMENT 'This documents the creature text.',
  PRIMARY KEY (`entry`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC COMMENT='Script Texts';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `creature_addon`
--

DROP TABLE IF EXISTS `creature_addon`;

CREATE TABLE `creature_addon` (
  `guid` int(10) unsigned NOT NULL DEFAULT '0' COMMENT 'Signifies a unique creature guid (See creature.guid).',
  `mount` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'The model ID of the mount to be used to make the creature appear mounted.',
  `bytes1` int(10) unsigned NOT NULL DEFAULT '0' COMMENT 'The value here overrides the value for the creature''s unit field UNIT_FIELD_BYTE',
  `b2_0_sheath` tinyint(3) unsigned NOT NULL DEFAULT '0' COMMENT 'SheathState.',
  `b2_1_flags` tinyint(3) unsigned NOT NULL DEFAULT '0' COMMENT 'The value here overrides the value for the creature''s unit field UNIT_FIELD_BYTE',
  `emote` int(10) unsigned NOT NULL DEFAULT '0' COMMENT 'Emote ID that the creature should continually perform.',
  `moveflags` int(10) unsigned NOT NULL DEFAULT '0' COMMENT 'Flags controlling how the creature will behave animation-wise while moving.',
  `auras` text COMMENT 'This field controls any auras to be applied on the creature.',
  PRIMARY KEY (`guid`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `creature_ai_scripts`
--

DROP TABLE IF EXISTS `creature_ai_scripts`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `creature_ai_scripts` (
  `id` int(11) unsigned NOT NULL AUTO_INCREMENT COMMENT 'The unique identifier for the AI script entry.',
  `creature_id` int(11) unsigned NOT NULL DEFAULT '0' COMMENT 'This references the Unique ID in the Creature Template table.',
  `event_type` tinyint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'Event Type ID',
  `event_inverse_phase_mask` int(11) NOT NULL DEFAULT '0' COMMENT 'Mask for the event.',
  `event_chance` int(3) unsigned NOT NULL DEFAULT '100' COMMENT 'The percentage chance for this event to happen.',
  `event_flags` int(3) unsigned NOT NULL DEFAULT '0' COMMENT 'Event flags allow you to modify how events are executed.',
  `event_param1` int(11) NOT NULL DEFAULT '0' COMMENT 'Parameter Value 1 for the eventtype (See creature_ai_scripts.event_type).',
  `event_param2` int(11) NOT NULL DEFAULT '0' COMMENT 'Parameter Value 2 for the eventtype (See creature_ai_scripts.event_type).',
  `event_param3` int(11) NOT NULL DEFAULT '0' COMMENT 'Parameter Value 3 for the eventtype (See creature_ai_scripts.event_type).',
  `event_param4` int(11) NOT NULL DEFAULT '0' COMMENT 'Parameter Value 4 for the eventtype (See creature_ai_scripts.event_type).',
  `action1_type` tinyint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'The first actiontype.',
  `action1_param1` int(11) NOT NULL DEFAULT '0' COMMENT 'Parameter 1 of the action1_type (See creature_ai_scripts.action1_type)',
  `action1_param2` int(11) NOT NULL DEFAULT '0' COMMENT 'Parameter 2 of the action1_type (See creature_ai_scripts.action1_type)',
  `action1_param3` int(11) NOT NULL DEFAULT '0' COMMENT 'Parameter 3 of the action1_type (See creature_ai_scripts.action1_type)',
  `action2_type` tinyint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'The Second actiontype (See creature_ai_scripts.action2_type)',
  `action2_param1` int(11) NOT NULL DEFAULT '0' COMMENT 'Parameter 1 of action2_type (See creature_ai_scripts.action2_type)',
  `action2_param2` int(11) NOT NULL DEFAULT '0' COMMENT 'Parameter 2 of action2_type (See creature_ai_scripts.action2_type)',
  `action2_param3` int(11) NOT NULL DEFAULT '0' COMMENT 'Parameter 3 of action2_type (See creature_ai_scripts.action2_type)',
  `action3_type` tinyint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'The Third actiontype (See creature_ai_scripts.action3_type)',
  `action3_param1` int(11) NOT NULL DEFAULT '0' COMMENT 'Parameter 1 of action3_type (See creature_ai_scripts.action3_type)',
  `action3_param2` int(11) NOT NULL DEFAULT '0' COMMENT 'Parameter 2 of action3_type (See creature_ai_scripts.action3_type)',
  `action3_param3` int(11) NOT NULL DEFAULT '0' COMMENT 'Parameter 3 of action3_type (See creature_ai_scripts.action3_type)',
  `comment` varchar(255) NOT NULL DEFAULT '' COMMENT 'Documents what an event script is supposed to do.',
  PRIMARY KEY (`id`)
) ENGINE=MyISAM AUTO_INCREMENT=4334033 DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC COMMENT='EventAI Scripts';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `creature_ai_summons`
--

DROP TABLE IF EXISTS `creature_ai_summons`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `creature_ai_summons` (
  `id` int(11) unsigned NOT NULL AUTO_INCREMENT COMMENT 'This references the third action parameter in the Creature_ai_scripts.',
  `position_x` float NOT NULL DEFAULT '0' COMMENT 'The X position for the creature to be spawned.',
  `position_y` float NOT NULL DEFAULT '0' COMMENT 'The Y position for the creature to be spawned.',
  `position_z` float NOT NULL DEFAULT '0' COMMENT 'The Z position for the creature to be spawned.',
  `orientation` float NOT NULL DEFAULT '0' COMMENT 'The orientation for the creature to be spawned.',
  `spawntimesecs` int(11) unsigned NOT NULL DEFAULT '120' COMMENT 'The despawn timer for the summoned creature.',
  `comment` varchar(255) NOT NULL DEFAULT '' COMMENT 'Documents what kind of creature will be summoned.',
  PRIMARY KEY (`id`)
) ENGINE=MyISAM AUTO_INCREMENT=23 DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC COMMENT='EventAI Summoning Locations';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `creature_ai_texts`
--

DROP TABLE IF EXISTS `creature_ai_texts`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `creature_ai_texts` (
  `entry` mediumint(8) NOT NULL COMMENT 'This references a script using an action of the type ACTION_T_TEXT.',
  `content_default` text NOT NULL COMMENT 'Contains the text presented in the default language English.',
  `content_loc1` text COMMENT 'Korean localization of content_default.',
  `content_loc2` text COMMENT 'French localization of content_default.',
  `content_loc3` text COMMENT 'German localization of content_default.',
  `content_loc4` text COMMENT 'Chinese localization of content_default.',
  `content_loc5` text COMMENT 'Taiwanese localization of content_default.',
  `content_loc6` text COMMENT 'Spanish (Spain) localization of content_default',
  `content_loc7` text COMMENT 'Spanish (Latin America) localization of content_default',
  `content_loc8` text COMMENT 'Russian localization of content_default',
  `sound` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'A sound identifier.',
  `type` tinyint(3) unsigned NOT NULL DEFAULT '0' COMMENT 'The type of message to display.',
  `language` tinyint(3) unsigned NOT NULL DEFAULT '0' COMMENT 'A language identifier.',
  `emote` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'Emote ID that the creature should continually perform.',
  `comment` text COMMENT 'This documents the creature text.',
  PRIMARY KEY (`entry`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC COMMENT='Script Texts';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `creature_battleground`
--

DROP TABLE IF EXISTS `creature_battleground`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `creature_battleground` (
  `guid` int(10) unsigned NOT NULL COMMENT 'A unique identifier given to each creature to distinguish them from each other.',
  `event1` tinyint(3) unsigned NOT NULL COMMENT 'Main Event.',
  `event2` tinyint(3) unsigned NOT NULL COMMENT 'Sub Event.',
  PRIMARY KEY (`guid`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC COMMENT='Creature battleground indexing system';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `creature_equip_template`
--

DROP TABLE IF EXISTS `creature_equip_template`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `creature_equip_template` (
  `entry` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'Unique Id of the equipment, no link with any official data.',
  `equipentry1` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'This is the item of the equipment used in the right hand (See Item.dbc).',
  `equipentry2` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'This is the item of the equipment used in the left hand (See Item.dbc).',
  `equipentry3` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'This is the item of the equipment used in the distance slot (See Item.dbc).',
  PRIMARY KEY (`entry`),
  UNIQUE KEY `unique_template` (`equipentry1`,`equipentry2`,`equipentry3`,`entry`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8 COMMENT='Creature System (Equipment)';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `creature_item_template`
--

DROP TABLE IF EXISTS `creature_item_template`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `creature_item_template` (
  `entry` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'The unique identifier of the item template entry.',
  `class` tinyint(3) unsigned DEFAULT '0' COMMENT 'The class of the item template.',
  `subclass` tinyint(3) unsigned DEFAULT '0' COMMENT 'The subclass of the item template.',
  `material` mediumint(8) unsigned DEFAULT '0' COMMENT 'The material that the item is made of.',
  `displayid` mediumint(8) unsigned DEFAULT '0' COMMENT 'A display model identifier for the Item.',
  `inventory_type` tinyint(3) unsigned DEFAULT '0' COMMENT 'Defines if and in which slot an item can be equipped.',
  `sheath_type` tinyint(3) unsigned DEFAULT '0' COMMENT 'The value of this field controls how characters will show or hide items worn.',
  PRIMARY KEY (`entry`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8 COMMENT='Creature System (Equipment)';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `creature_linking`
--

DROP TABLE IF EXISTS `creature_linking`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `creature_linking` (
  `guid` int(10) unsigned NOT NULL COMMENT 'This references the creature table tables unique ID.',
  `master_guid` int(10) unsigned NOT NULL COMMENT 'This references the creature table tables unique ID.',
  `flag` mediumint(8) unsigned NOT NULL COMMENT 'This flag determines how a linked creature will act.',
  PRIMARY KEY (`guid`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC COMMENT='Creature Linking System';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `creature_linking_template`
--

DROP TABLE IF EXISTS `creature_linking_template`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `creature_linking_template` (
  `entry` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'creature_template.entry of the slave mob that is linked.',
  `map` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'A map identifier',
  `master_entry` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'master entry to trigger events',
  `flag` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'This flag determines how a linked creature will act.',
  `search_range` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'IF given != 0 only mobs with spawn-dist <= search_range around the master_entry ',
  PRIMARY KEY (`entry`,`map`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC COMMENT='Creature Linking System';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `creature_movement`
--

DROP TABLE IF EXISTS `creature_movement`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `creature_movement` (
  `id` int(10) unsigned NOT NULL COMMENT 'Creature GUID (See creature.guid).',
  `point` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'An index count for all movement points attached to a creature spawn.',
  `position_x` float NOT NULL DEFAULT '0' COMMENT 'The X position for the creature''s movement point.',
  `position_y` float NOT NULL DEFAULT '0' COMMENT 'The Y position for the creature''s movement point.',
  `position_z` float NOT NULL DEFAULT '0' COMMENT 'The Z position for the creature''s movement point.',
  `waittime` int(10) unsigned NOT NULL DEFAULT '0' COMMENT 'If the creature should wait at the movement point.',
  `script_id` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'If a script should be executed.',
  `textid1` int(11) NOT NULL DEFAULT '0' COMMENT 'If a text should be emoted, this references the db_script_string table.',
  `textid2` int(11) NOT NULL DEFAULT '0' COMMENT 'If a text should be emoted, this references the db_script_string table.',
  `textid3` int(11) NOT NULL DEFAULT '0' COMMENT 'If a text should be emoted, this references the db_script_string table.',
  `textid4` int(11) NOT NULL DEFAULT '0' COMMENT 'If a text should be emoted, this references the db_script_string table.',
  `textid5` int(11) NOT NULL DEFAULT '0' COMMENT 'If a text should be emoted, this references the db_script_string table.',
  `emote` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'Emote ID that the creature should perform.',
  `spell` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'The spell identifier.',
  `orientation` float NOT NULL DEFAULT '0' COMMENT 'The orientation for the creature''s movement point.',
  `model1` mediumint(9) NOT NULL DEFAULT '0' COMMENT 'A display model identifier activated on the waypoint.',
  `model2` mediumint(9) NOT NULL DEFAULT '0' COMMENT 'An alternative display model identifier activated on the waypoint.',
  PRIMARY KEY (`id`,`point`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC COMMENT='Creature System';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `creature_movement_template`
--

DROP TABLE IF EXISTS `creature_movement_template`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `creature_movement_template` (
  `entry` mediumint(8) unsigned NOT NULL COMMENT 'Creature ID from creature_template (See creature_template.entry)',
  `point` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'An index count for all movement points attached to a creature spawn.',
  `position_x` float NOT NULL DEFAULT '0' COMMENT 'The X position for the creature''s movement point.',
  `position_y` float NOT NULL DEFAULT '0' COMMENT 'The Y position for the creature''s movement point.',
  `position_z` float NOT NULL DEFAULT '0' COMMENT 'The Z position for the creature''s movement point.',
  `waittime` int(10) unsigned NOT NULL DEFAULT '0' COMMENT 'Delay time in milliseconds',
  `script_id` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'If a script should be executed.',
  `textid1` int(11) NOT NULL DEFAULT '0' COMMENT 'Obsolete, Do not use this Field',
  `textid2` int(11) NOT NULL DEFAULT '0' COMMENT 'Obsolete, Do not use this Field',
  `textid3` int(11) NOT NULL DEFAULT '0' COMMENT 'Obsolete, Do not use this Field',
  `textid4` int(11) NOT NULL DEFAULT '0' COMMENT 'Obsolete, Do not use this Field',
  `textid5` int(11) NOT NULL DEFAULT '0' COMMENT 'Obsolete, Do not use this Field',
  `emote` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'Emote ID that the creature should perform.',
  `spell` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'The spell identifier.',
  `orientation` float NOT NULL DEFAULT '0' COMMENT 'The orientation for the creature''s movement point.',
  `model1` mediumint(9) NOT NULL DEFAULT '0' COMMENT 'A display model identifier activated on the waypoint.',
  `model2` mediumint(9) NOT NULL DEFAULT '0' COMMENT 'An alternative display model identifier activated on the waypoint.',
  PRIMARY KEY (`entry`,`point`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC COMMENT='Creature waypoint system';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `creature_template_addon`
--

DROP TABLE IF EXISTS `creature_template_addon`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `creature_template_addon` (
  `entry` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'This references the creature_template table''s unique ID.',
  `mount` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'A display model identifier used as mount for the creature_template.',
  `bytes1` int(10) unsigned NOT NULL DEFAULT '0' COMMENT 'TODO',
  `b2_0_sheath` tinyint(3) unsigned NOT NULL DEFAULT '0' COMMENT 'Defines the sheath state of the creature_template.',
  `b2_1_flags` tinyint(3) unsigned NOT NULL DEFAULT '0' COMMENT 'The value here overrides the value for the creature''s unit field UNIT_FIELD_BYTE',
  `emote` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'Emote ID that the creature should continually perform.',
  `moveflags` int(10) unsigned NOT NULL DEFAULT '0' COMMENT '\nThe flag controls how a creature_template will be animated while moving.',
  `auras` text COMMENT 'Allows to attach auras to a creature_template entry.',
  PRIMARY KEY (`entry`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `creature_template_classlevelstats`
--

DROP TABLE IF EXISTS `creature_template_classlevelstats`;

CREATE TABLE `creature_template_classlevelstats` (
  `Level` tinyint(4) NOT NULL COMMENT 'Creature level for the stats.',
  `Class` tinyint(4) NOT NULL COMMENT 'A creature''s class. The following table describes the available classes.',
  `BaseHealthExp0` mediumint(8) unsigned NOT NULL DEFAULT '1' COMMENT 'Base health value for expansion 0 aka. vanilla WoW.',
  `BaseMana` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'Base mana value for any creature of this level and class.',
  `BaseDamageExp0` float NOT NULL DEFAULT '0' COMMENT 'Base damage value for expansion 0 aka. vanilla WoW.',
  `BaseMeleeAttackPower` float NOT NULL DEFAULT '0' COMMENT 'Base melee attack power that has been factored for low level creatures.',
  `BaseRangedAttackPower` float NOT NULL DEFAULT '0' COMMENT 'Base ranged attack power.',
  `BaseArmor` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'Base armor value for any creature of this level and class.',
  PRIMARY KEY (`Level`,`Class`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;

--
-- Table structure for table `custom_texts`
--

DROP TABLE IF EXISTS `custom_texts`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `custom_texts` (
  `entry` mediumint(8) NOT NULL COMMENT 'The unique identifier of the script text entry.',
  `content_default` text NOT NULL COMMENT 'Contains the text presented in the default language English.',
  `content_loc1` text COMMENT 'Korean localization of content_default.',
  `content_loc2` text COMMENT 'French localization of content_default.',
  `content_loc3` text COMMENT 'German localization of content_default.',
  `content_loc4` text COMMENT 'Chinese localization of content_default.',
  `content_loc5` text COMMENT 'Taiwanese localization of content_default.',
  `content_loc6` text COMMENT 'Spanish (Spain) localization of content_default',
  `content_loc7` text COMMENT 'Spanish (Latin America) localization of content_default',
  `content_loc8` text COMMENT 'Russian localization of content_default',
  `sound` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'Reference to a SoundEntries.dbc table entry.',
  `type` tinyint(3) unsigned NOT NULL DEFAULT '0' COMMENT 'Selects one of various text emote types to be used for the script text.',
  `language` tinyint(3) unsigned NOT NULL DEFAULT '0' COMMENT 'A language identifier.',
  `emote` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'Emote ID that the creature should continually perform.',
  `comment` text COMMENT 'This documents the script text.',
  PRIMARY KEY (`entry`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC COMMENT='Custom Texts';

--
-- Table structure for table `db_script_string`
--

DROP TABLE IF EXISTS `db_script_string`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `db_script_string` (
  `entry` int(11) unsigned NOT NULL DEFAULT '0' COMMENT 'Text ID. See dataint parameter of the SCRIPT_COMMAND_TALK command.',
  `content_default` text NOT NULL COMMENT 'Contains the text presented in the default language English.',
  `content_loc1` text COMMENT 'Korean localization of content_default.',
  `content_loc2` text COMMENT 'French localization of content_default.',
  `content_loc3` text COMMENT 'German localization of content_default.',
  `content_loc4` text COMMENT 'Chinese localization of content_default.',
  `content_loc5` text COMMENT 'Taiwanese localization of content_default.',
  `content_loc6` text COMMENT 'Spanish (Spain) localization of content_default',
  `content_loc7` text COMMENT 'Spanish (Latin America) localization of content_default',
  `content_loc8` text COMMENT 'Russian localization of content_default',
  `sound` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'Sound ID. See SoundEntries.dbc.',
  `type` tinyint(3) unsigned NOT NULL DEFAULT '0' COMMENT 'Sound and speech type.',
  `language` tinyint(3) unsigned NOT NULL DEFAULT '0' COMMENT 'In-game language (See Languages.dbc).',
  `emote` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'Emote ID that the creature should continually perform.',
  `comment` text COMMENT 'Textual comment.',
  PRIMARY KEY (`entry`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `db_scripts`
--

DROP TABLE IF EXISTS `db_scripts`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `db_scripts` (
  `script_guid` mediumint(8) unsigned NOT NULL AUTO_INCREMENT COMMENT 'The Unique Identifier for this script',
  `script_type` smallint(2) unsigned NOT NULL DEFAULT '0' COMMENT 'The type of script',
  `id` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'Gossip script ID.',
  `delay` int(10) unsigned NOT NULL DEFAULT '0' COMMENT 'Delay (sec).',
  `command` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'Script command.',
  `datalong` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'Command parameter, see command description.',
  `datalong2` int(10) unsigned NOT NULL DEFAULT '0' COMMENT 'Command parameter, see command description.',
  `buddy_entry` int(10) unsigned NOT NULL DEFAULT '0' COMMENT 'Creature ID (creature_template.entry) for changing source/target.',
  `search_radius` int(10) unsigned NOT NULL DEFAULT '0' COMMENT 'Radius for the buddy search.',
  `data_flags` tinyint(3) unsigned NOT NULL DEFAULT '0' COMMENT 'Command flags.',
  `dataint` int(11) NOT NULL DEFAULT '0' COMMENT 'Command parameter, see command description.',
  `dataint2` int(11) NOT NULL DEFAULT '0' COMMENT 'Command parameter, see command description.',
  `dataint3` int(11) NOT NULL DEFAULT '0' COMMENT 'Command parameter, see command description.',
  `dataint4` int(11) NOT NULL DEFAULT '0' COMMENT 'Command parameter, see command description.',
  `x` float NOT NULL DEFAULT '0' COMMENT 'Position X.',
  `y` float NOT NULL DEFAULT '0' COMMENT 'Position Y.',
  `z` float NOT NULL DEFAULT '0' COMMENT 'Position Z.',
  `o` float NOT NULL DEFAULT '0' COMMENT 'Orientation angle (0 to 2*Pi).',
  `comments` varchar(255) NOT NULL COMMENT 'Textual comment.',
  PRIMARY KEY (`script_guid`)
) ENGINE=MyISAM AUTO_INCREMENT=2628 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `game_event_creature_data`
--

DROP TABLE IF EXISTS `game_event_creature_data`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `game_event_creature_data` (
  `guid` int(10) unsigned NOT NULL DEFAULT '0' COMMENT 'Creature GUID (See creature.guid).',
  `entry_id` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'New creature ID (See creature_template.entry).',
  `modelid` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'New modelID (See creature_template.ModelId1,2)',
  `equipment_id` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'New equipment ID (See creature_equip_template.entry).',
  `spell_start` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'Spell ID (See Spell.dbc) to be selfcasted.',
  `spell_end` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'Spell ID (See Spell.dbc) to be removed.',
  `event` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'Event ID (See game_event.entry).',
  PRIMARY KEY (`guid`,`event`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `game_event_mail`
--

DROP TABLE IF EXISTS `game_event_mail`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `game_event_mail` (
  `event` smallint(6) NOT NULL DEFAULT '0' COMMENT 'Event ID (See game_events.entry).',
  `raceMask` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'Races of affected players.',
  `quest` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'Quest (See quest_template.entry) which should be rewarded.',
  `mailTemplateId` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'Mail template ID (See MailTemplate.dbc).',
  `senderEntry` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'NPC entry (See creature_template.entry).',
  PRIMARY KEY (`event`,`raceMask`,`quest`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC COMMENT='Game event system';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `game_event_quest`
--

DROP TABLE IF EXISTS `game_event_quest`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `game_event_quest` (
  `quest` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'Quest ID (See quest_template.entry).',
  `event` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'Event ID (see game_event.entry).',
  PRIMARY KEY (`quest`,`event`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8 COMMENT='Game event system';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `gameobject_battleground`
--

DROP TABLE IF EXISTS `gameobject_battleground`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `gameobject_battleground` (
  `guid` int(10) unsigned NOT NULL COMMENT 'This references the gameobject table''s  unique ID.',
  `event1` tinyint(3) unsigned NOT NULL COMMENT 'The identifier for the event node in the battleground. ',
  `event2` tinyint(3) unsigned NOT NULL COMMENT 'The state of the event node. ',
  PRIMARY KEY (`guid`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC COMMENT='GameObject battleground indexing system';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `gossip_menu`
--

DROP TABLE IF EXISTS `gossip_menu`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `gossip_menu` (
  `entry` smallint(6) unsigned NOT NULL DEFAULT '0' COMMENT 'Gossip menu ID (See creature_template.GossipMenuId).',
  `text_id` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'Displayed text ID (See npc_text.ID).',
  `script_id` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'DB script ID (See dbscritps_on_gossip.id).',
  `condition_id` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'Condition ID (See conditions.condition_entry).',
  PRIMARY KEY (`entry`,`text_id`,`script_id`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `gossip_menu_option`
--

DROP TABLE IF EXISTS `gossip_menu_option`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `gossip_menu_option` (
  `menu_id` smallint(6) unsigned NOT NULL DEFAULT '0' COMMENT 'Gossip menu ID (See gossip_menu.entry).',
  `id` smallint(6) unsigned NOT NULL DEFAULT '0' COMMENT 'Menu item ID.',
  `option_icon` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'Icon type for the menu item.',
  `option_text` text COMMENT 'Menu item text displayed.',
  `option_id` tinyint(3) unsigned NOT NULL DEFAULT '0' COMMENT 'Gossip option ID.',
  `npc_option_npcflag` int(10) unsigned NOT NULL DEFAULT '0' COMMENT 'NPC flag required (see creature_template.NpcFlags).',
  `action_menu_id` mediumint(8) NOT NULL DEFAULT '0' COMMENT 'Gossip ID for the action (see gossip_menu.entry).',
  `action_poi_id` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'POI ID (See points_of_interest.entry).',
  `action_script_id` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'DB script ID (See dbscripts_on_gossip.id).',
  `box_coded` tinyint(3) unsigned NOT NULL DEFAULT '0' COMMENT 'Flag for text entering into the pop-up box.',
  `box_money` int(11) unsigned NOT NULL DEFAULT '0' COMMENT 'Sum of money requested by pop-up box.',
  `box_text` text COMMENT 'Text for the pop-up box.',
  `condition_id` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'Condition ID (See conditions.condition_entry).',
  PRIMARY KEY (`menu_id`,`id`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `gossip_texts`
--

DROP TABLE IF EXISTS `gossip_texts`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `gossip_texts` (
  `entry` mediumint(8) NOT NULL COMMENT 'Menu item ID text, referred by SD2 scripts.',
  `content_default` text NOT NULL COMMENT 'Contains the text presented in the default language English.',
  `content_loc1` text COMMENT 'Korean localization of content_default.',
  `content_loc2` text COMMENT 'French localization of content_default.',
  `content_loc3` text COMMENT 'German localization of content_default.',
  `content_loc4` text COMMENT 'Chinese localization of content_default.',
  `content_loc5` text COMMENT 'Taiwanese localization of content_default.',
  `content_loc6` text COMMENT 'Spanish (Spain) localization of content_default',
  `content_loc7` text COMMENT 'Spanish (Latin America) localization of content_default',
  `content_loc8` text COMMENT 'Russian localization of content_default',
  `comment` text COMMENT 'Textual comment.',
  PRIMARY KEY (`entry`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC COMMENT='Gossip Texts';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `item_required_target`
--

DROP TABLE IF EXISTS `item_required_target`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `item_required_target` (
  `entry` mediumint(8) unsigned NOT NULL COMMENT 'This references the item_template table tables unique ID.',
  `type` tinyint(3) unsigned NOT NULL DEFAULT '0' COMMENT 'Describes which type of target the spell requires.',
  `targetEntry` mediumint(8) unsigned NOT NULL DEFAULT '0',
  UNIQUE KEY `entry_type_target` (`entry`,`type`,`targetEntry`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;
/*!40101 SET character_set_client = @saved_cs_client */;


--
-- Table structure for table `mail_loot_template`
--

DROP TABLE IF EXISTS `mail_loot_template`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mail_loot_template` (
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


--
-- Table structure for table `mangos_string`
--

DROP TABLE IF EXISTS `mangos_string`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `mangos_string` (
  `entry` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'This table holds strings used internally by the server to allow translations.',
  `content_default` text NOT NULL COMMENT 'Contains the text presented in the default language English.',
  `content_loc1` text COMMENT 'Korean localization of content_default.',
  `content_loc2` text COMMENT 'French localization of content_default.',
  `content_loc3` text COMMENT 'German localization of content_default.',
  `content_loc4` text COMMENT 'Chinese localization of content_default.',
  `content_loc5` text COMMENT 'Taiwanese localization of content_default.',
  `content_loc6` text COMMENT 'Spanish (Spain) localization of content_default',
  `content_loc7` text COMMENT 'Spanish (Latin America) localization of content_default',
  `content_loc8` text COMMENT 'Russian localization of content_default',
  PRIMARY KEY (`entry`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `points_of_interest`
--

DROP TABLE IF EXISTS `points_of_interest`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `points_of_interest` (
  `entry` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'POI ID.',
  `x` float NOT NULL DEFAULT '0' COMMENT 'X coordinate.',
  `y` float NOT NULL DEFAULT '0' COMMENT 'Y coordinate.',
  `icon` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'POI icon.',
  `flags` mediumint(8) unsigned NOT NULL DEFAULT '0',
  `data` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'Custom data to be sent for a point of interest. ',
  `icon_name` text NOT NULL COMMENT 'The text to display as tooltip for the icon on the in-game map.',
  PRIMARY KEY (`entry`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `pool_creature`
--

DROP TABLE IF EXISTS `pool_creature`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `pool_creature` (
  `guid` int(10) unsigned NOT NULL DEFAULT '0' COMMENT 'Creature GUID (See creature.guid).',
  `pool_entry` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'Pool ID (See pool_template.entry).',
  `chance` float unsigned NOT NULL DEFAULT '0' COMMENT 'Chance in %.',
  `description` varchar(255) NOT NULL COMMENT 'Description.',
  PRIMARY KEY (`guid`),
  KEY `pool_idx` (`pool_entry`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `pool_creature_template`
--

DROP TABLE IF EXISTS `pool_creature_template`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `pool_creature_template` (
  `id` int(10) unsigned NOT NULL DEFAULT '0' COMMENT 'Creature ID (See creature_template.entry).',
  `pool_entry` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'Pool ID (See pool_template.entry).',
  `chance` float unsigned NOT NULL DEFAULT '0' COMMENT 'Chance, %.',
  `description` varchar(255) NOT NULL COMMENT 'Description.',
  PRIMARY KEY (`id`),
  KEY `pool_idx` (`pool_entry`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `pool_gameobject`
--

DROP TABLE IF EXISTS `pool_gameobject`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `pool_gameobject` (
  `guid` int(10) unsigned NOT NULL DEFAULT '0' COMMENT 'Gameobject GUID (See gameobject.guid).',
  `pool_entry` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'Pool ID (See pool_template.entry).',
  `chance` float unsigned NOT NULL DEFAULT '0' COMMENT 'Chance, %.',
  `description` varchar(255) NOT NULL COMMENT 'Description.',
  PRIMARY KEY (`guid`),
  KEY `pool_idx` (`pool_entry`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `pool_pool`
--

DROP TABLE IF EXISTS `pool_pool`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `pool_pool` (
  `pool_id` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'Pool ID (See pool_template.entry).',
  `mother_pool` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'Mother pool ID.',
  `chance` float NOT NULL DEFAULT '0' COMMENT 'Chance, %.',
  `description` varchar(255) NOT NULL COMMENT 'Description.',
  PRIMARY KEY (`pool_id`),
  KEY `pool_idx` (`mother_pool`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `pool_template`
--

DROP TABLE IF EXISTS `pool_template`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `pool_template` (
  `entry` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'Pool ID.',
  `max_limit` int(10) unsigned NOT NULL DEFAULT '0' COMMENT 'Maximum number of entities.',
  `description` varchar(255) NOT NULL COMMENT 'Description.',
  PRIMARY KEY (`entry`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;


--
-- Table structure for table `quest_template`
--

DROP TABLE IF EXISTS `quest_template`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `quest_template` (
  `entry` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'The unique identifier of the quest template entry.',
  `Method` tinyint(3) unsigned NOT NULL DEFAULT '2' COMMENT 'This flag decides how a quest will be handled.',
  `ZoneOrSort` smallint(6) NOT NULL DEFAULT '0' COMMENT 'Defines the category under which a quest will be listed.',
  `MinLevel` tinyint(3) unsigned NOT NULL DEFAULT '0' COMMENT 'The lowest level allowed to accept the quest.',
  `QuestLevel` tinyint(3) unsigned NOT NULL DEFAULT '0' COMMENT 'The quest''s level.',
  `Type` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'Quest''s difficulty.',
  `RequiredClasses` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'Required classes mask.',
  `RequiredRaces` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'Required races mask.',
  `RequiredSkill` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'Required skill type.',
  `RequiredSkillValue` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'Required skill value.',
  `RepObjectiveFaction` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'Faction (See Faction.dbc).',
  `RepObjectiveValue` mediumint(9) NOT NULL DEFAULT '0' COMMENT 'Reputation value.',
  `RequiredMinRepFaction` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'Faction (See Faction.dbc).',
  `RequiredMinRepValue` mediumint(9) NOT NULL DEFAULT '0' COMMENT 'Reputation value.',
  `RequiredMaxRepFaction` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'Faction ID of required faction to have max rep with.',
  `RequiredMaxRepValue` mediumint(9) NOT NULL DEFAULT '0' COMMENT 'The highest reputation value allowed for obtaining the quest.',
  `SuggestedPlayers` tinyint(3) unsigned NOT NULL DEFAULT '0' COMMENT 'Recommended  number of players to complete quest. ',
  `LimitTime` int(10) unsigned NOT NULL DEFAULT '0' COMMENT 'The time limit to complete the quest.',
  `QuestFlags` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'The quest flags give additional details on the quest type.',
  `SpecialFlags` tinyint(3) unsigned NOT NULL DEFAULT '0' COMMENT 'Flags used to define special behaviour.',
  `PrevQuestId` mediumint(9) NOT NULL DEFAULT '0' COMMENT 'Quest ID of the preceding or an exisiting quest.',
  `NextQuestId` mediumint(9) NOT NULL DEFAULT '0' COMMENT 'Quest ID of the follow-up quest (see description for more information)',
  `ExclusiveGroup` mediumint(9) NOT NULL DEFAULT '0' COMMENT 'This allows the grouping of multiple quests (see description).',
  `NextQuestInChain` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'Quest ID of next quest in chain.',
  `SrcItemId` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'Item ID of an item that the charcter starts the quest with.',
  `SrcItemCount` tinyint(3) unsigned NOT NULL DEFAULT '0' COMMENT 'Total number of items (SrcItemId) the character starts off with',
  `SrcSpell` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'Spell ID of the spell cast on the character on acceptance of the quest.',
  `Title` text COMMENT 'The title of the quest.',
  `Details` text COMMENT 'The quest text.',
  `Objectives` text COMMENT 'The quest''s objective in text form. ',
  `OfferRewardText` text COMMENT 'The text sent to a character when talking to the quest giver.',
  `RequestItemsText` text COMMENT 'The text sent to a character when talking to a quest giver.',
  `EndText` text COMMENT 'See description in the lower half of the page.',
  `ObjectiveText1` text COMMENT 'Set to a text string, to show up as requirement in the quest log entry.',
  `ObjectiveText2` text COMMENT 'Set to a text string, to show up as requirement in the quest log entry.',
  `ObjectiveText3` text COMMENT 'Set to a text string, to show up as requirement in the quest log entry.',
  `ObjectiveText4` text COMMENT 'Set to a text string, to show up as requirement in the quest log entry.',
  `ReqItemId1` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'item_template ID of an item required for quest completion.',
  `ReqItemId2` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'item_template ID of an item required for quest completion.',
  `ReqItemId3` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'item_template ID of an item required for quest completion.',
  `ReqItemId4` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'item_template ID of an item required for quest completion.',
  `ReqItemCount1` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'Amount of items (ReqItemId1)  needed to complete the quest.',
  `ReqItemCount2` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'Amount of items (ReqItemId2) needed to complete the quest.',
  `ReqItemCount3` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'Amount of items (ReqItemId3) needed to complete the quest',
  `ReqItemCount4` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'Amount of items (ReqItemId4) needed to complete the quest.',
  `ReqSourceId1` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'item_template ID of the item required for making quest items',
  `ReqSourceId2` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'item_template ID of the item required for making quest items',
  `ReqSourceId3` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'item_template ID of the item required for making quest items',
  `ReqSourceId4` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'item_template ID of the item required for making quest items',
  `ReqSourceCount1` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'If ReqSourceId1 is set, set this to the amount of required items.',
  `ReqSourceCount2` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'If ReqSourceId2 is set, set this to the amount of required items.',
  `ReqSourceCount3` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'If ReqSourceId3 is set, set this to the amount of required items.',
  `ReqSourceCount4` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'If ReqSourceId4 is set, set this to the amount of required items.',
  `ReqCreatureOrGOId1` mediumint(9) NOT NULL DEFAULT '0' COMMENT 'ID of required creature or game object.',
  `ReqCreatureOrGOId2` mediumint(9) NOT NULL DEFAULT '0' COMMENT 'ID of required creature or game object.',
  `ReqCreatureOrGOId3` mediumint(9) NOT NULL DEFAULT '0' COMMENT 'ID of required creature or game object.',
  `ReqCreatureOrGOId4` mediumint(9) NOT NULL DEFAULT '0' COMMENT 'ID of required creature or game object.',
  `ReqCreatureOrGOCount1` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'The amount of creatures or game objects required for the quest.',
  `ReqCreatureOrGOCount2` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'The amount of creatures or game objects required for the quest.',
  `ReqCreatureOrGOCount3` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'The amount of creatures or game objects required for the quest.',
  `ReqCreatureOrGOCount4` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'The amount of creatures or game objects required for the quest.',
  `ReqSpellCast1` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'Spell ID of the spell that must be cast for the quest.',
  `ReqSpellCast2` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'Spell ID of the spell that must be cast for the quest.',
  `ReqSpellCast3` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'Spell ID of the spell that must be cast for the quest.',
  `ReqSpellCast4` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'Spell ID of the spell that must be cast for the quest.',
  `RewChoiceItemId1` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'item_template Entry ID of one possible reward.',
  `RewChoiceItemId2` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'item_template Entry ID of one possible reward.',
  `RewChoiceItemId3` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'item_template Entry ID of one possible reward.',
  `RewChoiceItemId4` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'item_template Entry ID of one possible reward.',
  `RewChoiceItemId5` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'item_template Entry ID of one possible reward.',
  `RewChoiceItemId6` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'item_template Entry ID of one possible reward.',
  `RewChoiceItemCount1` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'This defines the number of charges available for the rewarded item.',
  `RewChoiceItemCount2` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'This defines the number of charges available for the rewarded item.',
  `RewChoiceItemCount3` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'This defines the number of charges available for the rewarded item.',
  `RewChoiceItemCount4` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'This defines the number of charges available for the rewarded item.',
  `RewChoiceItemCount5` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'This defines the number of charges available for the rewarded item.',
  `RewChoiceItemCount6` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'This defines the number of charges available for the rewarded item.',
  `RewItemId1` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'item_template Entry ID of the reward.',
  `RewItemId2` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'item_template Entry ID of the reward.',
  `RewItemId3` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'item_template Entry ID of the reward.',
  `RewItemId4` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'item_template Entry ID of the reward.',
  `RewItemCount1` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'This defines the amount if items to be rewarded.',
  `RewItemCount2` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'This defines the amount if items to be rewarded.',
  `RewItemCount3` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'This defines the amount if items to be rewarded.',
  `RewItemCount4` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'This defines the amount if items to be rewarded.',
  `RewRepFaction1` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'This is the faction ID of the faction whose rep is to be a reward.',
  `RewRepFaction2` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'This is the faction ID of the faction whose rep is to be a reward.',
  `RewRepFaction3` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'This is the faction ID of the faction whose rep is to be a reward.',
  `RewRepFaction4` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'This is the faction ID of the faction whose rep is to be a reward.',
  `RewRepFaction5` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'This is the faction ID of the faction whose rep is to be a reward.',
  `RewRepValue1` mediumint(9) NOT NULL DEFAULT '0' COMMENT 'This defines the amount of reputation gain or loss for the referenced faction.',
  `RewRepValue2` mediumint(9) NOT NULL DEFAULT '0' COMMENT 'This defines the amount of reputation gain or loss for the referenced faction.',
  `RewRepValue3` mediumint(9) NOT NULL DEFAULT '0' COMMENT 'This defines the amount of reputation gain or loss for the referenced faction.',
  `RewRepValue4` mediumint(9) NOT NULL DEFAULT '0' COMMENT 'This defines the amount of reputation gain or loss for the referenced faction.',
  `RewRepValue5` mediumint(9) NOT NULL DEFAULT '0' COMMENT 'This defines the amount of reputation gain or loss for the referenced faction.',
  `RewOrReqMoney` int(11) NOT NULL DEFAULT '0' COMMENT 'Required money for starting the quest, or award money for completing the quest.',
  `RewMoneyMaxLevel` int(10) unsigned NOT NULL DEFAULT '0' COMMENT 'The amount of experience or money to be rewarded.',
  `RewSpell` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'Spell ID of spell to be cast as a reward.',
  `RewSpellCast` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'Spell ID of additional spell to be cast, different to RewSpell.',
  `RewMailTemplateId` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'mail_loot_template Entry ID of mail to be sent as a reward.',
  `RewMailDelaySecs` int(11) unsigned NOT NULL DEFAULT '0' COMMENT 'The number of seconds to wait before sending the reward mail.',
  `PointMapId` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'A location (POI) to be highlighted on the map while the quest is active.',
  `PointX` float NOT NULL DEFAULT '0' COMMENT 'X coordinate of quest POI.',
  `PointY` float NOT NULL DEFAULT '0' COMMENT 'Y coordinate of quest POI.',
  `PointOpt` mediumint(8) unsigned NOT NULL DEFAULT '0',
  `DetailsEmote1` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'This references  the emote to be shown upon displaying quest details.',
  `DetailsEmote2` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'This references  the emote to be shown upon displaying quest details.',
  `DetailsEmote3` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'This references  the emote to be shown upon displaying quest details.',
  `DetailsEmote4` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'This references  the emote to be shown upon displaying quest details.',
  `DetailsEmoteDelay1` int(11) unsigned NOT NULL DEFAULT '0' COMMENT 'The number of seconds to delay the emote.',
  `DetailsEmoteDelay2` int(11) unsigned NOT NULL DEFAULT '0' COMMENT 'The number of seconds to delay the emote.',
  `DetailsEmoteDelay3` int(11) unsigned NOT NULL DEFAULT '0' COMMENT 'The number of seconds to delay the emote.',
  `DetailsEmoteDelay4` int(11) unsigned NOT NULL DEFAULT '0' COMMENT 'The number of seconds to delay the emote.',
  `IncompleteEmote` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'This references the emotes identifier in the Emotes.dbc table.',
  `CompleteEmote` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'This references the emotes identifier in the Emotes.dbc table.',
  `OfferRewardEmote1` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'Emotes.dbc ID of emote to be shown on quest completion.',
  `OfferRewardEmote2` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'Emotes.dbc ID of emote to be shown on quest completion.',
  `OfferRewardEmote3` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'Emotes.dbc ID of emote to be shown on quest completion.',
  `OfferRewardEmote4` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'Emotes.dbc ID of emote to be shown on quest completion.',
  `OfferRewardEmoteDelay1` int(11) unsigned NOT NULL DEFAULT '0' COMMENT 'The number of seconds to delay before the emote is actioned.',
  `OfferRewardEmoteDelay2` int(11) unsigned NOT NULL DEFAULT '0' COMMENT 'The number of seconds to delay before the emote is actioned.',
  `OfferRewardEmoteDelay3` int(11) unsigned NOT NULL DEFAULT '0' COMMENT 'The number of seconds to delay before the emote is actioned.',
  `OfferRewardEmoteDelay4` int(11) unsigned NOT NULL DEFAULT '0' COMMENT 'The number of seconds to delay before the emote is actioned.',
  `StartScript` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'dbscripts_on_quest_start Entry ID of the script to be run at quest start.',
  `CompleteScript` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'dbscripts_on_quest_start Entry ID of the script to be run at quest end.',
  PRIMARY KEY (`entry`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8 COMMENT='Quest System';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `reputation_spillover_template`
--

DROP TABLE IF EXISTS `reputation_spillover_template`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `reputation_spillover_template` (
  `faction` smallint(6) unsigned NOT NULL DEFAULT '0' COMMENT 'Base faction (See Faction.dbc).',
  `faction1` smallint(6) unsigned NOT NULL DEFAULT '0' COMMENT 'Dependent faction (See Faction.dbc).',
  `rate_1` float NOT NULL DEFAULT '0' COMMENT 'Rate for faction one.',
  `rank_1` tinyint(3) unsigned NOT NULL DEFAULT '0' COMMENT 'The topmost rank allowed.',
  `faction2` smallint(6) unsigned NOT NULL DEFAULT '0' COMMENT 'Dependent faction (See Faction.dbc).',
  `rate_2` float NOT NULL DEFAULT '0' COMMENT 'Rate for faction two.',
  `rank_2` tinyint(3) unsigned NOT NULL DEFAULT '0' COMMENT 'The topmost rank allowed.',
  `faction3` smallint(6) unsigned NOT NULL DEFAULT '0' COMMENT 'Dependent faction (See Faction.dbc).',
  `rate_3` float NOT NULL DEFAULT '0' COMMENT 'Rate for faction three.',
  `rank_3` tinyint(3) unsigned NOT NULL DEFAULT '0' COMMENT 'The topmost rank allowed.',
  `faction4` smallint(6) unsigned NOT NULL DEFAULT '0' COMMENT 'Dependent faction (See Faction.dbc).',
  `rate_4` float NOT NULL DEFAULT '0' COMMENT 'Rate for faction four.',
  `rank_4` tinyint(3) unsigned NOT NULL DEFAULT '0' COMMENT 'The topmost rank allowed.',
  PRIMARY KEY (`faction`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC COMMENT='Reputation spillover reputation gain';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `reserved_name`
--

DROP TABLE IF EXISTS `reserved_name`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `reserved_name` (
  `name` varchar(12) NOT NULL DEFAULT '' COMMENT 'The name to disallow for characters created on normal player accounts.',
  PRIMARY KEY (`name`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC COMMENT='Player Reserved Names';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `script_binding`
--

DROP TABLE IF EXISTS `script_binding`;

CREATE TABLE `script_binding` (
  `type` tinyint(2) unsigned NOT NULL COMMENT 'enum ScriptedObjectType',
  `ScriptName` char(64) NOT NULL COMMENT 'Script name, to be unique across all types',
  `bind` mediumint(10) NOT NULL COMMENT 'Bound to entry (>0) or GUID (<0)',
  `data` tinyint(2) unsigned DEFAULT '0' COMMENT 'Misc data; Effect number for spellscripts',
  PRIMARY KEY (`ScriptName`,`bind`),
  KEY `type` (`type`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC COMMENT='SD2 Script Names and Binding';

--
-- Table structure for table `script_texts`
--

DROP TABLE IF EXISTS `script_texts`;

CREATE TABLE `script_texts` (
  `entry` mediumint(8) NOT NULL COMMENT 'Script text ID.',
  `content_default` text NOT NULL COMMENT 'Contains the text presented in the default language English.',
  `content_loc1` text COMMENT 'Korean localization of content_default.',
  `content_loc2` text COMMENT 'French localization of content_default.',
  `content_loc3` text COMMENT 'German localization of content_default.',
  `content_loc4` text COMMENT 'Chinese localization of content_default.',
  `content_loc5` text COMMENT 'Taiwanese localization of content_default.',
  `content_loc6` text COMMENT 'Spanish (Spain) localization of content_default',
  `content_loc7` text COMMENT 'Spanish (Latin America) localization of content_default',
  `content_loc8` text COMMENT 'Russian localization of content_default',
  `sound` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'Sound ID (See SoundEntries.dbc).',
  `type` tinyint(3) unsigned NOT NULL DEFAULT '0' COMMENT 'Chat type (see enum ChatType in Creature.h).',
  `language` tinyint(3) unsigned NOT NULL DEFAULT '0' COMMENT 'In-game language.',
  `emote` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'Emote ID that the creature should continually perform.',
  `comment` text COMMENT 'Textual comment.',
  PRIMARY KEY (`entry`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC COMMENT='Script Texts';

--
-- Table structure for table `script_waypoint`
--

DROP TABLE IF EXISTS `script_waypoint`;

CREATE TABLE `script_waypoint` (
  `entry` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'Creature ID (See creature_template.entry).',
  `pointid` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'Point ID.',
  `location_x` float NOT NULL DEFAULT '0' COMMENT 'X position of WP.',
  `location_y` float NOT NULL DEFAULT '0' COMMENT 'Y position of WP.',
  `location_z` float NOT NULL DEFAULT '0' COMMENT 'Z position of WP.',
  `waittime` int(10) unsigned NOT NULL DEFAULT '0' COMMENT 'Wait time (msec).',
  `point_comment` text COMMENT 'Textual comment.',
  PRIMARY KEY (`entry`,`pointid`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC COMMENT='Script Creature waypoints';

--
-- Table structure for table `spell_affect`
--

DROP TABLE IF EXISTS `spell_affect`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `spell_affect` (
  `entry` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'Spell ID (See Spell.dbc#Id).',
  `effectId` tinyint(3) unsigned NOT NULL DEFAULT '0' COMMENT 'Effect ID (See Spell.dbc).',
  `SpellFamilyMask` bigint(20) unsigned NOT NULL DEFAULT '0' COMMENT 'SpellFamilyFlags (See Spell.dbc).',
  PRIMARY KEY (`entry`,`effectId`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;


--
-- Table structure for table `spell_area`
--

DROP TABLE IF EXISTS `spell_area`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `spell_area` (
  `spell` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'Spell ID (See Spell.dbc).',
  `area` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'Area ID (See AreaTable.dbc).',
  `quest_start` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'Quest ID (See quest_template.entry).',
  `quest_start_active` tinyint(1) unsigned NOT NULL DEFAULT '0' COMMENT 'Flag for quest_start.',
  `quest_end` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'Quest ID (See quest_template.entry).',
  `condition_id` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'Condition ID (See conditions.condition_entry).',
  `aura_spell` mediumint(8) NOT NULL DEFAULT '0' COMMENT 'Spell ID (See Spell.dbc).',
  `racemask` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'Race mask value.',
  `gender` tinyint(1) unsigned NOT NULL DEFAULT '2' COMMENT 'The gender of characters to which the spell is applied.',
  `autocast` tinyint(1) unsigned NOT NULL DEFAULT '0' COMMENT 'Autocast flag.',
  PRIMARY KEY (`spell`,`area`,`quest_start`,`quest_start_active`,`aura_spell`,`racemask`,`gender`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `spell_bonus_data`
--

DROP TABLE IF EXISTS `spell_bonus_data`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `spell_bonus_data` (
  `entry` smallint(5) unsigned NOT NULL COMMENT 'Spell ID (See Spell.dbc).',
  `direct_bonus` float NOT NULL DEFAULT '0' COMMENT 'Direct damage bonus.',
  `one_hand_direct_bonus` float NOT NULL DEFAULT '0' COMMENT 'Direct bonus for one-handed weapon.',
  `two_hand_direct_bonus` float NOT NULL DEFAULT '0' COMMENT 'Direct damage bonus for two-handed weapon.',
  `direct_bonus_done` float NOT NULL DEFAULT '0' COMMENT 'Direct bonus for done part.',
  `one_hand_direct_bonus_done` float NOT NULL DEFAULT '0' COMMENT 'Direct damage done bonus with one-handed weapon.',
  `two_hand_direct_bonus_done` float NOT NULL DEFAULT '0' COMMENT 'Direct damage done bonus with two-handed weapon.',
  `direct_bonus_taken` float NOT NULL DEFAULT '0' COMMENT 'Direct damage taken bonus.',
  `one_hand_direct_bonus_taken` float NOT NULL DEFAULT '0' COMMENT 'Direct damage taken bonus with one-handed weapon.',
  `two_hand_direct_bonus_taken` float NOT NULL DEFAULT '0' COMMENT 'Direct damage taken bonus with two-handed weapon.',
  `dot_bonus` float NOT NULL DEFAULT '0' COMMENT 'DoT tick bonus coefficient.',
  `ap_bonus` float NOT NULL DEFAULT '0' COMMENT 'Any value here will modify the spells attack power with the factor given here.',
  `ap_dot_bonus` float NOT NULL DEFAULT '0' COMMENT 'DoT bonus for physical damage.',
  `comments` varchar(255) DEFAULT NULL COMMENT 'Textual comment.',
  PRIMARY KEY (`entry`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `spell_elixir`
--

DROP TABLE IF EXISTS `spell_elixir`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `spell_elixir` (
  `entry` int(11) unsigned NOT NULL DEFAULT '0' COMMENT 'Spell ID (See Spell.dbc).',
  `mask` tinyint(1) unsigned NOT NULL DEFAULT '0' COMMENT 'Defines what type of potion/food spell this is.',
  PRIMARY KEY (`entry`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC COMMENT='Spell System';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `spell_learn_spell`
--

DROP TABLE IF EXISTS `spell_learn_spell`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `spell_learn_spell` (
  `entry` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'Spell ID (See Spell.dbc).',
  `SpellID` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'Spell ID (See Spell.dbc).',
  `Active` tinyint(3) unsigned NOT NULL DEFAULT '1' COMMENT 'Active flag.',
  PRIMARY KEY (`entry`,`SpellID`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC COMMENT='Item System';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `spell_linked`
--

DROP TABLE IF EXISTS `spell_linked`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `spell_linked` (
  `entry` int(10) unsigned NOT NULL COMMENT 'Spell ID (See Spell.dbc).',
  `linked_entry` int(10) unsigned NOT NULL COMMENT 'Spell ID (See Spell.dbc).',
  `type` int(10) unsigned NOT NULL DEFAULT '0' COMMENT 'Link type.',
  `effect_mask` int(10) unsigned NOT NULL DEFAULT '0' COMMENT 'Effect mask.',
  `comment` varchar(255) NOT NULL DEFAULT '' COMMENT 'Textual comment.',
  PRIMARY KEY (`entry`,`linked_entry`,`type`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8 PACK_KEYS=0 COMMENT='Linked spells storage';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `spell_pet_auras`
--

DROP TABLE IF EXISTS `spell_pet_auras`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `spell_pet_auras` (
  `spell` mediumint(8) unsigned NOT NULL COMMENT 'Spell ID (See Spell.dbc).',
  `pet` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'Creature ID (See creature_template.entry).',
  `aura` mediumint(8) unsigned NOT NULL COMMENT 'Spell ID (See Spell.dbc).',
  PRIMARY KEY (`spell`,`pet`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `spell_proc_event`
--

DROP TABLE IF EXISTS `spell_proc_event`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `spell_proc_event` (
  `entry` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'Spell ID (See Spell.dbc).',
  `SchoolMask` tinyint(4) unsigned NOT NULL DEFAULT '0' COMMENT 'Spell school mask.',
  `SpellFamilyName` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'Spell family name.',
  `SpellFamilyMask0` bigint(40) unsigned NOT NULL DEFAULT '0' COMMENT 'Spell family mask for effect 0.',
  `SpellFamilyMask1` bigint(40) unsigned NOT NULL DEFAULT '0' COMMENT 'Spell family mask for effect 1.',
  `SpellFamilyMask2` bigint(40) unsigned NOT NULL DEFAULT '0' COMMENT 'Spell family mask for effect 2.',
  `procFlags` int(10) unsigned NOT NULL DEFAULT '0' COMMENT 'Flags defining conditions for aura to proc.',
  `procEx` int(10) unsigned NOT NULL DEFAULT '0' COMMENT 'Flags refining proc condition.',
  `ppmRate` float NOT NULL DEFAULT '0' COMMENT 'Proc frequency limit.',
  `CustomChance` float NOT NULL DEFAULT '0' COMMENT 'Chance of proc.',
  `Cooldown` int(10) unsigned NOT NULL DEFAULT '0' COMMENT 'Cooldown (in msec).',
  PRIMARY KEY (`entry`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `spell_script_target`
--

DROP TABLE IF EXISTS `spell_script_target`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `spell_script_target` (
  `entry` mediumint(8) unsigned NOT NULL COMMENT 'Spell ID (See Spell.dbc).',
  `type` tinyint(3) unsigned NOT NULL DEFAULT '0' COMMENT 'Type of the target entry.',
  `targetEntry` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'Creature ID or Gameobject ID.',
  `inverseEffectMask` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'Inverse effect mask.',
  UNIQUE KEY `entry_type_target` (`entry`,`type`,`targetEntry`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8 COMMENT='Spell System';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `spell_target_position`
--

DROP TABLE IF EXISTS `spell_target_position`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `spell_target_position` (
  `id` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'The spell identifier. The value has to match with a defined spell identifier.',
  `target_map` smallint(5) unsigned NOT NULL DEFAULT '0' COMMENT 'The target map''s identifier.',
  `target_position_x` float NOT NULL DEFAULT '0' COMMENT 'The X position on the target map.',
  `target_position_y` float NOT NULL DEFAULT '0' COMMENT 'The Y position on the target map.',
  `target_position_z` float NOT NULL DEFAULT '0' COMMENT 'The Z position on the target map.',
  `target_orientation` float NOT NULL DEFAULT '0' COMMENT 'The orientation for the character on the target map.',
  PRIMARY KEY (`id`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC COMMENT='Spell System';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `spell_threat`
--

DROP TABLE IF EXISTS `spell_threat`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `spell_threat` (
  `entry` mediumint(8) unsigned NOT NULL COMMENT 'The spell identifier.',
  `Threat` smallint(6) NOT NULL COMMENT 'The value of threat to add or remove from the characters threat.',
  `multiplier` float NOT NULL DEFAULT '1' COMMENT 'Any value here will modify the spells threat with the factor given here.',
  `ap_bonus` float NOT NULL DEFAULT '0' COMMENT 'Any value here will modify the spells attack power with the factor given here.',
  PRIMARY KEY (`entry`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `pool_gameobject_template`
--

DROP TABLE IF EXISTS `pool_gameobject_template`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `pool_gameobject_template` (
  `id` int(10) unsigned NOT NULL DEFAULT '0' COMMENT 'Gameobject ID (See gameobject_template.entry).',
  `pool_entry` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'Pool ID (See pool_template.entry).',
  `chance` float unsigned NOT NULL DEFAULT '0' COMMENT 'Chance, %.',
  `description` varchar(255) NOT NULL COMMENT 'Description.',
  PRIMARY KEY (`id`),
  KEY `pool_idx` (`pool_entry`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `reputation_reward_rate`
--

DROP TABLE IF EXISTS `reputation_reward_rate`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `reputation_reward_rate` (
  `faction` mediumint(8) unsigned NOT NULL DEFAULT '0' COMMENT 'Faction (See Faction.dbc).',
  `quest_rate` float NOT NULL DEFAULT '1' COMMENT 'Rate for quest reputation.',
  `creature_rate` float NOT NULL DEFAULT '1' COMMENT 'Rate for creature kill reputation.',
  `spell_rate` float NOT NULL DEFAULT '1' COMMENT 'Rate for reputation spells.',
  PRIMARY KEY (`faction`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `spell_proc_item_enchant`
--

DROP TABLE IF EXISTS `spell_proc_item_enchant`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `spell_proc_item_enchant` (
  `entry` mediumint(8) unsigned NOT NULL COMMENT 'Spell ID (See Spell.dbc).',
  `ppmRate` float NOT NULL DEFAULT '0' COMMENT 'Proc frequency limit, per minute.',
  PRIMARY KEY (`entry`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

