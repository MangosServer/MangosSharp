==== EXTRACTING DBCs ====

1. Move the following files into your World of Warcraft directory:
   - 'DBCExtractor.exe'
   - 'MpqLib.dll'
2. Make sure your copy of World of Warcraft is at version 1.12.1.
3. Execute 'DBCExtractor.exe' and let it run, shouldn't take long at all.
4. Move all content within the folder 'dbc' that was created inside
   your World of Warcraft directory, into a folder named 'dbc' inside
   your MangosVB Zero binary folder.


==== EXTRACTING MAPS ====

1. Move the file 'ad.exe' into your World of Warcraft directory.
2. Make sure your copy of World of Warcraft is at version 1.12.1.
3. Create a folder named 'maps' within your World of Warcraft directory.
4. Now execute 'ad.exe' and let it run.
   NOTE: ad.exe defaults to a map resolution of 256, if you just run it by clicking it under Windows, be sure to set the MapResolution config setting to    256 to match this.  256 is the highest resolution, it will be the most accurate for map related calculations in the WorldServer, however this    resolution will use a lot more memory when running your WorldServers.  A map resolution of 64 will be the least accurate, but also use the least amount    of memory, and seems to be very playable to me.  In order to change the resolution that ad.exe extracts the maps at you have to run ad.exe using the    command line and add the -r flag, for instance to extract maps at a resolution of 64, under your command line change to your World of Warcraft 1.12.1    directory and execute the following command: ad.exe -r 64, this should extract the files at a resolution of 64, to extract at 128 use the following    command: ad.exe -r 128.  (When I extracted maps at 64 I ended up with map files sized at 81KB, when I extracted at 256 I ended up with map files sized    at 321KB), if creatures are bouncing higher and higher in the air, or not following the terrain it is a good sign that you have extracted the maps at a    different resolution than you have set in your WorldServer.ini file.
5. Move all content within the folder 'maps' you just created to a folder
   named 'maps' inside your MangosVB Zero binary folder.
   
   
==== EXTRACTING VMAPS (VERTICAL) ====

1. Move the following files into your World of Warcraft directory:
   - 'vmapextract_v2.exe'
   - 'vmap_assembler.exe'
   - 'splitConfig.txt'
   - 'Extract_VMaps.bat'
2. Make sure your copy of World of Warcraft is at version 1.12.1.
3. NOTE: If you are using more than one client version of World of Warcraft on
   your pc you will need to be sure the register key for the World of Warcraft
   Install Path is set to point at your 1.12.1 version.  This key should be
   located under the HKEY_LOCAL_MACHINE under "SOFTWARE\Blizzard Entertainment\World of Warcraft"
   the key should be named InstallPath.
   (My latest was actually under HKEY_LOCAL_MACHINE under "SOFTWARE\Wow6432Node\Blizzard Entertainment\World of Warcraft"
    I'm using Windows 7)
4. Execute 'Extract_VMaps.bat' and let it all run, note that it will
   execute two executives, so be sure they both ran. The second one
   will not print out what it's doing like the first one.
5. Move all content within the folder 'vmaps' that was created inside
   your World of Warcraft directory, into a folder named 'vmaps' inside
   your MangosVB Zero binary folder.