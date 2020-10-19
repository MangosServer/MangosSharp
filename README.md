<table border=0 cellpadding=0 cellspacing=0 valign='top'><tr>
<td><a href='https://www.getmangos.eu' target='getmangos.eu'><img src='https://www.getmangos.eu/!assets_mangos/logo.png' border=0></a></td>
<td valign='top'>
<a href='https://www.getmangos.eu/forums/' target='getmangos.forum'><img src='/icons/FORUM.gif' border=0></a>
<a href='https://www.getmangos.eu/wiki' target='getmangos.wiki'><img src='/icons/WIKI.gif' border=0></a>
<a href='https://www.getmangos.eu/github-activity/' target='getmangos.activity'><img src='/icons/ACTIVITY.gif' border=0></a>
<a href='https://www.getmangos.eu/bug-tracker/others/mangos-sharpe//' target='getmangos.tracker'><img src='/icons/TRACKER.gif' border=0></a>
<br />Build Status: <br/>Coming soon
</td></tr></table>

VANILLA WOW BRANCH
===
### A World of Warcraft server for Vanilla WoW  
----
*Mangos* is an open source project, built in [C#][7], it's fast, can store game data in 
[MySQL][40] and [MariaDB][41]. It aims to be 100% compatible with [World of Warcraft][2]
in its vanilla versions, namely [patch 1.12.1][5] and [patch 1.12.2][6].

If you liked the first incarnation of [World of Warcraft][2] and still want to play
[vanilla WoW][4], this is the branch for you. We provide an authentication
server where you can manage your users, and a world server which serves game
content just like the original did back then.

World of Warcraft, and all World of Warcraft or Warcraft art, images, and lore are
copyrighted by [Blizzard Entertainment, Inc.][1]

Requirements
------------
TODO !!

Operating systems
-----------------
Currently we support running *Mangos* on the following operating systems:

* **Windows**, 32 bit and 64 bit. [Windows][20] Server 2008 (or newer) or Windows 7 (or newer) is recommended.

Of course, newer versions should work, too. In the case of Windows, matching
server versions will work, too.

Compilers
---------
Building *Mangos* is currently possible with these compilers:

* **Microsoft Visual Studio 32 bit and 64 bit.** All editions of [Visual Studio][31]
  are supported. Only Visual Studio 2015 and above are now officially supported.

Dependencies
------------
The *Mangos* server stands on the shoulders of well-known Open Source
libraries, and a few awesome, but less known libraries to prevent us from
inventing the wheel again.

*Please note that Linux and Mac OS X users should install packages using
their systems package management instead of source packages.*

* **MySQL** / **MariaDB**: to store content, and user data, we rely on
  [MySQL][40]/[MariaDB][41] to handle data.
* **OpenSSL**: [OpenSSL][48] ([OpenSSL for Windows][55]) provides encryption
  algorithms used when authenticating clients.
Discuss
-------
If you need help with building and installing *Mangos* there are thousands of
users out there already running *Mangos* and many more you can find on our
project website and discussion forum to assist with any issues you may have.

* [getmangos.eu][10]

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

The full license is included in the file `License.md`.

In addition, as a special exception, permission is granted to link the code of
*Mangos* with the OpenSSL project's [OpenSSL library][48] (or with modified
versions of it that use the same license as the OpenSSL library), and distribute
the linked executables. You must obey the GNU General Public License in all
respects for all of the code used other than [OpenSSL][48].

[1]: http://blizzard.com/ "Blizzard Entertainment Inc. · we love you!"
[2]: http://blizzard.com/games/wow/ "World of Warcraft · Classic / Vanilla"
[3]: http://wowpedia.org/Beta#World_of_Warcraft "World of Warcraft - Classic Beta"
[4]: http://www.wowpedia.org/Patch_1.12.0 "Vanilla WoW · Patch 1.12.0 release notes"
[5]: http://www.wowpedia.org/Patch_1.12.1 "Vanilla WoW · Patch 1.12.1 release notes"
[6]: http://www.wowpedia.org/Patch_1.12.2 "Vanilla WoW · Patch 1.12.2 release notes"
[7]: http://www.cppreference.com/ "C / C++ reference"

[10]: https://getmangos.eu/ "mangos · project site"
[12]: https://github.com/mangosserver "MaNGOS Sharp · github organization"
[13]: https://github.com/mangosserver/mangosSharp "MaNGOS Sharp · server repository"
[15]: https://github.com/mangoszero/database "MaNGOS Zero · content database repository"
[17]: https://scan.coverity.com/ "Coverity Scan · Static Code Analysis"

[19]: http://www.cmake.org/ "CMake · Cross Platform Make"
[20]: http://windows.microsoft.com/ "Microsoft Windows"
[21]: http://www.debian.org/ "Debian · The Universal Operating System"
[22]: http://www.ubuntu.com/ "Ubuntu · The world's most popular free OS"
[23]: http://www.freebsd.org/ "FreeBSD · The Power To Serve"
[24]: http://www.netbsd.org/ "NetBSD · The NetBSD Project"
[25]: http://www.openbsd.org/ "OpenBSD · Free, functional and secure"
[31]: https://visualstudio.microsoft.com/vs/older-downloads/ "Visual Studio Downloads"
[33]: http://clang.llvm.org/ "clang · a C language family frontend for LLVM"
[34]: http://git-scm.com/ "Git · Distributed version control system"
[35]: http://windows.github.com/ "github · windows client"
[36]: http://www.sourcetreeapp.com/ "SourceTree · Free Mercurial and Git Client for Windows/Mac"

[40]: http://www.mysql.com/ "MySQL · The world's most popular open source database"
[41]: http://www.mariadb.org/ "MariaDB · An enhanced, drop-in replacement for MySQL"
[43]: http://www.dre.vanderbilt.edu/~schmidt/ACE.html "ACE · The ADAPTIVE Communication Environment"
[44]: http://github.com/memononen/recastnavigation "Recast · Navigation-mesh Toolset for Games"
[45]: http://sourceforge.net/projects/g3d/ "G3D · G3D Innovation Engine"
[46]: http://github.com/ge0rg/libmpq "libmpq · A library for reading data from MPQ archives"
[48]: http://www.openssl.org/ "OpenSSL · The Open Source toolkit for SSL/TLS"
[49]: http://www.stack.nl/~dimitri/doxygen/ "Doxygen · API documentation generator"
[50]: http://www.lua.org/ "Lua · The Programming Language"
[51]: http://gnuwin32.sourceforge.net/packages/zlib.htm "Zlib for Windows"
[52]: http://gnuwin32.sourceforge.net/packages/bzip2.htm "Bzip2 for Windows"
[53]: http://www.zlib.net/ "Zlib"
[54]: http://www.bzip.org/ "Bzip2"
[55]: http://slproweb.com/products/Win32OpenSSL.html "OpenSSL for Windows"
[56]: http://www.lua.org/ "Lua"
[57]: https://code.google.com/p/luaforwindows/ "Lua for Windows"

