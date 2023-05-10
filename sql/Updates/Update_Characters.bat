@echo off
:quick
rem Quick install section
rem This will automatically use the variables below to install the Characters database without prompting then optimize them and exit
rem To use: Set your environment variables below and change 'set quick=off' to 'set quick=on' 
set quick=off

rem Verify if mysql is installed on the computer' 
where mysql.exe >nul 2>nul
if %errorlevel% equ 0 (
    if %quick% == off (
        goto standard
    ) else (
        echo (( Mangos Account Database Quick Installer ))
        rem -- Change the values below to match your server --
        set svr=localhost
        set user=root
        set pass=rootpass
        set port=3306
        set wdb=mangosVBcharacters
        rem -- Don't change past this point --
        set yesno=y
        goto install
    )
)

else ( goto mysqlerror )

:standard
rem Standard install section
color 3B
echo .
echo MM   MM         MM   MM  MMMMM   MMMM   MMMMM
echo MM   MM         MM   MM MMM MMM MM  MM MMM MMM
echo MMM MMM         MMM  MM MMM MMM MM  MM MMM
echo MM M MM         MMMM MM MMM     MM  MM  MMM
echo MM M MM  MMMMM  MM MMMM MMM     MM  MM   MMM
echo MM M MM M   MMM MM  MMM MMMMMMM MM  MM    MMM
echo MM   MM     MMM MM   MM MM  MMM MM  MM     MMM
echo MM   MM MMMMMMM MM   MM MMM MMM MM  MM MMM MMM
echo MM   MM MM  MMM MM   MM  MMMMMM  MMMM   MMMMM
echo         MM  MMM http://getmangos.com
echo         MMMMMM
echo .

echo Credits to: Factionwars, Nemok, BrainDedd and Antz
color 02
echo ==================================================
echo .
set /p svr=What is your MySQL host name?           [localhost]   : 
if %svr%. == . set svr=localhost
set /p user=What is your MySQL user name?           [root]      : 
if %user%. == . set user=root
set /p pass=What is your MySQL password?            [rootpass]           : 
if %pass%. == . set pass=rootpass
set /p port=What is your MySQL port?                [3306]        : 
if %port%. == . set port=3306
set /p wdb=What is your Characters database name?       [mangosVBcharacters]      : 
if %wdb%. == . set wdb=mangosVBcharacters

:install
set dbpath=Characters
set mysql=tools

:checkpaths
if not exist %dbpath% then goto patherror
if not exist %mysql%\mysql.exe then goto patherror


:checkdatabase
rem Verify if mangosVBaccounts database exist' 
mysqlshow --user=%user% --password=%pass% %wdb% >nul 2>nul
if %errorlevel% equ 1 (
    %mysql%\mysql --user=%user% --password=%pass% -e "CREATE DATABASE %wdb%;"
    echo database created with success.
    goto world
) else (
    echo The database %wdb% exists.
    goto world
)

:mysqlerror
echo MySQL is not installed on this computer. Please install it and relaunch the script.
goto :eof

:patherror
echo Cannot find required files.
pause
goto :eof

:world
if %quick% == off echo.
if %quick% == off echo This will update your current Character database.
if %quick% == off set /p yesno=Do you wish to continue? (y/n) 

echo.
echo Updating Character database

for %%i in (%dbpath%\*.sql) do echo %%i & %mysql%\mysql -q -s -h %svr% --user=%user% --password=%pass% --port=%port% %wdb% < %%i

:done
echo.
echo Done :)
echo.
pause