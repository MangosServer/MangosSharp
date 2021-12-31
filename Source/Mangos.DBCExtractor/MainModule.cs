//
// Copyright (C) 2013-2022 getMaNGOS <https://getmangos.eu>
//
// This program is free software. You can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation. either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY. Without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program. If not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//

using Foole.Mpq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Mangos.DBCExtractor;

internal static class MainModule
{
    public static List<MpqArchive> MPQArchives = new();
    public static List<int> MapIDs = new();
    public static List<string> MapNames = new();
    public static Dictionary<int, int> MapAreas = new();
    public static int MaxAreaID = -1;
    public static Dictionary<int, int> MapLiqTypes = new();

    public static void Main()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("DBC extractor by UniX");
        Console.WriteLine("-----------------------------");
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Red;
        if (Directory.Exists("Data") == false)
        {
            Console.WriteLine("No data folder is found. Make sure this extractor is put in your World of Warcraft directory.");
            goto ExitNow;
        }

        List<string> MPQFilesToOpen = new() { "terrain.MPQ", "dbc.MPQ", "misc.MPQ", "patch.MPQ", "patch-2.MPQ" };
        foreach (var mpq in MPQFilesToOpen)
        {
            if (File.Exists(@"Data\" + mpq) == false)
            {
                Console.WriteLine("Missing [{0}]. Make sure this extractor is put in your World of Warcraft directory.", mpq);
                goto ExitNow;
            }
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        foreach (var mpq in MPQFilesToOpen)
        {
            var stream = File.Open(Path.GetFullPath(@"Data\" + mpq), FileMode.Open);
            MpqArchive newArchive = new(stream, true);
            MPQArchives.Add(newArchive);
            Console.WriteLine("Loaded archive [{0}].", mpq);
        }

        try
        {
            Directory.CreateDirectory("dbc");
            Directory.CreateDirectory("maps");
            Console.WriteLine("Created extract folders.");
        }
        catch (UnauthorizedAccessException)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Unable to create extract folders, you don't seem to have admin rights.");
            goto ExitNow;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Unable to create extract folders. Error: " + ex.Message);
            goto ExitNow;
        }

        try
        {
            ExtractDBCs();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Unable to extract DBC Files. Error: " + ex.Message);
        }

    // Try
    // ExtractMaps()
    // Catch ex As Exception
    // Console.ForegroundColor = ConsoleColor.Red
    // Console.WriteLine("Unable to extract Maps. Error: " & ex.Message)
    // GoTo ExitNow
    // End Try

    ExitNow:
        ;
        Console.ForegroundColor = ConsoleColor.DarkMagenta;
        Console.WriteLine();
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }

    public static void ExtractDBCs()
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("Extracting DBC Files");
        Console.ForegroundColor = ConsoleColor.Gray;
        var dbcFolder = Path.GetFullPath("dbc");
        var numDBCs = 0;
        foreach (var mpqArchive in MPQArchives)
        {
            numDBCs += mpqArchive.Where(x => x.Filename is not null).Where(x => x.Filename.EndsWith(".dbc")).Count();
        }

        var i = 0;
        var numDiv30 = numDBCs / 30;
        foreach (var mpqArchive in MPQArchives)
        {
            foreach (var mpqFile in mpqArchive.Where(x => x.Filename is not null).Where(x => x.Filename.EndsWith(".dbc")))
            {
                using (var mpqStream = mpqArchive.OpenFile(mpqFile))
                {
                    using var fileStream = File.Create(Path.Combine(dbcFolder, Path.GetFileName(mpqFile.Filename)));
                    mpqStream.CopyTo(fileStream);
                }

                i += 1;
                if (i % numDiv30 == 0)
                {
                    Console.Write(".");
                }
            }
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write(" Done.");
    }
}
