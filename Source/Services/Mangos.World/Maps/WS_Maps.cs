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
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using Mangos.Common;
using Mangos.Common.DataStores;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Map;
using Mangos.Common.Globals;
using Mangos.World.Globals;
using Mangos.World.Objects;
using Mangos.World.Player;
using Mangos.World.Server;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.World.Maps
{
	public class WS_Maps
	{
		public class TArea
		{
			public int ID;

			public int mapId;

			public byte Level;

			public int Zone;

			public int ZoneType;

			public AreaTeam Team;

			public string Name;

			public bool IsMyLand(ref WS_PlayerData.CharacterObject objCharacter)
			{
				if (Team == AreaTeam.AREATEAM_NONE)
				{
					return false;
				}
				if (!objCharacter.IsHorde)
				{
					return Team == AreaTeam.AREATEAM_ALLY;
				}
				if (objCharacter.IsHorde)
				{
					return Team == AreaTeam.AREATEAM_HORDE;
				}
				return false;
			}

			public bool IsCity()
			{
				return ZoneType == 312;
			}

			public bool NeedFlyingMount()
			{
				return (ZoneType & 0x1000) != 0;
			}

			public bool IsSanctuary()
			{
				return (ZoneType & 0x800) != 0;
			}

			public bool IsArena()
			{
				return (ZoneType & 0x80) != 0;
			}
		}

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
					string fileName = string.Format("{0}{1}{2}.map", Strings.Format(tileMap, "000"), Strings.Format(tileX, "00"), Strings.Format(tileY, "00"));
					if (!File.Exists("maps\\" + fileName))
					{
						WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "Map file [{0}] not found", fileName);
						return;
					}
					FileStream f = new FileStream("maps\\" + fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 82704, FileOptions.SequentialScan);
					BinaryReader b = new BinaryReader(f);
					string fileVersion = Encoding.ASCII.GetString(b.ReadBytes(8), 0, 8);
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "Loading map file [{0}] version [{1}]", fileName, fileVersion);
					int rESOLUTION_FLAGS = WorldServiceLocator._Global_Constants.RESOLUTION_FLAGS;
					for (int x = 0; x <= rESOLUTION_FLAGS; x++)
					{
						int rESOLUTION_FLAGS2 = WorldServiceLocator._Global_Constants.RESOLUTION_FLAGS;
						for (int y = 0; y <= rESOLUTION_FLAGS2; y++)
						{
							AreaFlag[x, y] = b.ReadUInt16();
						}
					}
					int rESOLUTION_TERRAIN = WorldServiceLocator._Global_Constants.RESOLUTION_TERRAIN;
					for (int x = 0; x <= rESOLUTION_TERRAIN; x++)
					{
						int rESOLUTION_TERRAIN2 = WorldServiceLocator._Global_Constants.RESOLUTION_TERRAIN;
						for (int y = 0; y <= rESOLUTION_TERRAIN2; y++)
						{
							AreaTerrain[x, y] = b.ReadByte();
						}
					}
					int rESOLUTION_WATER = WorldServiceLocator._Global_Constants.RESOLUTION_WATER;
					for (int x = 0; x <= rESOLUTION_WATER; x++)
					{
						int rESOLUTION_WATER2 = WorldServiceLocator._Global_Constants.RESOLUTION_WATER;
						for (int y = 0; y <= rESOLUTION_WATER2; y++)
						{
							WaterLevel[x, y] = b.ReadSingle();
						}
					}
					int rESOLUTION_ZMAP = WorldServiceLocator._WS_Maps.RESOLUTION_ZMAP;
					for (int x = 0; x <= rESOLUTION_ZMAP; x++)
					{
						int rESOLUTION_ZMAP2 = WorldServiceLocator._WS_Maps.RESOLUTION_ZMAP;
						for (int y = 0; y <= rESOLUTION_ZMAP2; y++)
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
				this.Dispose();
			}
		}

		public class TMap : IDisposable
		{
			public int ID;

			public MapTypes Type;

			public string Name;

			public bool[,] TileUsed;

			public TMapTile[,] Tiles;

			private bool _disposedValue;

			public bool IsDungeon => Type == MapTypes.MAP_INSTANCE || Type == MapTypes.MAP_RAID;

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
						}
						return WorldServiceLocator._Global_Constants.DEFAULT_INSTANCE_EXPIRE_TIME;
					}
				}
			}

			public TMap(int Map)
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
					int x = 0;
					do
					{
						int y = 0;
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
						BufferedDbc tmpDBC = new BufferedDbc("dbc" + Conversions.ToString(Path.DirectorySeparatorChar) + "Map.dbc");
						int num = tmpDBC.Rows - 1;
						for (int i = 0; i <= num; i++)
						{
							int tmpMap = tmpDBC.Read<int>(i, 0);
							if (tmpMap == Map)
							{
								ID = Map;
								Type = unchecked((MapTypes)tmpDBC.Read<int>(i, 2));
								Name = tmpDBC.Read<string>(i, 4);
								break;
							}
						}
						WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: 1 Map initialized.", tmpDBC.Rows - 1);
						tmpDBC.Dispose();
					}
					catch (DirectoryNotFoundException ex)
					{
						ProjectData.SetProjectError(ex);
						DirectoryNotFoundException e = ex;
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
						int i = 0;
						do
						{
							int j = 0;
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
				this.Dispose();
			}
		}

		public Dictionary<int, TArea> AreaTable;

		public int RESOLUTION_ZMAP;

		public Dictionary<uint, TMap> Maps;

		public string MapList;

		public WS_Maps()
		{
			AreaTable = new Dictionary<int, TArea>();
			RESOLUTION_ZMAP = 0;
			Maps = new Dictionary<uint, TMap>();
		}

		public int GetAreaIDByMapandParent(int mapId, int parentID)
		{
			foreach (KeyValuePair<int, TArea> thisArea in AreaTable)
			{
				int thisMap = thisArea.Value.mapId;
				int thisParent = thisArea.Value.Zone;
				if (thisMap == mapId && thisParent == parentID)
				{
					return thisArea.Key;
				}
			}
			return -999;
		}

		public void InitializeMaps()
		{
			IEnumerator e = WorldServiceLocator._ConfigurationProvider.GetConfiguration().Maps.GetEnumerator();
			e.Reset();
			if (e.MoveNext())
			{
				MapList = Conversions.ToString(e.Current);
				while (e.MoveNext())
				{
					ref string mapList = ref MapList;
					mapList = Conversions.ToString(Operators.AddObject(mapList, Operators.ConcatenateObject(", ", e.Current)));
				}
			}
			foreach (string map2 in WorldServiceLocator._ConfigurationProvider.GetConfiguration().Maps)
			{
				uint id = Conversions.ToUInteger(map2);
				TMap map = new TMap(checked((int)id));
			}
			WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "Initalizing: {0} Maps initialized.", Maps.Count);
		}

		public float ValidateMapCoord(float coord)
		{
			if (coord > 32f * WorldServiceLocator._Global_Constants.SIZE)
			{
				coord = 32f * WorldServiceLocator._Global_Constants.SIZE;
			}
			else if (coord < -32f * WorldServiceLocator._Global_Constants.SIZE)
			{
				coord = -32f * WorldServiceLocator._Global_Constants.SIZE;
			}
			return coord;
		}

		public void GetMapTile(float x, float y, ref byte MapTileX, ref byte MapTileY)
		{
			checked
			{
				MapTileX = (byte)(32f - ValidateMapCoord(x) / WorldServiceLocator._Global_Constants.SIZE);
				MapTileY = (byte)(32f - ValidateMapCoord(y) / WorldServiceLocator._Global_Constants.SIZE);
			}
		}

		public byte GetMapTileX(float x)
		{
			return checked((byte)(32f - ValidateMapCoord(x) / WorldServiceLocator._Global_Constants.SIZE));
		}

		public byte GetMapTileY(float y)
		{
			return checked((byte)(32f - ValidateMapCoord(y) / WorldServiceLocator._Global_Constants.SIZE));
		}

		public byte GetSubMapTileX(float x)
		{
			return checked((byte)((float)RESOLUTION_ZMAP * (32f - ValidateMapCoord(x) / WorldServiceLocator._Global_Constants.SIZE - Conversion.Fix(32f - ValidateMapCoord(x) / WorldServiceLocator._Global_Constants.SIZE))));
		}

		public byte GetSubMapTileY(float y)
		{
			return checked((byte)((float)RESOLUTION_ZMAP * (32f - ValidateMapCoord(y) / WorldServiceLocator._Global_Constants.SIZE - Conversion.Fix(32f - ValidateMapCoord(y) / WorldServiceLocator._Global_Constants.SIZE))));
		}

		public float GetZCoord(float x, float y, uint Map)
		{
			checked
			{
				try
				{
					x = ValidateMapCoord(x);
					y = ValidateMapCoord(y);
					byte MapTileX = (byte)(32f - x / WorldServiceLocator._Global_Constants.SIZE);
					byte MapTileY = (byte)(32f - y / WorldServiceLocator._Global_Constants.SIZE);
					byte MapTile_LocalX = (byte)Math.Round((float)RESOLUTION_ZMAP * (32f - x / WorldServiceLocator._Global_Constants.SIZE - (float)unchecked((int)MapTileX)));
					byte MapTile_LocalY = (byte)Math.Round((float)RESOLUTION_ZMAP * (32f - y / WorldServiceLocator._Global_Constants.SIZE - (float)unchecked((int)MapTileY)));
					float xNormalized;
					float yNormalized;
					unchecked
					{
						xNormalized = (float)RESOLUTION_ZMAP * (32f - x / WorldServiceLocator._Global_Constants.SIZE - (float)(int)MapTileX) - (float)(int)MapTile_LocalX;
						yNormalized = (float)RESOLUTION_ZMAP * (32f - y / WorldServiceLocator._Global_Constants.SIZE - (float)(int)MapTileY) - (float)(int)MapTile_LocalY;
						if (Maps[Map].Tiles[MapTileX, MapTileY] == null)
						{
							return 0f;
						}
					}
					try
					{
						float topHeight = WorldServiceLocator._Functions.MathLerp(GetHeight(Map, MapTileX, MapTileY, MapTile_LocalX, MapTile_LocalY), GetHeight(Map, MapTileX, MapTileY, (byte)(unchecked((int)MapTile_LocalX) + 1), MapTile_LocalY), xNormalized);
						float bottomHeight = WorldServiceLocator._Functions.MathLerp(GetHeight(Map, MapTileX, MapTileY, MapTile_LocalX, (byte)(unchecked((int)MapTile_LocalY) + 1)), GetHeight(Map, MapTileX, MapTileY, (byte)(unchecked((int)MapTile_LocalX) + 1), (byte)(unchecked((int)MapTile_LocalY) + 1)), xNormalized);
						return WorldServiceLocator._Functions.MathLerp(topHeight, bottomHeight, yNormalized);
					}
					catch (Exception projectError)
					{
						ProjectData.SetProjectError(projectError);
						float GetZCoord = Maps[Map].Tiles[MapTileX, MapTileY].ZCoord[MapTile_LocalX, MapTile_LocalY];
						ProjectData.ClearProjectError();
						return GetZCoord;
					}
				}
				catch (Exception ex)
				{
					ProjectData.SetProjectError(ex);
					Exception e = ex;
					float GetZCoord = 0f;
					ProjectData.ClearProjectError();
					return GetZCoord;
				}
			}
		}

		public float GetWaterLevel(float x, float y, int Map)
		{
			x = ValidateMapCoord(x);
			y = ValidateMapCoord(y);
			checked
			{
				byte MapTileX = (byte)(32f - x / WorldServiceLocator._Global_Constants.SIZE);
				byte MapTileY = (byte)(32f - y / WorldServiceLocator._Global_Constants.SIZE);
				byte MapTile_LocalX = (byte)Math.Round((float)WorldServiceLocator._Global_Constants.RESOLUTION_WATER * (32f - x / WorldServiceLocator._Global_Constants.SIZE - (float)unchecked((int)MapTileX)));
				byte MapTile_LocalY = (byte)Math.Round((float)WorldServiceLocator._Global_Constants.RESOLUTION_WATER * (32f - y / WorldServiceLocator._Global_Constants.SIZE - (float)unchecked((int)MapTileY)));
				if (Maps[(uint)Map].Tiles[MapTileX, MapTileY] == null)
				{
					return 0f;
				}
				return Maps[(uint)Map].Tiles[MapTileX, MapTileY].WaterLevel[MapTile_LocalX, MapTile_LocalY];
			}
		}

		public byte GetTerrainType(float x, float y, int Map)
		{
			x = ValidateMapCoord(x);
			y = ValidateMapCoord(y);
			checked
			{
				byte MapTileX = (byte)(32f - x / WorldServiceLocator._Global_Constants.SIZE);
				byte MapTileY = (byte)(32f - y / WorldServiceLocator._Global_Constants.SIZE);
				byte MapTile_LocalX = (byte)Math.Round((float)WorldServiceLocator._Global_Constants.RESOLUTION_TERRAIN * (32f - x / WorldServiceLocator._Global_Constants.SIZE - (float)unchecked((int)MapTileX)));
				byte MapTile_LocalY = (byte)Math.Round((float)WorldServiceLocator._Global_Constants.RESOLUTION_TERRAIN * (32f - y / WorldServiceLocator._Global_Constants.SIZE - (float)unchecked((int)MapTileY)));
				if (Maps[(uint)Map].Tiles[MapTileX, MapTileY] == null)
				{
					return 0;
				}
				return Maps[(uint)Map].Tiles[MapTileX, MapTileY].AreaTerrain[MapTile_LocalX, MapTile_LocalY];
			}
		}

		public int GetAreaFlag(float x, float y, int Map)
		{
			x = ValidateMapCoord(x);
			y = ValidateMapCoord(y);
			checked
			{
				byte MapTileX = (byte)(32f - x / WorldServiceLocator._Global_Constants.SIZE);
				byte MapTileY = (byte)(32f - y / WorldServiceLocator._Global_Constants.SIZE);
				byte MapTile_LocalX = (byte)Math.Round((float)WorldServiceLocator._Global_Constants.RESOLUTION_FLAGS * (32f - x / WorldServiceLocator._Global_Constants.SIZE - (float)unchecked((int)MapTileX)));
				byte MapTile_LocalY = (byte)Math.Round((float)WorldServiceLocator._Global_Constants.RESOLUTION_FLAGS * (32f - y / WorldServiceLocator._Global_Constants.SIZE - (float)unchecked((int)MapTileY)));
				if (Maps[(uint)Map].Tiles[MapTileX, MapTileY] == null)
				{
					return 0;
				}
				return Maps[(uint)Map].Tiles[MapTileX, MapTileY].AreaFlag[MapTile_LocalX, MapTile_LocalY];
			}
		}

		public bool IsOutsideOfMap(ref WS_Base.BaseObject objCharacter)
		{
			return false;
		}

		public float GetZCoord(float x, float y, float z, uint Map)
		{
			checked
			{
				try
				{
					x = ValidateMapCoord(x);
					y = ValidateMapCoord(y);
					z = ValidateMapCoord(z);
					byte MapTileX = (byte)(32f - x / WorldServiceLocator._Global_Constants.SIZE);
					byte MapTileY = (byte)(32f - y / WorldServiceLocator._Global_Constants.SIZE);
					byte MapTile_LocalX = (byte)Math.Round((float)RESOLUTION_ZMAP * (32f - x / WorldServiceLocator._Global_Constants.SIZE - (float)unchecked((int)MapTileX)));
					byte MapTile_LocalY = (byte)Math.Round((float)RESOLUTION_ZMAP * (32f - y / WorldServiceLocator._Global_Constants.SIZE - (float)unchecked((int)MapTileY)));
					float xNormalized;
					float yNormalized;
					unchecked
					{
						xNormalized = (float)RESOLUTION_ZMAP * (32f - x / WorldServiceLocator._Global_Constants.SIZE - (float)(int)MapTileX) - (float)(int)MapTile_LocalX;
						yNormalized = (float)RESOLUTION_ZMAP * (32f - y / WorldServiceLocator._Global_Constants.SIZE - (float)(int)MapTileY) - (float)(int)MapTile_LocalY;
						if (Maps[Map].Tiles[MapTileX, MapTileY] == null)
						{
							float VMapHeight2 = GetVMapHeight(Map, x, y, z + 5f);
							if (VMapHeight2 != WorldServiceLocator._Global_Constants.VMAP_INVALID_HEIGHT_VALUE)
							{
								return VMapHeight2;
							}
							return 0f;
						}
						if (Math.Abs(Maps[Map].Tiles[MapTileX, MapTileY].ZCoord[MapTile_LocalX, MapTile_LocalY] - z) >= 2f)
						{
							float VMapHeight = GetVMapHeight(Map, x, y, z + 5f);
							if (VMapHeight != WorldServiceLocator._Global_Constants.VMAP_INVALID_HEIGHT_VALUE)
							{
								return VMapHeight;
							}
						}
					}
					try
					{
						float topHeight = WorldServiceLocator._Functions.MathLerp(GetHeight(Map, MapTileX, MapTileY, MapTile_LocalX, MapTile_LocalY), GetHeight(Map, MapTileX, MapTileY, (byte)(unchecked((int)MapTile_LocalX) + 1), MapTile_LocalY), xNormalized);
						float bottomHeight = WorldServiceLocator._Functions.MathLerp(GetHeight(Map, MapTileX, MapTileY, MapTile_LocalX, (byte)(unchecked((int)MapTile_LocalY) + 1)), GetHeight(Map, MapTileX, MapTileY, (byte)(unchecked((int)MapTile_LocalX) + 1), (byte)(unchecked((int)MapTile_LocalY) + 1)), xNormalized);
						return WorldServiceLocator._Functions.MathLerp(topHeight, bottomHeight, yNormalized);
					}
					catch (Exception projectError)
					{
						ProjectData.SetProjectError(projectError);
						float GetZCoord = Maps[Map].Tiles[MapTileX, MapTileY].ZCoord[MapTile_LocalX, MapTile_LocalY];
						ProjectData.ClearProjectError();
						return GetZCoord;
					}
				}
				catch (Exception ex2)
				{
					ProjectData.SetProjectError(ex2);
					Exception ex = ex2;
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, ex.ToString());
					float GetZCoord = z;
					ProjectData.ClearProjectError();
					return GetZCoord;
				}
			}
		}

		private float GetHeight(uint Map, byte MapTileX, byte MapTileY, byte MapTileLocalX, byte MapTileLocalY)
		{
			checked
			{
				if (MapTileLocalX > RESOLUTION_ZMAP)
				{
					MapTileX = (byte)(unchecked((int)MapTileX) + 1);
					MapTileLocalX = (byte)(unchecked((int)MapTileLocalX) - (RESOLUTION_ZMAP + 1));
				}
				else if (MapTileLocalX < 0)
				{
					MapTileX = (byte)(unchecked((int)MapTileX) - 1);
					MapTileLocalX = (byte)((short)unchecked(-MapTileLocalX) - 1);
				}
				if (MapTileLocalY > RESOLUTION_ZMAP)
				{
					MapTileY = (byte)(unchecked((int)MapTileY) + 1);
					MapTileLocalY = (byte)(unchecked((int)MapTileLocalY) - (RESOLUTION_ZMAP + 1));
				}
				else if (MapTileLocalY < 0)
				{
					MapTileY = (byte)(unchecked((int)MapTileY) - 1);
					MapTileLocalY = (byte)((short)unchecked(-MapTileLocalY) - 1);
				}
				return Maps[Map].Tiles[MapTileX, MapTileY].ZCoord[MapTileLocalX, MapTileLocalY];
			}
		}

		public bool IsInLineOfSight(ref WS_Base.BaseObject obj, ref WS_Base.BaseObject obj2)
		{
			return IsInLineOfSight(obj.MapID, obj.positionX, obj.positionY, obj.positionZ + 2f, obj2.positionX, obj2.positionY, obj2.positionZ + 2f);
		}

		public bool IsInLineOfSight(ref WS_Base.BaseObject obj, float x2, float y2, float z2)
		{
			x2 = ValidateMapCoord(x2);
			y2 = ValidateMapCoord(y2);
			z2 = ValidateMapCoord(z2);
			return IsInLineOfSight(obj.MapID, obj.positionX, obj.positionY, obj.positionZ + 2f, x2, y2, z2);
		}

		public bool IsInLineOfSight(uint MapID, float x1, float y1, float z1, float x2, float y2, float z2)
		{
			x1 = ValidateMapCoord(x1);
			y1 = ValidateMapCoord(y1);
			z1 = ValidateMapCoord(z1);
			x2 = ValidateMapCoord(x2);
			y2 = ValidateMapCoord(y2);
			z2 = ValidateMapCoord(z2);
			return true;
		}

		public float GetVMapHeight(uint MapID, float x, float y, float z)
		{
			x = ValidateMapCoord(x);
			y = ValidateMapCoord(y);
			z = ValidateMapCoord(z);
			return WorldServiceLocator._Global_Constants.VMAP_INVALID_HEIGHT_VALUE;
		}

		public bool GetObjectHitPos(ref WS_Base.BaseObject obj, ref WS_Base.BaseObject obj2, ref float rx, ref float ry, ref float rz, float pModifyDist)
		{
			rx = ValidateMapCoord(rx);
			ry = ValidateMapCoord(ry);
			rz = ValidateMapCoord(rz);
			return GetObjectHitPos(obj.MapID, obj.positionX, obj.positionY, obj.positionZ + 2f, obj2.positionX, obj2.positionY, obj2.positionZ + 2f, ref rx, ref ry, ref rz, pModifyDist);
		}

		public bool GetObjectHitPos(ref WS_Base.BaseObject obj, float x2, float y2, float z2, ref float rx, ref float ry, ref float rz, float pModifyDist)
		{
			rx = ValidateMapCoord(rx);
			ry = ValidateMapCoord(ry);
			rz = ValidateMapCoord(rz);
			x2 = ValidateMapCoord(x2);
			y2 = ValidateMapCoord(y2);
			z2 = ValidateMapCoord(z2);
			return GetObjectHitPos(obj.MapID, obj.positionX, obj.positionY, obj.positionZ + 2f, x2, y2, z2, ref rx, ref ry, ref rz, pModifyDist);
		}

		public bool GetObjectHitPos(uint MapID, float x1, float y1, float z1, float x2, float y2, float z2, ref float rx, ref float ry, ref float rz, float pModifyDist)
		{
			x1 = ValidateMapCoord(x1);
			y1 = ValidateMapCoord(y1);
			z1 = ValidateMapCoord(z1);
			x2 = ValidateMapCoord(x2);
			y2 = ValidateMapCoord(y2);
			z2 = ValidateMapCoord(z2);
			return false;
		}

		public void LoadSpawns(byte TileX, byte TileY, uint TileMap, uint TileInstance)
		{
			checked
			{
				float MinX = (float)(32 - unchecked((int)TileX)) * WorldServiceLocator._Global_Constants.SIZE;
				float MaxX = (float)(32 - (unchecked((int)TileX) + 1)) * WorldServiceLocator._Global_Constants.SIZE;
				float MinY = (float)(32 - unchecked((int)TileY)) * WorldServiceLocator._Global_Constants.SIZE;
				float MaxY = (float)(32 - (unchecked((int)TileY) + 1)) * WorldServiceLocator._Global_Constants.SIZE;
				if (MinX > MaxX)
				{
					float tmpSng2 = MinX;
					MinX = MaxX;
					MaxX = tmpSng2;
				}
				if (MinY > MaxY)
				{
					float tmpSng = MinY;
					MinY = MaxY;
					MaxY = tmpSng;
				}
				ulong InstanceGuidAdd = 0uL;
				if (unchecked((long)TileInstance) > 0L)
				{
					InstanceGuidAdd = Convert.ToUInt64(decimal.Add(new decimal(1000000L), decimal.Multiply(new decimal(unchecked((long)TileInstance) - 1L), new decimal(100000L))));
				}
				DataTable MysqlQuery = new DataTable();
				WorldServiceLocator._WorldServer.WorldDatabase.Query($"SELECT * FROM creature LEFT OUTER JOIN game_event_creature ON creature.guid = game_event_creature.guid WHERE map={TileMap} AND position_X BETWEEN '{MinX}' AND '{MaxX}' AND position_Y BETWEEN '{MinY}' AND '{MaxY}';", ref MysqlQuery);
				IEnumerator enumerator = default(IEnumerator);
				try
				{
					enumerator = MysqlQuery.Rows.GetEnumerator();
					while (enumerator.MoveNext())
					{
						DataRow row = (DataRow)enumerator.Current;
						if (WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(Convert.ToUInt64(decimal.Add(decimal.Add(new decimal(row.As<long>("guid")), new decimal(InstanceGuidAdd)), new decimal(WorldServiceLocator._Global_Constants.GUID_UNIT)))))
						{
							continue;
						}
						try
						{
							WS_Creatures.CreatureObject tmpCr = new WS_Creatures.CreatureObject(Convert.ToUInt64(decimal.Add(new decimal(row.As<long>("guid")), new decimal(InstanceGuidAdd))), row);
							if (tmpCr.GameEvent == 0)
							{
								tmpCr.instance = TileInstance;
								tmpCr.AddToWorld();
							}
						}
						catch (Exception ex5)
						{
							ProjectData.SetProjectError(ex5);
							Exception ex4 = ex5;
							WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "Error when creating creature [{0}].{1}{2}", row["id"], Environment.NewLine, ex4.ToString());
							ProjectData.ClearProjectError();
						}
					}
				}
				finally
				{
					if (enumerator is IDisposable)
					{
						(enumerator as IDisposable).Dispose();
					}
				}
				MysqlQuery.Clear();
				WorldServiceLocator._WorldServer.WorldDatabase.Query($"SELECT * FROM gameobject LEFT OUTER JOIN game_event_gameobject ON gameobject.guid = game_event_gameobject.guid WHERE map={TileMap} AND spawntimesecs>=0 AND position_X BETWEEN '{MinX}' AND '{MaxX}' AND position_Y BETWEEN '{MinY}' AND '{MaxY}';", ref MysqlQuery);
				IEnumerator enumerator2 = default(IEnumerator);
				try
				{
					enumerator2 = MysqlQuery.Rows.GetEnumerator();
					while (enumerator2.MoveNext())
					{
						DataRow row = (DataRow)enumerator2.Current;
						if (WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs.ContainsKey(row.As<ulong>("guid") + InstanceGuidAdd + WorldServiceLocator._Global_Constants.GUID_GAMEOBJECT) || WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs.ContainsKey(row.As<ulong>("guid") + InstanceGuidAdd + WorldServiceLocator._Global_Constants.GUID_TRANSPORT))
						{
							continue;
						}
						try
						{
							WS_GameObjects.GameObjectObject tmpGo = new WS_GameObjects.GameObjectObject(row.As<ulong>("guid") + InstanceGuidAdd, row);
							if (tmpGo.GameEvent == 0)
							{
								tmpGo.instance = TileInstance;
								tmpGo.AddToWorld();
							}
						}
						catch (Exception ex6)
						{
							ProjectData.SetProjectError(ex6);
							Exception ex3 = ex6;
							WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "Error when creating gameobject [{0}].{1}{2}", row["id"], Environment.NewLine, ex3.ToString());
							ProjectData.ClearProjectError();
						}
					}
				}
				finally
				{
					if (enumerator2 is IDisposable)
					{
						(enumerator2 as IDisposable).Dispose();
					}
				}
				MysqlQuery.Clear();
				WorldServiceLocator._WorldServer.CharacterDatabase.Query(string.Format("SELECT * FROM corpse WHERE map={0} AND instance={5} AND position_x BETWEEN '{1}' AND '{2}' AND position_y BETWEEN '{3}' AND '{4}';", TileMap, MinX, MaxX, MinY, MaxY, TileInstance), ref MysqlQuery);
				IEnumerator enumerator3 = default(IEnumerator);
				try
				{
					enumerator3 = MysqlQuery.Rows.GetEnumerator();
					while (enumerator3.MoveNext())
					{
						DataRow InfoRow = (DataRow)enumerator3.Current;
						if (!WorldServiceLocator._WorldServer.WORLD_CORPSEOBJECTs.ContainsKey(Conversions.ToULong(InfoRow["guid"]) + WorldServiceLocator._Global_Constants.GUID_CORPSE))
						{
							try
							{
								WS_Corpses.CorpseObject tmpCorpse = new WS_Corpses.CorpseObject(Conversions.ToULong(InfoRow["guid"]), InfoRow)
								{
									instance = TileInstance
								};
								tmpCorpse.AddToWorld();
							}
							catch (Exception ex7)
							{
								ProjectData.SetProjectError(ex7);
								Exception ex2 = ex7;
								WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "Error when creating corpse [{0}].{1}{2}", InfoRow["guid"], Environment.NewLine, ex2.ToString());
								ProjectData.ClearProjectError();
							}
						}
					}
				}
				finally
				{
					if (enumerator3 is IDisposable)
					{
						(enumerator3 as IDisposable).Dispose();
					}
				}
				try
				{
					WorldServiceLocator._WorldServer.WORLD_TRANSPORTs_Lock.AcquireReaderLock(1000);
					foreach (KeyValuePair<ulong, WS_Transports.TransportObject> Transport in WorldServiceLocator._WorldServer.WORLD_TRANSPORTs)
					{
						try
						{
							if (Transport.Value.MapID == TileMap && Transport.Value.positionX >= MinX && Transport.Value.positionX <= MaxX && Transport.Value.positionY >= MinY && Transport.Value.positionY <= MaxY)
							{
								if (!Maps[TileMap].Tiles[TileX, TileY].GameObjectsHere.Contains(Transport.Value.GUID))
								{
									Maps[TileMap].Tiles[TileX, TileY].GameObjectsHere.Add(Transport.Value.GUID);
								}
								Transport.Value.NotifyEnter();
							}
						}
						catch (Exception ex8)
						{
							ProjectData.SetProjectError(ex8);
							Exception ex = ex8;
							WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "Error when creating transport [{0}].{1}{2}", Transport.Key - WorldServiceLocator._Global_Constants.GUID_MO_TRANSPORT, Environment.NewLine, ex.ToString());
							ProjectData.ClearProjectError();
						}
					}
				}
				catch (Exception projectError)
				{
					ProjectData.SetProjectError(projectError);
					ProjectData.ClearProjectError();
				}
				finally
				{
					WorldServiceLocator._WorldServer.WORLD_TRANSPORTs_Lock.ReleaseReaderLock();
				}
			}
		}

		public void UnloadSpawns(byte TileX, byte TileY, uint TileMap)
		{
			checked
			{
				float MinX = (float)(32 - unchecked((int)TileX)) * WorldServiceLocator._Global_Constants.SIZE;
				float MaxX = (float)(32 - (unchecked((int)TileX) + 1)) * WorldServiceLocator._Global_Constants.SIZE;
				float MinY = (float)(32 - unchecked((int)TileY)) * WorldServiceLocator._Global_Constants.SIZE;
				float MaxY = (float)(32 - (unchecked((int)TileY) + 1)) * WorldServiceLocator._Global_Constants.SIZE;
				if (MinX > MaxX)
				{
					float tmpSng2 = MinX;
					MinX = MaxX;
					MaxX = tmpSng2;
				}
				if (MinY > MaxY)
				{
					float tmpSng = MinY;
					MinY = MaxY;
					MaxY = tmpSng;
				}
				try
				{
					WorldServiceLocator._WorldServer.WORLD_CREATUREs_Lock.AcquireReaderLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
					foreach (KeyValuePair<ulong, WS_Creatures.CreatureObject> Creature in WorldServiceLocator._WorldServer.WORLD_CREATUREs)
					{
						if (Creature.Value.MapID == TileMap && Creature.Value.SpawnX >= MinX && Creature.Value.SpawnX <= MaxX && Creature.Value.SpawnY >= MinY && Creature.Value.SpawnY <= MaxY)
						{
							Creature.Value.Destroy();
						}
					}
				}
				catch (Exception ex2)
				{
					ProjectData.SetProjectError(ex2);
					Exception ex = ex2;
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, ex.ToString(), null);
					ProjectData.ClearProjectError();
				}
				finally
				{
					WorldServiceLocator._WorldServer.WORLD_CREATUREs_Lock.ReleaseReaderLock();
				}
				foreach (KeyValuePair<ulong, WS_GameObjects.GameObjectObject> Gameobject in WorldServiceLocator._WorldServer.WORLD_GAMEOBJECTs)
				{
					if (Gameobject.Value.MapID == TileMap && Gameobject.Value.positionX >= MinX && Gameobject.Value.positionX <= MaxX && Gameobject.Value.positionY >= MinY && Gameobject.Value.positionY <= MaxY)
					{
						Gameobject.Value.Destroy(Gameobject);
					}
				}
				foreach (KeyValuePair<ulong, WS_Corpses.CorpseObject> Corpseobject in WorldServiceLocator._WorldServer.WORLD_CORPSEOBJECTs)
				{
					if (Corpseobject.Value.MapID == TileMap && Corpseobject.Value.positionX >= MinX && Corpseobject.Value.positionX <= MaxX && Corpseobject.Value.positionY >= MinY && Corpseobject.Value.positionY <= MaxY)
					{
						Corpseobject.Value.Destroy();
					}
				}
			}
		}

		public void SendTransferAborted(ref WS_Network.ClientClass client, int Map, TransferAbortReason Reason)
		{
			WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_TRANSFER_ABORTED [{2}:{3}]", client.IP, client.Port, Map, Reason);
			Packets.PacketClass p = new Packets.PacketClass(OPCODES.SMSG_TRANSFER_ABORTED);
			try
			{
				p.AddInt32(Map);
				p.AddInt16((short)Reason);
				client.Send(ref p);
			}
			finally
			{
				p.Dispose();
			}
		}
	}
}
