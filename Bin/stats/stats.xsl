<?xml version="1.0" encoding="ISO-8859-1"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
<xsl:output method="html" indent="yes" /> 

<xsl:template match="/">
	<html xmlns="http://www.w3.org/1999/xhtml" >
	<head>
		<title>Server Status</title>
		<meta http-equiv="Pragma" content="no-cache"/>
		<meta http-equiv="Cache-Control" content="no-cache"/>
		<style type="text/css" media="screen">@import url(stats.css);</style>
	</head>
	<body>
		<center><div style="width: 80%;"><br/>
		<xsl:apply-templates/>
		</div>
		<div class="footer">
			Original design by mmorpg4free.com.<br/>
			World of Warcraft and Blizzard Entertainment are all trademarks or registered trademarks of Blizzard Entertainment in the United States and/or other countries.<br/> 
			These terms and all related materials, logos, and images are copyright © Blizzard Entertainment.<br/>
			This site is in no way associated with or endorsed by Blizzard Entertainment.<br/>
		</div>
		</center>
	</body>
	</html>
</xsl:template>



<xsl:template match="cluster">
	<xsl:variable name="kilo">1024</xsl:variable>
	<xsl:variable name="mega">1048576</xsl:variable>
	<xsl:variable name="giga">1073741824</xsl:variable>
	<table width="100%" border="0" cellspacing="1" cellpadding="3">
	<tr class="head"><th colspan="4">Cluster Status</th></tr>
		<tr>
			<th>Uptime: </th><td><xsl:value-of select="uptime"/></td>
			<th>CPU Usage: </th><td><xsl:value-of select="cpu"/>%</td>
		</tr>
		<tr>
			<th>Online Players: </th><td><xsl:value-of select="onlineplayers"/></td>
			<th>Memory Usage: </th><td><xsl:value-of select="ram"/> MB</td>

		</tr>
		<tr>
			<th>Online GM Count: </th><td><xsl:value-of select="gmcount"/></td>
			<th>Connections Accepted: </th><td><xsl:value-of select="connaccepted"/></td>
		</tr>
		<tr>			
			<th>Average Latency: </th><td><xsl:value-of select="latency"/> ms</td>
			<th>Connections Peak: </th><td><xsl:value-of select="connpeak"/></td>
		</tr>
		<tr>
			<th>Alliance Online: </th><td><xsl:value-of select="alliance"/></td>
			<th>Network Transfer (in): </th><td>
								<xsl:choose>
									<xsl:when test="networkin &lt; 1024">
										<xsl:value-of select="networkin"/> B
									</xsl:when>
									<xsl:when test="networkin &lt; 1048576">
										<xsl:value-of select="format-number(networkin div $kilo, '###,##0.##')"/> kB
									</xsl:when>
									<xsl:when test="networkin &lt; 1073741824">
										<xsl:value-of select="format-number(networkin div $mega, '###,##0.##')"/> MB
									</xsl:when>
									<xsl:when test="networkin &lt; 1099511627776">
										<xsl:value-of select="format-number(networkin div $giga, '###,##0.##')"/> GB
									</xsl:when>
				 				</xsl:choose>
							</td>
		</tr>
		<tr>
			<th>Horde Online: </th><td><xsl:value-of select="horde"/></td>
			<th>Network Transfer (out): </th><td>
								<xsl:choose>
									<xsl:when test="networkout &lt; 1024">
										<xsl:value-of select="networkout"/> B
									</xsl:when>
									<xsl:when test="networkout &lt; 1048576">
										<xsl:value-of select="format-number(networkout div $kilo, '###,##0.##')"/> kB
									</xsl:when>
									<xsl:when test="networkout &lt; 1073741824">
										<xsl:value-of select="format-number(networkout div $mega, '###,##0.##')"/> MB
									</xsl:when>
									<xsl:when test="networkout &lt; 1099511627776">
										<xsl:value-of select="format-number(networkout div $giga, '###,##0.##')"/> GB
									</xsl:when>
				 				</xsl:choose>
							</td>
		</tr>
		<tr>
			<th>Platform: </th><td><xsl:value-of select="platform"/></td>
			<th>Threads (worker): </th><td><xsl:value-of select="threadsw"/></td>
		</tr>
		<tr>
			<th>Last Update: </th><td><xsl:value-of select="lastupdate"/></td>
			<th>Threads (completion): </th><td><xsl:value-of select="threadsc"/></td>		
		</tr>

			
			
	<xsl:apply-templates/>

	</table>
</xsl:template>

<xsl:template match="platform"></xsl:template>
<xsl:template match="uptime"></xsl:template>
<xsl:template match="onlineplayers"></xsl:template>
<xsl:template match="cpu"></xsl:template>
<xsl:template match="ram"></xsl:template>
<xsl:template match="latency"></xsl:template>
<xsl:template match="gmcount"></xsl:template>
<xsl:template match="lastupdate"></xsl:template>
<xsl:template match="alliance"></xsl:template>
<xsl:template match="horde"></xsl:template>
<xsl:template match="connaccepted"></xsl:template>
<xsl:template match="connpeak"></xsl:template>
<xsl:template match="conncurrent"></xsl:template>
<xsl:template match="networkin"></xsl:template>
<xsl:template match="networkout"></xsl:template>
<xsl:template match="threadsw"></xsl:template>
<xsl:template match="threadsc"></xsl:template>

<xsl:template match="instance">
	<table width="100%" border="0" cellspacing="1" cellpadding="3">
	<tr class="head"><th colspan="4">World Status</th></tr>
		<tr>
			<th>Uptime: </th><td><xsl:value-of select="uptime"/></td>
			<th>CPU Usage: </th><td><xsl:value-of select="cpu"/>%</td>
		</tr>
		<tr>
			<th>Players: </th><td><xsl:value-of select="players"/></td>
			<th>Memory Usage: </th><td><xsl:value-of select="ram"/> MB</td>
		</tr>
		<tr>
			<th>Latency: </th><td><xsl:value-of select="latency"/> ms</td>
			<th>Threads: </th><td><xsl:value-of select="threads"/></td>
		</tr>
		<tr>			
			<th>Maps: </th><td colspan="3"><xsl:value-of select="maps"/></td>
		</tr>
	<xsl:apply-templates/>

	</table>
</xsl:template>





<xsl:template match="cpu"></xsl:template>
<xsl:template match="ram"></xsl:template>
<xsl:template match="map"></xsl:template>
<xsl:template match="id"></xsl:template>
<xsl:template match="threads"></xsl:template>
<xsl:template match="latency"></xsl:template>
<xsl:template match="players"></xsl:template>
<xsl:template match="maps"></xsl:template>







<xsl:template match="users">
	<table width="100%" border="0" cellspacing="1" cellpadding="2">
		<tr class="head">
			<th colspan="2">Online GMs</th>
		</tr>
		<tr>
			<th>Name</th>
			<th>Access</th>
		</tr>

		<xsl:for-each select="gmplayer">
		<xsl:sort select="access" data-type="number" order="descending"/>
		<xsl:sort select="name" data-type="text" order="ascending"/>
		<tr>
			<td><xsl:value-of select="name"/></td>
			<td>
				<xsl:choose>
					<xsl:when test="access = 0">Trial</xsl:when>
					<xsl:when test="access = 1">Player</xsl:when>
					<xsl:when test="access = 2">VIP</xsl:when>
					<xsl:when test="access = 3">Gamemaster</xsl:when>
					<xsl:when test="access = 4">Administrator</xsl:when>
					<xsl:when test="access = 5">Developer</xsl:when>
				 </xsl:choose>
			</td>
		</tr>
		</xsl:for-each>
	</table>
</xsl:template>



<xsl:template match="sessions">
	<table width="100%" border="0" cellspacing="1" cellpadding="2">
		<tr class="head">
			<th colspan="9">Online Players</th>
		</tr>
		<tr>
			<th>Name</th>
			<th width="32"><center>Race</center></th>
			<th width="32"><center>Class</center></th>
			<th width="32"><center>Rank</center></th>
			<th width="32"><center>Level</center></th>
			<th>Map</th>
			<th>Zone</th>
			<th>Online Time</th>
			<th style="text-align:right">Latency</th>
		</tr>

		<xsl:for-each select="player">
		<xsl:sort select="level" data-type="number" order="descending" />
		<xsl:sort select="name" data-type="text" order="ascending"/>
		<tr>
			<td><xsl:value-of select="name"/></td>
			<td align="center">
				<xsl:choose>
					<xsl:when test="race = 1"><img src="icon/race/1-0.gif" alt="Human" /></xsl:when>
					<xsl:when test="race = 2"><img src="icon/race/2-0.gif" alt="Orc" /></xsl:when>
					<xsl:when test="race = 3"><img src="icon/race/3-0.gif" alt="Dwarf" /></xsl:when>
					<xsl:when test="race = 4"><img src="icon/race/4-0.gif" alt="Night Elf" /></xsl:when>
					<xsl:when test="race = 5"><img src="icon/race/5-0.gif" alt="Undead" /></xsl:when>
					<xsl:when test="race = 6"><img src="icon/race/6-0.gif" alt="Tauren" /></xsl:when>
					<xsl:when test="race = 7"><img src="icon/race/7-0.gif" alt="Gnome" /></xsl:when>
					<xsl:when test="race = 8"><img src="icon/race/8-0.gif" alt="Troll" /></xsl:when>
					<xsl:when test="race = 10"><img src="icon/race/10-0.gif" alt="Blood Elf" /></xsl:when>
					<xsl:when test="race = 11"><img src="icon/race/11-0.gif" alt="Draenei" /></xsl:when>
				</xsl:choose>
			</td>
			<td align="center">
				<xsl:choose>
					<xsl:when test="class = 1"><img src="icon/class/1.gif" alt="Warrior" /></xsl:when>
					<xsl:when test="class = 2"><img src="icon/class/2.gif" alt="Paladin" /></xsl:when>
					<xsl:when test="class = 3"><img src="icon/class/3.gif" alt="Hunter" /></xsl:when>
					<xsl:when test="class = 4"><img src="icon/class/4.gif" alt="Rogue" /></xsl:when>
					<xsl:when test="class = 5"><img src="icon/class/5.gif" alt="Priest" /></xsl:when>
					<xsl:when test="class = 6"><img src="icon/class/6.gif" alt="Death Knight" /></xsl:when>
					<xsl:when test="class = 7"><img src="icon/class/7.gif" alt="Shaman" /></xsl:when>
					<xsl:when test="class = 8"><img src="icon/class/8.gif" alt="Mage" /></xsl:when>
					<xsl:when test="class = 9"><img src="icon/class/9.gif" alt="Warlock" /></xsl:when>
					<xsl:when test="class = 11"><img src="icon/class/11.gif" alt="Druid" /></xsl:when>
				</xsl:choose>
			</td>
			<td align="center">
				<xsl:choose>
					<xsl:when test="race = 1 or race = 3 or race = 4 or race = 7 or race = 11"><img src="icon/pvpranks/rank_default_0.gif" alt="Alliance" /></xsl:when>
					<xsl:when test="race = 2 or race = 5 or race = 6 or race = 8 or race = 10"><img src="icon/pvpranks/rank_default_1.gif" alt="Horde" /></xsl:when>
				</xsl:choose>
			</td>
			<td align="center">
				<xsl:value-of select="level"/>
			</td>

			<td nowrap="nowrap">
			<xsl:choose>
				<xsl:when test="map = 0">Eastern Kingdoms</xsl:when>
				<xsl:when test="map = 1">Kalimdor</xsl:when>
				<xsl:when test="map = 13">Testing</xsl:when>
				<xsl:when test="map = 25">Scott Test</xsl:when>
				<xsl:when test="map = 29">CashTest</xsl:when>

				<xsl:when test="map = 30">Alterac Valley</xsl:when>
				<xsl:when test="map = 33">Shadowfang Keep</xsl:when>
				<xsl:when test="map = 34">Stormwind Stockade</xsl:when>
				<xsl:when test="map = 35">StormwindPrison</xsl:when>
				<xsl:when test="map = 36">Deadmines</xsl:when>
				<xsl:when test="map = 37">Azshara Crater</xsl:when>

				<xsl:when test="map = 42">Collin's Test</xsl:when>
				<xsl:when test="map = 43">Wailing Caverns</xsl:when>
				<xsl:when test="map = 44">Monastery</xsl:when>
				<xsl:when test="map = 47">Razorfen Kraul</xsl:when>
				<xsl:when test="map = 48">Blackfathom Deeps</xsl:when>
				<xsl:when test="map = 70">Uldaman</xsl:when>

				<xsl:when test="map = 90">Gnomeregan</xsl:when>
				<xsl:when test="map = 109">Sunken Temple</xsl:when>
				<xsl:when test="map = 129">Razorfen Downs</xsl:when>
				<xsl:when test="map = 169">Emerald Dream</xsl:when>
				<xsl:when test="map = 189">Scarlet Monastery</xsl:when>
				<xsl:when test="map = 209">Zul'Farrak</xsl:when>

				<xsl:when test="map = 229">Blackrock Spire</xsl:when>
				<xsl:when test="map = 230">Blackrock Depths</xsl:when>
				<xsl:when test="map = 249">Onyxia's Lair</xsl:when>
				<xsl:when test="map = 269">Opening of the Dark Portal</xsl:when>
				<xsl:when test="map = 289">Scholomance</xsl:when>
				<xsl:when test="map = 309">Zul'Gurub</xsl:when>

				<xsl:when test="map = 329">Stratholme</xsl:when>
				<xsl:when test="map = 349">Maraudon</xsl:when>
				<xsl:when test="map = 369">Deeprun Tram</xsl:when>
				<xsl:when test="map = 389">Ragefire Chasm</xsl:when>
				<xsl:when test="map = 409">Molten Core</xsl:when>
				<xsl:when test="map = 429">Dire Maul</xsl:when>

				<xsl:when test="map = 449">Alliance PVP Barracks</xsl:when>
				<xsl:when test="map = 450">Horde PVP Barracks</xsl:when>
				<xsl:when test="map = 451">Development Land</xsl:when>
				<xsl:when test="map = 469">Blackwing Lair</xsl:when>
				<xsl:when test="map = 489">Warsong Gulch</xsl:when>
				<xsl:when test="map = 509">Ruins of Ahn'Qiraj</xsl:when>

				<xsl:when test="map = 529">Arathi Basin</xsl:when>
				<xsl:when test="map = 530">Outland</xsl:when>
				<xsl:when test="map = 531">Ahn'Qiraj Temple</xsl:when>
				<xsl:when test="map = 532">Karazhan</xsl:when>
				<xsl:when test="map = 533">Naxxramas</xsl:when>
				<xsl:when test="map = 534">The Battle for Mount Hyjal</xsl:when>

				<xsl:when test="map = 540">Hellfire Citadel: The Shattered Halls</xsl:when>
				<xsl:when test="map = 542">Hellfire Citadel: The Blood Furnace</xsl:when>
				<xsl:when test="map = 543">Hellfire Citadel: Ramparts</xsl:when>
				<xsl:when test="map = 544">Magtheridon's Lair</xsl:when>
				<xsl:when test="map = 545">Coilfang: The Steamvault</xsl:when>
				<xsl:when test="map = 546">Coilfang: The Underbog</xsl:when>

				<xsl:when test="map = 547">Coilfang: The Slave Pens</xsl:when>
				<xsl:when test="map = 548">Coilfang: Serpentshrine Cavern</xsl:when>
				<xsl:when test="map = 550">Tempest Keep</xsl:when>
				<xsl:when test="map = 552">Tempest Keep: The Arcatraz</xsl:when>
				<xsl:when test="map = 553">Tempest Keep: The Botanica</xsl:when>
				<xsl:when test="map = 554">Tempest Keep: The Mechanar</xsl:when>

				<xsl:when test="map = 555">Auchindoun: Shadow Labyrinth</xsl:when>
				<xsl:when test="map = 556">Auchindoun: Sethekk Halls</xsl:when>
				<xsl:when test="map = 557">Auchindoun: Mana-Tombs</xsl:when>
				<xsl:when test="map = 558">Auchindoun: Auchenai Crypts</xsl:when>
				<xsl:when test="map = 559">Nagrand Arena</xsl:when>
				<xsl:when test="map = 560">The Escape From Durnholde</xsl:when>

				<xsl:when test="map = 562">Blade's Edge Arena</xsl:when>
				<xsl:when test="map = 564">Black Temple</xsl:when>
				<xsl:when test="map = 565">Gruul's Lair</xsl:when>
				<xsl:when test="map = 566">Eye of the Storm</xsl:when>
				<xsl:when test="map = 568">Zul'Aman</xsl:when>
			 </xsl:choose>
			 <!-- (<xsl:value-of select="map"/>) -->
			 </td>

			<td nowrap="nowrap">
			
			<xsl:choose>
				<xsl:when test="zone = 0">Unknown</xsl:when>
				<xsl:when test="zone = 1">Dun Morogh</xsl:when>
				<xsl:when test="zone = 2">Longshore</xsl:when>
				<xsl:when test="zone = 3">Badlands</xsl:when>

				<xsl:when test="zone = 4">Blasted Lands</xsl:when>
				<xsl:when test="zone = 7">Blackwater Cove</xsl:when>
				<xsl:when test="zone = 8">Swamp of Sorrows</xsl:when>
				<xsl:when test="zone = 9">Northshire Valley</xsl:when>
				<xsl:when test="zone = 10">Duskwood</xsl:when>
				<xsl:when test="zone = 11">Wetlands</xsl:when>

				<xsl:when test="zone = 12">Elwynn Forest</xsl:when>
				<xsl:when test="zone = 13">The World Tree</xsl:when>
				<xsl:when test="zone = 14">Durotar</xsl:when>
				<xsl:when test="zone = 15">Dustwallow Marsh</xsl:when>
				<xsl:when test="zone = 16">Azshara</xsl:when>
				<xsl:when test="zone = 17">The Barrens</xsl:when>

				<xsl:when test="zone = 18">Crystal Lake</xsl:when>
				<xsl:when test="zone = 19">Zul'Gurub</xsl:when>
				<xsl:when test="zone = 20">Moonbrook</xsl:when>
				<xsl:when test="zone = 21">Kul Tiras</xsl:when>
				<xsl:when test="zone = 22">Programmer Isle</xsl:when>
				<xsl:when test="zone = 23">Northshire River</xsl:when>

				<xsl:when test="zone = 24">Northshire Abbey</xsl:when>
				<xsl:when test="zone = 25">Blackrock Mountain</xsl:when>
				<xsl:when test="zone = 26">Lighthouse</xsl:when>
				<xsl:when test="zone = 28">Western Plaguelands</xsl:when>
				<xsl:when test="zone = 30">Nine</xsl:when>
				<xsl:when test="zone = 32">The Cemetary</xsl:when>

				<xsl:when test="zone = 33">Stranglethorn Vale</xsl:when>
				<xsl:when test="zone = 34">Echo Ridge Mine</xsl:when>
				<xsl:when test="zone = 35">Booty Bay</xsl:when>
				<xsl:when test="zone = 36">Alterac Mountains</xsl:when>
				<xsl:when test="zone = 37">Lake Nazferiti</xsl:when>
				<xsl:when test="zone = 38">Loch Modan</xsl:when>

				<xsl:when test="zone = 40">Westfall</xsl:when>
				<xsl:when test="zone = 41">Deadwind Pass</xsl:when>
				<xsl:when test="zone = 42">Darkshire</xsl:when>
				<xsl:when test="zone = 43">Wild Shore</xsl:when>
				<xsl:when test="zone = 44">Redridge Mountains</xsl:when>
				<xsl:when test="zone = 45">Arathi Highlands</xsl:when>

				<xsl:when test="zone = 46">Burning Steppes</xsl:when>
				<xsl:when test="zone = 47">The Hinterlands</xsl:when>
				<xsl:when test="zone = 49">Dead Man's Hole</xsl:when>
				<xsl:when test="zone = 51">Searing Gorge</xsl:when>
				<xsl:when test="zone = 53">Thieves Camp</xsl:when>
				<xsl:when test="zone = 54">Jasperlode Mine</xsl:when>

				<xsl:when test="zone = 55">Valley of Heroes UNUSED</xsl:when>
				<xsl:when test="zone = 56">Heroes' Vigil</xsl:when>
				<xsl:when test="zone = 57">Fargodeep Mine</xsl:when>
				<xsl:when test="zone = 59">Northshire Vineyards</xsl:when>
				<xsl:when test="zone = 60">Forest's Edge</xsl:when>
				<xsl:when test="zone = 61">Thunder Falls</xsl:when>

				<xsl:when test="zone = 62">Brackwell Pumpkin Patch</xsl:when>
				<xsl:when test="zone = 63">The Stonefield Farm</xsl:when>
				<xsl:when test="zone = 64">The Maclure Vineyards</xsl:when>
				<xsl:when test="zone = 65">***On Map Dungeon***</xsl:when>
				<xsl:when test="zone = 66">***On Map Dungeon***</xsl:when>
				<xsl:when test="zone = 67">***On Map Dungeon***</xsl:when>

				<xsl:when test="zone = 68">Lake Everstill</xsl:when>
				<xsl:when test="zone = 69">Lakeshire</xsl:when>
				<xsl:when test="zone = 70">Stonewatch</xsl:when>
				<xsl:when test="zone = 71">Stonewatch Falls</xsl:when>
				<xsl:when test="zone = 72">The Dark Portal</xsl:when>
				<xsl:when test="zone = 73">The Tainted Scar</xsl:when>

				<xsl:when test="zone = 74">Pool of Tears</xsl:when>
				<xsl:when test="zone = 75">Stonard</xsl:when>
				<xsl:when test="zone = 76">Fallow Sanctuary</xsl:when>
				<xsl:when test="zone = 77">Anvilmar</xsl:when>
				<xsl:when test="zone = 80">Stormwind Mountains</xsl:when>
				<xsl:when test="zone = 81">Jeff NE Quadrant Changed</xsl:when>

				<xsl:when test="zone = 82">Jeff NW Quadrant</xsl:when>
				<xsl:when test="zone = 83">Jeff SE Quadrant</xsl:when>
				<xsl:when test="zone = 84">Jeff SW Quadrant</xsl:when>
				<xsl:when test="zone = 85">Tirisfal Glades</xsl:when>
				<xsl:when test="zone = 86">Stone Cairn Lake</xsl:when>
				<xsl:when test="zone = 87">Goldshire</xsl:when>

				<xsl:when test="zone = 88">Eastvale Logging Camp</xsl:when>
				<xsl:when test="zone = 89">Mirror Lake Orchard</xsl:when>
				<xsl:when test="zone = 91">Tower of Azora</xsl:when>
				<xsl:when test="zone = 92">Mirror Lake</xsl:when>
				<xsl:when test="zone = 93">Vul'Gol Ogre Mound</xsl:when>
				<xsl:when test="zone = 94">Raven Hill</xsl:when>

				<xsl:when test="zone = 95">Redridge Canyons</xsl:when>
				<xsl:when test="zone = 96">Tower of Ilgalar</xsl:when>
				<xsl:when test="zone = 97">Alther's Mill</xsl:when>
				<xsl:when test="zone = 98">Rethban Caverns</xsl:when>
				<xsl:when test="zone = 99">Rebel Camp</xsl:when>
				<xsl:when test="zone = 100">Nesingwary's Expedition</xsl:when>

				<xsl:when test="zone = 101">Kurzen's Compound</xsl:when>
				<xsl:when test="zone = 102">Ruins of Zul'Kunda</xsl:when>
				<xsl:when test="zone = 103">Ruins of Zul'Mamwe</xsl:when>
				<xsl:when test="zone = 104">The Vile Reef</xsl:when>
				<xsl:when test="zone = 105">Mosh'Ogg Ogre Mound</xsl:when>
				<xsl:when test="zone = 106">The Stockpile</xsl:when>

				<xsl:when test="zone = 107">Saldean's Farm</xsl:when>
				<xsl:when test="zone = 108">Sentinel Hill</xsl:when>
				<xsl:when test="zone = 109">Furlbrow's Pumpkin Farm</xsl:when>
				<xsl:when test="zone = 111">Jangolode Mine</xsl:when>
				<xsl:when test="zone = 113">Gold Coast Quarry</xsl:when>
				<xsl:when test="zone = 115">Westfall Lighthouse</xsl:when>

				<xsl:when test="zone = 116">Misty Valley</xsl:when>
				<xsl:when test="zone = 117">Grom'gol Base Camp</xsl:when>
				<xsl:when test="zone = 118">Whelgar's Excavation Site</xsl:when>
				<xsl:when test="zone = 120">Westbrook Garrison</xsl:when>
				<xsl:when test="zone = 121">Tranquil Gardens Cemetery</xsl:when>
				<xsl:when test="zone = 122">Zuuldaia Ruins</xsl:when>

				<xsl:when test="zone = 123">Bal'lal Ruins</xsl:when>
				<xsl:when test="zone = 125">Kal'ai Ruins</xsl:when>
				<xsl:when test="zone = 126">Tkashi Ruins</xsl:when>
				<xsl:when test="zone = 127">Balia'mah Ruins</xsl:when>
				<xsl:when test="zone = 128">Ziata'jai Ruins</xsl:when>
				<xsl:when test="zone = 129">Mizjah Ruins</xsl:when>

				<xsl:when test="zone = 130">Silverpine Forest</xsl:when>
				<xsl:when test="zone = 131">Kharanos</xsl:when>
				<xsl:when test="zone = 132">Coldridge Valley</xsl:when>
				<xsl:when test="zone = 133">Gnomeregan</xsl:when>
				<xsl:when test="zone = 134">Gol'Bolar Quarry</xsl:when>
				<xsl:when test="zone = 135">Frostmane Hold</xsl:when>

				<xsl:when test="zone = 136">The Grizzled Den</xsl:when>
				<xsl:when test="zone = 137">Brewnall Village</xsl:when>
				<xsl:when test="zone = 138">Misty Pine Refuge</xsl:when>
				<xsl:when test="zone = 139">Eastern Plaguelands</xsl:when>
				<xsl:when test="zone = 141">Teldrassil</xsl:when>
				<xsl:when test="zone = 142">Ironband's Excavation Site</xsl:when>

				<xsl:when test="zone = 143">Mo'grosh Stronghold</xsl:when>
				<xsl:when test="zone = 144">Thelsamar</xsl:when>
				<xsl:when test="zone = 145">Algaz Gate</xsl:when>
				<xsl:when test="zone = 146">Stonewrought Dam</xsl:when>
				<xsl:when test="zone = 147">The Farstrider Lodge</xsl:when>
				<xsl:when test="zone = 148">Darkshore</xsl:when>

				<xsl:when test="zone = 149">Silver Stream Mine</xsl:when>
				<xsl:when test="zone = 150">Menethil Harbor</xsl:when>
				<xsl:when test="zone = 151">Designer Island</xsl:when>
				<xsl:when test="zone = 152">The Bulwark</xsl:when>
				<xsl:when test="zone = 153">Ruins of Lordaeron</xsl:when>
				<xsl:when test="zone = 154">Deathknell</xsl:when>

				<xsl:when test="zone = 155">Night Web's Hollow</xsl:when>
				<xsl:when test="zone = 156">Solliden Farmstead</xsl:when>
				<xsl:when test="zone = 157">Agamand Mills</xsl:when>
				<xsl:when test="zone = 158">Agamand Family Crypt</xsl:when>
				<xsl:when test="zone = 159">Brill</xsl:when>
				<xsl:when test="zone = 160">Whispering Gardens</xsl:when>

				<xsl:when test="zone = 161">Terrace of Repose</xsl:when>
				<xsl:when test="zone = 162">Brightwater Lake</xsl:when>
				<xsl:when test="zone = 163">Gunther's Retreat</xsl:when>
				<xsl:when test="zone = 164">Garren's Haunt</xsl:when>
				<xsl:when test="zone = 165">Balnir Farmstead</xsl:when>
				<xsl:when test="zone = 166">Cold Hearth Manor</xsl:when>

				<xsl:when test="zone = 167">Crusader Outpost</xsl:when>
				<xsl:when test="zone = 168">The North Coast</xsl:when>
				<xsl:when test="zone = 169">Whispering Shore</xsl:when>
				<xsl:when test="zone = 170">Lordamere Lake</xsl:when>
				<xsl:when test="zone = 172">Fenris Isle</xsl:when>
				<xsl:when test="zone = 173">Faol's Rest</xsl:when>

				<xsl:when test="zone = 186">Dolanaar</xsl:when>
				<xsl:when test="zone = 187">Darnassus UNUSED</xsl:when>
				<xsl:when test="zone = 188">Shadowglen</xsl:when>
				<xsl:when test="zone = 189">Steelgrill's Depot</xsl:when>
				<xsl:when test="zone = 190">Hearthglen</xsl:when>
				<xsl:when test="zone = 192">Northridge Lumber Camp</xsl:when>

				<xsl:when test="zone = 193">Ruins of Andorhal</xsl:when>
				<xsl:when test="zone = 195">School of Necromancy</xsl:when>
				<xsl:when test="zone = 196">Uther's Tomb</xsl:when>
				<xsl:when test="zone = 197">Sorrow Hill</xsl:when>
				<xsl:when test="zone = 198">The Weeping Cave</xsl:when>
				<xsl:when test="zone = 199">Felstone Field</xsl:when>

				<xsl:when test="zone = 200">Dalson's Tears</xsl:when>
				<xsl:when test="zone = 201">Gahrron's Withering</xsl:when>
				<xsl:when test="zone = 202">The Writhing Haunt</xsl:when>
				<xsl:when test="zone = 203">Mardenholde Keep</xsl:when>
				<xsl:when test="zone = 204">Pyrewood Village</xsl:when>
				<xsl:when test="zone = 205">Dun Modr</xsl:when>

				<xsl:when test="zone = 206">Westfall</xsl:when>
				<xsl:when test="zone = 207">The Great Sea</xsl:when>
				<xsl:when test="zone = 208">Unused Ironcladcove</xsl:when>
				<xsl:when test="zone = 209">Shadowfang Keep</xsl:when>
				<xsl:when test="zone = 210">***On Map Dungeon***</xsl:when>
				<xsl:when test="zone = 211">Iceflow Lake</xsl:when>

				<xsl:when test="zone = 212">Helm's Bed Lake</xsl:when>
				<xsl:when test="zone = 213">Deep Elem Mine</xsl:when>
				<xsl:when test="zone = 214">The Great Sea</xsl:when>
				<xsl:when test="zone = 215">Mulgore</xsl:when>
				<xsl:when test="zone = 219">Alexston Farmstead</xsl:when>
				<xsl:when test="zone = 220">Red Cloud Mesa</xsl:when>

				<xsl:when test="zone = 221">Camp Narache</xsl:when>
				<xsl:when test="zone = 222">Bloodhoof Village</xsl:when>
				<xsl:when test="zone = 223">Stonebull Lake</xsl:when>
				<xsl:when test="zone = 224">Ravaged Caravan</xsl:when>
				<xsl:when test="zone = 225">Red Rocks</xsl:when>
				<xsl:when test="zone = 226">The Skittering Dark</xsl:when>

				<xsl:when test="zone = 227">Valgan's Field</xsl:when>
				<xsl:when test="zone = 228">The Sepulcher</xsl:when>
				<xsl:when test="zone = 229">Olsen's Farthing</xsl:when>
				<xsl:when test="zone = 230">The Greymane Wall</xsl:when>
				<xsl:when test="zone = 231">Beren's Peril</xsl:when>
				<xsl:when test="zone = 232">The Dawning Isles</xsl:when>

				<xsl:when test="zone = 233">Ambermill</xsl:when>
				<xsl:when test="zone = 235">Fenris Keep</xsl:when>
				<xsl:when test="zone = 236">Shadowfang Keep</xsl:when>
				<xsl:when test="zone = 237">The Decrepit Ferry</xsl:when>
				<xsl:when test="zone = 238">Malden's Orchard</xsl:when>
				<xsl:when test="zone = 239">The Ivar Patch</xsl:when>

				<xsl:when test="zone = 240">The Dead Field</xsl:when>
				<xsl:when test="zone = 241">The Rotting Orchard</xsl:when>
				<xsl:when test="zone = 242">Brightwood Grove</xsl:when>
				<xsl:when test="zone = 243">Forlorn Rowe</xsl:when>
				<xsl:when test="zone = 244">The Whipple Estate</xsl:when>
				<xsl:when test="zone = 245">The Yorgen Farmstead</xsl:when>

				<xsl:when test="zone = 246">The Cauldron</xsl:when>
				<xsl:when test="zone = 247">Grimesilt Dig Site</xsl:when>
				<xsl:when test="zone = 249">Dreadmaul Rock</xsl:when>
				<xsl:when test="zone = 250">Ruins of Thaurissan</xsl:when>
				<xsl:when test="zone = 251">Flame Crest</xsl:when>
				<xsl:when test="zone = 252">Blackrock Stronghold</xsl:when>

				<xsl:when test="zone = 253">The Pillar of Ash</xsl:when>
				<xsl:when test="zone = 254">Blackrock Mountain</xsl:when>
				<xsl:when test="zone = 255">Altar of Storms</xsl:when>
				<xsl:when test="zone = 256">Aldrassil</xsl:when>
				<xsl:when test="zone = 257">Shadowthread Cave</xsl:when>
				<xsl:when test="zone = 258">Fel Rock</xsl:when>

				<xsl:when test="zone = 259">Lake Al'Ameth</xsl:when>
				<xsl:when test="zone = 260">Starbreeze Village</xsl:when>
				<xsl:when test="zone = 261">Gnarlpine Hold</xsl:when>
				<xsl:when test="zone = 262">Ban'ethil Barrow Den</xsl:when>
				<xsl:when test="zone = 263">The Cleft</xsl:when>
				<xsl:when test="zone = 264">The Oracle Glade</xsl:when>

				<xsl:when test="zone = 265">Wellspring River</xsl:when>
				<xsl:when test="zone = 266">Wellspring Lake</xsl:when>
				<xsl:when test="zone = 267">Hillsbrad Foothills</xsl:when>
				<xsl:when test="zone = 268">Azshara Crater</xsl:when>
				<xsl:when test="zone = 269">Dun Algaz</xsl:when>
				<xsl:when test="zone = 271">Southshore</xsl:when>

				<xsl:when test="zone = 272">Tarren Mill</xsl:when>
				<xsl:when test="zone = 275">Durnholde Keep</xsl:when>
				<xsl:when test="zone = 276">UNUSED Stonewrought Pass</xsl:when>
				<xsl:when test="zone = 277">The Foothill Caverns</xsl:when>
				<xsl:when test="zone = 278">Lordamere Internment Camp</xsl:when>
				<xsl:when test="zone = 279">Dalaran</xsl:when>

				<xsl:when test="zone = 280">Strahnbrad</xsl:when>
				<xsl:when test="zone = 281">Ruins of Alterac</xsl:when>
				<xsl:when test="zone = 282">Crushridge Hold</xsl:when>
				<xsl:when test="zone = 283">Slaughter Hollow</xsl:when>
				<xsl:when test="zone = 284">The Uplands</xsl:when>
				<xsl:when test="zone = 285">Southpoint Tower</xsl:when>

				<xsl:when test="zone = 286">Hillsbrad Fields</xsl:when>
				<xsl:when test="zone = 287">Hillsbrad</xsl:when>
				<xsl:when test="zone = 288">Azurelode Mine</xsl:when>
				<xsl:when test="zone = 289">Nethander Stead</xsl:when>
				<xsl:when test="zone = 290">Dun Garok</xsl:when>
				<xsl:when test="zone = 293">Thoradin's Wall</xsl:when>

				<xsl:when test="zone = 294">Eastern Strand</xsl:when>
				<xsl:when test="zone = 295">Western Strand</xsl:when>
				<xsl:when test="zone = 296">South Seas UNUSED</xsl:when>
				<xsl:when test="zone = 297">Jaguero Isle</xsl:when>
				<xsl:when test="zone = 298">Baradin Bay</xsl:when>
				<xsl:when test="zone = 299">Menethil Bay</xsl:when>

				<xsl:when test="zone = 300">Misty Reed Strand</xsl:when>
				<xsl:when test="zone = 301">The Savage Coast</xsl:when>
				<xsl:when test="zone = 302">The Crystal Shore</xsl:when>
				<xsl:when test="zone = 303">Shell Beach</xsl:when>
				<xsl:when test="zone = 305">North Tide's Run</xsl:when>
				<xsl:when test="zone = 306">South Tide's Run</xsl:when>

				<xsl:when test="zone = 307">The Overlook Cliffs</xsl:when>
				<xsl:when test="zone = 308">The Forbidding Sea</xsl:when>
				<xsl:when test="zone = 309">Ironbeard's Tomb</xsl:when>
				<xsl:when test="zone = 310">Crystalvein Mine</xsl:when>
				<xsl:when test="zone = 311">Ruins of Aboraz</xsl:when>
				<xsl:when test="zone = 312">Janeiro's Point</xsl:when>

				<xsl:when test="zone = 313">Northfold Manor</xsl:when>
				<xsl:when test="zone = 314">Go'Shek Farm</xsl:when>
				<xsl:when test="zone = 315">Dabyrie's Farmstead</xsl:when>
				<xsl:when test="zone = 316">Boulderfist Hall</xsl:when>
				<xsl:when test="zone = 317">Witherbark Village</xsl:when>
				<xsl:when test="zone = 318">Drywhisker Gorge</xsl:when>

				<xsl:when test="zone = 320">Refuge Pointe</xsl:when>
				<xsl:when test="zone = 321">Hammerfall</xsl:when>
				<xsl:when test="zone = 322">Blackwater Shipwrecks</xsl:when>
				<xsl:when test="zone = 323">O'Breen's Camp</xsl:when>
				<xsl:when test="zone = 324">Stromgarde Keep</xsl:when>
				<xsl:when test="zone = 325">The Tower of Arathor</xsl:when>

				<xsl:when test="zone = 326">The Sanctum</xsl:when>
				<xsl:when test="zone = 327">Faldir's Cove</xsl:when>
				<xsl:when test="zone = 328">The Drowned Reef</xsl:when>
				<xsl:when test="zone = 330">Thandol Span</xsl:when>
				<xsl:when test="zone = 331">Ashenvale</xsl:when>
				<xsl:when test="zone = 332">The Great Sea</xsl:when>

				<xsl:when test="zone = 333">Circle of East Binding</xsl:when>
				<xsl:when test="zone = 334">Circle of West Binding</xsl:when>
				<xsl:when test="zone = 335">Circle of Inner Binding</xsl:when>
				<xsl:when test="zone = 336">Circle of Outer Binding</xsl:when>
				<xsl:when test="zone = 337">Apocryphan's Rest</xsl:when>
				<xsl:when test="zone = 338">Angor Fortress</xsl:when>

				<xsl:when test="zone = 339">Lethlor Ravine</xsl:when>
				<xsl:when test="zone = 340">Kargath</xsl:when>
				<xsl:when test="zone = 341">Camp Kosh</xsl:when>
				<xsl:when test="zone = 342">Camp Boff</xsl:when>
				<xsl:when test="zone = 343">Camp Wurg</xsl:when>
				<xsl:when test="zone = 344">Camp Cagg</xsl:when>

				<xsl:when test="zone = 345">Agmond's End</xsl:when>
				<xsl:when test="zone = 346">Hammertoe's Digsite</xsl:when>
				<xsl:when test="zone = 347">Dustbelch Grotto</xsl:when>
				<xsl:when test="zone = 348">Aerie Peak</xsl:when>
				<xsl:when test="zone = 349">Wildhammer Keep</xsl:when>
				<xsl:when test="zone = 350">Quel'Danil Lodge</xsl:when>

				<xsl:when test="zone = 351">Skulk Rock</xsl:when>
				<xsl:when test="zone = 352">Zun'watha</xsl:when>
				<xsl:when test="zone = 353">Shadra'Alor</xsl:when>
				<xsl:when test="zone = 354">Jintha'Alor</xsl:when>
				<xsl:when test="zone = 355">The Altar of Zul</xsl:when>
				<xsl:when test="zone = 356">Seradane</xsl:when>

				<xsl:when test="zone = 357">Feralas</xsl:when>
				<xsl:when test="zone = 358">Brambleblade Ravine</xsl:when>
				<xsl:when test="zone = 359">Bael Modan</xsl:when>
				<xsl:when test="zone = 360">The Venture Co. Mine</xsl:when>
				<xsl:when test="zone = 361">Felwood</xsl:when>
				<xsl:when test="zone = 362">Razor Hill</xsl:when>

				<xsl:when test="zone = 363">Valley of Trials</xsl:when>
				<xsl:when test="zone = 364">The Den</xsl:when>
				<xsl:when test="zone = 365">Burning Blade Coven</xsl:when>
				<xsl:when test="zone = 366">Kolkar Crag</xsl:when>
				<xsl:when test="zone = 367">Sen'jin Village</xsl:when>
				<xsl:when test="zone = 368">Echo Isles</xsl:when>

				<xsl:when test="zone = 369">Thunder Ridge</xsl:when>
				<xsl:when test="zone = 370">Drygulch Ravine</xsl:when>
				<xsl:when test="zone = 371">Dustwind Cave</xsl:when>
				<xsl:when test="zone = 372">Tiragarde Keep</xsl:when>
				<xsl:when test="zone = 373">Scuttle Coast</xsl:when>
				<xsl:when test="zone = 374">Bladefist Bay</xsl:when>

				<xsl:when test="zone = 375">Deadeye Shore</xsl:when>
				<xsl:when test="zone = 377">Southfury River</xsl:when>
				<xsl:when test="zone = 378">Camp Taurajo</xsl:when>
				<xsl:when test="zone = 379">Far Watch Post</xsl:when>
				<xsl:when test="zone = 380">The Crossroads</xsl:when>
				<xsl:when test="zone = 381">Boulder Lode Mine</xsl:when>

				<xsl:when test="zone = 382">The Sludge Fen</xsl:when>
				<xsl:when test="zone = 383">The Dry Hills</xsl:when>
				<xsl:when test="zone = 384">Dreadmist Peak</xsl:when>
				<xsl:when test="zone = 385">Northwatch Hold</xsl:when>
				<xsl:when test="zone = 386">The Forgotten Pools</xsl:when>
				<xsl:when test="zone = 387">Lushwater Oasis</xsl:when>

				<xsl:when test="zone = 388">The Stagnant Oasis</xsl:when>
				<xsl:when test="zone = 390">Field of Giants</xsl:when>
				<xsl:when test="zone = 391">The Merchant Coast</xsl:when>
				<xsl:when test="zone = 392">Ratchet</xsl:when>
				<xsl:when test="zone = 393">Darkspear Strand</xsl:when>
				<xsl:when test="zone = 394">Darrowmere Lake UNUSED</xsl:when>

				<xsl:when test="zone = 395">Caer Darrow UNUSED</xsl:when>
				<xsl:when test="zone = 396">Winterhoof Water Well</xsl:when>
				<xsl:when test="zone = 397">Thunderhorn Water Well</xsl:when>
				<xsl:when test="zone = 398">Wildmane Water Well</xsl:when>
				<xsl:when test="zone = 399">Skyline Ridge</xsl:when>
				<xsl:when test="zone = 400">Thousand Needles</xsl:when>

				<xsl:when test="zone = 401">The Tidus Stair</xsl:when>
				<xsl:when test="zone = 403">Shady Rest Inn</xsl:when>
				<xsl:when test="zone = 404">Bael'dun Digsite</xsl:when>
				<xsl:when test="zone = 405">Desolace</xsl:when>
				<xsl:when test="zone = 406">Stonetalon Mountains</xsl:when>
				<xsl:when test="zone = 407">Orgrimmar UNUSED</xsl:when>

				<xsl:when test="zone = 408">Gillijim's Isle</xsl:when>
				<xsl:when test="zone = 409">Island of Doctor Lapidis</xsl:when>
				<xsl:when test="zone = 410">Razorwind Canyon</xsl:when>
				<xsl:when test="zone = 411">Bathran's Haunt</xsl:when>
				<xsl:when test="zone = 412">The Ruins of Ordil'Aran</xsl:when>
				<xsl:when test="zone = 413">Maestra's Post</xsl:when>

				<xsl:when test="zone = 414">The Zoram Strand</xsl:when>
				<xsl:when test="zone = 415">Astranaar</xsl:when>
				<xsl:when test="zone = 416">The Shrine of Aessina</xsl:when>
				<xsl:when test="zone = 417">Fire Scar Shrine</xsl:when>
				<xsl:when test="zone = 418">The Ruins of Stardust</xsl:when>
				<xsl:when test="zone = 419">The Howling Vale</xsl:when>

				<xsl:when test="zone = 420">Silverwind Refuge</xsl:when>
				<xsl:when test="zone = 421">Mystral Lake</xsl:when>
				<xsl:when test="zone = 422">Fallen Sky Lake</xsl:when>
				<xsl:when test="zone = 424">Iris Lake</xsl:when>
				<xsl:when test="zone = 425">Moonwell</xsl:when>
				<xsl:when test="zone = 426">Raynewood Retreat</xsl:when>

				<xsl:when test="zone = 427">The Shady Nook</xsl:when>
				<xsl:when test="zone = 428">Night Run</xsl:when>
				<xsl:when test="zone = 429">Xavian</xsl:when>
				<xsl:when test="zone = 430">Satyrnaar</xsl:when>
				<xsl:when test="zone = 431">Splintertree Post</xsl:when>
				<xsl:when test="zone = 432">The Dor'Danil Barrow Den</xsl:when>

				<xsl:when test="zone = 433">Falfarren River</xsl:when>
				<xsl:when test="zone = 434">Felfire Hill</xsl:when>
				<xsl:when test="zone = 435">Demon Fall Canyon</xsl:when>
				<xsl:when test="zone = 436">Demon Fall Ridge</xsl:when>
				<xsl:when test="zone = 437">Warsong Lumber Camp</xsl:when>
				<xsl:when test="zone = 438">Bough Shadow</xsl:when>

				<xsl:when test="zone = 439">The Shimmering Flats</xsl:when>
				<xsl:when test="zone = 440">Tanaris</xsl:when>
				<xsl:when test="zone = 441">Lake Falathim</xsl:when>
				<xsl:when test="zone = 442">Auberdine</xsl:when>
				<xsl:when test="zone = 443">Ruins of Mathystra</xsl:when>
				<xsl:when test="zone = 444">Tower of Althalaxx</xsl:when>

				<xsl:when test="zone = 445">Cliffspring Falls</xsl:when>
				<xsl:when test="zone = 446">Bashal'Aran</xsl:when>
				<xsl:when test="zone = 447">Ameth'Aran</xsl:when>
				<xsl:when test="zone = 448">Grove of the Ancients</xsl:when>
				<xsl:when test="zone = 449">The Master's Glaive</xsl:when>
				<xsl:when test="zone = 450">Remtravel's Excavation</xsl:when>

				<xsl:when test="zone = 452">Mist's Edge</xsl:when>
				<xsl:when test="zone = 453">The Long Wash</xsl:when>
				<xsl:when test="zone = 454">Wildbend River</xsl:when>
				<xsl:when test="zone = 455">Blackwood Den</xsl:when>
				<xsl:when test="zone = 456">Cliffspring River</xsl:when>
				<xsl:when test="zone = 457">The Veiled Sea</xsl:when>

				<xsl:when test="zone = 458">Gold Road</xsl:when>
				<xsl:when test="zone = 459">Scarlet Watch Post</xsl:when>
				<xsl:when test="zone = 460">Sun Rock Retreat</xsl:when>
				<xsl:when test="zone = 461">Windshear Crag</xsl:when>
				<xsl:when test="zone = 463">Cragpool Lake</xsl:when>
				<xsl:when test="zone = 464">Mirkfallon Lake</xsl:when>

				<xsl:when test="zone = 465">The Charred Vale</xsl:when>
				<xsl:when test="zone = 466">Valley of the Bloodfuries</xsl:when>
				<xsl:when test="zone = 467">Stonetalon Peak</xsl:when>
				<xsl:when test="zone = 468">The Talon Den</xsl:when>
				<xsl:when test="zone = 469">Greatwood Vale</xsl:when>
				<xsl:when test="zone = 470">Thunder Bluff UNUSED</xsl:when>

				<xsl:when test="zone = 471">Brave Wind Mesa</xsl:when>
				<xsl:when test="zone = 472">Fire Stone Mesa</xsl:when>
				<xsl:when test="zone = 473">Mantle Rock</xsl:when>
				<xsl:when test="zone = 474">Hunter Rise UNUSED</xsl:when>
				<xsl:when test="zone = 475">Spirit RiseUNUSED</xsl:when>
				<xsl:when test="zone = 476">Elder RiseUNUSED</xsl:when>

				<xsl:when test="zone = 477">Ruins of Jubuwal</xsl:when>
				<xsl:when test="zone = 478">Pools of Arlithrien</xsl:when>
				<xsl:when test="zone = 479">The Rustmaul Dig Site</xsl:when>
				<xsl:when test="zone = 480">Camp E'thok</xsl:when>
				<xsl:when test="zone = 481">Splithoof Crag</xsl:when>
				<xsl:when test="zone = 482">Highperch</xsl:when>

				<xsl:when test="zone = 483">The Screeching Canyon</xsl:when>
				<xsl:when test="zone = 484">Freewind Post</xsl:when>
				<xsl:when test="zone = 485">The Great Lift</xsl:when>
				<xsl:when test="zone = 486">Galak Hold</xsl:when>
				<xsl:when test="zone = 487">Roguefeather Den</xsl:when>
				<xsl:when test="zone = 488">The Weathered Nook</xsl:when>

				<xsl:when test="zone = 489">Thalanaar</xsl:when>
				<xsl:when test="zone = 490">Un'Goro Crater</xsl:when>
				<xsl:when test="zone = 491">Razorfen Kraul</xsl:when>
				<xsl:when test="zone = 492">Raven Hill Cemetery</xsl:when>
				<xsl:when test="zone = 493">Moonglade</xsl:when>
				<xsl:when test="zone = 495">DELETE ME</xsl:when>

				<xsl:when test="zone = 496">Brackenwall Village</xsl:when>
				<xsl:when test="zone = 497">Swamplight Manor</xsl:when>
				<xsl:when test="zone = 498">Bloodfen Burrow</xsl:when>
				<xsl:when test="zone = 499">Darkmist Cavern</xsl:when>
				<xsl:when test="zone = 500">Moggle Point</xsl:when>
				<xsl:when test="zone = 501">Beezil's Wreck</xsl:when>

				<xsl:when test="zone = 502">Witch Hill</xsl:when>
				<xsl:when test="zone = 503">Sentry Point</xsl:when>
				<xsl:when test="zone = 504">North Point Tower</xsl:when>
				<xsl:when test="zone = 505">West Point Tower</xsl:when>
				<xsl:when test="zone = 506">Lost Point</xsl:when>
				<xsl:when test="zone = 507">Bluefen</xsl:when>

				<xsl:when test="zone = 508">Stonemaul Ruins</xsl:when>
				<xsl:when test="zone = 509">The Den of Flame</xsl:when>
				<xsl:when test="zone = 510">The Dragonmurk</xsl:when>
				<xsl:when test="zone = 511">Wyrmbog</xsl:when>
				<xsl:when test="zone = 512">Onyxia's Lair UNUSED</xsl:when>
				<xsl:when test="zone = 513">Theramore Isle</xsl:when>

				<xsl:when test="zone = 514">Foothold Citadel</xsl:when>
				<xsl:when test="zone = 515">Ironclad Prison</xsl:when>
				<xsl:when test="zone = 516">Dustwallow Bay</xsl:when>
				<xsl:when test="zone = 517">Tidefury Cove</xsl:when>
				<xsl:when test="zone = 518">Dreadmurk Shore</xsl:when>
				<xsl:when test="zone = 536">Addle's Stead</xsl:when>

				<xsl:when test="zone = 537">Fire Plume Ridge</xsl:when>
				<xsl:when test="zone = 538">Lakkari Tar Pits</xsl:when>
				<xsl:when test="zone = 539">Terror Run</xsl:when>
				<xsl:when test="zone = 540">The Slithering Scar</xsl:when>
				<xsl:when test="zone = 541">Marshal's Refuge</xsl:when>
				<xsl:when test="zone = 542">Fungal Rock</xsl:when>

				<xsl:when test="zone = 543">Golakka Hot Springs</xsl:when>
				<xsl:when test="zone = 556">The Loch</xsl:when>
				<xsl:when test="zone = 576">Beggar's Haunt</xsl:when>
				<xsl:when test="zone = 596">Kodo Graveyard</xsl:when>
				<xsl:when test="zone = 597">Ghost Walker Post</xsl:when>
				<xsl:when test="zone = 598">Sar'theris Strand</xsl:when>

				<xsl:when test="zone = 599">Thunder Axe Fortress</xsl:when>
				<xsl:when test="zone = 600">Bolgan's Hole</xsl:when>
				<xsl:when test="zone = 602">Mannoroc Coven</xsl:when>
				<xsl:when test="zone = 603">Sargeron</xsl:when>
				<xsl:when test="zone = 604">Magram Village</xsl:when>
				<xsl:when test="zone = 606">Gelkis Village</xsl:when>

				<xsl:when test="zone = 607">Valley of Spears</xsl:when>
				<xsl:when test="zone = 608">Nijel's Point</xsl:when>
				<xsl:when test="zone = 609">Kolkar Village</xsl:when>
				<xsl:when test="zone = 616">Hyjal</xsl:when>
				<xsl:when test="zone = 618">Winterspring</xsl:when>
				<xsl:when test="zone = 636">Blackwolf River</xsl:when>

				<xsl:when test="zone = 637">Kodo Rock</xsl:when>
				<xsl:when test="zone = 638">Hidden Path</xsl:when>
				<xsl:when test="zone = 639">Spirit Rock</xsl:when>
				<xsl:when test="zone = 640">Shrine of the Dormant Flame</xsl:when>
				<xsl:when test="zone = 656">Lake Elune'ara</xsl:when>
				<xsl:when test="zone = 657">The Harborage</xsl:when>

				<xsl:when test="zone = 676">Outland</xsl:when>
				<xsl:when test="zone = 696">Craftsmen's Terrace UNUSED</xsl:when>
				<xsl:when test="zone = 697">Tradesmen's Terrace UNUSED</xsl:when>
				<xsl:when test="zone = 698">The Temple Gardens UNUSED</xsl:when>
				<xsl:when test="zone = 699">Temple of Elune UNUSED</xsl:when>
				<xsl:when test="zone = 700">Cenarion Enclave UNUSED</xsl:when>

				<xsl:when test="zone = 701">Warrior's Terrace UNUSED</xsl:when>
				<xsl:when test="zone = 702">Rut'theran Village</xsl:when>
				<xsl:when test="zone = 716">Ironband's Compound</xsl:when>
				<xsl:when test="zone = 717">The Stockade</xsl:when>
				<xsl:when test="zone = 718">Wailing Caverns</xsl:when>
				<xsl:when test="zone = 719">Blackfathom Deeps</xsl:when>

				<xsl:when test="zone = 720">Fray Island</xsl:when>
				<xsl:when test="zone = 721">Gnomeregan</xsl:when>
				<xsl:when test="zone = 722">Razorfen Downs</xsl:when>
				<xsl:when test="zone = 736">Ban'ethil Hollow</xsl:when>
				<xsl:when test="zone = 796">Scarlet Monastery</xsl:when>
				<xsl:when test="zone = 797">Jerod's Landing</xsl:when>

				<xsl:when test="zone = 798">Ridgepoint Tower</xsl:when>
				<xsl:when test="zone = 799">The Darkened Bank</xsl:when>
				<xsl:when test="zone = 800">Coldridge Pass</xsl:when>
				<xsl:when test="zone = 801">Chill Breeze Valley</xsl:when>
				<xsl:when test="zone = 802">Shimmer Ridge</xsl:when>
				<xsl:when test="zone = 803">Amberstill Ranch</xsl:when>

				<xsl:when test="zone = 804">The Tundrid Hills</xsl:when>
				<xsl:when test="zone = 805">South Gate Pass</xsl:when>
				<xsl:when test="zone = 806">South Gate Outpost</xsl:when>
				<xsl:when test="zone = 807">North Gate Pass</xsl:when>
				<xsl:when test="zone = 808">North Gate Outpost</xsl:when>
				<xsl:when test="zone = 809">Gates of Ironforge</xsl:when>

				<xsl:when test="zone = 810">Stillwater Pond</xsl:when>
				<xsl:when test="zone = 811">Nightmare Vale</xsl:when>
				<xsl:when test="zone = 812">Venomweb Vale</xsl:when>
				<xsl:when test="zone = 813">The Bulwark</xsl:when>
				<xsl:when test="zone = 814">Southfury River</xsl:when>
				<xsl:when test="zone = 815">Southfury River</xsl:when>

				<xsl:when test="zone = 816">Razormane Grounds</xsl:when>
				<xsl:when test="zone = 817">Skull Rock</xsl:when>
				<xsl:when test="zone = 818">Palemane Rock</xsl:when>
				<xsl:when test="zone = 819">Windfury Ridge</xsl:when>
				<xsl:when test="zone = 820">The Golden Plains</xsl:when>
				<xsl:when test="zone = 821">The Rolling Plains</xsl:when>

				<xsl:when test="zone = 836">Dun Algaz</xsl:when>
				<xsl:when test="zone = 837">Dun Algaz</xsl:when>
				<xsl:when test="zone = 838">North Gate Pass</xsl:when>
				<xsl:when test="zone = 839">South Gate Pass</xsl:when>
				<xsl:when test="zone = 856">Twilight Grove</xsl:when>
				<xsl:when test="zone = 876">GM Island</xsl:when>

				<xsl:when test="zone = 877">Delete ME</xsl:when>
				<xsl:when test="zone = 878">Southfury River</xsl:when>
				<xsl:when test="zone = 879">Southfury River</xsl:when>
				<xsl:when test="zone = 880">Thandol Span</xsl:when>
				<xsl:when test="zone = 881">Thandol Span</xsl:when>
				<xsl:when test="zone = 896">Purgation Isle</xsl:when>

				<xsl:when test="zone = 916">The Jansen Stead</xsl:when>
				<xsl:when test="zone = 917">The Dead Acre</xsl:when>
				<xsl:when test="zone = 918">The Molsen Farm</xsl:when>
				<xsl:when test="zone = 919">Stendel's Pond</xsl:when>
				<xsl:when test="zone = 920">The Dagger Hills</xsl:when>
				<xsl:when test="zone = 921">Demont's Place</xsl:when>

				<xsl:when test="zone = 922">The Dust Plains</xsl:when>
				<xsl:when test="zone = 923">Stonesplinter Valley</xsl:when>
				<xsl:when test="zone = 924">Valley of Kings</xsl:when>
				<xsl:when test="zone = 925">Algaz Station</xsl:when>
				<xsl:when test="zone = 926">Bucklebree Farm</xsl:when>
				<xsl:when test="zone = 927">The Shining Strand</xsl:when>

				<xsl:when test="zone = 928">North Tide's Hollow</xsl:when>
				<xsl:when test="zone = 936">Grizzlepaw Ridge</xsl:when>
				<xsl:when test="zone = 956">The Verdant Fields</xsl:when>
				<xsl:when test="zone = 976">Gadgetzan</xsl:when>
				<xsl:when test="zone = 977">Steamwheedle Port</xsl:when>
				<xsl:when test="zone = 978">Zul'Farrak</xsl:when>

				<xsl:when test="zone = 979">Sandsorrow Watch</xsl:when>
				<xsl:when test="zone = 980">Thistleshrub Valley</xsl:when>
				<xsl:when test="zone = 981">The Gaping Chasm</xsl:when>
				<xsl:when test="zone = 982">The Noxious Lair</xsl:when>
				<xsl:when test="zone = 983">Dunemaul Compound</xsl:when>
				<xsl:when test="zone = 984">Eastmoon Ruins</xsl:when>

				<xsl:when test="zone = 985">Waterspring Field</xsl:when>
				<xsl:when test="zone = 986">Zalashji's Den</xsl:when>
				<xsl:when test="zone = 987">Land's End Beach</xsl:when>
				<xsl:when test="zone = 988">Wavestrider Beach</xsl:when>
				<xsl:when test="zone = 989">Uldum</xsl:when>
				<xsl:when test="zone = 990">Valley of the Watchers</xsl:when>

				<xsl:when test="zone = 991">Gunstan's Post</xsl:when>
				<xsl:when test="zone = 992">Southmoon Ruins</xsl:when>
				<xsl:when test="zone = 996">Render's Camp</xsl:when>
				<xsl:when test="zone = 997">Render's Valley</xsl:when>
				<xsl:when test="zone = 998">Render's Rock</xsl:when>
				<xsl:when test="zone = 999">Stonewatch Tower</xsl:when>

				<xsl:when test="zone = 1000">Galardell Valley</xsl:when>
				<xsl:when test="zone = 1001">Lakeridge Highway</xsl:when>
				<xsl:when test="zone = 1002">Three Corners</xsl:when>
				<xsl:when test="zone = 1016">Direforge Hill</xsl:when>
				<xsl:when test="zone = 1017">Raptor Ridge</xsl:when>
				<xsl:when test="zone = 1018">Black Channel Marsh</xsl:when>

				<xsl:when test="zone = 1019">The Green Belt</xsl:when>
				<xsl:when test="zone = 1020">Mosshide Fen</xsl:when>
				<xsl:when test="zone = 1021">Thelgen Rock</xsl:when>
				<xsl:when test="zone = 1022">Bluegill Marsh</xsl:when>
				<xsl:when test="zone = 1023">Saltspray Glen</xsl:when>
				<xsl:when test="zone = 1024">Sundown Marsh</xsl:when>

				<xsl:when test="zone = 1025">The Green Belt</xsl:when>
				<xsl:when test="zone = 1036">Angerfang Encampment</xsl:when>
				<xsl:when test="zone = 1037">Grim Batol</xsl:when>
				<xsl:when test="zone = 1038">Dragonmaw Gates</xsl:when>
				<xsl:when test="zone = 1039">The Lost Fleet</xsl:when>
				<xsl:when test="zone = 1056">Darrow Hill</xsl:when>

				<xsl:when test="zone = 1057">Thoradin's Wall</xsl:when>
				<xsl:when test="zone = 1076">Webwinder Path</xsl:when>
				<xsl:when test="zone = 1097">The Hushed Bank</xsl:when>
				<xsl:when test="zone = 1098">Manor Mistmantle</xsl:when>
				<xsl:when test="zone = 1099">Camp Mojache</xsl:when>
				<xsl:when test="zone = 1100">Grimtotem Compound</xsl:when>

				<xsl:when test="zone = 1101">The Writhing Deep</xsl:when>
				<xsl:when test="zone = 1102">Wildwind Lake</xsl:when>
				<xsl:when test="zone = 1103">Gordunni Outpost</xsl:when>
				<xsl:when test="zone = 1104">Mok'Gordun</xsl:when>
				<xsl:when test="zone = 1105">Feral Scar Vale</xsl:when>
				<xsl:when test="zone = 1106">Frayfeather Highlands</xsl:when>

				<xsl:when test="zone = 1107">Idlewind Lake</xsl:when>
				<xsl:when test="zone = 1108">The Forgotten Coast</xsl:when>
				<xsl:when test="zone = 1109">East Pillar</xsl:when>
				<xsl:when test="zone = 1110">West Pillar</xsl:when>
				<xsl:when test="zone = 1111">Dream Bough</xsl:when>
				<xsl:when test="zone = 1112">Jademir Lake</xsl:when>

				<xsl:when test="zone = 1113">Oneiros</xsl:when>
				<xsl:when test="zone = 1114">Ruins of Ravenwind</xsl:when>
				<xsl:when test="zone = 1115">Rage Scar Hold</xsl:when>
				<xsl:when test="zone = 1116">Feathermoon Stronghold</xsl:when>
				<xsl:when test="zone = 1117">Ruins of Solarsal</xsl:when>
				<xsl:when test="zone = 1118">Lower Wilds UNUSED</xsl:when>

				<xsl:when test="zone = 1119">The Twin Colossals</xsl:when>
				<xsl:when test="zone = 1120">Sardor Isle</xsl:when>
				<xsl:when test="zone = 1121">Isle of Dread</xsl:when>
				<xsl:when test="zone = 1136">High Wilderness</xsl:when>
				<xsl:when test="zone = 1137">Lower Wilds</xsl:when>
				<xsl:when test="zone = 1156">Southern Barrens</xsl:when>

				<xsl:when test="zone = 1157">Southern Gold Road</xsl:when>
				<xsl:when test="zone = 1176">Zul'Farrak</xsl:when>
				<xsl:when test="zone = 1196">UNUSEDAlcaz Island</xsl:when>
				<xsl:when test="zone = 1216">Timbermaw Hold</xsl:when>
				<xsl:when test="zone = 1217">Vanndir Encampment</xsl:when>
				<xsl:when test="zone = 1218">TESTAzshara</xsl:when>

				<xsl:when test="zone = 1219">Legash Encampment</xsl:when>
				<xsl:when test="zone = 1220">Thalassian Base Camp</xsl:when>
				<xsl:when test="zone = 1221">Ruins of Eldarath </xsl:when>
				<xsl:when test="zone = 1222">Hetaera's Clutch</xsl:when>
				<xsl:when test="zone = 1223">Temple of Zin-Malor</xsl:when>
				<xsl:when test="zone = 1224">Bear's Head</xsl:when>

				<xsl:when test="zone = 1225">Ursolan</xsl:when>
				<xsl:when test="zone = 1226">Temple of Arkkoran</xsl:when>
				<xsl:when test="zone = 1227">Bay of Storms</xsl:when>
				<xsl:when test="zone = 1228">The Shattered Strand</xsl:when>
				<xsl:when test="zone = 1229">Tower of Eldara</xsl:when>
				<xsl:when test="zone = 1230">Jagged Reef</xsl:when>

				<xsl:when test="zone = 1231">Southridge Beach</xsl:when>
				<xsl:when test="zone = 1232">Ravencrest Monument</xsl:when>
				<xsl:when test="zone = 1233">Forlorn Ridge</xsl:when>
				<xsl:when test="zone = 1234">Lake Mennar</xsl:when>
				<xsl:when test="zone = 1235">Shadowsong Shrine</xsl:when>
				<xsl:when test="zone = 1236">Haldarr Encampment</xsl:when>

				<xsl:when test="zone = 1237">Valormok</xsl:when>
				<xsl:when test="zone = 1256">The Ruined Reaches</xsl:when>
				<xsl:when test="zone = 1276">The Talondeep Path</xsl:when>
				<xsl:when test="zone = 1277">The Talondeep Path</xsl:when>
				<xsl:when test="zone = 1296">Rocktusk Farm</xsl:when>
				<xsl:when test="zone = 1297">Jaggedswine Farm</xsl:when>

				<xsl:when test="zone = 1316">Razorfen Downs</xsl:when>
				<xsl:when test="zone = 1336">Lost Rigger Cove</xsl:when>
				<xsl:when test="zone = 1337">Uldaman</xsl:when>
				<xsl:when test="zone = 1338">Lordamere Lake</xsl:when>
				<xsl:when test="zone = 1339">Lordamere Lake</xsl:when>
				<xsl:when test="zone = 1357">Gallows' Corner</xsl:when>

				<xsl:when test="zone = 1377">Silithus</xsl:when>
				<xsl:when test="zone = 1397">Emerald Forest</xsl:when>
				<xsl:when test="zone = 1417">Sunken Temple</xsl:when>
				<xsl:when test="zone = 1437">Dreadmaul Hold</xsl:when>
				<xsl:when test="zone = 1438">Nethergarde Keep</xsl:when>
				<xsl:when test="zone = 1439">Dreadmaul Post</xsl:when>

				<xsl:when test="zone = 1440">Serpent's Coil</xsl:when>
				<xsl:when test="zone = 1441">Altar of Storms</xsl:when>
				<xsl:when test="zone = 1442">Firewatch Ridge</xsl:when>
				<xsl:when test="zone = 1443">The Slag Pit</xsl:when>
				<xsl:when test="zone = 1444">The Sea of Cinders</xsl:when>
				<xsl:when test="zone = 1445">Blackrock Mountain</xsl:when>

				<xsl:when test="zone = 1446">Thorium Point</xsl:when>
				<xsl:when test="zone = 1457">Garrison Armory</xsl:when>
				<xsl:when test="zone = 1477">The Temple of Atal'Hakkar</xsl:when>
				<xsl:when test="zone = 1497">Undercity</xsl:when>
				<xsl:when test="zone = 1517">Uldaman</xsl:when>
				<xsl:when test="zone = 1518">Not Used Deadmines</xsl:when>

				<xsl:when test="zone = 1519">Stormwind City</xsl:when>
				<xsl:when test="zone = 1537">Ironforge</xsl:when>
				<xsl:when test="zone = 1557">Splithoof Hold</xsl:when>
				<xsl:when test="zone = 1577">The Cape of Stranglethorn</xsl:when>
				<xsl:when test="zone = 1578">Southern Savage Coast</xsl:when>
				<xsl:when test="zone = 1579">Unused The Deadmines 002</xsl:when>

				<xsl:when test="zone = 1580">Unused Ironclad Cove 003</xsl:when>
				<xsl:when test="zone = 1581">The Deadmines</xsl:when>
				<xsl:when test="zone = 1582">Ironclad Cove</xsl:when>
				<xsl:when test="zone = 1583">Blackrock Spire</xsl:when>
				<xsl:when test="zone = 1584">Blackrock Depths</xsl:when>
				<xsl:when test="zone = 1597">Raptor Grounds UNUSED</xsl:when>

				<xsl:when test="zone = 1598">Grol'dom Farm UNUSED</xsl:when>
				<xsl:when test="zone = 1599">Mor'shan Base Camp</xsl:when>
				<xsl:when test="zone = 1600">Honor's Stand UNUSED</xsl:when>
				<xsl:when test="zone = 1601">Blackthorn Ridge UNUSED</xsl:when>
				<xsl:when test="zone = 1602">Bramblescar UNUSED</xsl:when>
				<xsl:when test="zone = 1603">Agama'gor UNUSED</xsl:when>

				<xsl:when test="zone = 1617">Valley of Heroes</xsl:when>
				<xsl:when test="zone = 1637">Orgrimmar</xsl:when>
				<xsl:when test="zone = 1638">Thunder Bluff</xsl:when>
				<xsl:when test="zone = 1639">Elder Rise</xsl:when>
				<xsl:when test="zone = 1640">Spirit Rise</xsl:when>
				<xsl:when test="zone = 1641">Hunter Rise</xsl:when>

				<xsl:when test="zone = 1657">Darnassus</xsl:when>
				<xsl:when test="zone = 1658">Cenarion Enclave</xsl:when>
				<xsl:when test="zone = 1659">Craftsmen's Terrace</xsl:when>
				<xsl:when test="zone = 1660">Warrior's Terrace</xsl:when>
				<xsl:when test="zone = 1661">The Temple Gardens</xsl:when>
				<xsl:when test="zone = 1662">Tradesmen's Terrace</xsl:when>

				<xsl:when test="zone = 1677">Gavin's Naze</xsl:when>
				<xsl:when test="zone = 1678">Sofera's Naze</xsl:when>
				<xsl:when test="zone = 1679">Corrahn's Dagger</xsl:when>
				<xsl:when test="zone = 1680">The Headland</xsl:when>
				<xsl:when test="zone = 1681">Misty Shore</xsl:when>
				<xsl:when test="zone = 1682">Dandred's Fold</xsl:when>

				<xsl:when test="zone = 1683">Growless Cave</xsl:when>
				<xsl:when test="zone = 1684">Chillwind Point</xsl:when>
				<xsl:when test="zone = 1697">Raptor Grounds</xsl:when>
				<xsl:when test="zone = 1698">Bramblescar</xsl:when>
				<xsl:when test="zone = 1699">Thorn Hill</xsl:when>
				<xsl:when test="zone = 1700">Agama'gor</xsl:when>

				<xsl:when test="zone = 1701">Blackthorn Ridge</xsl:when>
				<xsl:when test="zone = 1702">Honor's Stand</xsl:when>
				<xsl:when test="zone = 1703">The Mor'shan Rampart</xsl:when>
				<xsl:when test="zone = 1704">Grol'dom Farm</xsl:when>
				<xsl:when test="zone = 1717">Razorfen Kraul</xsl:when>
				<xsl:when test="zone = 1718">The Great Lift</xsl:when>

				<xsl:when test="zone = 1737">Mistvale Valley</xsl:when>
				<xsl:when test="zone = 1738">Nek'mani Wellspring</xsl:when>
				<xsl:when test="zone = 1739">Bloodsail Compound</xsl:when>
				<xsl:when test="zone = 1740">Venture Co. Base Camp</xsl:when>
				<xsl:when test="zone = 1741">Gurubashi Arena</xsl:when>
				<xsl:when test="zone = 1742">Spirit Den</xsl:when>

				<xsl:when test="zone = 1757">The Crimson Veil</xsl:when>
				<xsl:when test="zone = 1758">The Riptide</xsl:when>
				<xsl:when test="zone = 1759">The Damsel's Luck</xsl:when>
				<xsl:when test="zone = 1760">Venture Co. Operations Center</xsl:when>
				<xsl:when test="zone = 1761">Deadwood Village</xsl:when>
				<xsl:when test="zone = 1762">Felpaw Village</xsl:when>

				<xsl:when test="zone = 1763">Jaedenar</xsl:when>
				<xsl:when test="zone = 1764">Bloodvenom River</xsl:when>
				<xsl:when test="zone = 1765">Bloodvenom Falls</xsl:when>
				<xsl:when test="zone = 1766">Shatter Scar Vale</xsl:when>
				<xsl:when test="zone = 1767">Irontree Woods</xsl:when>
				<xsl:when test="zone = 1768">Irontree Cavern</xsl:when>

				<xsl:when test="zone = 1769">Timbermaw Hold</xsl:when>
				<xsl:when test="zone = 1770">Shadow Hold</xsl:when>
				<xsl:when test="zone = 1771">Shrine of the Deceiver</xsl:when>
				<xsl:when test="zone = 1777">Itharius's Cave</xsl:when>
				<xsl:when test="zone = 1778">Sorrowmurk</xsl:when>
				<xsl:when test="zone = 1779">Draenil'dur Village</xsl:when>

				<xsl:when test="zone = 1780">Splinterspear Junction</xsl:when>
				<xsl:when test="zone = 1797">Stagalbog</xsl:when>
				<xsl:when test="zone = 1798">The Shifting Mire</xsl:when>
				<xsl:when test="zone = 1817">Stagalbog Cave</xsl:when>
				<xsl:when test="zone = 1837">Witherbark Caverns</xsl:when>
				<xsl:when test="zone = 1857">Thoradin's Wall</xsl:when>

				<xsl:when test="zone = 1858">Boulder'gor</xsl:when>
				<xsl:when test="zone = 1877">Valley of Fangs</xsl:when>
				<xsl:when test="zone = 1878">The Dustbowl</xsl:when>
				<xsl:when test="zone = 1879">Mirage Flats</xsl:when>
				<xsl:when test="zone = 1880">Featherbeard's Hovel</xsl:when>
				<xsl:when test="zone = 1881">Shindigger's Camp</xsl:when>

				<xsl:when test="zone = 1882">Plaguemist Ravine</xsl:when>
				<xsl:when test="zone = 1883">Valorwind Lake</xsl:when>
				<xsl:when test="zone = 1884">Agol'watha</xsl:when>
				<xsl:when test="zone = 1885">Hiri'watha</xsl:when>
				<xsl:when test="zone = 1886">The Creeping Ruin</xsl:when>
				<xsl:when test="zone = 1887">Bogen's Ledge</xsl:when>

				<xsl:when test="zone = 1897">The Maker's Terrace</xsl:when>
				<xsl:when test="zone = 1898">Dustwind Gulch</xsl:when>
				<xsl:when test="zone = 1917">Shaol'watha</xsl:when>
				<xsl:when test="zone = 1937">Noonshade Ruins</xsl:when>
				<xsl:when test="zone = 1938">Broken Pillar</xsl:when>
				<xsl:when test="zone = 1939">Abyssal Sands</xsl:when>

				<xsl:when test="zone = 1940">Southbreak Shore</xsl:when>
				<xsl:when test="zone = 1941">Caverns of Time</xsl:when>
				<xsl:when test="zone = 1942">The Marshlands</xsl:when>
				<xsl:when test="zone = 1943">Ironstone Plateau</xsl:when>
				<xsl:when test="zone = 1957">Blackchar Cave</xsl:when>
				<xsl:when test="zone = 1958">Tanner Camp</xsl:when>

				<xsl:when test="zone = 1959">Dustfire Valley</xsl:when>
				<xsl:when test="zone = 1977">Zul'Gurub</xsl:when>
				<xsl:when test="zone = 1978">Misty Reed Post</xsl:when>
				<xsl:when test="zone = 1997">Bloodvenom Post </xsl:when>
				<xsl:when test="zone = 1998">Talonbranch Glade </xsl:when>
				<xsl:when test="zone = 2017">Stratholme</xsl:when>

				<xsl:when test="zone = 2037">Quel'thalas</xsl:when>
				<xsl:when test="zone = 2057">Scholomance</xsl:when>
				<xsl:when test="zone = 2077">Twilight Vale</xsl:when>
				<xsl:when test="zone = 2078">Twilight Shore</xsl:when>
				<xsl:when test="zone = 2079">Alcaz Island</xsl:when>
				<xsl:when test="zone = 2097">Darkcloud Pinnacle</xsl:when>

				<xsl:when test="zone = 2098">Dawning Wood Catacombs</xsl:when>
				<xsl:when test="zone = 2099">Stonewatch Keep</xsl:when>
				<xsl:when test="zone = 2100">Maraudon</xsl:when>
				<xsl:when test="zone = 2101">Stoutlager Inn</xsl:when>
				<xsl:when test="zone = 2102">Thunderbrew Distillery</xsl:when>
				<xsl:when test="zone = 2103">Menethil Keep</xsl:when>

				<xsl:when test="zone = 2104">Deepwater Tavern</xsl:when>
				<xsl:when test="zone = 2117">Shadow Grave</xsl:when>
				<xsl:when test="zone = 2118">Brill Town Hall</xsl:when>
				<xsl:when test="zone = 2119">Gallows' End Tavern</xsl:when>
				<xsl:when test="zone = 2137">The Pools of VisionUNUSED</xsl:when>
				<xsl:when test="zone = 2138">Dreadmist Den</xsl:when>

				<xsl:when test="zone = 2157">Bael'dun Keep</xsl:when>
				<xsl:when test="zone = 2158">Emberstrife's Den</xsl:when>
				<xsl:when test="zone = 2159">Onyxia's Lair</xsl:when>
				<xsl:when test="zone = 2160">Windshear Mine</xsl:when>
				<xsl:when test="zone = 2161">Roland's Doom</xsl:when>
				<xsl:when test="zone = 2177">Battle Ring</xsl:when>

				<xsl:when test="zone = 2197">The Pools of Vision</xsl:when>
				<xsl:when test="zone = 2198">Shadowbreak Ravine</xsl:when>
				<xsl:when test="zone = 2217">Broken Spear Village</xsl:when>
				<xsl:when test="zone = 2237">Whitereach Post</xsl:when>
				<xsl:when test="zone = 2238">Gornia</xsl:when>
				<xsl:when test="zone = 2239">Zane's Eye Crater</xsl:when>

				<xsl:when test="zone = 2240">Mirage Raceway</xsl:when>
				<xsl:when test="zone = 2241">Frostsaber Rock</xsl:when>
				<xsl:when test="zone = 2242">The Hidden Grove</xsl:when>
				<xsl:when test="zone = 2243">Timbermaw Post</xsl:when>
				<xsl:when test="zone = 2244">Winterfall Village</xsl:when>
				<xsl:when test="zone = 2245">Mazthoril</xsl:when>

				<xsl:when test="zone = 2246">Frostfire Hot Springs</xsl:when>
				<xsl:when test="zone = 2247">Ice Thistle Hills</xsl:when>
				<xsl:when test="zone = 2248">Dun Mandarr</xsl:when>
				<xsl:when test="zone = 2249">Frostwhisper Gorge</xsl:when>
				<xsl:when test="zone = 2250">Owl Wing Thicket</xsl:when>
				<xsl:when test="zone = 2251">Lake Kel'Theril</xsl:when>

				<xsl:when test="zone = 2252">The Ruins of Kel'Theril</xsl:when>
				<xsl:when test="zone = 2253">Starfall Village</xsl:when>
				<xsl:when test="zone = 2254">Ban'Thallow Barrow Den</xsl:when>
				<xsl:when test="zone = 2255">Everlook</xsl:when>
				<xsl:when test="zone = 2256">Darkwhisper Gorge</xsl:when>
				<xsl:when test="zone = 2257">Deeprun Tram</xsl:when>

				<xsl:when test="zone = 2258">The Fungal Vale</xsl:when>
				<xsl:when test="zone = 2259">UNUSEDThe Marris Stead</xsl:when>
				<xsl:when test="zone = 2260">The Marris Stead</xsl:when>
				<xsl:when test="zone = 2261">The Undercroft</xsl:when>
				<xsl:when test="zone = 2262">Darrowshire</xsl:when>
				<xsl:when test="zone = 2263">Crown Guard Tower</xsl:when>

				<xsl:when test="zone = 2264">Corin's Crossing</xsl:when>
				<xsl:when test="zone = 2265">Scarlet Base Camp</xsl:when>
				<xsl:when test="zone = 2266">Tyr's Hand</xsl:when>
				<xsl:when test="zone = 2267">The Scarlet Basilica</xsl:when>
				<xsl:when test="zone = 2268">Light's Hope Chapel</xsl:when>
				<xsl:when test="zone = 2269">Browman Mill</xsl:when>

				<xsl:when test="zone = 2270">The Noxious Glade</xsl:when>
				<xsl:when test="zone = 2271">Eastwall Tower</xsl:when>
				<xsl:when test="zone = 2272">Northdale</xsl:when>
				<xsl:when test="zone = 2273">Zul'Mashar</xsl:when>
				<xsl:when test="zone = 2274">Mazra'Alor</xsl:when>
				<xsl:when test="zone = 2275">Northpass Tower</xsl:when>

				<xsl:when test="zone = 2276">Quel'Lithien Lodge</xsl:when>
				<xsl:when test="zone = 2277">Plaguewood</xsl:when>
				<xsl:when test="zone = 2278">Scourgehold</xsl:when>
				<xsl:when test="zone = 2279">Stratholme</xsl:when>
				<xsl:when test="zone = 2280">UNUSED Stratholme</xsl:when>
				<xsl:when test="zone = 2297">Darrowmere Lake</xsl:when>

				<xsl:when test="zone = 2298">Caer Darrow</xsl:when>
				<xsl:when test="zone = 2299">Darrowmere Lake</xsl:when>
				<xsl:when test="zone = 2300">Caverns of Time</xsl:when>
				<xsl:when test="zone = 2301">Thistlefur Village</xsl:when>
				<xsl:when test="zone = 2302">The Quagmire</xsl:when>
				<xsl:when test="zone = 2303">Windbreak Canyon</xsl:when>

				<xsl:when test="zone = 2317">South Seas</xsl:when>
				<xsl:when test="zone = 2318">The Great Sea</xsl:when>
				<xsl:when test="zone = 2319">The Great Sea</xsl:when>
				<xsl:when test="zone = 2320">The Great Sea</xsl:when>
				<xsl:when test="zone = 2321">The Great Sea</xsl:when>
				<xsl:when test="zone = 2322">The Veiled Sea</xsl:when>

				<xsl:when test="zone = 2323">The Veiled Sea</xsl:when>
				<xsl:when test="zone = 2324">The Veiled Sea</xsl:when>
				<xsl:when test="zone = 2325">The Veiled Sea</xsl:when>
				<xsl:when test="zone = 2326">The Veiled Sea</xsl:when>
				<xsl:when test="zone = 2337">Razor Hill Barracks</xsl:when>
				<xsl:when test="zone = 2338">South Seas</xsl:when>

				<xsl:when test="zone = 2339">The Great Sea</xsl:when>
				<xsl:when test="zone = 2357">Bloodtooth Camp</xsl:when>
				<xsl:when test="zone = 2358">Forest Song</xsl:when>
				<xsl:when test="zone = 2359">Greenpaw Village</xsl:when>
				<xsl:when test="zone = 2360">Silverwing Outpost</xsl:when>
				<xsl:when test="zone = 2361">Nighthaven</xsl:when>

				<xsl:when test="zone = 2362">Shrine of Remulos</xsl:when>
				<xsl:when test="zone = 2363">Stormrage Barrow Dens</xsl:when>
				<xsl:when test="zone = 2364">The Great Sea</xsl:when>
				<xsl:when test="zone = 2365">The Great Sea</xsl:when>
				<xsl:when test="zone = 2366">The Black Morass</xsl:when>
				<xsl:when test="zone = 2367">Old Hillsbrad Foothills</xsl:when>

				<xsl:when test="zone = 2368">Tarren Mill</xsl:when>
				<xsl:when test="zone = 2369">Southshore</xsl:when>
				<xsl:when test="zone = 2370">Durnholde Keep</xsl:when>
				<xsl:when test="zone = 2371">Dun Garok</xsl:when>
				<xsl:when test="zone = 2372">Hillsbrad Fields</xsl:when>
				<xsl:when test="zone = 2373">Eastern Strand</xsl:when>

				<xsl:when test="zone = 2374">Nethander Stead</xsl:when>
				<xsl:when test="zone = 2375">Darrow Hill</xsl:when>
				<xsl:when test="zone = 2376">Southpoint Tower</xsl:when>
				<xsl:when test="zone = 2377">Thoradin's Wall</xsl:when>
				<xsl:when test="zone = 2378">Western Strand</xsl:when>
				<xsl:when test="zone = 2379">Azurelode Mine</xsl:when>

				<xsl:when test="zone = 2397">The Great Sea</xsl:when>
				<xsl:when test="zone = 2398">The Great Sea</xsl:when>
				<xsl:when test="zone = 2399">The Great Sea</xsl:when>
				<xsl:when test="zone = 2400">The Forbidding Sea</xsl:when>
				<xsl:when test="zone = 2401">The Forbidding Sea</xsl:when>
				<xsl:when test="zone = 2402">The Forbidding Sea</xsl:when>

				<xsl:when test="zone = 2403">The Forbidding Sea</xsl:when>
				<xsl:when test="zone = 2404">Tethris Aran</xsl:when>
				<xsl:when test="zone = 2405">Ethel Rethor</xsl:when>
				<xsl:when test="zone = 2406">Ranazjar Isle</xsl:when>
				<xsl:when test="zone = 2407">Kormek's Hut</xsl:when>
				<xsl:when test="zone = 2408">Shadowprey Village</xsl:when>

				<xsl:when test="zone = 2417">Blackrock Pass</xsl:when>
				<xsl:when test="zone = 2418">Morgan's Vigil</xsl:when>
				<xsl:when test="zone = 2419">Slither Rock</xsl:when>
				<xsl:when test="zone = 2420">Terror Wing Path</xsl:when>
				<xsl:when test="zone = 2421">Draco'dar</xsl:when>
				<xsl:when test="zone = 2437">Ragefire Chasm</xsl:when>

				<xsl:when test="zone = 2457">Nightsong Woods</xsl:when>
				<xsl:when test="zone = 2477">The Veiled Sea</xsl:when>
				<xsl:when test="zone = 2478">Morlos'Aran</xsl:when>
				<xsl:when test="zone = 2479">Emerald Sanctuary</xsl:when>
				<xsl:when test="zone = 2480">Jadefire Glen</xsl:when>
				<xsl:when test="zone = 2481">Ruins of Constellas</xsl:when>

				<xsl:when test="zone = 2497">Bitter Reaches</xsl:when>
				<xsl:when test="zone = 2517">Rise of the Defiler</xsl:when>
				<xsl:when test="zone = 2518">Lariss Pavilion</xsl:when>
				<xsl:when test="zone = 2519">Woodpaw Hills</xsl:when>
				<xsl:when test="zone = 2520">Woodpaw Den</xsl:when>
				<xsl:when test="zone = 2521">Verdantis River</xsl:when>

				<xsl:when test="zone = 2522">Ruins of Isildien</xsl:when>
				<xsl:when test="zone = 2537">Grimtotem Post</xsl:when>
				<xsl:when test="zone = 2538">Camp Aparaje</xsl:when>
				<xsl:when test="zone = 2539">Malaka'jin</xsl:when>
				<xsl:when test="zone = 2540">Boulderslide Ravine</xsl:when>
				<xsl:when test="zone = 2541">Sishir Canyon</xsl:when>

				<xsl:when test="zone = 2557">Dire Maul</xsl:when>
				<xsl:when test="zone = 2558">Deadwind Ravine</xsl:when>
				<xsl:when test="zone = 2559">Diamondhead River</xsl:when>
				<xsl:when test="zone = 2560">Ariden's Camp</xsl:when>
				<xsl:when test="zone = 2561">The Vice</xsl:when>
				<xsl:when test="zone = 2562">Karazhan</xsl:when>

				<xsl:when test="zone = 2563">Morgan's Plot</xsl:when>
				<xsl:when test="zone = 2577">Dire Maul</xsl:when>
				<xsl:when test="zone = 2597">Alterac Valley</xsl:when>
				<xsl:when test="zone = 2617">Scrabblescrew's Camp</xsl:when>
				<xsl:when test="zone = 2618">Jadefire Run</xsl:when>
				<xsl:when test="zone = 2619">Thondroril River</xsl:when>

				<xsl:when test="zone = 2620">Thondroril River</xsl:when>
				<xsl:when test="zone = 2621">Lake Mereldar</xsl:when>
				<xsl:when test="zone = 2622">Pestilent Scar</xsl:when>
				<xsl:when test="zone = 2623">The Infectis Scar</xsl:when>
				<xsl:when test="zone = 2624">Blackwood Lake</xsl:when>
				<xsl:when test="zone = 2625">Eastwall Gate</xsl:when>

				<xsl:when test="zone = 2626">Terrorweb Tunnel</xsl:when>
				<xsl:when test="zone = 2627">Terrordale</xsl:when>
				<xsl:when test="zone = 2637">Kargathia Keep</xsl:when>
				<xsl:when test="zone = 2657">Valley of Bones</xsl:when>
				<xsl:when test="zone = 2677">Blackwing Lair</xsl:when>
				<xsl:when test="zone = 2697">Deadman's Crossing</xsl:when>

				<xsl:when test="zone = 2717">Molten Core</xsl:when>
				<xsl:when test="zone = 2737">The Scarab Wall</xsl:when>
				<xsl:when test="zone = 2738">Southwind Village</xsl:when>
				<xsl:when test="zone = 2739">Twilight Base Camp</xsl:when>
				<xsl:when test="zone = 2740">The Crystal Vale</xsl:when>
				<xsl:when test="zone = 2741">The Scarab Dais</xsl:when>

				<xsl:when test="zone = 2742">Hive'Ashi</xsl:when>
				<xsl:when test="zone = 2743">Hive'Zora</xsl:when>
				<xsl:when test="zone = 2744">Hive'Regal</xsl:when>
				<xsl:when test="zone = 2757">Shrine of the Fallen Warrior</xsl:when>
				<xsl:when test="zone = 2777">UNUSED Alterac Valley</xsl:when>
				<xsl:when test="zone = 2797">Blackfathom Deeps</xsl:when>

				<xsl:when test="zone = 2817">***On Map Dungeon***</xsl:when>
				<xsl:when test="zone = 2837">The Master's Cellar</xsl:when>
				<xsl:when test="zone = 2838">Stonewrought Pass</xsl:when>
				<xsl:when test="zone = 2839">Alterac Valley</xsl:when>
				<xsl:when test="zone = 2857">The Rumble Cage</xsl:when>
				<xsl:when test="zone = 2877">Chunk Test</xsl:when>

				<xsl:when test="zone = 2897">Zoram'gar Outpost</xsl:when>
				<xsl:when test="zone = 2917">Hall of Legends</xsl:when>
				<xsl:when test="zone = 2918">Champions' Hall</xsl:when>
				<xsl:when test="zone = 2937">Grosh'gok Compound</xsl:when>
				<xsl:when test="zone = 2938">Sleeping Gorge</xsl:when>
				<xsl:when test="zone = 2957">Irondeep Mine</xsl:when>

				<xsl:when test="zone = 2958">Stonehearth Outpost</xsl:when>
				<xsl:when test="zone = 2959">Dun Baldar</xsl:when>
				<xsl:when test="zone = 2960">Icewing Pass</xsl:when>
				<xsl:when test="zone = 2961">Frostwolf Village</xsl:when>
				<xsl:when test="zone = 2962">Tower Point</xsl:when>
				<xsl:when test="zone = 2963">Coldtooth Mine</xsl:when>

				<xsl:when test="zone = 2964">Winterax Hold</xsl:when>
				<xsl:when test="zone = 2977">Iceblood Garrison</xsl:when>
				<xsl:when test="zone = 2978">Frostwolf Keep</xsl:when>
				<xsl:when test="zone = 2979">Tor'kren Farm</xsl:when>
				<xsl:when test="zone = 3017">Frost Dagger Pass</xsl:when>
				<xsl:when test="zone = 3037">Ironstone Camp</xsl:when>

				<xsl:when test="zone = 3038">Weazel's Crater</xsl:when>
				<xsl:when test="zone = 3039">Tahonda Ruins</xsl:when>
				<xsl:when test="zone = 3057">Field of Strife</xsl:when>
				<xsl:when test="zone = 3058">Icewing Cavern</xsl:when>
				<xsl:when test="zone = 3077">Valor's Rest</xsl:when>
				<xsl:when test="zone = 3097">The Swarming Pillar</xsl:when>

				<xsl:when test="zone = 3098">Twilight Post</xsl:when>
				<xsl:when test="zone = 3099">Twilight Outpost</xsl:when>
				<xsl:when test="zone = 3100">Ravaged Twilight Camp</xsl:when>
				<xsl:when test="zone = 3117">Shalzaru's Lair</xsl:when>
				<xsl:when test="zone = 3137">Talrendis Point</xsl:when>
				<xsl:when test="zone = 3138">Rethress Sanctum</xsl:when>

				<xsl:when test="zone = 3139">Moon Horror Den</xsl:when>
				<xsl:when test="zone = 3140">Scalebeard's Cave</xsl:when>
				<xsl:when test="zone = 3157">Boulderslide Cavern</xsl:when>
				<xsl:when test="zone = 3177">Warsong Labor Camp</xsl:when>
				<xsl:when test="zone = 3197">Chillwind Camp</xsl:when>
				<xsl:when test="zone = 3217">The Maul</xsl:when>

				<xsl:when test="zone = 3237">The Maul UNUSED</xsl:when>
				<xsl:when test="zone = 3257">Bones of Grakkarond</xsl:when>
				<xsl:when test="zone = 3277">Warsong Gulch</xsl:when>
				<xsl:when test="zone = 3297">Frostwolf Graveyard</xsl:when>
				<xsl:when test="zone = 3298">Frostwolf Pass</xsl:when>
				<xsl:when test="zone = 3299">Dun Baldar Pass</xsl:when>

				<xsl:when test="zone = 3300">Iceblood Graveyard</xsl:when>
				<xsl:when test="zone = 3301">Snowfall Graveyard</xsl:when>
				<xsl:when test="zone = 3302">Stonehearth Graveyard</xsl:when>
				<xsl:when test="zone = 3303">Stormpike Graveyard</xsl:when>
				<xsl:when test="zone = 3304">Icewing Bunker</xsl:when>
				<xsl:when test="zone = 3305">Stonehearth Bunker</xsl:when>

				<xsl:when test="zone = 3306">Wildpaw Ridge</xsl:when>
				<xsl:when test="zone = 3317">Revantusk Village</xsl:when>
				<xsl:when test="zone = 3318">Rock of Durotan</xsl:when>
				<xsl:when test="zone = 3319">Silverwing Grove</xsl:when>
				<xsl:when test="zone = 3320">Warsong Lumber Mill</xsl:when>
				<xsl:when test="zone = 3321">Silverwing Hold</xsl:when>

				<xsl:when test="zone = 3337">Wildpaw Cavern</xsl:when>
				<xsl:when test="zone = 3338">The Veiled Cleft</xsl:when>
				<xsl:when test="zone = 3357">Yojamba Isle</xsl:when>
				<xsl:when test="zone = 3358">Arathi Basin</xsl:when>
				<xsl:when test="zone = 3377">The Coil</xsl:when>
				<xsl:when test="zone = 3378">Altar of Hir'eek</xsl:when>

				<xsl:when test="zone = 3379">Shadra'zaar</xsl:when>
				<xsl:when test="zone = 3380">Hakkari Grounds</xsl:when>
				<xsl:when test="zone = 3381">Naze of Shirvallah</xsl:when>
				<xsl:when test="zone = 3382">Temple of Bethekk</xsl:when>
				<xsl:when test="zone = 3383">The Bloodfire Pit</xsl:when>
				<xsl:when test="zone = 3384">Altar of the Blood God</xsl:when>

				<xsl:when test="zone = 3397">Zanza's Rise</xsl:when>
				<xsl:when test="zone = 3398">Edge of Madness</xsl:when>
				<xsl:when test="zone = 3417">Trollbane Hall</xsl:when>
				<xsl:when test="zone = 3418">Defiler's Den</xsl:when>
				<xsl:when test="zone = 3419">Pagle's Pointe</xsl:when>
				<xsl:when test="zone = 3420">Farm</xsl:when>

				<xsl:when test="zone = 3421">Blacksmith</xsl:when>
				<xsl:when test="zone = 3422">Lumber Mill</xsl:when>
				<xsl:when test="zone = 3423">Gold Mine</xsl:when>
				<xsl:when test="zone = 3424">Stables</xsl:when>
				<xsl:when test="zone = 3425">Cenarion Hold</xsl:when>
				<xsl:when test="zone = 3426">Staghelm Point</xsl:when>

				<xsl:when test="zone = 3427">Bronzebeard Encampment</xsl:when>
				<xsl:when test="zone = 3428">Ahn'Qiraj</xsl:when>
				<xsl:when test="zone = 3429">Ruins of Ahn'Qiraj</xsl:when>
				<xsl:when test="zone = 3430">Eversong Woods</xsl:when>
				<xsl:when test="zone = 3431">Sunstrider Isle</xsl:when>
				<xsl:when test="zone = 3432">Shrine of Dath'Remar</xsl:when>

				<xsl:when test="zone = 3433">Ghostlands</xsl:when>
				<xsl:when test="zone = 3434">Scarab Terrace</xsl:when>
				<xsl:when test="zone = 3435">General's Terrace</xsl:when>
				<xsl:when test="zone = 3436">The Reservoir</xsl:when>
				<xsl:when test="zone = 3437">The Hatchery</xsl:when>
				<xsl:when test="zone = 3438">The Comb</xsl:when>

				<xsl:when test="zone = 3439">Watchers' Terrace</xsl:when>
				<xsl:when test="zone = 3440">Scarab Terrace</xsl:when>
				<xsl:when test="zone = 3441">General's Terrace</xsl:when>
				<xsl:when test="zone = 3442">The Reservoir</xsl:when>
				<xsl:when test="zone = 3443">The Hatchery</xsl:when>
				<xsl:when test="zone = 3444">The Comb</xsl:when>

				<xsl:when test="zone = 3445">Watchers' Terrace</xsl:when>
				<xsl:when test="zone = 3446">Twilight's Run</xsl:when>
				<xsl:when test="zone = 3447">Ortell's Hideout</xsl:when>
				<xsl:when test="zone = 3448">Scarab Terrace</xsl:when>
				<xsl:when test="zone = 3449">General's Terrace</xsl:when>
				<xsl:when test="zone = 3450">The Reservoir</xsl:when>

				<xsl:when test="zone = 3451">The Hatchery</xsl:when>
				<xsl:when test="zone = 3452">The Comb</xsl:when>
				<xsl:when test="zone = 3453">Watchers' Terrace</xsl:when>
				<xsl:when test="zone = 3454">Ruins of Ahn'Qiraj</xsl:when>
				<xsl:when test="zone = 3455">The North Sea</xsl:when>
				<xsl:when test="zone = 3456">Naxxramas</xsl:when>

				<xsl:when test="zone = 3457">Karazhan</xsl:when>
				<xsl:when test="zone = 3459">City</xsl:when>
				<xsl:when test="zone = 3460">Golden Strand</xsl:when>
				<xsl:when test="zone = 3461">Sunsail Anchorage</xsl:when>
				<xsl:when test="zone = 3462">Fairbreeze Village</xsl:when>
				<xsl:when test="zone = 3463">Magisters Gate</xsl:when>

				<xsl:when test="zone = 3464">Farstrider Retreat</xsl:when>
				<xsl:when test="zone = 3465">North Sanctum</xsl:when>
				<xsl:when test="zone = 3466">West Sanctum</xsl:when>
				<xsl:when test="zone = 3467">East Sanctum</xsl:when>
				<xsl:when test="zone = 3468">Saltheril's Haven</xsl:when>
				<xsl:when test="zone = 3469">Thuron's Livery</xsl:when>

				<xsl:when test="zone = 3470">Stillwhisper Pond</xsl:when>
				<xsl:when test="zone = 3471">The Living Wood</xsl:when>
				<xsl:when test="zone = 3472">Azurebreeze Coast</xsl:when>
				<xsl:when test="zone = 3473">Lake Elrendar</xsl:when>
				<xsl:when test="zone = 3474">The Scorched Grove</xsl:when>
				<xsl:when test="zone = 3475">Zeb'Watha</xsl:when>

				<xsl:when test="zone = 3476">Tor'Watha</xsl:when>
				<xsl:when test="zone = 3477">Karazhan *UNUSED*</xsl:when>
				<xsl:when test="zone = 3478">Gates of Ahn'Qiraj</xsl:when>
				<xsl:when test="zone = 3479">The Veiled Sea</xsl:when>
				<xsl:when test="zone = 3480">Duskwither Grounds</xsl:when>
				<xsl:when test="zone = 3481">Duskwither Spire</xsl:when>

				<xsl:when test="zone = 3482">The Dead Scar</xsl:when>
				<xsl:when test="zone = 3483">Hellfire Peninsula</xsl:when>
				<xsl:when test="zone = 3484">The Sunspire</xsl:when>
				<xsl:when test="zone = 3485">Falthrien Academy</xsl:when>
				<xsl:when test="zone = 3486">Ravenholdt Manor</xsl:when>
				<xsl:when test="zone = 3487">Silvermoon City</xsl:when>

				<xsl:when test="zone = 3488">Tranquillien</xsl:when>
				<xsl:when test="zone = 3489">Suncrown Village</xsl:when>
				<xsl:when test="zone = 3490">Goldenmist Village</xsl:when>
				<xsl:when test="zone = 3491">Windrunner Village</xsl:when>
				<xsl:when test="zone = 3492">Windrunner Spire</xsl:when>
				<xsl:when test="zone = 3493">Sanctum of the Sun</xsl:when>

				<xsl:when test="zone = 3494">Sanctum of the Moon</xsl:when>
				<xsl:when test="zone = 3495">Dawnstar Spire</xsl:when>
				<xsl:when test="zone = 3496">Farstrider Enclave</xsl:when>
				<xsl:when test="zone = 3497">An'daroth</xsl:when>
				<xsl:when test="zone = 3498">An'telas</xsl:when>
				<xsl:when test="zone = 3499">An'owyn</xsl:when>

				<xsl:when test="zone = 3500">Deatholme</xsl:when>
				<xsl:when test="zone = 3501">Bleeding Ziggurat</xsl:when>
				<xsl:when test="zone = 3502">Howling Ziggurat</xsl:when>
				<xsl:when test="zone = 3503">Shalandis Isle</xsl:when>
				<xsl:when test="zone = 3504">Toryl Estate</xsl:when>
				<xsl:when test="zone = 3505">Underlight Mines</xsl:when>

				<xsl:when test="zone = 3506">Andilien Estate</xsl:when>
				<xsl:when test="zone = 3507">Hatchet Hills</xsl:when>
				<xsl:when test="zone = 3508">Amani Pass</xsl:when>
				<xsl:when test="zone = 3509">Sungraze Peak</xsl:when>
				<xsl:when test="zone = 3510">Amani Catacombs</xsl:when>
				<xsl:when test="zone = 3511">Tower of the Damned</xsl:when>

				<xsl:when test="zone = 3512">Zeb'Sora</xsl:when>
				<xsl:when test="zone = 3513">Lake Elrendar</xsl:when>
				<xsl:when test="zone = 3514">The Dead Scar</xsl:when>
				<xsl:when test="zone = 3515">Elrendar River</xsl:when>
				<xsl:when test="zone = 3516">Zeb'Tela</xsl:when>
				<xsl:when test="zone = 3517">Zeb'Nowa</xsl:when>

				<xsl:when test="zone = 3518">Nagrand</xsl:when>
				<xsl:when test="zone = 3519">Terokkar Forest</xsl:when>
				<xsl:when test="zone = 3520">Shadowmoon Valley</xsl:when>
				<xsl:when test="zone = 3521">Zangarmarsh</xsl:when>
				<xsl:when test="zone = 3522">Blade's Edge Mountains</xsl:when>
				<xsl:when test="zone = 3523">Netherstorm</xsl:when>

				<xsl:when test="zone = 3524">Azuremyst Isle</xsl:when>
				<xsl:when test="zone = 3525">Bloodmyst Isle</xsl:when>
				<xsl:when test="zone = 3526">Ammen Vale</xsl:when>
				<xsl:when test="zone = 3527">Crash Site</xsl:when>
				<xsl:when test="zone = 3528">Silverline Lake</xsl:when>
				<xsl:when test="zone = 3529">Nestlewood Thicket</xsl:when>

				<xsl:when test="zone = 3530">Shadow Ridge</xsl:when>
				<xsl:when test="zone = 3531">Skulking Row</xsl:when>
				<xsl:when test="zone = 3532">Dawning Lane</xsl:when>
				<xsl:when test="zone = 3533">Ruins of Silvermoon</xsl:when>
				<xsl:when test="zone = 3534">Feth's Way</xsl:when>
				<xsl:when test="zone = 3535">Hellfire Citadel</xsl:when>

				<xsl:when test="zone = 3536">Thrallmar</xsl:when>
				<xsl:when test="zone = 3537">REUSE</xsl:when>
				<xsl:when test="zone = 3538">Honor Hold</xsl:when>
				<xsl:when test="zone = 3539">The Stair of Destiny</xsl:when>
				<xsl:when test="zone = 3540">Twisting Nether</xsl:when>
				<xsl:when test="zone = 3541">Forge Camp: Mageddon</xsl:when>

				<xsl:when test="zone = 3542">The Path of Glory</xsl:when>
				<xsl:when test="zone = 3543">The Great Fissure</xsl:when>
				<xsl:when test="zone = 3544">Plain of Shards</xsl:when>
				<xsl:when test="zone = 3545">Hellfire Citadel</xsl:when>
				<xsl:when test="zone = 3546">Expedition Armory</xsl:when>
				<xsl:when test="zone = 3547">Throne of Kil'jaeden</xsl:when>

				<xsl:when test="zone = 3548">Forge Camp: Rage</xsl:when>
				<xsl:when test="zone = 3549">Invasion Point: Annihilator</xsl:when>
				<xsl:when test="zone = 3550">Borune Ruins</xsl:when>
				<xsl:when test="zone = 3551">Ruins of Sha'naar</xsl:when>
				<xsl:when test="zone = 3552">Temple of Telhamat</xsl:when>
				<xsl:when test="zone = 3553">Pools of Aggonar</xsl:when>

				<xsl:when test="zone = 3554">Falcon Watch</xsl:when>
				<xsl:when test="zone = 3555">Mag'har Post</xsl:when>
				<xsl:when test="zone = 3556">Den of Haal'esh</xsl:when>
				<xsl:when test="zone = 3557">The Exodar</xsl:when>
				<xsl:when test="zone = 3558">Elrendar Falls</xsl:when>
				<xsl:when test="zone = 3559">Nestlewood Hills</xsl:when>

				<xsl:when test="zone = 3560">Ammen Fields</xsl:when>
				<xsl:when test="zone = 3561">The Sacred Grove</xsl:when>
				<xsl:when test="zone = 3562">Hellfire Ramparts</xsl:when>
				<xsl:when test="zone = 3563">Hellfire Citadel</xsl:when>
				<xsl:when test="zone = 3564">Emberglade</xsl:when>
				<xsl:when test="zone = 3565">Cenarion Refuge</xsl:when>

				<xsl:when test="zone = 3566">Moonwing Den</xsl:when>
				<xsl:when test="zone = 3567">Pod Cluster</xsl:when>
				<xsl:when test="zone = 3568">Pod Wreckage</xsl:when>
				<xsl:when test="zone = 3569">Tides' Hollow</xsl:when>
				<xsl:when test="zone = 3570">Wrathscale Point</xsl:when>
				<xsl:when test="zone = 3571">Bristlelimb Village</xsl:when>

				<xsl:when test="zone = 3572">Stillpine Hold</xsl:when>
				<xsl:when test="zone = 3573">Odesyus' Landing</xsl:when>
				<xsl:when test="zone = 3574">Valaar's Berth</xsl:when>
				<xsl:when test="zone = 3575">Silting Shore</xsl:when>
				<xsl:when test="zone = 3576">Azure Watch</xsl:when>
				<xsl:when test="zone = 3577">Geezle's Camp</xsl:when>

				<xsl:when test="zone = 3578">Menagerie Wreckage</xsl:when>
				<xsl:when test="zone = 3579">Traitor's Cove</xsl:when>
				<xsl:when test="zone = 3580">Wildwind Peak</xsl:when>
				<xsl:when test="zone = 3581">Wildwind Path</xsl:when>
				<xsl:when test="zone = 3582">Zeth'Gor</xsl:when>
				<xsl:when test="zone = 3583">Beryl Coast</xsl:when>

				<xsl:when test="zone = 3584">Blood Watch</xsl:when>
				<xsl:when test="zone = 3585">Bladewood</xsl:when>
				<xsl:when test="zone = 3586">The Vector Coil</xsl:when>
				<xsl:when test="zone = 3587">The Warp Piston</xsl:when>
				<xsl:when test="zone = 3588">The Cryo-Core</xsl:when>
				<xsl:when test="zone = 3589">The Crimson Reach</xsl:when>

				<xsl:when test="zone = 3590">Wrathscale Lair</xsl:when>
				<xsl:when test="zone = 3591">Ruins of Loreth'Aran</xsl:when>
				<xsl:when test="zone = 3592">Nazzivian</xsl:when>
				<xsl:when test="zone = 3593">Axxarien</xsl:when>
				<xsl:when test="zone = 3594">Blacksilt Shore</xsl:when>
				<xsl:when test="zone = 3595">The Foul Pool</xsl:when>

				<xsl:when test="zone = 3596">The Hidden Reef</xsl:when>
				<xsl:when test="zone = 3597">Amberweb Pass</xsl:when>
				<xsl:when test="zone = 3598">Wyrmscar Island</xsl:when>
				<xsl:when test="zone = 3599">Talon Stand</xsl:when>
				<xsl:when test="zone = 3600">Bristlelimb Enclave</xsl:when>
				<xsl:when test="zone = 3601">Ragefeather Ridge</xsl:when>

				<xsl:when test="zone = 3602">Kessel's Crossing</xsl:when>
				<xsl:when test="zone = 3603">Tel'athion's Camp</xsl:when>
				<xsl:when test="zone = 3604">The Bloodcursed Reef</xsl:when>
				<xsl:when test="zone = 3605">Hyjal Past</xsl:when>
				<xsl:when test="zone = 3606">Hyjal Summit</xsl:when>
				<xsl:when test="zone = 3607">Coilfang Reservoir</xsl:when>

				<xsl:when test="zone = 3608">Vindicator's Rest</xsl:when>
				<xsl:when test="zone = 3609">Unused3</xsl:when>
				<xsl:when test="zone = 3610">Burning Blade Ruins</xsl:when>
				<xsl:when test="zone = 3611">Clan Watch</xsl:when>
				<xsl:when test="zone = 3612">Bloodcurse Isle</xsl:when>
				<xsl:when test="zone = 3613">Garadar</xsl:when>

				<xsl:when test="zone = 3614">Skysong Lake</xsl:when>
				<xsl:when test="zone = 3615">Throne of the Elements</xsl:when>
				<xsl:when test="zone = 3616">Laughing Skull Ruins</xsl:when>
				<xsl:when test="zone = 3617">Warmaul Hill</xsl:when>
				<xsl:when test="zone = 3618">Gruul's Lair</xsl:when>
				<xsl:when test="zone = 3619">Auren Ridge</xsl:when>

				<xsl:when test="zone = 3620">Auren Falls</xsl:when>
				<xsl:when test="zone = 3621">Lake Sunspring</xsl:when>
				<xsl:when test="zone = 3622">Sunspring Post</xsl:when>
				<xsl:when test="zone = 3623">Aeris Landing</xsl:when>
				<xsl:when test="zone = 3624">Forge Camp: Fear</xsl:when>
				<xsl:when test="zone = 3625">Forge Camp: Hate</xsl:when>

				<xsl:when test="zone = 3626">Telaar</xsl:when>
				<xsl:when test="zone = 3627">Northwind Cleft</xsl:when>
				<xsl:when test="zone = 3628">Halaa</xsl:when>
				<xsl:when test="zone = 3629">Southwind Cleft</xsl:when>
				<xsl:when test="zone = 3630">Oshu'gun</xsl:when>
				<xsl:when test="zone = 3631">Spirit Fields</xsl:when>

				<xsl:when test="zone = 3632">Shamanar</xsl:when>
				<xsl:when test="zone = 3633">Ancestral Grounds</xsl:when>
				<xsl:when test="zone = 3634">Windyreed Village</xsl:when>
				<xsl:when test="zone = 3635">Unused2</xsl:when>
				<xsl:when test="zone = 3636">Elemental Plateau</xsl:when>
				<xsl:when test="zone = 3637">Kil'sorrow Fortress</xsl:when>

				<xsl:when test="zone = 3638">The Ring of Trials</xsl:when>
				<xsl:when test="zone = 3639">Silvermyst Isle</xsl:when>
				<xsl:when test="zone = 3640">Daggerfen Village</xsl:when>
				<xsl:when test="zone = 3641">Umbrafen Village</xsl:when>
				<xsl:when test="zone = 3642">Feralfen Village</xsl:when>
				<xsl:when test="zone = 3643">Bloodscale Enclave</xsl:when>

				<xsl:when test="zone = 3644">Telredor</xsl:when>
				<xsl:when test="zone = 3645">Zabra'jin</xsl:when>
				<xsl:when test="zone = 3646">Quagg Ridge</xsl:when>
				<xsl:when test="zone = 3647">The Spawning Glen</xsl:when>
				<xsl:when test="zone = 3648">The Dead Mire</xsl:when>
				<xsl:when test="zone = 3649">Sporeggar</xsl:when>

				<xsl:when test="zone = 3650">Ango'rosh Grounds</xsl:when>
				<xsl:when test="zone = 3651">Ango'rosh Stronghold</xsl:when>
				<xsl:when test="zone = 3652">Funggor Cavern</xsl:when>
				<xsl:when test="zone = 3653">Serpent Lake</xsl:when>
				<xsl:when test="zone = 3654">The Drain</xsl:when>
				<xsl:when test="zone = 3655">Umbrafen Lake</xsl:when>

				<xsl:when test="zone = 3656">Marshlight Lake</xsl:when>
				<xsl:when test="zone = 3657">Portal Clearing</xsl:when>
				<xsl:when test="zone = 3658">Sporewind Lake</xsl:when>
				<xsl:when test="zone = 3659">The Lagoon</xsl:when>
				<xsl:when test="zone = 3660">Blades' Run</xsl:when>
				<xsl:when test="zone = 3661">Blade Tooth Canyon</xsl:when>

				<xsl:when test="zone = 3662">Commons Hall</xsl:when>
				<xsl:when test="zone = 3663">Derelict Manor</xsl:when>
				<xsl:when test="zone = 3664">Huntress of the Sun</xsl:when>
				<xsl:when test="zone = 3665">Falconwing Square</xsl:when>
				<xsl:when test="zone = 3666">Halaani Basin</xsl:when>
				<xsl:when test="zone = 3667">Hewn Bog</xsl:when>

				<xsl:when test="zone = 3668">Boha'mu Ruins</xsl:when>
				<xsl:when test="zone = 3669">The Stadium</xsl:when>
				<xsl:when test="zone = 3670">The Overlook</xsl:when>
				<xsl:when test="zone = 3671">Broken Hill</xsl:when>
				<xsl:when test="zone = 3672">Mag'hari Procession</xsl:when>
				<xsl:when test="zone = 3673">Nesingwary Safari</xsl:when>

				<xsl:when test="zone = 3674">Cenarion Thicket</xsl:when>
				<xsl:when test="zone = 3675">Tuurem</xsl:when>
				<xsl:when test="zone = 3676">Veil Shienor</xsl:when>
				<xsl:when test="zone = 3677">Veil Skith</xsl:when>
				<xsl:when test="zone = 3678">Veil Shalas</xsl:when>
				<xsl:when test="zone = 3679">Skettis</xsl:when>

				<xsl:when test="zone = 3680">Blackwind Valley</xsl:when>
				<xsl:when test="zone = 3681">Firewing Point</xsl:when>
				<xsl:when test="zone = 3682">Grangol'var Village</xsl:when>
				<xsl:when test="zone = 3683">Stonebreaker Hold</xsl:when>
				<xsl:when test="zone = 3684">Allerian Stronghold</xsl:when>
				<xsl:when test="zone = 3685">Bonechewer Ruins</xsl:when>

				<xsl:when test="zone = 3686">Veil Lithic</xsl:when>
				<xsl:when test="zone = 3687">Olembas</xsl:when>
				<xsl:when test="zone = 3688">Auchindoun</xsl:when>
				<xsl:when test="zone = 3689">Veil Reskk</xsl:when>
				<xsl:when test="zone = 3690">Blackwind Lake</xsl:when>
				<xsl:when test="zone = 3691">Lake Ere'Noru</xsl:when>

				<xsl:when test="zone = 3692">Lake Jorune</xsl:when>
				<xsl:when test="zone = 3693">Skethyl Mountains</xsl:when>
				<xsl:when test="zone = 3694">Misty Ridge</xsl:when>
				<xsl:when test="zone = 3695">The Broken Hills</xsl:when>
				<xsl:when test="zone = 3696">The Barrier Hills</xsl:when>
				<xsl:when test="zone = 3697">The Bone Wastes</xsl:when>

				<xsl:when test="zone = 3698">Nagrand Arena</xsl:when>
				<xsl:when test="zone = 3699">Laughing Skull Courtyard</xsl:when>
				<xsl:when test="zone = 3700">The Ring of Blood</xsl:when>
				<xsl:when test="zone = 3701">Arena Floor</xsl:when>
				<xsl:when test="zone = 3702">Blade's Edge Arena</xsl:when>
				<xsl:when test="zone = 3703">Shattrath City</xsl:when>

				<xsl:when test="zone = 3704">The Shepherd's Gate</xsl:when>
				<xsl:when test="zone = 3705">Telaari Basin</xsl:when>
				<xsl:when test="zone = 3706">The Dark Portal</xsl:when>
				<xsl:when test="zone = 3707">Alliance Base</xsl:when>
				<xsl:when test="zone = 3708">Horde Encampment</xsl:when>
				<xsl:when test="zone = 3709">Night Elf Village</xsl:when>

				<xsl:when test="zone = 3710">Nordrassil</xsl:when>
				<xsl:when test="zone = 3711">Black Temple</xsl:when>
				<xsl:when test="zone = 3712">Area 52</xsl:when>
				<xsl:when test="zone = 3713">The Blood Furnace</xsl:when>
				<xsl:when test="zone = 3714">The Shattered Halls</xsl:when>
				<xsl:when test="zone = 3715">The Steamvault</xsl:when>

				<xsl:when test="zone = 3716">The Underbog</xsl:when>
				<xsl:when test="zone = 3717">The Slave Pens</xsl:when>
				<xsl:when test="zone = 3718">Swamprat Post</xsl:when>
				<xsl:when test="zone = 3719">Bleeding Hollow Ruins</xsl:when>
				<xsl:when test="zone = 3720">Twin Spire Ruins</xsl:when>
				<xsl:when test="zone = 3721">The Crumbling Waste</xsl:when>

				<xsl:when test="zone = 3722">Manaforge Ara</xsl:when>
				<xsl:when test="zone = 3723">Arklon Ruins</xsl:when>
				<xsl:when test="zone = 3724">Cosmowrench</xsl:when>
				<xsl:when test="zone = 3725">Ruins of Enkaat</xsl:when>
				<xsl:when test="zone = 3726">Manaforge B'naar</xsl:when>
				<xsl:when test="zone = 3727">The Scrap Field</xsl:when>

				<xsl:when test="zone = 3728">The Vortex Fields</xsl:when>
				<xsl:when test="zone = 3729">The Heap</xsl:when>
				<xsl:when test="zone = 3730">Manaforge Coruu</xsl:when>
				<xsl:when test="zone = 3731">The Tempest Rift</xsl:when>
				<xsl:when test="zone = 3732">Kirin'Var Village</xsl:when>
				<xsl:when test="zone = 3733">The Violet Tower</xsl:when>

				<xsl:when test="zone = 3734">Manaforge Duro</xsl:when>
				<xsl:when test="zone = 3735">Voidwind Plateau</xsl:when>
				<xsl:when test="zone = 3736">Manaforge Ultris</xsl:when>
				<xsl:when test="zone = 3737">Celestial Ridge</xsl:when>
				<xsl:when test="zone = 3738">The Stormspire</xsl:when>
				<xsl:when test="zone = 3739">Forge Base: Oblivion</xsl:when>

				<xsl:when test="zone = 3740">Forge Base: Gehenna</xsl:when>
				<xsl:when test="zone = 3741">Ruins of Farahlon</xsl:when>
				<xsl:when test="zone = 3742">Socrethar's Seat</xsl:when>
				<xsl:when test="zone = 3743">Legion Hold</xsl:when>
				<xsl:when test="zone = 3744">Shadowmoon Village</xsl:when>
				<xsl:when test="zone = 3745">Wildhammer Stronghold</xsl:when>

				<xsl:when test="zone = 3746">The Hand of Gul'dan</xsl:when>
				<xsl:when test="zone = 3747">The Fel Pits</xsl:when>
				<xsl:when test="zone = 3748">The Deathforge</xsl:when>
				<xsl:when test="zone = 3749">Coilskar Cistern</xsl:when>
				<xsl:when test="zone = 3750">Coilskar Point</xsl:when>
				<xsl:when test="zone = 3751">Sunfire Point</xsl:when>

				<xsl:when test="zone = 3752">Illidari Point</xsl:when>
				<xsl:when test="zone = 3753">Ruins of Baa'ri</xsl:when>
				<xsl:when test="zone = 3754">Altar of Sha'tar</xsl:when>
				<xsl:when test="zone = 3755">The Stair of Doom</xsl:when>
				<xsl:when test="zone = 3756">Ruins of Karabor</xsl:when>
				<xsl:when test="zone = 3757">Ata'mal Terrace</xsl:when>

				<xsl:when test="zone = 3758">Netherwing Fields</xsl:when>
				<xsl:when test="zone = 3759">Netherwing Ledge</xsl:when>
				<xsl:when test="zone = 3760">The Barrier Hills</xsl:when>
				<xsl:when test="zone = 3761">The High Path</xsl:when>
				<xsl:when test="zone = 3762">Windyreed Pass</xsl:when>
				<xsl:when test="zone = 3763">Zangar Ridge</xsl:when>

				<xsl:when test="zone = 3764">The Twilight Ridge</xsl:when>
				<xsl:when test="zone = 3765">Razorthorn Trail</xsl:when>
				<xsl:when test="zone = 3766">Orebor Harborage</xsl:when>
				<xsl:when test="zone = 3767">Blades' Run</xsl:when>
				<xsl:when test="zone = 3768">Jagged Ridge</xsl:when>
				<xsl:when test="zone = 3769">Thunderlord Stronghold</xsl:when>

				<xsl:when test="zone = 3770">Blade Tooth Canyon</xsl:when>
				<xsl:when test="zone = 3771">The Living Grove</xsl:when>
				<xsl:when test="zone = 3772">Sylvanaar</xsl:when>
				<xsl:when test="zone = 3773">Bladespire Hold</xsl:when>
				<xsl:when test="zone = 3774">Gruul's Lair</xsl:when>
				<xsl:when test="zone = 3775">Circle of Blood</xsl:when>

				<xsl:when test="zone = 3776">Bloodmaul Outpost</xsl:when>
				<xsl:when test="zone = 3777">Bloodmaul Camp</xsl:when>
				<xsl:when test="zone = 3778">Draenethyst Mine</xsl:when>
				<xsl:when test="zone = 3779">Trogma's Claim</xsl:when>
				<xsl:when test="zone = 3780">Blackwing Coven</xsl:when>
				<xsl:when test="zone = 3781">Grishnath</xsl:when>

				<xsl:when test="zone = 3782">Veil Lashh</xsl:when>
				<xsl:when test="zone = 3783">Veil Vekh</xsl:when>
				<xsl:when test="zone = 3784">Forge Camp: Terror</xsl:when>
				<xsl:when test="zone = 3785">Forge Camp: Wrath</xsl:when>
				<xsl:when test="zone = 3786">Felstorm Point</xsl:when>
				<xsl:when test="zone = 3787">Forge Camp: Anger</xsl:when>

				<xsl:when test="zone = 3788">The Low Path</xsl:when>
				<xsl:when test="zone = 3789">Shadow Labyrinth</xsl:when>
				<xsl:when test="zone = 3790">Auchenai Crypts</xsl:when>
				<xsl:when test="zone = 3791">Sethekk Halls</xsl:when>
				<xsl:when test="zone = 3792">Mana-Tombs</xsl:when>
				<xsl:when test="zone = 3793">Felspark Ravine</xsl:when>

				<xsl:when test="zone = 3794">Valley of Bones</xsl:when>
				<xsl:when test="zone = 3795">Sha'naari Wastes</xsl:when>
				<xsl:when test="zone = 3796">The Warp Fields</xsl:when>
				<xsl:when test="zone = 3797">Fallen Sky Ridge</xsl:when>
				<xsl:when test="zone = 3798">Haal'eshi Gorge</xsl:when>
				<xsl:when test="zone = 3799">Stonewall Canyon</xsl:when>

				<xsl:when test="zone = 3800">Thornfang Hill</xsl:when>
				<xsl:when test="zone = 3801">Mag'har Grounds</xsl:when>
				<xsl:when test="zone = 3802">Void Ridge</xsl:when>
				<xsl:when test="zone = 3803">The Abyssal Shelf</xsl:when>
				<xsl:when test="zone = 3804">The Legion Front</xsl:when>
				<xsl:when test="zone = 3805">Zul'Aman</xsl:when>

				<xsl:when test="zone = 3806">Supply Caravan</xsl:when>
				<xsl:when test="zone = 3807">Reaver's Fall</xsl:when>
				<xsl:when test="zone = 3808">Cenarion Post</xsl:when>
				<xsl:when test="zone = 3809">Southern Rampart</xsl:when>
				<xsl:when test="zone = 3810">Northern Rampart</xsl:when>
				<xsl:when test="zone = 3811">Gor'gaz Outpost</xsl:when>

				<xsl:when test="zone = 3812">Spinebreaker Post</xsl:when>
				<xsl:when test="zone = 3813">The Path of Anguish</xsl:when>
				<xsl:when test="zone = 3814">East Supply Caravan</xsl:when>
				<xsl:when test="zone = 3815">Expedition Point</xsl:when>
				<xsl:when test="zone = 3816">Zeppelin Crash</xsl:when>
				<xsl:when test="zone = 3817">Testing</xsl:when>

				<xsl:when test="zone = 3818">Bloodscale Grounds</xsl:when>
				<xsl:when test="zone = 3819">Darkcrest Enclave</xsl:when>
				<xsl:when test="zone = 3820">Eye of the Storm</xsl:when>
				<xsl:when test="zone = 3821">Warden's Cage</xsl:when>
				<xsl:when test="zone = 3822">Eclipse Point</xsl:when>
				<xsl:when test="zone = 3823">Isle of Tribulations</xsl:when>

				<xsl:when test="zone = 3824">Bloodmaul Ravine</xsl:when>
				<xsl:when test="zone = 3825">Dragons' End</xsl:when>
				<xsl:when test="zone = 3826">Daggermaw Canyon</xsl:when>
				<xsl:when test="zone = 3827">Vekhaar Stand</xsl:when>
				<xsl:when test="zone = 3828">Ruuan Weald</xsl:when>
				<xsl:when test="zone = 3829">Veil Ruuan</xsl:when>

				<xsl:when test="zone = 3830">Raven's Wood</xsl:when>
				<xsl:when test="zone = 3831">Death's Door</xsl:when>
				<xsl:when test="zone = 3832">Vortex Pinnacle</xsl:when>
				<xsl:when test="zone = 3833">Razor Ridge</xsl:when>
				<xsl:when test="zone = 3834">Ridge of Madness</xsl:when>
				<xsl:when test="zone = 3835">Dustquill Ravine</xsl:when>

				<xsl:when test="zone = 3836">Magtheridon's Lair</xsl:when>
				<xsl:when test="zone = 3837">Sunfury Hold</xsl:when>
				<xsl:when test="zone = 3838">Spinebreaker Mountains</xsl:when>
				<xsl:when test="zone = 3839">Abandoned Armory</xsl:when>
				<xsl:when test="zone = 3840">The Black Temple</xsl:when>
				<xsl:when test="zone = 3841">Darkcrest Shore</xsl:when>

				<xsl:when test="zone = 3842">Tempest Keep</xsl:when>
				<xsl:when test="zone = 3844">Mok'Nathal Village</xsl:when>
				<xsl:when test="zone = 3845">Tempest Keep</xsl:when>
				<xsl:when test="zone = 3846">The Arcatraz</xsl:when>
				<xsl:when test="zone = 3847">The Botanica</xsl:when>
				<xsl:when test="zone = 3848">The Arcatraz</xsl:when>

				<xsl:when test="zone = 3849">The Mechanar</xsl:when>
				<xsl:when test="zone = 3850">Netherstone</xsl:when>
				<xsl:when test="zone = 3851">Midrealm Post</xsl:when>
				<xsl:when test="zone = 3852">Tuluman's Landing</xsl:when>
				<xsl:when test="zone = 3854">Protectorate Watch Post</xsl:when>
				<xsl:when test="zone = 3855">Circle of Blood Arena</xsl:when>

				<xsl:when test="zone = 3856">Elrendar Crossing</xsl:when>
				<xsl:when test="zone = 3857">Ammen Ford</xsl:when>
				<xsl:when test="zone = 3858">Razorthorn Shelf</xsl:when>
				<xsl:when test="zone = 3859">Silmyr Lake</xsl:when>
				<xsl:when test="zone = 3860">Raastok Glade</xsl:when>
				<xsl:when test="zone = 3861">Thalassian Pass</xsl:when>

				<xsl:when test="zone = 3862">Churning Gulch</xsl:when>
				<xsl:when test="zone = 3863">Broken Wilds</xsl:when>
				<xsl:when test="zone = 3864">Bash'ir Landing</xsl:when>
				<xsl:when test="zone = 3865">Crystal Spine</xsl:when>
				<xsl:when test="zone = 3866">Skald</xsl:when>
				<xsl:when test="zone = 3867">Bladed Gulch</xsl:when>

				<xsl:when test="zone = 3868">Gyro-Plank Bridge</xsl:when>
				<xsl:when test="zone = 3869">Mage Tower</xsl:when>
				<xsl:when test="zone = 3870">Blood Elf Tower</xsl:when>
				<xsl:when test="zone = 3871">Draenei Ruins</xsl:when>
				<xsl:when test="zone = 3872">Fel Reaver Ruins</xsl:when>
				<xsl:when test="zone = 3873">The Proving Grounds</xsl:when>

				<xsl:when test="zone = 3874">Eco-Dome Farfield</xsl:when>
				<xsl:when test="zone = 3875">Eco-Dome Skyperch</xsl:when>
				<xsl:when test="zone = 3876">Eco-Dome Sutheron</xsl:when>
				<xsl:when test="zone = 3877">Eco-Dome Midrealm</xsl:when>
				<xsl:when test="zone = 3878">Ethereum Staging Grounds</xsl:when>
				<xsl:when test="zone = 3879">Chapel Yard</xsl:when>

				<xsl:when test="zone = 3880">Access Shaft Zeon</xsl:when>
				<xsl:when test="zone = 3881">Trelleum Mine</xsl:when>
				<xsl:when test="zone = 3882">Invasion Point: Destroyer</xsl:when>
				<xsl:when test="zone = 3883">Camp of Boom</xsl:when>
				<xsl:when test="zone = 3884">Spinebreaker Pass</xsl:when>
				<xsl:when test="zone = 3885">Netherweb Ridge</xsl:when>

				<xsl:when test="zone = 3886">Derelict Caravan</xsl:when>
				<xsl:when test="zone = 3887">Refugee Caravan</xsl:when>
				<xsl:when test="zone = 3888">Shadow Tomb</xsl:when>
				<xsl:when test="zone = 3889">Veil Rhaze</xsl:when>
				<xsl:when test="zone = 3890">Tomb of Lights</xsl:when>
				<xsl:when test="zone = 3891">Carrion Hill</xsl:when>

				<xsl:when test="zone = 3892">Writhing Mound</xsl:when>
				<xsl:when test="zone = 3893">Ring of Observance</xsl:when>
				<xsl:when test="zone = 3894">Auchenai Grounds</xsl:when>
				<xsl:when test="zone = 3895">Cenarion Watchpost</xsl:when>
				<xsl:when test="zone = 3896">Aldor Rise</xsl:when>
				<xsl:when test="zone = 3897">Terrace of Light</xsl:when>

				<xsl:when test="zone = 3898">Scryer's Tier</xsl:when>
				<xsl:when test="zone = 3899">Lower City</xsl:when>
				<xsl:when test="zone = 3900">Invasion Point: Overlord</xsl:when>
				<xsl:when test="zone = 3901">Allerian Post</xsl:when>
				<xsl:when test="zone = 3902">Stonebreaker Camp</xsl:when>
				<xsl:when test="zone = 3903">Boulder'mok</xsl:when>

				<xsl:when test="zone = 3904">Cursed Hollow</xsl:when>
				<xsl:when test="zone = 3905">Coilfang Reservoir</xsl:when>
				<xsl:when test="zone = 3906">The Bloodwash</xsl:when>
				<xsl:when test="zone = 3907">Veridian Point</xsl:when>
				<xsl:when test="zone = 3908">Middenvale</xsl:when>
				<xsl:when test="zone = 3909">The Lost Fold</xsl:when>

				<xsl:when test="zone = 3910">Mystwood</xsl:when>
				<xsl:when test="zone = 3911">Tranquil Shore</xsl:when>
				<xsl:when test="zone = 3912">Goldenbough Pass</xsl:when>
				<xsl:when test="zone = 3913">Runestone Falithas</xsl:when>
				<xsl:when test="zone = 3914">Runestone Shan'dor</xsl:when>
				<xsl:when test="zone = 3915">Fairbridge Strand</xsl:when>

				<xsl:when test="zone = 3916">Moongraze Woods</xsl:when>
				<xsl:when test="zone = 3917">Auchindoun</xsl:when>
				<xsl:when test="zone = 3918">Toshley's Station</xsl:when>
				<xsl:when test="zone = 3919">Singing Ridge</xsl:when>
				<xsl:when test="zone = 3920">Shatter Point</xsl:when>
				<xsl:when test="zone = 3921">Arklonis Ridge</xsl:when>

				<xsl:when test="zone = 3922">Bladespire Outpost</xsl:when>
				<xsl:when test="zone = 3923">Gruul's Lair</xsl:when>
				<xsl:when test="zone = 3924">Northmaul Tower</xsl:when>
				<xsl:when test="zone = 3925">Southmaul Tower</xsl:when>
				<xsl:when test="zone = 3926">Shattered Plains</xsl:when>
				<xsl:when test="zone = 3927">Oronok's Farm</xsl:when>

				<xsl:when test="zone = 3928">The Altar of Damnation</xsl:when>
				<xsl:when test="zone = 3929">The Path of Conquest</xsl:when>
				<xsl:when test="zone = 3930">Eclipsion Fields</xsl:when>
				<xsl:when test="zone = 3931">Bladespire Grounds</xsl:when>
				<xsl:when test="zone = 3932">Sketh'lon Base Camp</xsl:when>
				<xsl:when test="zone = 3933">Sketh'lon Wreckage</xsl:when>

				<xsl:when test="zone = 3934">Town Square</xsl:when>
				<xsl:when test="zone = 3935">Wizard Row</xsl:when>
				<xsl:when test="zone = 3936">Deathforge Tower</xsl:when>
				<xsl:when test="zone = 3937">Slag Watch</xsl:when>
				<xsl:when test="zone = 3938">Sanctum of the Stars</xsl:when>
				<xsl:when test="zone = 3939">Dragonmaw Fortress</xsl:when>

				<xsl:when test="zone = 3940">The Fetid Pool</xsl:when>
				<xsl:when test="zone = 3941">Test</xsl:when>
				<xsl:when test="zone = 3942">Razaan's Landing</xsl:when>
				<xsl:when test="zone = 3943">Invasion Point: Cataclysm</xsl:when>
				<xsl:when test="zone = 3944">The Altar of Shadows</xsl:when>
				<xsl:when test="zone = 3945">Netherwing Pass</xsl:when>

				<xsl:when test="zone = 3946">Wayne's Refuge</xsl:when>
				<xsl:when test="zone = 3947">The Scalding Pools</xsl:when>
				<xsl:when test="zone = 3948">Brian and Pat Test</xsl:when>
				<xsl:when test="zone = 3949">Magma Fields</xsl:when>
				<xsl:when test="zone = 3950">Crimson Watch</xsl:when>
				<xsl:when test="zone = 3951">Evergrove</xsl:when>

				<xsl:when test="zone = 3952">Wyrmskull Bridge</xsl:when>
				<xsl:when test="zone = 3953">Scalewing Shelf</xsl:when>
				<xsl:when test="zone = 3954">Wyrmskull Tunnel</xsl:when>
				<xsl:when test="zone = 3955">Hellfire Basin</xsl:when>
				<xsl:when test="zone = 3956">The Shadow Stair</xsl:when>
				<xsl:when test="zone = 3957">Sha'tari Outpost</xsl:when>

				<xsl:when test="zone = 3958">Sha'tari Base Camp</xsl:when>
			</xsl:choose>
			<!-- (<xsl:value-of select="zone"/>) -->
			</td>
			<td nowrap="nowrap"><xsl:value-of select="ontime"/></td>
			<td nowrap="nowrap" align="right"><xsl:value-of select="latency"/>ms</td>

      </tr>
      </xsl:for-each>
</table>
</xsl:template>
















</xsl:stylesheet>