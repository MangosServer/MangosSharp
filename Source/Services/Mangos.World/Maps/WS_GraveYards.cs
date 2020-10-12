using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Mangos.Common.DataStores;
using Mangos.Common.Enums.Global;
using Mangos.World.Player;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.World.Maps
{
	public class WS_GraveYards : IDisposable
	{
		public struct TGraveyard
		{
			private float _locationPosX;

			private float _locationPosY;

			private float _locationPosZ;

			private int _locationMapID;

			public int X
			{
				get
				{
					return checked((int)Math.Round(_locationPosX));
				}
				set
				{
					_locationPosX = X;
				}
			}

			public int Y
			{
				get
				{
					return checked((int)Math.Round(_locationPosY));
				}
				set
				{
					_locationPosY = Y;
				}
			}

			public int Z
			{
				get
				{
					return checked((int)Math.Round(_locationPosZ));
				}
				set
				{
					_locationPosZ = Z;
				}
			}

			public int Map
			{
				get
				{
					return _locationMapID;
				}
				set
				{
					_locationMapID = Map;
				}
			}

			public TGraveyard(float locationPosX, float locationPosY, float locationPosZ, int locationMapID)
			{
				this = default(TGraveyard);
				_locationPosX = locationPosX;
				_locationPosY = locationPosY;
				_locationPosZ = locationPosZ;
				_locationMapID = locationMapID;
			}
		}

		public Dictionary<int, TGraveyard> Graveyards;

		private bool _disposedValue;

		public WS_GraveYards()
		{
			Graveyards = new Dictionary<int, TGraveyard>();
		}

		public void AddGraveYard(int ID, float locationPosX, float locationPosY, float locationPosZ, int locationMapID)
		{
			Graveyards.Add(ID, new TGraveyard(locationPosX, locationPosY, locationPosZ, locationMapID));
		}

		public TGraveyard GetGraveYard(int ID)
		{
			TGraveyard ret = default(TGraveyard);
			return Graveyards[ID];
		}

		public void InitializeGraveyards()
		{
			checked
			{
				try
				{
					Graveyards.Clear();
					BufferedDbc tmpDBC = new BufferedDbc("dbc" + Conversions.ToString(Path.DirectorySeparatorChar) + "WorldSafeLocs.dbc");
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "Loading.... {0} Graveyard Locations", tmpDBC.Rows - 1);
					int num = tmpDBC.Rows - 1;
					for (int i = 0; i <= num; i++)
					{
						int locationIndex = Conversions.ToInteger(tmpDBC[i, 0, DBCValueType.DBC_INTEGER]);
						int locationMapID = Conversions.ToInteger(tmpDBC[i, 1, DBCValueType.DBC_INTEGER]);
						float locationPosX = Conversions.ToSingle(tmpDBC[i, 2, DBCValueType.DBC_FLOAT]);
						float locationPosY = Conversions.ToSingle(tmpDBC[i, 3, DBCValueType.DBC_FLOAT]);
						float locationPosZ = Conversions.ToSingle(tmpDBC[i, 4, DBCValueType.DBC_FLOAT]);
						if (WorldServiceLocator._ConfigurationProvider.GetConfiguration().Maps.Contains(locationMapID.ToString()))
						{
							Graveyards.Add(locationIndex, new TGraveyard(locationPosX, locationPosY, locationPosZ, locationMapID));
							WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "         : Map: {0}  X: {1}  Y: {2}  Z: {3}", locationMapID, locationPosX, locationPosY, locationPosZ);
						}
					}
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "Finished loading Graveyard Locations", tmpDBC.Rows - 1);
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} Graveyards initialized.", tmpDBC.Rows - 1);
					tmpDBC.Dispose();
				}
				catch (DirectoryNotFoundException ex)
				{
					ProjectData.SetProjectError(ex);
					DirectoryNotFoundException e = ex;
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("DBC File : WorldSafeLocs missing.");
					Console.ForegroundColor = ConsoleColor.Gray;
					ProjectData.ClearProjectError();
				}
			}
		}

		public void GoToNearestGraveyard(ref WS_PlayerData.CharacterObject Character, bool Alive, bool Teleport)
		{
			DataTable GraveQuery = new DataTable();
			bool foundNear = false;
			float distNear = 0f;
			TGraveyard entryNear = default(TGraveyard);
			TGraveyard entryFar = default(TGraveyard);
			checked
			{
				if (WorldServiceLocator._WS_Maps.Maps[Character.MapID].IsDungeon | WorldServiceLocator._WS_Maps.Maps[Character.MapID].IsBattleGround | WorldServiceLocator._WS_Maps.Maps[Character.MapID].IsRaid)
				{
					Character.ZoneCheckInstance();
					int Ghostzone = WorldServiceLocator._WS_Maps.AreaTable[WorldServiceLocator._WS_Maps.GetAreaIDByMapandParent((int)Character.MapID, WorldServiceLocator._WS_Maps.AreaTable[WorldServiceLocator._WS_Maps.GetAreaFlag(Character.resurrectPositionX, Character.resurrectPositionY, (int)Character.MapID)].Zone)].ID;
					WorldServiceLocator._WorldServer.WorldDatabase.Query($"SELECT id, faction FROM game_graveyard_zone WHERE ghost_zone = {Ghostzone} and (faction = 0 or faction = {Character.Team}) ", ref GraveQuery);
					if (GraveQuery.Rows.Count == 0)
					{
						WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "GraveYards: No near graveyards for map [{0}], zone [{1}]", Character.MapID, Character.ZoneID);
						return;
					}
					if (WorldServiceLocator._WS_Maps.Maps[Character.MapID].IsDungeon | WorldServiceLocator._WS_Maps.Maps[Character.MapID].IsBattleGround | WorldServiceLocator._WS_Maps.Maps[Character.MapID].IsRaid)
					{
						if (Graveyards.ContainsKey(Conversions.ToInteger(GraveQuery.Rows[0]["id"])))
						{
							entryFar = Graveyards[Conversions.ToInteger(GraveQuery.Rows[0]["id"])];
							entryNear = entryFar;
						}
						else
						{
							WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "GraveYard: {0} is missing for map [{1}], zone [{2}]", GraveQuery.Rows[0]["id"], Character.MapID, Character.ZoneID);
						}
					}
					else
					{
						IEnumerator enumerator = default(IEnumerator);
						try
						{
							enumerator = GraveQuery.Rows.GetEnumerator();
							while (enumerator.MoveNext())
							{
								DataRow GraveLink2 = (DataRow)enumerator.Current;
								int GraveyardID2 = Conversions.ToInteger(GraveLink2["id"]);
								int GraveyardFaction2 = Conversions.ToInteger(GraveLink2["faction"]);
								if (!Graveyards.ContainsKey(GraveyardID2))
								{
									WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "GraveYards: Graveyard link invalid [{0}]", GraveyardID2);
								}
								else if (Character.MapID != Graveyards[GraveyardID2].Map)
								{
									if (Information.IsNothing(entryFar))
									{
										entryFar = Graveyards[GraveyardID2];
									}
								}
								else
								{
									if (GraveyardFaction2 != 0 && GraveyardFaction2 != Character.Team)
									{
										continue;
									}
									float dist3 = WorldServiceLocator._WS_Combat.GetDistance(Character.positionX, Graveyards[GraveyardID2].X, Character.positionY, Graveyards[GraveyardID2].Y, Character.positionZ, Graveyards[GraveyardID2].Z);
									if (foundNear)
									{
										if (dist3 < distNear)
										{
											distNear = dist3;
											entryNear = Graveyards[GraveyardID2];
										}
									}
									else
									{
										foundNear = true;
										distNear = dist3;
										entryNear = Graveyards[GraveyardID2];
									}
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
					}
					TGraveyard selectedGraveyard2 = entryNear;
					if (Information.IsNothing(selectedGraveyard2))
					{
						selectedGraveyard2 = entryFar;
					}
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "GraveYards: GraveYard.Map[{0}], GraveYard.X[{1}], GraveYard.Y[{2}], GraveYard.Z[{3}]", selectedGraveyard2.Map, selectedGraveyard2.X, selectedGraveyard2.Y, selectedGraveyard2.Z);
					Character.Teleport(selectedGraveyard2.X, selectedGraveyard2.Y, selectedGraveyard2.Z, 0f, selectedGraveyard2.Map);
					Character.SendDeathReleaseLoc(selectedGraveyard2.X, selectedGraveyard2.Y, selectedGraveyard2.Z, selectedGraveyard2.Map);
					return;
				}
				Character.ZoneCheck();
				WorldServiceLocator._WorldServer.WorldDatabase.Query($"SELECT id, faction FROM game_graveyard_zone WHERE ghost_zone = {Character.ZoneID}", ref GraveQuery);
				if (GraveQuery.Rows.Count == 0)
				{
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "GraveYards: No near graveyards for map [{0}], zone [{1}]", Character.MapID, Character.ZoneID);
					return;
				}
				IEnumerator enumerator2 = default(IEnumerator);
				try
				{
					enumerator2 = GraveQuery.Rows.GetEnumerator();
					while (enumerator2.MoveNext())
					{
						DataRow GraveLink = (DataRow)enumerator2.Current;
						int GraveyardID = Conversions.ToInteger(GraveLink["id"]);
						int GraveyardFaction = Conversions.ToInteger(GraveLink["faction"]);
						if (!Graveyards.ContainsKey(GraveyardID))
						{
							WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "GraveYards: Graveyard link invalid [{0}]", GraveyardID);
						}
						else if (Character.MapID != Graveyards[GraveyardID].Map)
						{
							if (Information.IsNothing(entryFar))
							{
								entryFar = Graveyards[GraveyardID];
							}
						}
						else
						{
							if (GraveyardFaction != 0 && GraveyardFaction != Character.Team)
							{
								continue;
							}
							float dist2 = WorldServiceLocator._WS_Combat.GetDistance(Character.positionX, Graveyards[GraveyardID].X, Character.positionY, Graveyards[GraveyardID].Y, Character.positionZ, Graveyards[GraveyardID].Z);
							if (foundNear)
							{
								if (dist2 < distNear)
								{
									distNear = dist2;
									entryNear = Graveyards[GraveyardID];
								}
							}
							else
							{
								foundNear = true;
								distNear = dist2;
								entryNear = Graveyards[GraveyardID];
							}
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
				TGraveyard selectedGraveyard = entryNear;
				if (Information.IsNothing(selectedGraveyard))
				{
					selectedGraveyard = entryFar;
				}
				if (Teleport)
				{
					if (Alive & Character.DEAD)
					{
						WorldServiceLocator._WS_Handlers_Misc.CharacterResurrect(ref Character);
						Character.Life.Current = Character.Life.Maximum;
						if (Character.ManaType == ManaTypes.TYPE_MANA)
						{
							Character.Mana.Current = Character.Mana.Maximum;
						}
						if (selectedGraveyard.Map == Character.MapID)
						{
							Character.SetUpdateFlag(22, Character.Life.Current);
							if (Character.ManaType == ManaTypes.TYPE_MANA)
							{
								Character.SetUpdateFlag(23, Character.Mana.Current);
							}
							Character.SendCharacterUpdate();
						}
					}
					WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "GraveYards: GraveYard.Map[{0}], GraveYard.X[{1}], GraveYard.Y[{2}], GraveYard.Z[{3}]", selectedGraveyard.Map, selectedGraveyard.X, selectedGraveyard.Y, selectedGraveyard.Z);
					Character.Teleport(selectedGraveyard.X, selectedGraveyard.Y, selectedGraveyard.Z, 0f, selectedGraveyard.Map);
					Character.SendDeathReleaseLoc(selectedGraveyard.X, selectedGraveyard.Y, selectedGraveyard.Z, selectedGraveyard.Map);
				}
				else
				{
					Character.positionX = selectedGraveyard.X;
					Character.positionY = selectedGraveyard.Y;
					Character.positionZ = selectedGraveyard.Z;
					Character.MapID = (uint)selectedGraveyard.Map;
				}
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposedValue || disposing)
			{
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
}
