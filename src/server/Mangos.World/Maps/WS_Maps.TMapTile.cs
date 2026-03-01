//
// Copyright (C) 2013-2025 getMaNGOS <https://www.getmangos.eu>
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
using System;
using System.Collections.Generic;
using System.IO;

namespace Mangos.World.Maps;

public partial class WS_Maps
{
    public class TMapTile : IDisposable
    {
        public List<ulong> PlayersHere;

        public List<ulong> CreaturesHere;

        public List<ulong> GameObjectsHere;

        public List<ulong> CorpseObjectsHere;

        public List<ulong> DynamicObjectsHere;

        /// <summary>
        /// The GridMap instance for this tile. All height/area/liquid/terrain
        /// queries are served through this instance (MangosZero format).
        /// </summary>
        public GridMap GridMapData;

        private readonly byte CellX;

        private readonly byte CellY;

        private readonly uint CellMap;

        private bool _disposedValue;

        public TMapTile(byte tileX, byte tileY, uint tileMap)
        {
            PlayersHere = new List<ulong>();
            CreaturesHere = new List<ulong>();
            GameObjectsHere = new List<ulong>();
            CorpseObjectsHere = new List<ulong>();
            DynamicObjectsHere = new List<ulong>();

            if (!WorldServiceLocator.WSMaps.Maps.ContainsKey(tileMap))
            {
                return;
            }

            CellX = tileX;
            CellY = tileY;
            CellMap = tileMap;

            // MangosZero format: {mapId:D4}{tileY:D2}{tileX:D2}.map (note Y,X order, 4-digit map ID)
            var fileName = MapFileFormats.GetMapFileName(tileMap, tileX, tileY);
            var filePath = Path.Combine("maps", fileName);

            if (!File.Exists(filePath))
            {
                WorldServiceLocator.WorldServer.Log.WriteLine(LogType.WARNING, "Map file [{0}] not found", fileName);
                return;
            }

            LoadMapFile(filePath, fileName);
        }

        private void LoadMapFile(string filePath, string fileName)
        {
            WorldServiceLocator.WorldServer.Log.WriteLine(LogType.INFORMATION, "Loading map file [{0}]", fileName);

            var gridMap = new GridMap();
            if (!gridMap.LoadData(filePath))
            {
                WorldServiceLocator.WorldServer.Log.WriteLine(LogType.WARNING, "Failed to load map file [{0}]", fileName);
                gridMap.Dispose();
                return;
            }

            GridMapData = gridMap;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                GridMapData?.Dispose();
                GridMapData = null;
                WorldServiceLocator.WSMaps.UnloadSpawns(CellX, CellY, CellMap);
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
            Dispose();
        }
    }
}
