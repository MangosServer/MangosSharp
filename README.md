[<img src='https://www.getmangos.eu/!assets_mangos/currentlogo.gif' width="48" border=0>][8]
[<img src='https://www.getmangos.eu/!assets_mangos/logo2.png' border=0>][3]

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<B>Build Status:</b>
Windows: [<img src='https://github.com/MangosServer/MangosSharp/actions/workflows/build.yml/badge.svg' border=0 valign="middle"/>][11]
 <br><b>Repository Status:</b> 
[<img src='https://api.codacy.com/project/badge/Grade/f77c3dbb9e124188b0cf4ec6da878721' border=0 valign="middle"/>][12]
[<img src='https://www.codefactor.io/repository/github/mangosserver/mangossharp/badge' border=0 valign="middle"/>][13]
[<img src='https://img.shields.io/badge/PRs-welcome-brightgreen.svg?style=flat' border=0 valign="middle"/>][14]
[<img src='https://img.shields.io/discord/286167585270005763.svg' border=0 valign="middle"/>][9]

---

[<img src="https://www.getmangos.eu/!assets_mangos/MangosSharp0.png" width="48" valign="middle"/>][8]
 **MangosSharp0 - Vanilla WoW server**
===

**MangosSharp** is an open source project written in [C#][7]. It's fast and stores game data in 
[MySQL][40] or [MariaDB][41].

If you liked the original incarnation of [World of Warcraft][2] and still want to play it,
this is the branch for you. We provide an authentication server where you can manage your users, 
and a world server which serves game content just like the original did back then.

It aims to be 100% compatible with the 3 final versions of Vanilla [World of Warcraft][2], 
namely [patch 1.12.1][4], [patch 1.12.2][5] & [patch 1.12.3][6].
<br>**IT DOES NOT SUPPORT 1.13.x** and beyond which is the newly released Classic Experience (NuClassic).


Requirements
------------
    Supported platforms: Windows 8+, Linux, MacOS, Docker
    .NET 6 SDK
    MySQL 8.0
    Visual Studio 2022 or any other editor with .NET 6 support


Dependencies
------------
The server stands on the shoulders of several well-known Open Source libraries plus
a few awesome, but less known libraries to prevent us from inventing the wheel again.

* **[MySQL][40]** / **[MariaDB][41]**: These databases are used to store content and user data.


<br>We have a small, but extremely friendly and helpful community managed by MadMax and Antz.


Our discord/forum motto is: 
```js
'Be nice or Be somewhere else'
```
Any trolling or unpleasantness is swiftly dealt with !!

**Official Website**
----

We welcome anyone who is interested in enjoying older versions of wow or contributing and helping out !

* [**Official MaNGOS Website**][3]  

**Discord Server**
----

We also have a Discord server where many of us hang out and discuss Mangos related stuff.

* [**Discord Server**][9]

**Main Wiki**
----

The repository of as much information as we can pack in. Details regarding the Database, file type definitions, packet definitons etc.

* [**Wiki Table of Contents**][15]


**Bug / Issue Tracker**
----

Found an issue or something which doesn't seem right, please log it in the relevant section of the Bug Tracker.

* [**Bug Tracker**][16]

**Installation Guides**
----

Installation instructions for various operation systems can be found here.

* [**Installation Guides**][17] 


License
-------
This program is free software; you can redistribute it and/or modify it under
the terms of the GNU General Public License as published by the Free Software
Foundation; either version 2 of the License, or (at your option) any later
version.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE.  See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with
this program; if not, write to the Free Software Foundation, Inc., 51 Franklin
Street, Fifth Floor, Boston, MA 02110-1301 USA.

The full license is included in the file [LICENSE](LICENSE).

We have all put in hundreds of hours of time for free to make the server what it
is today.
<br>All we ask is that if you modify the code and make improvements, please have
the decency to feed those changes back to us.

In addition, as a special exception, permission is granted to link the code of
*Mangos* with the OpenSSL project's [OpenSSL library][48] (or with modified
versions of it that use the same license as the OpenSSL library), and distribute
the linked executables. You must obey the GNU General Public License in all
respects for all of the code used other than [OpenSSL][48].

Acknowledgements
--------
World of Warcraft, and all related art, images, and lore are copyright [Blizzard Entertainment, Inc.][1]


[1]: http://blizzard.com/ "Blizzard Entertainment Inc. - We love you!"
[2]: https://worldofwarcraft.com/ "World of Warcraft"
[3]: https://www.getmangos.eu "Main MaNGOS Website"
[4]: http://www.wowpedia.org/Patch_1.12.1 "Vanilla WoW - Patch 1.12.1 release notes"
[5]: http://www.wowpedia.org/Patch_1.12.2 "Vanilla WoW - Patch 1.12.2 release notes"
[6]: http://www.wowpedia.org/Patch_1.12.3 "Vanilla WoW - Patch 1.12.3 release notes"
[7]: http://www.cppreference.com/ "C / C++ reference"
[8]: https://github.com/mangos/MaNGOS/blob/master/mangosFamily.md "The MaNGOS family of Icons"
[9]: https://discord.gg/fPxMjHS8xs "Our community hub on Discord"
[10]: https://travis-ci.com/github/mangoszero/server/builds "Travis CI - Linux/MAC build status"
[11]: https://github.com/MangosServer/MangosSharp/actions/workflows/build.yml "AppVeyor Scan - Windows build status"
[12]: https://app.codacy.com/gh/MangosServer/MangosSharp/dashboard "Codacy Code Status"
[13]: https://www.codefactor.io/repository/github/mangosserver/mangossharp "Codefactor Code Status"
[14]: http://makeapullrequest.com "Show PR's Welcome Icon"
[15]: http://getmangos.eu/wiki "Mangos Wiki"
[16]: https://www.getmangos.eu/bug-tracker/others/mangos-sharp/ "Mangos Online tracker"
[17]: https://www.getmangos.eu/wiki/documentation/installation-guides/ "Installation Guides"
[19]: http://www.cmake.org/ "CMake - Cross Platform Make"
[20]: http://windows.microsoft.com/ "Microsoft Windows"
[21]: http://www.debian.org/ "Debian - The Universal Operating System"
[22]: http://www.ubuntu.com/ "Ubuntu - The world's most popular free OS"
[23]: http://www.freebsd.org/ "FreeBSD - The Power To Serve"
[24]: http://www.netbsd.org/ "NetBSD - The NetBSD Project"
[25]: http://www.openbsd.org/ "OpenBSD - Free, functional and secure"
[31]: https://visualstudio.microsoft.com/vs/older-downloads/ "Visual Studio Downloads"
[33]: http://clang.llvm.org/ "clang - a C language family frontend for LLVM"
[34]: http://git-scm.com/ "Git - Distributed version control system"
[35]: http://windows.github.com/ "github - windows client"
[40]: https://dev.mysql.com/downloads/ "MySQL - The world's most popular open source database"
[41]: https://mariadb.org/download/ "MariaDB - An enhanced, drop-in replacement for MySQL"
[43]: http://www.dre.vanderbilt.edu/~schmidt/ACE.html "ACE - The ADAPTIVE Communication Environment"
[44]: http://github.com/memononen/recastnavigation "Recast - Navigation-mesh Toolset for Games"
[45]: http://sourceforge.net/projects/g3d/ "G3D - G3D Innovation Engine"
[46]: http://zezula.net/en/mpq/stormlib.html "Stormlib - A library for reading data from MPQ archives"
[48]: http://www.openssl.org/ "OpenSSL - The Open Source toolkit for SSL/TLS"
[49]: https://www.doxygen.nl/download.html "Doxygen - API documentation generator"
[51]: http://gnuwin32.sourceforge.net/packages/zlib.htm "Zlib for Windows"
[52]: http://gnuwin32.sourceforge.net/packages/bzip2.htm "Bzip2 for Windows"
[53]: http://www.zlib.net/ "Zlib"
[54]: http://www.bzip.org/ "Bzip2"
[55]: http://slproweb.com/products/Win32OpenSSL.html "OpenSSL for Windows"
