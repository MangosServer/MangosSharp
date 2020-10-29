
SELECT 
	realmlist.address, 
	realmlist.name, 
	realmlist.port, 
	realmlist.timezone, 
	realmlist.icon, 
	realmlist.realmflags,
	realmlist.population, 
	realmcharacters.numchars 
FROM realmlist left join realmcharacters on realmlist.id = realmcharacters.realmid
