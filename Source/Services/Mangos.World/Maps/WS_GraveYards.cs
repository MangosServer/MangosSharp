// 
// Copyright (C) 2013-2020 getMaNGOS <https://getmangos.eu>
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

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Mangos.Common.DataStores;
using Mangos.Common.Enums.Global;
using Mangos.Common.Globals;
using Mangos.World.Player;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.World.Maps
{
    public class WS_GraveYards : IDisposable
    {
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public Dictionary<int, TGraveyard> Graveyards = new Dictionary<int, TGraveyard>();

        public struct TGraveyard
        {
            // Dim x As Single
            // Dim y As Single
            // Dim z As Single
            // Dim MapID As Integer
            private float _locationPosX;
            private float _locationPosY;
            private float _locationPosZ;
            private int _locationMapID;

            public TGraveyard(float locationPosX, float locationPosY, float locationPosZ, int locationMapID)
            {
                // TODO: Complete member initialization
                _locationPosX = locationPosX;
                _locationPosY = locationPosY;
                _locationPosZ = locationPosZ;
                _locationMapID = locationMapID;
            }

            /// <summary>
            /// Gets or sets the X Coord.
            /// </summary>
            /// <value>The X Coord.</value>
            public int X
            {
                get
                {
                    return (int)_locationPosX;
                }

                set
                {
                    _locationPosX = X;
                }
            }

            /// <summary>
            /// Gets or sets the Y Coord.
            /// </summary>
            /// <value>The Y Coord.</value>
            public int Y
            {
                get
                {
                    return (int)_locationPosY;
                }

                set
                {
                    _locationPosY = Y;
                }
            }

            /// <summary>
            /// Gets or sets the Z Coord.
            /// </summary>
            /// <value>The Z Coord.</value>
            public int Z
            {
                get
                {
                    return (int)_locationPosZ;
                }

                set
                {
                    _locationPosZ = Z;
                }
            }

            /// <summary>
            /// Gets or sets the Map ID.
            /// </summary>
            /// <value>The Map ID.</value>
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
        }

        /// <summary>
        /// Adds the coords.
        /// </summary>
        /// <param name="ID">The ID.</param>
        /// <param name="locationPosX">The location pos X.</param>
        /// <param name="locationPosY">The location pos Y.</param>
        /// <param name="locationPosZ">The location pos Z.</param>
        /// <param name="locationMapID">The location map ID.</param>
        public void AddGraveYard(int ID, float locationPosX, float locationPosY, float locationPosZ, int locationMapID)
        {
            // TODO: Complete member initialization
            Graveyards.Add(ID, new TGraveyard(locationPosX, locationPosY, locationPosZ, locationMapID));
        }

        /// <summary>
        /// Gets the coords.
        /// </summary>
        /// <param name="ID">The ID.</param>
        /// <returns>a <c>classCoords</c> structure</returns>
        public TGraveyard GetGraveYard(int ID)
        {
            var ret = new TGraveyard();
            ret = Graveyards[ID];
            return ret;
        }

        // Public Sub New(ByVal px As Single, ByVal py As Single, ByVal pz As Single, ByVal pMap As Integer)
        // x = px
        // y = py
        // z = pz
        // Map = pMap
        // End Sub

        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public void InitializeGraveyards()
        {
            try
            {
                Graveyards.Clear();
                var tmpDBC = new BufferedDbc("dbc" + Path.DirectorySeparatorChar + "WorldSafeLocs.dbc");
                float locationPosX;
                float locationPosY;
                float locationPosZ;
                int locationMapID;
                int locationIndex;
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "Loading.... {0} Graveyard Locations", tmpDBC.Rows - 1);
                for (int i = 0, loopTo = tmpDBC.Rows - 1; i <= loopTo; i++)
                {
                    locationIndex = (int)tmpDBC.Item(i, 0);
                    locationMapID = (int)tmpDBC.Item(i, 1);
                    locationPosX = (float)tmpDBC.Item(i, 2, DBCValueType.DBC_FLOAT);
                    locationPosY = (float)tmpDBC.Item(i, 3, DBCValueType.DBC_FLOAT);
                    locationPosZ = (float)tmpDBC.Item(i, 4, DBCValueType.DBC_FLOAT);
                    if (WorldServiceLocator._WorldServer.Config.Maps.Contains(locationMapID.ToString()))
                    {
                        Graveyards.Add(locationIndex, new TGraveyard(locationPosX, locationPosY, locationPosZ, locationMapID));
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "         : Map: {0}  X: {1}  Y: {2}  Z: {3}", locationMapID, locationPosX, locationPosY, locationPosZ);
                    }
                }

                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "Finished loading Graveyard Locations", tmpDBC.Rows - 1);
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "DBC: {0} Graveyards initialized.", tmpDBC.Rows - 1);
                tmpDBC.Dispose();
            }
            catch (DirectoryNotFoundException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("DBC File : WorldSafeLocs missing.");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }
        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        public void GoToNearestGraveyard(ref WS_PlayerData.CharacterObject Character, bool Alive, bool Teleport)
        {
            var GraveQuery = new DataTable();
            int Ghostzone;
            bool foundNear = false;
            float distNear = 0.0f;
            TGraveyard entryNear = default;
            TGraveyard entryFar = default;

            // Death in an instance ?
            if (WorldServiceLocator._WS_Maps.Maps[Character.MapID].IsDungeon == true | WorldServiceLocator._WS_Maps.Maps[Character.MapID].IsBattleGround == true | WorldServiceLocator._WS_Maps.Maps[Character.MapID].IsRaid == true)   // In an instance
            {
                Character.ZoneCheckInstance();
                Ghostzone = WorldServiceLocator._WS_Maps.AreaTable[WorldServiceLocator._WS_Maps.GetAreaIDByMapandParent((int)Character.MapID, WorldServiceLocator._WS_Maps.AreaTable[WorldServiceLocator._WS_Maps.GetAreaFlag(Character.resurrectPositionX, Character.resurrectPositionY, (int)Character.MapID)].Zone)].ID;
                WorldServiceLocator._WorldServer.WorldDatabase.Query(string.Format("SELECT id, faction FROM game_graveyard_zone WHERE ghost_zone = {0} and (faction = 0 or faction = {1}) ", (object)Ghostzone, (object)Character.Team), ref GraveQuery);

                // AreaTable(GetAreaFlag(Character.resurrectPositionX, Character.resurrectPositionY, Character.MapID)).Zone()
                if (GraveQuery.Rows.Count == 0)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "GraveYards: No near graveyards for map [{0}], zone [{1}]", Character.MapID, Character.ZoneID);
                    return;
                }

                if (WorldServiceLocator._WS_Maps.Maps[Character.MapID].IsDungeon == true | WorldServiceLocator._WS_Maps.Maps[Character.MapID].IsBattleGround == true | WorldServiceLocator._WS_Maps.Maps[Character.MapID].IsRaid == true)   // In an instance
                {
                    if (Graveyards.ContainsKey(Conversions.ToInteger(GraveQuery.Rows[0]["id"])) == true)
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
                    foreach (DataRow GraveLink in GraveQuery.Rows)
                    {
                        int GraveyardID = Conversions.ToInteger(GraveLink["id"]);
                        int GraveyardFaction = Conversions.ToInteger(GraveLink["faction"]);
                        if (Graveyards.ContainsKey(GraveyardID) == false)
                        {
                            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "GraveYards: Graveyard link invalid [{0}]", GraveyardID);
                            continue;
                        }

                        if (Character.MapID != Graveyards[GraveyardID].Map)
                        {
                            if (Information.IsNothing(entryFar))
                                entryFar = Graveyards[GraveyardID];
                            continue;
                        }

                        // Skip graveyards that ain't for your faction
                        if (GraveyardFaction != 0 && GraveyardFaction != Character.Team)
                            continue;
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

                var selectedGraveyard = entryNear;
                if (Information.IsNothing(selectedGraveyard))
                    selectedGraveyard = entryFar;

                // If Teleport Then
                // If Alive = False And Character.DEAD = True Then
                // CharacterResurrect(Character)
                // Character.Life.Current = Character.Life.Maximum
                // If Character.ManaType = ManaTypes.TYPE_MANA Then Character.Mana.Current = Character.Mana.Maximum
                // If selectedGraveyard.Map = Character.MapID Then
                // Character.SetUpdateFlag(EUnitFields.UNIT_FIELD_HEALTH, Character.Life.Current)
                // If Character.ManaType = ManaTypes.TYPE_MANA Then Character.SetUpdateFlag(EUnitFields.UNIT_FIELD_POWER1, Character.Mana.Current)
                // Character.SendCharacterUpdate()
                // End If
                // End If

                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "GraveYards: GraveYard.Map[{0}], GraveYard.X[{1}], GraveYard.Y[{2}], GraveYard.Z[{3}]", selectedGraveyard.Map, selectedGraveyard.X, selectedGraveyard.Y, selectedGraveyard.Z);
                Character.Teleport(selectedGraveyard.X, selectedGraveyard.Y, selectedGraveyard.Z, 0f, selectedGraveyard.Map);
                Character.SendDeathReleaseLoc(selectedGraveyard.X, selectedGraveyard.Y, selectedGraveyard.Z, selectedGraveyard.Map);
            }
            // Else
            // Character.positionX = selectedGraveyard.X
            // Character.positionY = selectedGraveyard.Y
            // Character.positionZ = selectedGraveyard.Z
            // Character.MapID = selectedGraveyard.Map
            // End If
            else            // Non instanced Death
            {
                Character.ZoneCheck();

                // _WorldServer.WorldDatabase.Query(String.Format("SELECT id, faction FROM world_graveyard_zone WHERE ghost_map = {0} AND ghost_zone = {1}", Character.MapID, Character.ZoneID), GraveQuery)
                WorldServiceLocator._WorldServer.WorldDatabase.Query(string.Format("SELECT id, faction FROM game_graveyard_zone WHERE ghost_zone = {0}", (object)Character.ZoneID), GraveQuery);
                if (GraveQuery.Rows.Count == 0)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "GraveYards: No near graveyards for map [{0}], zone [{1}]", Character.MapID, Character.ZoneID);
                    return;
                }

                foreach (DataRow GraveLink in GraveQuery.Rows)
                {
                    int GraveyardID = Conversions.ToInteger(GraveLink["id"]);
                    int GraveyardFaction = Conversions.ToInteger(GraveLink["faction"]);
                    if (Graveyards.ContainsKey(GraveyardID) == false)
                    {
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.INFORMATION, "GraveYards: Graveyard link invalid [{0}]", GraveyardID);
                        continue;
                    }

                    if (Character.MapID != Graveyards[GraveyardID].Map)
                    {
                        if (Information.IsNothing(entryFar))
                            entryFar = Graveyards[GraveyardID];
                        continue;
                    }

                    // Skip graveyards that ain't for your faction
                    if (GraveyardFaction != 0 && GraveyardFaction != Character.Team)
                        continue;
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

                var selectedGraveyard = entryNear;
                if (Information.IsNothing(selectedGraveyard))
                    selectedGraveyard = entryFar;
                if (Teleport)
                {
                    if (Alive & Character.DEAD)
                    {
                        WorldServiceLocator._WS_Handlers_Misc.CharacterResurrect(ref Character);
                        Character.Life.Current = Character.Life.Maximum;
                        if (Character.ManaType == ManaTypes.TYPE_MANA)
                            Character.Mana.Current = Character.Mana.Maximum;
                        if (selectedGraveyard.Map == Character.MapID)
                        {
                            Character.SetUpdateFlag((int)EUnitFields.UNIT_FIELD_HEALTH, Character.Life.Current);
                            if (Character.ManaType == ManaTypes.TYPE_MANA)
                                Character.SetUpdateFlag((int)EUnitFields.UNIT_FIELD_POWER1, Character.Mana.Current);
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

        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        private bool _disposedValue; // To detect redundant calls

        // IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                // TODO: set large fields to null.
            }

            _disposedValue = true;
        }

        // TODO: override Finalize() only if Dispose(ByVal disposing As Boolean) above has code to free unmanaged resources.
        // Protected Overrides Sub Finalize()
        // ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
        // Dispose(False)
        // MyBase.Finalize()
        // End Sub

        // This code added by Visual Basic to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
    }
}