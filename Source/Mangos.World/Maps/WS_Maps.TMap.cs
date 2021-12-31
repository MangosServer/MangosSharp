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

using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Map;
using Mangos.DataStores;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.IO;

namespace Mangos.World.Maps;

public partial class WS_Maps
{
    public class TMap : IDisposable
    {
        public int ID;

        public MapTypes Type;

        public string Name;

        public bool[,] TileUsed;

        public TMapTile[,] Tiles;

        private bool _disposedValue;

        public bool IsDungeon => Type is MapTypes.MAP_INSTANCE or MapTypes.MAP_RAID;

        public bool IsRaid => Type == MapTypes.MAP_RAID;

        public bool IsBattleGround => Type == MapTypes.MAP_BATTLEGROUND;

        public int ResetTime
        {
            get
            {
                checked
                {
                    switch (Type)
                    {
                        case MapTypes.MAP_BATTLEGROUND:
                            return WorldServiceLocator._Global_Constants.DEFAULT_BATTLEFIELD_EXPIRE_TIME;

                        case MapTypes.MAP_INSTANCE:
                        case MapTypes.MAP_RAID:
                            switch (ID)
                            {
                                case 249:
                                    return (int)Math.Round(WorldServiceLocator._Functions.GetNextDate(5, 3).Subtract(DateAndTime.Now).TotalSeconds);

                                case 309:
                                case 509:
                                    return (int)Math.Round(WorldServiceLocator._Functions.GetNextDate(3, 3).Subtract(DateAndTime.Now).TotalSeconds);

                                case 409:
                                case 469:
                                case 531:
                                case 533:
                                    return (int)Math.Round(WorldServiceLocator._Functions.GetNextDay(DayOfWeek.Tuesday, 3).Subtract(DateAndTime.Now).TotalSeconds);
                            }
                            break;
                        default:
                            break;
                    }
                    return WorldServiceLocator._Global_Constants.DEFAULT_INSTANCE_EXPIRE_TIME;
                }
            }
        }

        public TMap(int Map, DataStore mapDataStore)
        {
            Type = MapTypes.MAP_COMMON;
            Name = "";
            TileUsed = new bool[64, 64];
            Tiles = new TMapTile[64, 64];
            checked
            {
                if (WorldServiceLocator._WS_Maps.Maps.ContainsKey((uint)Map))
                {
                    return;
                }
                WorldServiceLocator._WS_Maps.Maps.Add((uint)Map, this);
                var x = 0;
                do
                {
                    var y = 0;
                    do
                    {
                        TileUsed[x, y] = false;
                        y++;
                    }
                    while (y <= 63);
                    x++;
                }
                while (x <= 63);
                try
                {
                    for (var i = 0; i <= mapDataStore.Rows - 1; i++)
                    {
                        if (mapDataStore.ReadInt(i, 0) == Map)
                        {
                            ID = Map;
                            Type = (MapTypes)mapDataStore.ReadInt(i, 2);
                            Name = mapDataStore.ReadString(i, 4);
                            break;
                        }
                    }
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: 1 Map initialized.", mapDataStore.Rows - 1);
                }
                catch (DirectoryNotFoundException ex)
                {
                    ProjectData.SetProjectError(ex);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("DBC File : Map missing.");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    ProjectData.ClearProjectError();
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            checked
            {
                if (!_disposedValue)
                {
                    var i = 0;
                    do
                    {
                        var j = 0;
                        do
                        {
                            if (Tiles[i, j] != null)
                            {
                                Tiles[i, j].Dispose();
                            }
                            j++;
                        }
                        while (j <= 63);
                        i++;
                    }
                    while (i <= 63);
                    WorldServiceLocator._WS_Maps.Maps.Remove((uint)ID);
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        void IDisposable.Dispose()
        {
            //ILSpy generated this explicit interface implementation from .override directive in Dispose
            Dispose();
        }
    }
}
