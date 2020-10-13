//
//  Copyright (C) 2013-2020 getMaNGOS <https:\\getmangos.eu>
//  
//  This program is free software. You can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation. either version 2 of the License, or
//  (at your option) any later version.
//  
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY. Without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//  
//  You should have received a copy of the GNU General Public License
//  along with this program. If not, write to the Free Software
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Foole.Mpq;
using Mangos.Common.DataStores;
using Mangos.Common.Enums.Global;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.DBCExtractor
{
    static class MainModule
    {
        public static List<MpqArchive> MPQArchives = new List<MpqArchive>();
        public static List<int> MapIDs = new List<int>();
        public static List<string> MapNames = new List<string>();
        public static Dictionary<int, int> MapAreas = new Dictionary<int, int>();
        public static int MaxAreaID = -1;
        public static Dictionary<int, int> MapLiqTypes = new Dictionary<int, int>();

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

            var MPQFilesToOpen = new List<string>() { "terrain.MPQ", "dbc.MPQ", "misc.MPQ", "patch.MPQ", "patch-2.MPQ" };
            foreach (string mpq in MPQFilesToOpen)
            {
                if (File.Exists(@"Data\" + mpq) == false)
                {
                    Console.WriteLine("Missing [{0}]. Make sure this extractor is put in your World of Warcraft directory.", mpq);
                    goto ExitNow;
                }
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            foreach (string mpq in MPQFilesToOpen)
            {
                var stream = File.Open(Path.GetFullPath(@"Data\" + mpq), FileMode.Open);
                var newArchive = new MpqArchive(stream, true);
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
                goto ExitNow;
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
            string dbcFolder = Path.GetFullPath("dbc");
            int numDBCs = 0;
            foreach (var mpqArchive in MPQArchives)
                numDBCs += mpqArchive.Where(x => x.Filename is object).Where(x => x.Filename.EndsWith(".dbc")).Count();
            int i = 0;
            int numDiv30 = numDBCs / 30;
            foreach (var mpqArchive in MPQArchives)
            {
                foreach (var mpqFile in mpqArchive.Where(x => x.Filename is object).Where(x => x.Filename.EndsWith(".dbc")))
                {
                    using (var mpqStream = mpqArchive.OpenFile(mpqFile))
                    {
                        using (var fileStream = File.Create(Path.Combine(dbcFolder, Path.GetFileName(mpqFile.Filename))))
                        {
                            mpqStream.CopyTo(fileStream);
                        }
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

        // Public Sub ExtractMaps()
        // Dim mapCount As Integer = ReadMapDBC()
        // ReadAreaTableDBC()
        // ReadLiquidTypeTableDBC()

        // Console.WriteLine()
        // Console.ForegroundColor = ConsoleColor.White
        // Console.Write("Extracting Maps")
        // Console.ForegroundColor = ConsoleColor.Gray

        // Dim mapFolder As String = Path.GetFullPath("maps")

        // Dim total As Integer = mapCount * 64 * 64
        // Dim done As Integer = 0
        // Dim totalDiv30 As Integer = total \ 30
        // For i As Integer = 0 To mapCount - 1
        // 'Dim wdtName As String = String.Format("World\Maps\{0}\{0}.wdt", MapNames(i))
        // 'Dim wdt As New WDT_File()
        // 'If Not wdt.LoadFile(wdtName) Then
        // '    Console.ForegroundColor = ConsoleColor.Red
        // '    Console.WriteLine()
        // '    Console.WriteLine("WDT File [{0}] could not be read.", wdtName)
        // '    Console.ForegroundColor = ConsoleColor.Gray
        // 'End If

        // For x As Integer = 0 To 63
        // For y As Integer = 0 To 63
        // Dim mpqFileName As String = String.Format("World\Maps\{0}\{0}_{1}_{2}.adt", MapNames(i), x, y)
        // Dim outputName As String = String.Format("{0}{1}{2}.map", Format(MapIDs(i), "000"), Format(x, "00"), Format(y, "00"))
        // ConvertADT(mpqFileName, Path.Combine(mapFolder, outputName))
        // done += 1
        // If (done Mod totalDiv30) = 0 Then Console.Write(".")
        // Next
        // Next
        // Next

        // Console.Write(" Done!")
        // End Sub

        public static int ReadMapDBC()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Reading Map.dbc... ");
            var mapDBC = new BufferedDbc(@"dbc\Map.dbc");
            for (int i = 0, loopTo = mapDBC.Rows - 1; i <= loopTo; i++)
            {
                MapIDs.Add(Conversions.ToInteger(mapDBC[i, 0]));
                MapNames.Add(Conversions.ToString(mapDBC[i, 1, DBCValueType.DBC_STRING]));
            }

            Console.WriteLine("Done! ({0} maps loaded)", mapDBC.Rows);
            return mapDBC.Rows;
        }

        public static void ReadAreaTableDBC()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Reading AreaTable.dbc... ");
            var areaDBC = new BufferedDbc(@"dbc\AreaTable.dbc");
            int maxID = -1;
            for (int i = 0, loopTo = areaDBC.Rows - 1; i <= loopTo; i++)
            {
                int areaID = (int)areaDBC[i, 0];
                int areaFlag = (int)areaDBC[i, 3];
                MapAreas.Add(areaID, areaFlag);
                if (areaID > maxID)
                    maxID = areaID;
            }

            MaxAreaID = maxID;
            Console.WriteLine("Done! ({0} areas loaded)", areaDBC.Rows);
        }

        public static void ReadLiquidTypeTableDBC()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Reading LiquidType.dbc... ");
            var liquidDBC = new BufferedDbc(@"dbc\LiquidType.dbc");
            for (int i = 0, loopTo = liquidDBC.Rows - 1; i <= loopTo; i++)
                MapLiqTypes.Add((int)liquidDBC[i, 0], (int)liquidDBC[i, 3]);
            Console.WriteLine("Done! ({0} LiqTypes loaded)", liquidDBC.Rows);
        }
    }
}