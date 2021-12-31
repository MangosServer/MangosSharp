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
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Mangos.World.Maps;

public partial class WS_Maps
{
    public class TMapTile : IDisposable
    {
        public ushort[,] AreaFlag;

        public byte[,] AreaTerrain;

        public float[,] WaterLevel;

        public float[,] ZCoord;

        public List<ulong> PlayersHere;

        public List<ulong> CreaturesHere;

        public List<ulong> GameObjectsHere;

        public List<ulong> CorpseObjectsHere;

        public List<ulong> DynamicObjectsHere;

        private readonly byte CellX;

        private readonly byte CellY;

        private readonly uint CellMap;

        private bool _disposedValue;

        public TMapTile(byte tileX, byte tileY, uint tileMap)
        {
            checked
            {
                AreaFlag = new ushort[WorldServiceLocator._Global_Constants.RESOLUTION_FLAGS + 1, WorldServiceLocator._Global_Constants.RESOLUTION_FLAGS + 1];
                AreaTerrain = new byte[WorldServiceLocator._Global_Constants.RESOLUTION_TERRAIN + 1, WorldServiceLocator._Global_Constants.RESOLUTION_TERRAIN + 1];
                WaterLevel = new float[WorldServiceLocator._Global_Constants.RESOLUTION_WATER + 1, WorldServiceLocator._Global_Constants.RESOLUTION_WATER + 1];
                PlayersHere = new List<ulong>();
                CreaturesHere = new List<ulong>();
                GameObjectsHere = new List<ulong>();
                CorpseObjectsHere = new List<ulong>();
                DynamicObjectsHere = new List<ulong>();
                if (!WorldServiceLocator._WS_Maps.Maps.ContainsKey(tileMap))
                {
                    return;
                }
                ZCoord = new float[WorldServiceLocator._WS_Maps.RESOLUTION_ZMAP + 1, WorldServiceLocator._WS_Maps.RESOLUTION_ZMAP + 1];
                CellX = tileX;
                CellY = tileY;
                CellMap = tileMap;
                var fileName = string.Format("{0}{1}{2}.map", Strings.Format(tileMap, "000"), Strings.Format(tileX, "00"), Strings.Format(tileY, "00"));
                if (!File.Exists("maps\\" + fileName))
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "Map file [{0}] not found", fileName);
                    return;
                }
                FileStream f = new("maps\\" + fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 82704, FileOptions.SequentialScan);
                BinaryReader b = new(f);
                var fileVersion = Encoding.ASCII.GetString(b.ReadBytes(8), 0, 8);
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "Loading map file [{0}] version [{1}]", fileName, fileVersion);
                var rESOLUTION_FLAGS = WorldServiceLocator._Global_Constants.RESOLUTION_FLAGS;
                for (var x = 0; x <= rESOLUTION_FLAGS; x++)
                {
                    var rESOLUTION_FLAGS2 = WorldServiceLocator._Global_Constants.RESOLUTION_FLAGS;
                    for (var y = 0; y <= rESOLUTION_FLAGS2; y++)
                    {
                        AreaFlag[x, y] = b.ReadUInt16();
                    }
                }
                var rESOLUTION_TERRAIN = WorldServiceLocator._Global_Constants.RESOLUTION_TERRAIN;
                for (var x = 0; x <= rESOLUTION_TERRAIN; x++)
                {
                    var rESOLUTION_TERRAIN2 = WorldServiceLocator._Global_Constants.RESOLUTION_TERRAIN;
                    for (var y = 0; y <= rESOLUTION_TERRAIN2; y++)
                    {
                        AreaTerrain[x, y] = b.ReadByte();
                    }
                }
                var rESOLUTION_WATER = WorldServiceLocator._Global_Constants.RESOLUTION_WATER;
                for (var x = 0; x <= rESOLUTION_WATER; x++)
                {
                    var rESOLUTION_WATER2 = WorldServiceLocator._Global_Constants.RESOLUTION_WATER;
                    for (var y = 0; y <= rESOLUTION_WATER2; y++)
                    {
                        WaterLevel[x, y] = b.ReadSingle();
                    }
                }
                var rESOLUTION_ZMAP = WorldServiceLocator._WS_Maps.RESOLUTION_ZMAP;
                for (var x = 0; x <= rESOLUTION_ZMAP; x++)
                {
                    var rESOLUTION_ZMAP2 = WorldServiceLocator._WS_Maps.RESOLUTION_ZMAP;
                    for (var y = 0; y <= rESOLUTION_ZMAP2; y++)
                    {
                        ZCoord[x, y] = b.ReadSingle();
                    }
                }
                b.Close();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                WorldServiceLocator._WS_Maps.UnloadSpawns(CellX, CellY, CellMap);
            }
            _disposedValue = true;
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
