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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Mangos.Common.Enums.Chat;
using Mangos.Common.Enums.Faction;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Group;
using Mangos.Common.Enums.Misc;
using Mangos.Common.Enums.Player;
using Mangos.Common.Enums.Spell;
using Mangos.Common.Enums.Unit;
using Mangos.Common.Globals;
using Mangos.World.AI;
using Mangos.World.Globals;
using Mangos.World.Loots;
using Mangos.World.Player;
using Mangos.World.Server;
using Mangos.World.Spells;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.World.Objects
{
    public class WS_Creatures
    {

        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public const int SKILL_DETECTION_PER_LEVEL = 5;

        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */        // WARNING: Use only with _WorldServer.WORLD_CREATUREs()
        public class CreatureObject : WS_Base.BaseUnit, IDisposable
        {
            public CreatureInfo CreatureInfo
            {
                get
                {
                    return WorldServiceLocator._WorldServer.CREATURESDatabase[ID];
                }
            }

            public int ID = 0;
            public WS_Creatures_AI.TBaseAI aiScript = null;
            public float SpawnX = 0f;
            public float SpawnY = 0f;
            public float SpawnZ = 0f;
            public float SpawnO = 0f;
            public short Faction = 0;
            public float SpawnRange = 0f;
            public byte MoveType = 0;
            public int MoveFlags = 0;
            public byte cStandState = 0;
            public Timer ExpireTimer = null;
            public int SpawnTime = 0;
            public float SpeedMod = 1.0f;
            public int EquipmentID = 0;
            public int WaypointID = 0;
            public int GameEvent = 0;
            public WS_Spells.CastSpellParameters SpellCasted = null;
            public bool DestroyAtNoCombat = false;
            public bool Flying = false;
            public int LastPercent = 100;

            public string Name
            {
                get
                {
                    return CreatureInfo.Name;
                }
            }

            public float MaxDistance
            {
                get
                {
                    return 35.0f; // BoundingRadius * 35
                }
            }

            public bool isAbleToWalkOnWater
            {
                get
                {
                    // TODO: Fix family filter
                    switch (CreatureInfo.CreatureFamily)
                    {
                        case 3:
                        case 10:
                        case 11:
                        case 12:
                        case 20:
                        case 21:
                        case 27:
                            {
                                return false;
                            }

                        default:
                            {
                                return true;
                            }
                    }
                }
            }

            public bool isAbleToWalkOnGround
            {
                get
                {
                    // TODO: Fix family filter
                    switch (CreatureInfo.CreatureFamily)
                    {
                        case 255:
                            {
                                return false;
                            }

                        default:
                            {
                                return true;
                            }
                    }
                }
            }

            public bool isCritter
            {
                get
                {
                    return CreatureInfo.CreatureType == UNIT_TYPE.CRITTER;
                }
            }

            public bool isGuard
            {
                get
                {
                    return (CreatureInfo.cNpcFlags & NPCFlags.UNIT_NPC_FLAG_GUARD) == NPCFlags.UNIT_NPC_FLAG_GUARD;
                    // Select Case ID
                    // Case 68, 197, 240, 466, 727, 853, 1423, 1496, 1642, 1652, 1736, 1738, 1741, 1743, 1744, 1745, 1746, 1756, 1965, 2041, 2714, 2721, 3083, 3084, 3210, 3211, 3212, 3213, 3214, 3215, 3220, 3221, 3222, 3223, 3224, 3296, 3297, 3469, 3502, 3571, 4262, 4624, 5595, 5624, 5952, 5953, 5597, 7980, 8017, 9460, 10676, 10682, 10881, 11190, 11822, 12160, 12996, 13839, 14304, 14375, 14377, 15184, 15371, 15442, 15616, 15940, 16096, 16221, 16222, 16733, 16864, 16921, 18038, 18103, 18948, 18949, 18971, 18986, 19541, 20484, 20485, 20672, 20674, 21976, 22494, 23636, 23721, 25992
                    // Return True
                    // Case Else
                    // Return False
                    // End Select
                }
            }

            public override bool IsDead
            {
                get
                {
                    if (aiScript is object)
                    {
                        return Life.Current == 0 || aiScript.State == AIState.AI_DEAD || aiScript.State == AIState.AI_RESPAWN;
                    }
                    else
                    {
                        return Life.Current == 0;
                    }
                }
            }

            public bool Evade
            {
                get
                {
                    if (aiScript is object && aiScript.State == AIState.AI_MOVING_TO_SPAWN)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            public int NPCTextID
            {
                get
                {
                    if (WorldServiceLocator._WS_DBCDatabase.CreatureGossip.ContainsKey(GUID - WorldServiceLocator._Global_Constants.GUID_UNIT))
                        return WorldServiceLocator._WS_DBCDatabase.CreatureGossip(GUID - WorldServiceLocator._Global_Constants.GUID_UNIT);
                    return 0xFFFFFF;
                }
            }

            public override bool IsFriendlyTo(ref WS_Base.BaseUnit Unit)
            {
                if (ReferenceEquals(Unit, this))
                    return true;
                if (Unit is WS_PlayerData.CharacterObject)
                {
                    {
                        var withBlock = (WS_PlayerData.CharacterObject)Unit;
                        if (withBlock.GM)
                            return true;
                        if (withBlock.GetReputation(withBlock.Faction) < ReputationRank.Friendly)
                            return false;
                        if (withBlock.GetReaction(withBlock.Faction) < TReaction.NEUTRAL)
                            return false;

                        // TODO: At war with faction?
                    }
                }
                else if (Unit is CreatureObject)
                {
                    {
                        // TODO!!
                    }
                }

                return true;
            }

            public override bool IsEnemyTo(ref WS_Base.BaseUnit Unit)
            {
                if (ReferenceEquals(Unit, this))
                    return false;
                if (Unit is WS_PlayerData.CharacterObject)
                {
                    {
                        var withBlock = (WS_PlayerData.CharacterObject)Unit;
                        if (withBlock.GM)
                            return false;
                        if (withBlock.GetReputation(withBlock.Faction) < ReputationRank.Friendly)
                            return true;
                        if (withBlock.GetReaction(withBlock.Faction) < TReaction.NEUTRAL)
                            return true;

                        // TODO: At war with faction?
                    }
                }
                else if (Unit is CreatureObject)
                {
                    {
                        // TODO!!
                    }
                }

                return false;
            }

            public float AggroRange(WS_PlayerData.CharacterObject objCharacter)
            {
                short LevelDiff = (short)(Level - objCharacter.Level);
                float Range = 20 + LevelDiff;
                if (Range < 5f)
                    Range = 5f;
                if (Range > 45f)
                    Range = 45f;
                return Range;
            }

            public void SendTargetUpdate(ulong TargetGUID)
            {
                var packet = new Packets.UpdatePacketClass();
                var tmpUpdate = new Packets.UpdateClass((int)EUnitFields.UNIT_END);
                tmpUpdate.SetUpdateFlag((int)EUnitFields.UNIT_FIELD_TARGET, TargetGUID);
                tmpUpdate.AddToPacket(packet, ObjectUpdateType.UPDATETYPE_VALUES, this);
                tmpUpdate.Dispose();
                SendToNearPlayers(ref (Packets.PacketClass)packet);
                packet.Dispose();
            }

            public WS_Base.BaseUnit GetRandomTarget()
            {
                if (aiScript is null)
                    return null;
                if (aiScript.aiHateTable.Count == 0)
                    return null;
                int i = 0;
                int target = WorldServiceLocator._WorldServer.Rnd.Next(0, aiScript.aiHateTable.Count);
                foreach (KeyValuePair<WS_Base.BaseUnit, int> tmpUnit in aiScript.aiHateTable)
                {
                    if (target == i)
                        return tmpUnit.Key;
                    i += 1;
                }

                return null;
            }

            public void FillAllUpdateFlags(ref Packets.UpdateClass Update)
            {
                Update.SetUpdateFlag((int)EObjectFields.OBJECT_FIELD_GUID, GUID);
                Update.SetUpdateFlag((int)EObjectFields.OBJECT_FIELD_SCALE_X, Size);
                Update.SetUpdateFlag((int)EObjectFields.OBJECT_FIELD_TYPE, (long)Common.Globals.ObjectType.TYPE_OBJECT + (long)Common.Globals.ObjectType.TYPE_UNIT);
                Update.SetUpdateFlag((int)EObjectFields.OBJECT_FIELD_ENTRY, ID);
                if (aiScript is object && aiScript.aiTarget is object)
                {
                    Update.SetUpdateFlag((int)EUnitFields.UNIT_FIELD_TARGET, aiScript.aiTarget.GUID);
                }

                if (SummonedBy > 0m)
                    Update.SetUpdateFlag((int)EUnitFields.UNIT_FIELD_SUMMONEDBY, SummonedBy);
                if (CreatedBy > 0m)
                    Update.SetUpdateFlag((int)EUnitFields.UNIT_FIELD_CREATEDBY, CreatedBy);
                if (CreatedBySpell > 0)
                    Update.SetUpdateFlag((int)EUnitFields.UNIT_CREATED_BY_SPELL, CreatedBySpell);
                Update.SetUpdateFlag((int)EUnitFields.UNIT_FIELD_DISPLAYID, Model);
                Update.SetUpdateFlag((int)EUnitFields.UNIT_FIELD_NATIVEDISPLAYID, WorldServiceLocator._WorldServer.CREATURESDatabase[ID].GetFirstModel);
                if (Mount > 0)
                    Update.SetUpdateFlag((int)EUnitFields.UNIT_FIELD_MOUNTDISPLAYID, Mount);
                Update.SetUpdateFlag((int)EUnitFields.UNIT_FIELD_BYTES_0, cBytes0);
                Update.SetUpdateFlag((int)EUnitFields.UNIT_FIELD_BYTES_1, cBytes1);
                Update.SetUpdateFlag((int)EUnitFields.UNIT_FIELD_BYTES_2, cBytes2);
                Update.SetUpdateFlag((int)EUnitFields.UNIT_NPC_EMOTESTATE, cEmoteState);
                Update.SetUpdateFlag((int)EUnitFields.UNIT_FIELD_HEALTH, Life.Current);
                Update.SetUpdateFlag((int)(EUnitFields.UNIT_FIELD_POWER1 + WorldServiceLocator._WorldServer.CREATURESDatabase[ID].ManaType), Mana.Current);
                Update.SetUpdateFlag((int)EUnitFields.UNIT_FIELD_MAXHEALTH, Life.Maximum);
                Update.SetUpdateFlag((int)(EUnitFields.UNIT_FIELD_MAXPOWER1 + WorldServiceLocator._WorldServer.CREATURESDatabase[ID].ManaType), Mana.Maximum);
                Update.SetUpdateFlag((int)EUnitFields.UNIT_FIELD_LEVEL, Level);
                Update.SetUpdateFlag((int)EUnitFields.UNIT_FIELD_FACTIONTEMPLATE, Faction);
                Update.SetUpdateFlag((int)EUnitFields.UNIT_NPC_FLAGS, WorldServiceLocator._WorldServer.CREATURESDatabase[ID].cNpcFlags);
                Update.SetUpdateFlag((int)EUnitFields.UNIT_FIELD_FLAGS, cUnitFlags);
                Update.SetUpdateFlag((int)EUnitFields.UNIT_DYNAMIC_FLAGS, cDynamicFlags);
                Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCES + DamageTypes.DMG_PHYSICAL, (int)WorldServiceLocator._WorldServer.CREATURESDatabase[ID].Resistances[DamageTypes.DMG_PHYSICAL]);
                Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCES + DamageTypes.DMG_HOLY, (int)WorldServiceLocator._WorldServer.CREATURESDatabase[ID].Resistances[DamageTypes.DMG_HOLY]);
                Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCES + DamageTypes.DMG_FIRE, (int)WorldServiceLocator._WorldServer.CREATURESDatabase[ID].Resistances[DamageTypes.DMG_FIRE]);
                Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCES + DamageTypes.DMG_NATURE, (int)WorldServiceLocator._WorldServer.CREATURESDatabase[ID].Resistances[DamageTypes.DMG_NATURE]);
                Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCES + DamageTypes.DMG_FROST, (int)WorldServiceLocator._WorldServer.CREATURESDatabase[ID].Resistances[DamageTypes.DMG_FROST]);
                Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCES + DamageTypes.DMG_SHADOW, (int)WorldServiceLocator._WorldServer.CREATURESDatabase[ID].Resistances[DamageTypes.DMG_SHADOW]);
                Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCES + DamageTypes.DMG_ARCANE, (int)WorldServiceLocator._WorldServer.CREATURESDatabase[ID].Resistances[DamageTypes.DMG_ARCANE]);
                if (EquipmentID > 0)
                {
                    try
                    {
                        if (WorldServiceLocator._WS_DBCDatabase.CreatureEquip.ContainsKey(EquipmentID))
                        {
                            var EquipmentInfo = WorldServiceLocator._WS_DBCDatabase.CreatureEquip[EquipmentID];
                            Update.SetUpdateFlag((int)EUnitFields.UNIT_VIRTUAL_ITEM_SLOT_DISPLAY, EquipmentInfo.EquipModel[0]);
                            Update.SetUpdateFlag((int)EUnitFields.UNIT_VIRTUAL_ITEM_INFO, EquipmentInfo.EquipInfo[0]);
                            Update.SetUpdateFlag((int)(EUnitFields.UNIT_VIRTUAL_ITEM_INFO + 1), EquipmentInfo.EquipSlot[0]);
                            Update.SetUpdateFlag((int)(EUnitFields.UNIT_VIRTUAL_ITEM_SLOT_DISPLAY + 1), EquipmentInfo.EquipModel[1]);
                            Update.SetUpdateFlag((int)(EUnitFields.UNIT_VIRTUAL_ITEM_INFO + 2), EquipmentInfo.EquipInfo[1]);
                            Update.SetUpdateFlag((int)(EUnitFields.UNIT_VIRTUAL_ITEM_INFO + 2 + 1), EquipmentInfo.EquipSlot[1]);
                            Update.SetUpdateFlag((int)(EUnitFields.UNIT_VIRTUAL_ITEM_SLOT_DISPLAY + 2), EquipmentInfo.EquipModel[2]);
                            Update.SetUpdateFlag((int)(EUnitFields.UNIT_VIRTUAL_ITEM_INFO + 4), EquipmentInfo.EquipInfo[2]);
                            Update.SetUpdateFlag((int)(EUnitFields.UNIT_VIRTUAL_ITEM_INFO + 4 + 1), EquipmentInfo.EquipSlot[2]);
                        }
                    }
                    catch (DataException)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(string.Format("FillAllUpdateFlags : Unable to equip items {0} for Creature", EquipmentID));
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }
                }

                // Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_BASEATTACKTIME, _WorldServer.CREATURESDatabase(ID).BaseAttackTime)
                // Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_OFFHANDATTACKTIME, _WorldServer.CREATURESDatabase(ID).BaseAttackTime)
                // Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_RANGEDATTACKTIME, _WorldServer.CREATURESDatabase(ID).BaseRangedAttackTime)
                // Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_ATTACK_POWER, _WorldServer.CREATURESDatabase(ID).AtackPower)
                // Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_RANGED_ATTACK_POWER, _WorldServer.CREATURESDatabase(ID).RangedAtackPower)

                Update.SetUpdateFlag((int)EUnitFields.UNIT_FIELD_BOUNDINGRADIUS, BoundingRadius);
                Update.SetUpdateFlag((int)EUnitFields.UNIT_FIELD_COMBATREACH, CombatReach);
                // Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_MINRANGEDDAMAGE, _WorldServer.CREATURESDatabase(ID).RangedDamage.Minimum)
                // Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXRANGEDDAMAGE, _WorldServer.CREATURESDatabase(ID).RangedDamage.Maximum)

                for (int i = 0, loopTo = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs_VISIBLE - 1; i <= loopTo; i++)
                {
                    if (ActiveSpells[i] is object)
                    {
                        Update.SetUpdateFlag((int)(EUnitFields.UNIT_FIELD_AURA + i), ActiveSpells[i].SpellID);
                    }
                }

                for (int i = 0, loopTo1 = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECT_FLAGs - 1; i <= loopTo1; i++)
                    Update.SetUpdateFlag((int)(EUnitFields.UNIT_FIELD_AURAFLAGS + i), ActiveSpells_Flags[i]);
                for (int i = 0, loopTo2 = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECT_LEVELSs - 1; i <= loopTo2; i++)
                {
                    Update.SetUpdateFlag((int)(EUnitFields.UNIT_FIELD_AURAAPPLICATIONS + i), ActiveSpells_Count[i]);
                    Update.SetUpdateFlag((int)(EUnitFields.UNIT_FIELD_AURALEVELS + i), ActiveSpells_Level[i]);
                }
            }

            public void MoveToInstant(float x, float y, float z, float o)
            {
                positionX = x;
                positionY = y;
                positionZ = z;
                orientation = o;
                if (SeenBy.Count > 0)
                {
                    var packet = new Packets.PacketClass(OPCODES.MSG_MOVE_HEARTBEAT);
                    packet.AddPackGUID(GUID);
                    packet.AddInt32(0); // Movementflags
                    packet.AddInt32(WorldServiceLocator._NativeMethods.timeGetTime(""));
                    packet.AddSingle(positionX);
                    packet.AddSingle(positionY);
                    packet.AddSingle(positionZ);
                    packet.AddSingle(orientation);
                    packet.AddInt32(0);
                    SendToNearPlayers(ref packet);
                    packet.Dispose();
                }
            }

            public float OldX = 0.0f;
            public float OldY = 0.0f;
            public float OldZ = 0.0f;
            public float MoveX = 0.0f;
            public float MoveY = 0.0f;
            public float MoveZ = 0.0f;
            public int LastMove = 0;
            public int LastMove_Time = 0;
            public bool PositionUpdated = true;

            public void SetToRealPosition(bool Forced = false)
            {
                if (aiScript is null)
                    return;
                if (Forced == false && aiScript.State == AIState.AI_MOVING_TO_SPAWN)
                    return;
                int timeDiff = WorldServiceLocator._NativeMethods.timeGetTime("") - LastMove;
                if ((Forced || aiScript.IsMoving()) && LastMove > 0 && timeDiff < LastMove_Time)
                {
                    float distance;
                    if (aiScript.State == AIState.AI_MOVING || aiScript.State == AIState.AI_WANDERING)
                    {
                        distance = timeDiff / 1000.0f * (CreatureInfo.WalkSpeed * SpeedMod);
                    }
                    else
                    {
                        distance = timeDiff / 1000.0f * (CreatureInfo.RunSpeed * SpeedMod);
                    }

                    positionX = (float)(OldX + Math.Cos(orientation) * distance);
                    positionY = (float)(OldY + Math.Sin(orientation) * distance);
                    positionZ = WorldServiceLocator._WS_Maps.GetZCoord(positionX, positionY, positionZ, MapID);
                }
                else if (PositionUpdated == false && timeDiff >= LastMove_Time)
                {
                    PositionUpdated = true;
                    positionX = MoveX;
                    positionY = MoveY;
                    positionZ = MoveZ;
                }
            }

            public void StopMoving()
            {
                if (aiScript is null)
                    return;
                if (aiScript.InCombat())
                    return;
                aiScript.Pause(10000);
                SetToRealPosition(true);
                MoveToInstant(positionX, positionY, positionZ, orientation);
            }

            public int MoveTo(float x, float y, float z, float o = 0.0f, bool Running = false)
            {
                try
                {
                    if (SeenBy.Count == 0)
                    {
                        return 10000;
                    }
                }
                catch
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "MoveTo:SeenBy Failed");
                }

                int TimeToMove = 1;
                var SMSG_MONSTER_MOVE = new Packets.PacketClass(OPCODES.SMSG_MONSTER_MOVE);
                try
                {
                    SMSG_MONSTER_MOVE.AddPackGUID(GUID);
                    SMSG_MONSTER_MOVE.AddSingle(positionX);
                    SMSG_MONSTER_MOVE.AddSingle(positionY);
                    SMSG_MONSTER_MOVE.AddSingle(positionZ);
                    SMSG_MONSTER_MOVE.AddInt32(WorldServiceLocator._WS_Network.MsTime());         // Sequence/MSTime?
                    if (o == 0.0f)
                    {
                        SMSG_MONSTER_MOVE.AddInt8(0);                    // Type [If type is 1 then the packet ends here]
                    }
                    else
                    {
                        SMSG_MONSTER_MOVE.AddInt8(4);
                        SMSG_MONSTER_MOVE.AddSingle(o);
                    }

                    float moveDist = WorldServiceLocator._WS_Combat.GetDistance(positionX, x, positionY, y, positionZ, z);
                    if (Flying)
                    {
                        SMSG_MONSTER_MOVE.AddInt32(0x300);           // Flags [0x0 - Walk, 0x100 - Run, 0x200 - Waypoint, 0x300 - Fly]
                        TimeToMove = (int)(moveDist / (CreatureInfo.RunSpeed * SpeedMod) * 1000f + 0.5f);
                    }
                    else if (Running)
                    {
                        SMSG_MONSTER_MOVE.AddInt32(0x100);           // Flags [0x0 - Walk, 0x100 - Run, 0x200 - Waypoint, 0x300 - Fly]
                        TimeToMove = (int)(moveDist / (CreatureInfo.RunSpeed * SpeedMod) * 1000f + 0.5f);
                    }
                    else
                    {
                        SMSG_MONSTER_MOVE.AddInt32(0);
                        TimeToMove = (int)(moveDist / (CreatureInfo.WalkSpeed * SpeedMod) * 1000f + 0.5f);
                    }

                    orientation = WorldServiceLocator._WS_Combat.GetOrientation(positionX, x, positionY, y);
                    OldX = positionX;
                    OldY = positionY;
                    OldZ = positionZ;
                    LastMove = WorldServiceLocator._NativeMethods.timeGetTime("");
                    LastMove_Time = TimeToMove;
                    PositionUpdated = false;
                    positionX = x;
                    positionY = y;
                    positionZ = z;
                    MoveX = x;
                    MoveY = y;
                    MoveZ = z;
                    SMSG_MONSTER_MOVE.AddInt32(TimeToMove);  // Time
                    SMSG_MONSTER_MOVE.AddInt32(1);           // Points Count
                    SMSG_MONSTER_MOVE.AddSingle(x);          // First Point X
                    SMSG_MONSTER_MOVE.AddSingle(y);          // First Point Y
                    SMSG_MONSTER_MOVE.AddSingle(z);          // First Point Z

                    // The points after that are in the same format only if flag 0x200 is set, else they are compressed in 1 uint32

                    SendToNearPlayers(ref SMSG_MONSTER_MOVE);
                }
                catch (Exception ex)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "MoveTo:Main Failed - {0}", ex.Message);
                }
                finally
                {
                    SMSG_MONSTER_MOVE.Dispose();
                }

                MoveCell();
                return TimeToMove;
            }

            public bool CanMoveTo(float x, float y, float z)
            {
                WS_Base.BaseObject argobjCharacter = this;
                if (WorldServiceLocator._WS_Maps.IsOutsideOfMap(argobjCharacter))
                    return false;
                if (z < WorldServiceLocator._WS_Maps.GetWaterLevel(x, y, (int)MapID))
                {
                    if (!isAbleToWalkOnWater)
                        return false;
                }
                else if (!isAbleToWalkOnGround)
                    return false;
                return true;
            }

            public void TurnTo(ref WS_Base.BaseObject Target)
            {
                TurnTo(Target.positionX, Target.positionY);
            }

            public void TurnTo(float x, float y)
            {
                orientation = WorldServiceLocator._WS_Combat.GetOrientation(positionX, x, positionY, y);
                TurnTo(orientation);
            }

            public void TurnTo(float orientation_)
            {
                orientation = orientation_;
                if (SeenBy.Count > 0)
                {
                    if (aiScript is null || aiScript.IsMoving() == false)
                    {
                        var packet = new Packets.PacketClass(OPCODES.MSG_MOVE_HEARTBEAT);
                        try
                        {
                            packet.AddPackGUID(GUID);
                            packet.AddInt32(0); // Movementflags
                            packet.AddInt32(WorldServiceLocator._NativeMethods.timeGetTime(""));
                            packet.AddSingle(positionX);
                            packet.AddSingle(positionY);
                            packet.AddSingle(positionZ);
                            packet.AddSingle(orientation);
                            packet.AddInt32(0);
                            SendToNearPlayers(ref packet);
                        }
                        finally
                        {
                            packet.Dispose();
                        }
                    }
                }
            }

            public override void Die(ref WS_Base.BaseUnit Attacker)
            {
                cUnitFlags = (int)UnitFlags.UNIT_FLAG_DEAD; // cUnitFlags Or UnitFlags.UNIT_FLAG_DEAD
                Life.Current = 0;
                Mana.Current = 0;

                // DONE: Creature stops while it's dead and everyone sees it at the same position
                if (aiScript is object)
                {
                    SetToRealPosition(true);
                    MoveToInstant(positionX, positionY, positionZ, orientation);
                    PositionUpdated = true;
                    LastMove = 0;
                    LastMove_Time = 0;
                    aiScript.State = AIState.AI_DEAD;
                    aiScript.DoThink();
                }

                if (aiScript is object)
                    aiScript.OnDeath();
                if (Attacker is object && Attacker is CreatureObject)
                {
                    if (((CreatureObject)Attacker).aiScript is object)
                    {
                        WS_Base.BaseUnit argVictim = this;
                        ((CreatureObject)Attacker).aiScript.OnKill(ref argVictim);
                    }
                }

                // DONE: Send the update
                var packetForNear = new Packets.UpdatePacketClass();
                var UpdateData = new Packets.UpdateClass((int)EUnitFields.UNIT_END);

                // DONE: Remove all spells when the creature die
                for (int i = 0, loopTo = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs_VISIBLE - 1; i <= loopTo; i++)
                {
                    if (ActiveSpells[i] is object)
                    {
                        RemoveAura(i, ref ActiveSpells[i].SpellCaster, SendUpdate: false);
                        UpdateData.SetUpdateFlag((int)(EUnitFields.UNIT_FIELD_AURA + i), 0);
                    }
                }

                UpdateData.SetUpdateFlag((int)EUnitFields.UNIT_FIELD_HEALTH, Life.Current);
                UpdateData.SetUpdateFlag((int)((int)EUnitFields.UNIT_FIELD_POWER1 + base.ManaType), Mana.Current);
                UpdateData.SetUpdateFlag((int)EUnitFields.UNIT_FIELD_FLAGS, cUnitFlags);
                UpdateData.AddToPacket(packetForNear, ObjectUpdateType.UPDATETYPE_VALUES, this);
                Packets.PacketClass argpacket = packetForNear;
                SendToNearPlayers(ref argpacket);
                packetForNear.Dispose();
                UpdateData.Dispose();
                if (Attacker is WS_PlayerData.CharacterObject)
                {
                    ((WS_PlayerData.CharacterObject)Attacker).RemoveFromCombat(this);

                    // DONE: Don't give xp or loot for guards, civilians or critters
                    if (isCritter == false && isGuard == false && CreatureInfo.cNpcFlags == 0)
                    {
                        WS_PlayerData.CharacterObject argCharacter = (WS_PlayerData.CharacterObject)Attacker;
                        GiveXP(ref argCharacter);
                        WS_PlayerData.CharacterObject argCharacter1 = (WS_PlayerData.CharacterObject)Attacker;
                        LootCorpse(ref argCharacter1);
                    }

                    // DONE: Fire quest event to check for if this monster is required for quest
                    WS_PlayerData.CharacterObject argobjCharacter = (WS_PlayerData.CharacterObject)Attacker;
                    var argcreature = this;
                    WorldServiceLocator._WorldServer.ALLQUESTS.OnQuestKill(ref argobjCharacter, ref argcreature);
                }
            }

            public override void DealDamage(int Damage, [Optional, DefaultParameterValue(null)] ref WS_Base.BaseUnit Attacker)
            {
                if (Life.Current == 0)
                    return;

                // DONE: Break some spells when taking any damage
                RemoveAurasByInterruptFlag((int)SpellAuraInterruptFlags.AURA_INTERRUPT_FLAG_DAMAGE);
                Life.Current -= Damage;

                // DONE: Generate hate
                if (Attacker is object && aiScript is object)
                    aiScript.OnGenerateHate(ref Attacker, Damage);

                // DONE: Check for dead
                if (Life.Current == 0)
                {
                    Die(ref Attacker);
                    return;
                }

                int tmpPercent = (int)Conversion.Fix(Life.Current / (double)Life.Maximum * 100d);
                if (tmpPercent != LastPercent)
                {
                    LastPercent = tmpPercent;
                    if (aiScript is object)
                        aiScript.OnHealthChange(LastPercent);
                }

                // DONE: Do health update
                if (SeenBy.Count > 0)
                {
                    var packetForNear = new Packets.UpdatePacketClass();
                    var UpdateData = new Packets.UpdateClass((int)EUnitFields.UNIT_END);
                    UpdateData.SetUpdateFlag((int)EUnitFields.UNIT_FIELD_HEALTH, Life.Current);
                    UpdateData.SetUpdateFlag((int)((int)EUnitFields.UNIT_FIELD_POWER1 + base.ManaType), Mana.Current);
                    UpdateData.AddToPacket(packetForNear, ObjectUpdateType.UPDATETYPE_VALUES, this);
                    SendToNearPlayers(ref (Packets.PacketClass)packetForNear);
                    packetForNear.Dispose();
                    UpdateData.Dispose();
                }
            }

            public void Heal(int Damage, [Optional, DefaultParameterValue(null)] WS_Base.BaseUnit Attacker)
            {
                if (Life.Current == 0)
                    return;
                Life.Current += Damage;

                // DONE: Do health update
                if (SeenBy.Count > 0)
                {
                    var packetForNear = new Packets.UpdatePacketClass();
                    var UpdateData = new Packets.UpdateClass((int)EUnitFields.UNIT_END);
                    UpdateData.SetUpdateFlag((int)EUnitFields.UNIT_FIELD_HEALTH, Life.Current);
                    UpdateData.AddToPacket(packetForNear, ObjectUpdateType.UPDATETYPE_VALUES, this);
                    SendToNearPlayers(ref (Packets.PacketClass)packetForNear);
                    packetForNear.Dispose();
                    UpdateData.Dispose();
                }
            }

            public override void Energize(int Damage, ManaTypes Power, [Optional, DefaultParameterValue(null)] WS_Base.BaseUnit Attacker)
            {
                if (ManaType != Power)
                    return;
                Mana.Current += Damage;

                // DONE: Do health update
                if (SeenBy.Count > 0)
                {
                    var packetForNear = new Packets.UpdatePacketClass();
                    var UpdateData = new Packets.UpdateClass((int)EUnitFields.UNIT_END);
                    UpdateData.SetUpdateFlag((int)((int)EUnitFields.UNIT_FIELD_POWER1 + base.ManaType), Mana.Current);
                    UpdateData.AddToPacket(packetForNear, ObjectUpdateType.UPDATETYPE_VALUES, this);
                    SendToNearPlayers(ref (Packets.PacketClass)packetForNear);
                    packetForNear.Dispose();
                    UpdateData.Dispose();
                }
            }

            public void LootCorpse(ref WS_PlayerData.CharacterObject Character)
            {
                if (GenerateLoot(ref Character, LootType.LOOTTYPE_CORPSE))
                {
                    cDynamicFlags = (int)DynamicFlags.UNIT_DYNFLAG_LOOTABLE;
                }
                else if (CreatureInfo.SkinLootID > 0)
                {
                    cUnitFlags |= UnitFlags.UNIT_FLAG_SKINNABLE;
                }
                else
                {
                    // No loot or skinnable
                    return;
                }

                // DONE: Create packet
                var packet = new Packets.PacketClass(OPCODES.SMSG_UPDATE_OBJECT);
                packet.AddInt32(1);
                packet.AddInt8(0);
                var UpdateData = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_PLAYER);
                UpdateData.SetUpdateFlag((int)EUnitFields.UNIT_DYNAMIC_FLAGS, cDynamicFlags);
                UpdateData.SetUpdateFlag((int)EUnitFields.UNIT_FIELD_FLAGS, cUnitFlags);
                UpdateData.AddToPacket(packet, ObjectUpdateType.UPDATETYPE_VALUES, this);
                UpdateData.Dispose();
                if (WorldServiceLocator._WS_Loot.LootTable.ContainsKey(GUID) == false && (cUnitFlags & UnitFlags.UNIT_FLAG_SKINNABLE) == UnitFlags.UNIT_FLAG_SKINNABLE)
                {
                    // DONE: There was no loot, so send the skinning update to every nearby player
                    SendToNearPlayers(ref packet);
                }
                else if (Character.IsInGroup)
                {
                    // DONE: Group loot rules
                    WorldServiceLocator._WS_Loot.LootTable[GUID].LootOwner = 0UL;
                    switch (Character.Group.LootMethod)
                    {
                        case var @case when @case == GroupLootMethod.LOOT_FREE_FOR_ALL:
                            {
                                foreach (ulong objCharacter in Character.Group.LocalMembers)
                                {
                                    if (SeenBy.Contains(objCharacter))
                                    {
                                        WorldServiceLocator._WS_Loot.LootTable[GUID].LootOwner = objCharacter;
                                        WorldServiceLocator._WorldServer.CHARACTERs[objCharacter].client.Send(packet);
                                    }
                                }

                                break;
                            }

                        case var case1 when case1 == GroupLootMethod.LOOT_MASTER:
                            {
                                if (Character.Group.LocalLootMaster is null)
                                {
                                    WorldServiceLocator._WS_Loot.LootTable[GUID].LootOwner = Character.GUID;
                                    Character.client.Send(packet);
                                }
                                else
                                {
                                    WorldServiceLocator._WS_Loot.LootTable[GUID].LootOwner = Character.Group.LocalLootMaster.GUID;
                                    Character.Group.LocalLootMaster.client.Send(packet);
                                }

                                break;
                            }

                        case var case2 when case2 == GroupLootMethod.LOOT_GROUP:
                        case var case3 when case3 == GroupLootMethod.LOOT_NEED_BEFORE_GREED:
                        case var case4 when case4 == GroupLootMethod.LOOT_ROUND_ROBIN:
                            {
                                var cLooter = Character.Group.GetNextLooter();
                                while (!SeenBy.Contains(cLooter.GUID) && !ReferenceEquals(cLooter, Character))
                                    cLooter = Character.Group.GetNextLooter();
                                WorldServiceLocator._WS_Loot.LootTable[GUID].LootOwner = cLooter.GUID;
                                cLooter.client.Send(packet);
                                break;
                            }
                    }
                }
                else
                {
                    WorldServiceLocator._WS_Loot.LootTable[GUID].LootOwner = Character.GUID;
                    Character.client.Send(packet);
                }

                // DONE: Dispose packet
                packet.Dispose();
            }

            public bool GenerateLoot(ref WS_PlayerData.CharacterObject Character, LootType LootType)
            {
                if (CreatureInfo.LootID == 0)
                    return false;

                // DONE: Loot generation
                var Loot = new WS_Loot.LootObject(GUID, LootType);
                var Template = WorldServiceLocator._WS_Loot.LootTemplates_Creature.GetLoot(CreatureInfo.LootID);
                if (Template is object)
                {
                    Template.Process(ref Loot, 0);
                }

                // DONE: Money loot
                if (LootType == LootType.LOOTTYPE_CORPSE && CreatureInfo.CreatureType == UNIT_TYPE.HUMANOID)
                {
                    Loot.Money = WorldServiceLocator._WorldServer.Rnd.Next((int)CreatureInfo.MinGold, (int)(CreatureInfo.MaxGold + 1L));
                }

                Loot.LootOwner = Character.GUID;
                return true;
            }

            public void GiveXP(ref WS_PlayerData.CharacterObject Character)
            {
                // NOTE: Formulas taken from http://www.wowwiki.com/Formulas:Mob_XP
                int XP = Level * 5 + 45;
                int lvlDifference = Character.Level - Level;
                if (lvlDifference > 0) // Higher level mobs
                {
                    XP = (int)(XP * (1d + 0.05d * (Level - Character.Level)));
                }
                else if (lvlDifference < 0) // Lower level mobs
                {
                    byte GrayLevel;
                    switch (Character.Level)
                    {
                        case var @case when @case <= 5:
                            {
                                GrayLevel = 0;
                                break;
                            }

                        case var case1 when case1 <= 39:
                            {
                                GrayLevel = (byte)(Character.Level - Math.Floor(Character.Level / 10d) - 5d);
                                break;
                            }

                        case var case2 when case2 <= 59:
                            {
                                GrayLevel = (byte)(Character.Level - Math.Floor(Character.Level / 5d) - 1d);
                                break;
                            }

                        default:
                            {
                                GrayLevel = (byte)(Character.Level - 9);
                                break;
                            }
                    }

                    if (Level > GrayLevel)
                    {
                        int ZD;
                        switch (Character.Level)
                        {
                            case var case3 when case3 <= 7:
                                {
                                    ZD = 5;
                                    break;
                                }

                            case var case4 when case4 <= 9:
                                {
                                    ZD = 6;
                                    break;
                                }

                            case var case5 when case5 <= 11:
                                {
                                    ZD = 7;
                                    break;
                                }

                            case var case6 when case6 <= 15:
                                {
                                    ZD = 8;
                                    break;
                                }

                            case var case7 when case7 <= 19:
                                {
                                    ZD = 9;
                                    break;
                                }

                            case var case8 when case8 <= 29:
                                {
                                    ZD = 11;
                                    break;
                                }

                            case var case9 when case9 <= 39:
                                {
                                    ZD = 12;
                                    break;
                                }

                            case var case10 when case10 <= 44:
                                {
                                    ZD = 13;
                                    break;
                                }

                            case var case11 when case11 <= 49:
                                {
                                    ZD = 14;
                                    break;
                                }

                            case var case12 when case12 <= 54:
                                {
                                    ZD = 15;
                                    break;
                                }

                            case var case13 when case13 <= 59:
                                {
                                    ZD = 16;
                                    break;
                                }

                            default:
                                {
                                    ZD = 17;
                                    break;
                                }
                        }

                        XP = (int)(XP * (1d - (Character.Level - Level) / (double)ZD));
                    }
                    else
                    {
                        XP = 0;
                    }
                }

                // DONE: Killing elites
                if (WorldServiceLocator._WorldServer.CREATURESDatabase[ID].Elite > 0)
                    XP *= 2;
                // DONE: XP Rate config
                XP = (int)(XP * WorldServiceLocator._WorldServer.Config.XPRate);
                if (!Character.IsInGroup)
                {
                    // DONE: Rested
                    int RestedXP = 0;
                    if (Character.RestBonus >= 0)
                    {
                        RestedXP = XP;
                        if (RestedXP > Character.RestBonus)
                            RestedXP = Character.RestBonus;
                        Character.RestBonus -= RestedXP;
                        XP += RestedXP;
                    }

                    // DONE: Single kill
                    Character.AddXP(XP, RestedXP, GUID, true);
                }
                else
                {

                    // DONE: Party bonus
                    XP = (int)(XP / (double)Character.Group.GetMembersCount());
                    switch (Character.Group.GetMembersCount())
                    {
                        case var case14 when case14 <= 2:
                            {
                                XP *= 1;
                                break;
                            }

                        case 3:
                            {
                                XP = (int)(XP * 1.166d);
                                break;
                            }

                        case 4:
                            {
                                XP = (int)(XP * 1.3d);
                                break;
                            }

                        default:
                            {
                                XP = (int)(XP * 1.4d);
                                break;
                            }
                    }

                    // DONE: Party calculate all levels
                    int baseLvl = 0;
                    foreach (ulong Member in Character.Group.LocalMembers)
                    {
                        {
                            var withBlock = WorldServiceLocator._WorldServer.CHARACTERs[Member];
                            if (withBlock.DEAD == false && Math.Sqrt(Math.Pow(positionX - positionX, 2d) + Math.Pow(positionY - positionY, 2d)) <= VisibleDistance)
                            {
                                baseLvl += Level;
                            }
                        }
                    }

                    // DONE: Party share
                    foreach (ulong Member in Character.Group.LocalMembers)
                    {
                        {
                            var withBlock1 = WorldServiceLocator._WorldServer.CHARACTERs[Member];
                            if (withBlock1.DEAD == false && Math.Sqrt(Math.Pow(positionX - positionX, 2d) + Math.Pow(positionY - positionY, 2d)) <= VisibleDistance)
                            {
                                int tmpXP = XP;
                                // DONE: Rested
                                int RestedXP = 0;
                                if (withBlock1.RestBonus >= 0)
                                {
                                    RestedXP = tmpXP;
                                    if (RestedXP > withBlock1.RestBonus)
                                        RestedXP = withBlock1.RestBonus;
                                    withBlock1.RestBonus -= RestedXP;
                                    tmpXP += RestedXP;
                                }

                                tmpXP = (int)Conversion.Fix(tmpXP * Level / (double)baseLvl);
                                withBlock1.AddXP(tmpXP, RestedXP, GUID, false);
                                withBlock1.LogXPGain(tmpXP, RestedXP, GUID, (float)((Character.Group.GetMembersCount() - 1) / 10d));
                            }
                        }
                    }
                }
            }

            public void StopCasting()
            {
                if (SpellCasted is null || SpellCasted.Finished)
                    return;
                SpellCasted.StopCast();

                // TODO: Send interrupt to other players
            }

            public void ApplySpell(int SpellID)
            {
                // TODO: Check if the creature can cast the spell

                if (WorldServiceLocator._WS_Spells.SPELLs.ContainsKey(SpellID) == false)
                    return;
                var t = new WS_Spells.SpellTargets();
                WS_Base.BaseUnit argobjCharacter = this;
                t.SetTarget_SELF(ref argobjCharacter);
                WS_Base.BaseObject argcaster = this;
                WorldServiceLocator._WS_Spells.SPELLs[SpellID].Apply(ref argcaster, t);
            }

            public int CastSpellOnSelf(int SpellID)
            {
                if (Spell_Silenced)
                    return -1;
                var Targets = new WS_Spells.SpellTargets();
                WS_Base.BaseUnit argobjCharacter = this;
                Targets.SetTarget_SELF(ref argobjCharacter);
                WS_Base.BaseObject argCaster = this;
                var tmpSpell = new WS_Spells.CastSpellParameters(ref Targets, ref argCaster, SpellID);
                if (WorldServiceLocator._WS_Spells.SPELLs[SpellID].GetDuration > 0)
                    SpellCasted = tmpSpell;
                ThreadPool.QueueUserWorkItem(new WaitCallback(tmpSpell.Cast));
                return WorldServiceLocator._WS_Spells.SPELLs[SpellID].GetCastTime;
            }

            public int CastSpell(int SpellID, WS_Base.BaseUnit Target)
            {
                if (Spell_Silenced)
                    return -1;
                if (Target is null)
                    return -1;

                // DONE: Shouldn't be able to cast if we're out of range
                // TODO: Is combatreach used here as well?
                if (WorldServiceLocator._WS_Combat.GetDistance(this, Target) > WorldServiceLocator._WS_Spells.SPELLs[SpellID].GetRange)
                    return -1;
                var Targets = new WS_Spells.SpellTargets();
                Targets.SetTarget_UNIT(ref Target);
                WS_Base.BaseObject argCaster = this;
                var tmpSpell = new WS_Spells.CastSpellParameters(ref Targets, ref argCaster, SpellID);
                if (WorldServiceLocator._WS_Spells.SPELLs[SpellID].GetDuration > 0)
                    SpellCasted = tmpSpell;
                ThreadPool.QueueUserWorkItem(new WaitCallback(tmpSpell.Cast));
                return WorldServiceLocator._WS_Spells.SPELLs[SpellID].GetCastTime;
            }

            public int CastSpell(int SpellID, float x, float y, float z)
            {
                if (Spell_Silenced)
                    return -1;

                // DONE: Shouldn't be able to cast if we're out of range
                // TODO: Is combatreach used here as well?
                if (WorldServiceLocator._WS_Combat.GetDistance(this, x, y, z) > WorldServiceLocator._WS_Spells.SPELLs[SpellID].GetRange)
                    return -1;
                var Targets = new WS_Spells.SpellTargets();
                Targets.SetTarget_DESTINATIONLOCATION(x, y, z);
                WS_Base.BaseObject argCaster = this;
                var tmpSpell = new WS_Spells.CastSpellParameters(ref Targets, ref argCaster, SpellID);
                if (WorldServiceLocator._WS_Spells.SPELLs[SpellID].GetDuration > 0)
                    SpellCasted = tmpSpell;
                ThreadPool.QueueUserWorkItem(new WaitCallback(tmpSpell.Cast));
                return WorldServiceLocator._WS_Spells.SPELLs[SpellID].GetCastTime;
            }

            public void SpawnCreature(int Entry, float PosX, float PosY, float PosZ)
            {
                var tmpCreature = new CreatureObject(Entry, PosX, PosY, PosZ, 0.0f, (int)MapID)
                {
                    instance = instance,
                    DestroyAtNoCombat = true
                };
                tmpCreature.AddToWorld();
                if (tmpCreature.aiScript is object)
                    tmpCreature.aiScript.Dispose();
                tmpCreature.aiScript = new WS_Creatures_AI.DefaultAI(ref tmpCreature);
                tmpCreature.aiScript.aiHateTable = aiScript.aiHateTable;
                tmpCreature.aiScript.OnEnterCombat();
                tmpCreature.aiScript.State = AIState.AI_ATTACKING;
                tmpCreature.aiScript.DoThink();
            }

            public void SendChatMessage(string Message, ChatMsg msgType, LANGUAGES msgLanguage, ulong SecondGUID = 0UL)
            {
                var packet = new Packets.PacketClass(OPCODES.SMSG_MESSAGECHAT);
                byte flag = 0;
                packet.AddInt8((byte)msgType);
                packet.AddInt32((int)msgLanguage);
                switch (msgType)
                {
                    case var @case when @case == ChatMsg.CHAT_MSG_MONSTER_SAY:
                    case var case1 when case1 == ChatMsg.CHAT_MSG_MONSTER_EMOTE:
                    case var case2 when case2 == ChatMsg.CHAT_MSG_MONSTER_YELL:
                        {
                            packet.AddUInt64(GUID);
                            packet.AddInt32(System.Text.Encoding.UTF8.GetByteCount(Name) + 1);
                            packet.AddString(Name);
                            packet.AddUInt64(SecondGUID);
                            break;
                        }

                    default:
                        {
                            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "Creature.SendChatMessage() must not handle this chat type!");
                            break;
                        }
                }

                packet.AddInt32(System.Text.Encoding.UTF8.GetByteCount(Message) + 1);
                packet.AddString(Message);
                packet.AddInt8(flag);
                SendToNearPlayers(ref packet);
                packet.Dispose();
            }

            public void ResetAI()
            {
                aiScript.Dispose();
                var argCreature = this;
                aiScript = new WS_Creatures_AI.DefaultAI(ref argCreature);
                MoveType = 1;
            }

            public void Initialize()
            {
                // DONE: Database loading
                Level = (byte)WorldServiceLocator._WorldServer.Rnd.Next(WorldServiceLocator._WorldServer.CREATURESDatabase[ID].LevelMin, WorldServiceLocator._WorldServer.CREATURESDatabase[ID].LevelMax);
                Size = WorldServiceLocator._WorldServer.CREATURESDatabase[ID].Size;
                if (Size == 0f)
                    Size = 1f;
                Model = WorldServiceLocator._WorldServer.CREATURESDatabase[ID].GetRandomModel;
                ManaType = (ManaTypes)WorldServiceLocator._WorldServer.CREATURESDatabase[ID].ManaType;
                Mana.Base = WorldServiceLocator._WorldServer.CREATURESDatabase[ID].Mana;
                Mana.Current = Mana.Maximum;
                Life.Base = WorldServiceLocator._WorldServer.CREATURESDatabase[ID].Life;
                Life.Current = Life.Maximum;
                Faction = WorldServiceLocator._WorldServer.CREATURESDatabase[ID].Faction;
                for (byte i = (byte)DamageTypes.DMG_PHYSICAL, loopTo = (byte)DamageTypes.DMG_ARCANE; i <= loopTo; i++)
                    Resistances[i].Base = WorldServiceLocator._WorldServer.CREATURESDatabase[ID].Resistances[i];
                if (EquipmentID == 0 && WorldServiceLocator._WorldServer.CREATURESDatabase[ID].EquipmentID > 0)
                {
                    EquipmentID = WorldServiceLocator._WorldServer.CREATURESDatabase[ID].EquipmentID;
                }

                if (WorldServiceLocator._WS_DBCDatabase.CreatureModel.ContainsKey(Model))
                {
                    BoundingRadius = WorldServiceLocator._WS_DBCDatabase.CreatureModel[Model].BoundingRadius;
                    CombatReach = WorldServiceLocator._WS_DBCDatabase.CreatureModel[Model].CombatReach;
                }

                MechanicImmunity = WorldServiceLocator._WorldServer.CREATURESDatabase[ID].MechanicImmune;

                // DONE: Internal Initializators
                CanSeeInvisibility_Stealth = SKILL_DETECTION_PER_LEVEL * Level;
                CanSeeInvisibility_Invisibility = 0;
                if ((WorldServiceLocator._WorldServer.CREATURESDatabase[ID].cNpcFlags & NPCFlags.UNIT_NPC_FLAG_SPIRITHEALER) == NPCFlags.UNIT_NPC_FLAG_SPIRITHEALER)
                {
                    Invisibility = InvisibilityLevel.DEAD;
                    cUnitFlags = (int)UnitFlags.UNIT_FLAG_SPIRITHEALER;
                }

                cDynamicFlags = WorldServiceLocator._WorldServer.CREATURESDatabase[ID].DynFlags;
                StandState = cStandState;
                cBytes2 = (int)SHEATHE_SLOT.SHEATHE_WEAPON;
                if (this is WS_Pets.PetObject)
                {
                    // DONE: Load pet AI
                    var argCreature = this;
                    aiScript = new WS_Pets.PetAI(ref argCreature);
                }
                else
                {
                    // DONE: Load scripted AI
                    if (!string.IsNullOrEmpty(WorldServiceLocator._WorldServer.CREATURESDatabase[ID].AIScriptSource))
                    {
                        aiScript = (WS_Creatures_AI.TBaseAI)WorldServiceLocator._WorldServer.AI.InvokeConstructor(WorldServiceLocator._WorldServer.CREATURESDatabase[ID].AIScriptSource, new object[] { this });
                    }
                    else if (System.IO.File.Exists(@"scripts\creatures\" + WorldServiceLocator._Functions.FixName(Name) + ".vb"))
                    {
                        var tmpScript = new ScriptedObject(@"scripts\creatures\" + WorldServiceLocator._Functions.FixName(Name) + ".vb", "", true);
                        aiScript = (WS_Creatures_AI.TBaseAI)tmpScript.InvokeConstructor("CreatureAI_" + WorldServiceLocator._Functions.FixName(Name).Replace(" ", "_"), new object[] { this });
                        tmpScript.Dispose();
                    }

                    // DONE: Load default AI
                    if (aiScript is null)
                    {
                        if (isCritter)
                        {
                            var argCreature1 = this;
                            aiScript = new WS_Creatures_AI.CritterAI(ref argCreature1);
                        }
                        else if (isGuard)
                        {
                            if (MoveType == 2)
                            {
                                var argCreature3 = this;
                                aiScript = new WS_Creatures_AI.GuardWaypointAI(ref argCreature3);
                            }
                            else
                            {
                                var argCreature4 = this;
                                aiScript = new WS_Creatures_AI.GuardAI(ref argCreature4);
                            }
                        }
                        else if (MoveType == 1)
                        {
                            var argCreature5 = this;
                            aiScript = new WS_Creatures_AI.DefaultAI(ref argCreature5);
                        }
                        else if (MoveType == 2)
                        {
                            var argCreature6 = this;
                            aiScript = new WS_Creatures_AI.WaypointAI(ref argCreature6);
                        }
                        else
                        {
                            var argCreature2 = this;
                            aiScript = new WS_Creatures_AI.StandStillAI(ref argCreature2);
                        }
                    }
                }
            }
            // WARNING: Use only for loading creature from DB
            public CreatureObject(ulong GUID_, [Optional, DefaultParameterValue(null)] ref DataRow Info) : base()
            {
                if (Info is null)
                {
                    var MySQLQuery = new DataTable();
                    WorldServiceLocator._WorldServer.WorldDatabase.Query(string.Format("SELECT * FROM creature LEFT OUTER JOIN game_event_creature ON creature.guid = game_event_creature.guid WHERE creature.guid = {0};", (object)GUID_), ref MySQLQuery);
                    if (MySQLQuery.Rows.Count > 0)
                    {
                        Info = MySQLQuery.Rows[0];
                    }
                    else
                    {
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Creature Spawn not found in database. [GUID={0:X}]", GUID_);
                        return;
                    }
                }

                DataRow AddonInfo = null;
                var AddonInfoQuery = new DataTable();
                WorldServiceLocator._WorldServer.WorldDatabase.Query(string.Format("SELECT * FROM spawns_creatures_addon WHERE spawn_id = {0};", (object)GUID_), ref AddonInfoQuery);
                if (AddonInfoQuery.Rows.Count > 0)
                {
                    AddonInfo = AddonInfoQuery.Rows[0];
                }

                positionX = Conversions.ToSingle(Info["position_X"]);
                positionY = Conversions.ToSingle(Info["position_Y"]);
                positionZ = Conversions.ToSingle(Info["position_Z"]);
                orientation = Conversions.ToSingle(Info["orientation"]);
                OldX = positionX;
                OldY = positionY;
                OldZ = positionZ;
                SpawnX = positionX;
                SpawnY = positionY;
                SpawnZ = positionZ;
                SpawnO = orientation;
                ID = Conversions.ToInteger(Info["id"]);
                MapID = Conversions.ToUInteger(Info["map"]);
                SpawnID = Conversions.ToInteger(Info["guid"]);
                Model = Conversions.ToInteger(Info["modelid"]);
                SpawnTime = Conversions.ToInteger(Info["spawntimesecs"]);
                SpawnRange = Conversions.ToSingle(Info["spawndist"]);
                MoveType = Conversions.ToByte(Info["MovementType"]);
                Life.Current = Conversions.ToInteger(Info["curhealth"]);
                Mana.Current = Conversions.ToInteger(Info["curmana"]);
                EquipmentID = Conversions.ToInteger(Info["equipment_id"]);

                // If Not Info.Item("event") Is DBNull.Value Then
                // GameEvent = Info.Item("event")
                // Else
                // GameEvent = 0
                // End If

                // TODO: spawn_deathstate?

                if (AddonInfo is object)
                {
                    Mount = Conversions.ToInteger(AddonInfo["spawn_mount"]);
                    cEmoteState = Conversions.ToInteger(AddonInfo["spawn_emote"]);
                    MoveFlags = Conversions.ToInteger(AddonInfo["spawn_moveflags"]);
                    cBytes0 = Conversions.ToInteger(AddonInfo["spawn_bytes0"]);
                    cBytes1 = Conversions.ToInteger(AddonInfo["spawn_bytes1"]);
                    cBytes2 = Conversions.ToInteger(AddonInfo["spawn_bytes2"]);
                    WaypointID = Conversions.ToInteger(AddonInfo["spawn_pathid"]);
                    // TODO: spawn_auras
                }

                if (!WorldServiceLocator._WorldServer.CREATURESDatabase.ContainsKey(ID))
                {
                    var baseCreature = new CreatureInfo(ID);
                }

                GUID = GUID_ + WorldServiceLocator._Global_Constants.GUID_UNIT;
                Initialize();
                try
                {
                    WorldServiceLocator._WorldServer.WORLD_CREATUREs_Lock.AcquireWriterLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
                    WorldServiceLocator._WorldServer.WORLD_CREATUREs.Add(GUID, this);
                    WorldServiceLocator._WorldServer.WORLD_CREATUREsKeys.Add(GUID);
                    WorldServiceLocator._WorldServer.WORLD_CREATUREs_Lock.ReleaseWriterLock();
                }
                catch (Exception ex)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "WS_Creatures:New failed - Guid: {1}  {0}", ex.Message, GUID_);
                }
            }
            // WARNING: Use only for spawning new creature
            public CreatureObject(ulong GUID_, int ID_) : base()
            {
                if (!WorldServiceLocator._WorldServer.CREATURESDatabase.ContainsKey(ID_))
                {
                    var baseCreature = new CreatureInfo(ID_);
                }

                ID = ID_;
                GUID = GUID_;
                Initialize();
                try
                {
                    WorldServiceLocator._WorldServer.WORLD_CREATUREs_Lock.AcquireWriterLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
                    WorldServiceLocator._WorldServer.WORLD_CREATUREs.Add(GUID, this);
                    WorldServiceLocator._WorldServer.WORLD_CREATUREsKeys.Add(GUID);
                    WorldServiceLocator._WorldServer.WORLD_CREATUREs_Lock.ReleaseWriterLock();
                }
                catch (Exception ex)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "WS_Creatures:New failed - Guid: {1} ID: {2}  {0}", ex.Message, GUID_, ID_);
                }
            }
            // WARNING: Use only for spawning new creature
            public CreatureObject(int ID_) : base()
            {
                if (!WorldServiceLocator._WorldServer.CREATURESDatabase.ContainsKey(ID_))
                {
                    var baseCreature = new CreatureInfo(ID_);
                }

                ID = ID_;
                GUID = WorldServiceLocator._WS_Creatures.GetNewGUID();
                Initialize();
                try
                {
                    WorldServiceLocator._WorldServer.WORLD_CREATUREs_Lock.AcquireWriterLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
                    WorldServiceLocator._WorldServer.WORLD_CREATUREs.Add(GUID, this);
                    WorldServiceLocator._WorldServer.WORLD_CREATUREsKeys.Add(GUID);
                    WorldServiceLocator._WorldServer.WORLD_CREATUREs_Lock.ReleaseWriterLock();
                }
                catch (Exception ex)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "WS_Creatures:New failed - Guid: {1} ID: {2}  {0}", ex.Message, ID_);
                }
            }
            // WARNING: Use only for spawning new creature
            public CreatureObject(int ID_, float PosX, float PosY, float PosZ, float Orientation_, int Map, int Duration = 0) : base()
            {
                if (!WorldServiceLocator._WorldServer.CREATURESDatabase.ContainsKey(ID_))
                {
                    var baseCreature = new CreatureInfo(ID_);
                }

                ID = ID_;
                GUID = WorldServiceLocator._WS_Creatures.GetNewGUID();
                positionX = PosX;
                positionY = PosY;
                positionZ = PosZ;
                orientation = Orientation_;
                MapID = (uint)Map;
                SpawnX = PosX;
                SpawnY = PosY;
                SpawnZ = PosZ;
                SpawnO = Orientation_;
                Initialize();

                // TODO: Duration
                if (Duration > 0)
                {
                    ExpireTimer = new Timer(Destroy, null, Duration, Duration);
                }

                try
                {
                    WorldServiceLocator._WorldServer.WORLD_CREATUREs_Lock.AcquireWriterLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
                    WorldServiceLocator._WorldServer.WORLD_CREATUREs.Add(GUID, this);
                    WorldServiceLocator._WorldServer.WORLD_CREATUREsKeys.Add(GUID);
                    WorldServiceLocator._WorldServer.WORLD_CREATUREs_Lock.ReleaseWriterLock();
                }
                catch (Exception ex)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "WS_Creatures:New failed - Guid: {1} ID: {2} Map: {3}  {0}", ex.Message, GUID, ID_, Map);
                }
            }

            /* TODO ERROR: Skipped RegionDirectiveTrivia */
            private bool _disposedValue; // To detect redundant calls

            // IDisposable
            protected virtual void Dispose(bool disposing)
            {
                if (!_disposedValue)
                {
                    // TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                    // TODO: set large fields to null.
                    if (aiScript is object)
                        aiScript.Dispose();
                    RemoveFromWorld();
                    try
                    {
                        WorldServiceLocator._WorldServer.WORLD_CREATUREs_Lock.AcquireWriterLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
                        WorldServiceLocator._WorldServer.WORLD_CREATUREs.Remove(GUID);
                        WorldServiceLocator._WorldServer.WORLD_CREATUREsKeys.Remove(GUID);
                        WorldServiceLocator._WorldServer.WORLD_CREATUREs_Lock.ReleaseWriterLock();
                        ExpireTimer.Dispose();
                    }
                    catch (Exception ex)
                    {
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "WS_Creatures:Dispose failed -  {0}", ex.Message);
                    }
                }

                _disposedValue = true;
            }

            // This code added by Visual Basic to correctly implement the disposable pattern.
            public void Dispose()
            {
                // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
            public void Destroy(object state = null)
            {
                // TODO: Remove pets also
                if (SummonedBy > 0m)
                {
                    if (WorldServiceLocator._CommonGlobalFunctions.GuidIsPlayer(SummonedBy) && WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(SummonedBy))
                    {
                        if (WorldServiceLocator._WorldServer.CHARACTERs[SummonedBy].NonCombatPet is object && ReferenceEquals(WorldServiceLocator._WorldServer.CHARACTERs[SummonedBy].NonCombatPet, this))
                        {
                            WorldServiceLocator._WorldServer.CHARACTERs[SummonedBy].NonCombatPet = null;
                        }
                    }
                }

                var packet = new Packets.PacketClass(OPCODES.SMSG_DESTROY_OBJECT);
                packet.AddUInt64(GUID);
                SendToNearPlayers(ref packet);
                packet.Dispose();
                Dispose();
            }

            public void Despawn()
            {
                RemoveFromWorld();
                if (WorldServiceLocator._WS_Loot.LootTable.ContainsKey(GUID))
                {
                    WorldServiceLocator._WS_Loot.LootTable[GUID].Dispose();
                }

                if (SpawnTime > 0)
                {
                    if (aiScript is object)
                    {
                        aiScript.State = AIState.AI_RESPAWN;
                        aiScript.Pause(SpawnTime * 1000);
                    }
                }
                else
                {
                    Dispose();
                }
            }

            public void Respawn()
            {
                Life.Current = Life.Maximum;
                Mana.Current = Mana.Maximum;
                cUnitFlags &= !UnitFlags.UNIT_FLAG_DEAD;
                cDynamicFlags = 0;
                positionX = SpawnX;
                positionY = SpawnY;
                positionZ = SpawnZ;
                orientation = SpawnO;
                if (aiScript is object)
                {
                    aiScript.OnLeaveCombat(false);
                    aiScript.State = AIState.AI_WANDERING;
                }

                if (SeenBy.Count > 0)
                {
                    var packetForNear = new Packets.UpdatePacketClass();
                    var UpdateData = new Packets.UpdateClass((int)EUnitFields.UNIT_END);
                    UpdateData.SetUpdateFlag((int)EUnitFields.UNIT_FIELD_HEALTH, Life.Current);
                    UpdateData.SetUpdateFlag((int)((int)EUnitFields.UNIT_FIELD_POWER1 + base.ManaType), Mana.Current);
                    UpdateData.SetUpdateFlag((int)EUnitFields.UNIT_FIELD_FLAGS, cUnitFlags);
                    UpdateData.SetUpdateFlag((int)EUnitFields.UNIT_DYNAMIC_FLAGS, cDynamicFlags);
                    UpdateData.AddToPacket(packetForNear, ObjectUpdateType.UPDATETYPE_VALUES, this);
                    Packets.PacketClass argpacket = packetForNear;
                    SendToNearPlayers(ref argpacket);
                    packetForNear.Dispose();
                    UpdateData.Dispose();
                    MoveToInstant(SpawnX, SpawnY, SpawnZ, SpawnO);
                }
                else
                {
                    AddToWorld();
                }
            }

            public void AddToWorld()
            {
                WorldServiceLocator._WS_Maps.GetMapTile(positionX, positionY, ref CellX, ref CellY);
                if (WorldServiceLocator._WS_Maps.Maps[MapID].Tiles[CellX, CellY] is null)
                    WorldServiceLocator._WS_CharMovement.MAP_Load(CellX, CellY, MapID);
                try
                {
                    WorldServiceLocator._WS_Maps.Maps[MapID].Tiles[CellX, CellY].CreaturesHere.Add(GUID);
                }
                catch (Exception ex)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "WS_Creatures:AddToWorld failed - Guid: {1} ID: {2}  {0}", ex.Message);
                    return;
                }

                ulong[] list;

                // DONE: Sending to players in nearby cells
                for (short i = -1; i <= 1; i++)
                {
                    for (short j = -1; j <= 1; j++)
                    {
                        if (CellX + i >= 0 && CellX + i <= 63 && CellY + j >= 0 && CellY + j <= 63 && WorldServiceLocator._WS_Maps.Maps[MapID].Tiles[CellX + i, CellY + j] is object && WorldServiceLocator._WS_Maps.Maps[MapID].Tiles[CellX + i, CellY + j].PlayersHere.Count > 0)
                        {
                            {
                                var withBlock = WorldServiceLocator._WS_Maps.Maps[MapID].Tiles[CellX + i, CellY + j];
                                list = withBlock.PlayersHere.ToArray();
                                foreach (ulong plGUID in list)
                                {
                                    WS_Base.BaseObject argobjCharacter = this;
                                    if (WorldServiceLocator._WorldServer.CHARACTERs[plGUID].CanSee(ref argobjCharacter))
                                    {
                                        var packet = new Packets.PacketClass(OPCODES.SMSG_UPDATE_OBJECT);
                                        try
                                        {
                                            packet.AddInt32(1);
                                            packet.AddInt8(0);
                                            var tmpUpdate = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_UNIT);
                                            FillAllUpdateFlags(ref tmpUpdate);
                                            tmpUpdate.AddToPacket(packet, ObjectUpdateType.UPDATETYPE_CREATE_OBJECT, this);
                                            tmpUpdate.Dispose();
                                            WorldServiceLocator._WorldServer.CHARACTERs[plGUID].client.SendMultiplyPackets(ref packet);
                                            WorldServiceLocator._WorldServer.CHARACTERs[plGUID].creaturesNear.Add(GUID);
                                            SeenBy.Add(plGUID);
                                        }
                                        finally
                                        {
                                            packet.Dispose();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            public void RemoveFromWorld()
            {
                WorldServiceLocator._WS_Maps.GetMapTile(positionX, positionY, ref CellX, ref CellY);
                WorldServiceLocator._WS_Maps.Maps[MapID].Tiles[CellX, CellY].CreaturesHere.Remove(GUID);

                // DONE: Removing from players who can see the creature
                foreach (ulong plGUID in SeenBy.ToArray())
                {
                    if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(plGUID))
                    {
                        WorldServiceLocator._WorldServer.CHARACTERs[plGUID].guidsForRemoving_Lock.AcquireWriterLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
                        WorldServiceLocator._WorldServer.CHARACTERs[plGUID].guidsForRemoving.Add(GUID);
                        WorldServiceLocator._WorldServer.CHARACTERs[plGUID].guidsForRemoving_Lock.ReleaseWriterLock();
                        WorldServiceLocator._WorldServer.CHARACTERs[plGUID].creaturesNear.Remove(GUID);
                    }
                }

                SeenBy.Clear();
            }

            public void MoveCell()
            {
                try
                {
                    if (CellX != WorldServiceLocator._WS_Maps.GetMapTileX(positionX) || CellY != WorldServiceLocator._WS_Maps.GetMapTileY(positionY))
                    {
                        if (Information.IsNothing(WorldServiceLocator._WS_Maps.Maps[MapID].Tiles[CellX, CellY].CreaturesHere.Remove(GUID)) == false)
                        {
                            WorldServiceLocator._WS_Maps.Maps[MapID].Tiles[CellX, CellY].CreaturesHere.Remove(GUID);
                        }

                        WorldServiceLocator._WS_Maps.GetMapTile(positionX, positionY, ref CellX, ref CellY);

                        // If creature changes cell then it's sent back to spawn, if the creature is a waypoint walker this won't be very good :/
                        if (WorldServiceLocator._WS_Maps.Maps[MapID].Tiles[CellX, CellY] is null)
                        {
                            aiScript.Reset();
                            return;
                        }
                        else
                        {
                            WorldServiceLocator._WS_Maps.Maps[MapID].Tiles[CellX, CellY].CreaturesHere.Add(GUID);
                        }
                    }
                }
                catch (Exception e)
                {
                    // Creature ran outside of mapbounds, reset it
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "WS_Creatures:MoveCell - Creature outside of map bounds, Resetting  {0}", e.Message);
                    try
                    {
                        aiScript.Reset();
                    }
                    catch (Exception ex)
                    {
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "WS_Creatures:MoveCell - Couldn't reset creature outside of map bounds, Disposing  {0}", ex.Message);
                        aiScript.Dispose();
                    }
                }
            }
        }
        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public int[] CorpseDecay = new int[] { 30, 150, 150, 150, 1800 };

        public void On_CMSG_CREATURE_QUERY(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 17)
                return;
            var response = new Packets.PacketClass(OPCODES.SMSG_CREATURE_QUERY_RESPONSE);
            packet.GetInt16();
            int CreatureID = packet.GetInt32();
            ulong CreatureGUID = packet.GetUInt64();
            try
            {
                CreatureInfo Creature;
                if (WorldServiceLocator._WorldServer.CREATURESDatabase.ContainsKey(CreatureID) == false)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CREATURE_QUERY [Creature {2} not loaded.]", client.IP, client.Port, CreatureID);
                    response.AddUInt32((uint)(CreatureID | 0x80000000));
                    client.Send(response);
                    response.Dispose();
                    return;
                }
                else
                {
                    Creature = WorldServiceLocator._WorldServer.CREATURESDatabase[CreatureID];
                    // _WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CREATURE_QUERY [CreatureID={2} CreatureGUID={3:X}]", Format(TimeOfDay, "hh:mm:ss"), client.IP, client.Port, CreatureID, CreatureGUID - _Global_Constants.GUID_UNIT)
                }

                response.AddInt32(Creature.Id);
                response.AddString(Creature.Name);
                response.AddInt8(0);                         // Creature.Name2
                response.AddInt8(0);                         // Creature.Name3
                response.AddInt8(0);                         // Creature.Name4
                response.AddString(Creature.SubName);
                response.AddInt32((int)Creature.TypeFlags);       // TypeFlags
                response.AddInt32(Creature.CreatureType);    // Type
                response.AddInt32(Creature.CreatureFamily);  // Family
                response.AddInt32(Creature.Elite);           // Rank
                response.AddInt32(0);                        // Unk
                response.AddInt32(Creature.PetSpellDataID);  // PetSpellDataID
                response.AddInt32(Creature.ModelA1);         // ModelA1
                response.AddInt32(Creature.ModelA2);         // ModelA2
                response.AddInt32(Creature.ModelH1);         // ModelH1
                response.AddInt32(Creature.ModelH2);         // ModelH2
                response.AddSingle(1.0f);                    // Unk
                response.AddSingle(1.0f);                    // Unk
                response.AddInt8(Creature.Leader);           // RacialLeader
                client.Send(response);
                response.Dispose();
            }
            // _WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_CREATURE_QUERY_RESPONSE", client.IP, client.Port)
            catch (Exception ex)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Unknown Error: Unable to find CreatureID={0} in database. {1}", CreatureID, ex.Message);
            }
        }

        public void On_CMSG_NPC_TEXT_QUERY(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 17)
                return;
            packet.GetInt16();
            long TextID = packet.GetInt32();
            ulong TargetGUID = packet.GetUInt64();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_NPC_TEXT_QUERY [TextID={2}]", client.IP, client.Port, TextID);
            client.Character.SendTalking((int)TextID);
        }

        public void On_CMSG_GOSSIP_HELLO(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 13)
                return;
            packet.GetInt16();
            ulong GUID = packet.GetUInt64();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_GOSSIP_HELLO [GUID={2:X}]", client.IP, client.Port, GUID);
            if (WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(GUID) == false || WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].CreatureInfo.cNpcFlags == 0)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "[{0}:{1}] Client tried to speak with a creature that didn't exist or couldn't interact with. [GUID={2:X}  ID={3}]", client.IP, client.Port, GUID, WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].ID);
                return;
            }

            if (WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].Evade)
                return;
            WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].StopMoving();
            client.Character.RemoveAurasByInterruptFlag((int)SpellAuraInterruptFlags.AURA_INTERRUPT_FLAG_TALK);
            try
            {
                if (WorldServiceLocator._WorldServer.CREATURESDatabase[WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].ID].TalkScript is null)
                {
                    var test = new Packets.PacketClass(OPCODES.SMSG_NPC_WONT_TALK);
                    test.AddUInt64(GUID);
                    test.AddInt8(1);
                    client.Send(test);
                    test.Dispose();
                    if (NPCTexts.ContainsKey(34) == false)
                    {
                        var tmpText = new NPCText(34, "Hi $N, I'm not yet scripted to talk with you.");
                    }

                    client.Character.SendTalking(34);
                    GossipMenu argMenu = null;
                    QuestMenu argqMenu = null;
                    client.Character.SendGossip(GUID, 34, Menu: ref argMenu, qMenu: ref argqMenu);
                }
                else
                {
                    WorldServiceLocator._WorldServer.CREATURESDatabase[WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].ID].TalkScript.OnGossipHello(ref client.Character, GUID);
                }
            }
            catch (Exception ex)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "Error in gossip hello.{0}{1}", Environment.NewLine, ex.ToString());
            }
        }

        public void On_CMSG_GOSSIP_SELECT_OPTION(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 17)
                return;
            packet.GetInt16();
            ulong GUID = packet.GetUInt64();
            int SelOption = packet.GetInt32();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_GOSSIP_SELECT_OPTION [SelOption={3} GUID={2:X}]", client.IP, client.Port, GUID, SelOption);
            if (WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(GUID) == false || WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].CreatureInfo.cNpcFlags == 0)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "[{0}:{1}] Client tried to speak with a creature that didn't exist or couldn't interact with. [GUID={2:X}  ID={3}]", client.IP, client.Port, GUID, WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].ID);
                return;
            }

            if (WorldServiceLocator._WorldServer.CREATURESDatabase[WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].ID].TalkScript is null)
            {
                throw new ApplicationException("Invoked OnGossipSelect() on creature without initialized TalkScript!");
            }
            else
            {
                WorldServiceLocator._WorldServer.CREATURESDatabase[WorldServiceLocator._WorldServer.WORLD_CREATUREs[GUID].ID].TalkScript.OnGossipSelect(ref client.Character, GUID, SelOption);
            }
        }

        public void On_CMSG_SPIRIT_HEALER_ACTIVATE(ref Packets.PacketClass packet, ref WS_Network.ClientClass client)
        {
            if (packet.Data.Length - 1 < 13)
                return;
            packet.GetInt16();
            ulong GUID = packet.GetUInt64();
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_SPIRIT_HEALER_ACTIVATE [GUID={2}]", client.IP, client.Port, GUID);
            try
            {
                for (byte i = 0, loopTo = (byte)(EquipmentSlots.EQUIPMENT_SLOT_END - 1); i <= loopTo; i++)
                {
                    if (client.Character.Items.ContainsKey(i))
                        client.Character.Items[i].ModifyDurability(0.25f, ref client);
                }
            }
            catch (Exception ex)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Error activating spirit healer: {0}", ex.ToString());
            }

            WorldServiceLocator._WS_Handlers_Misc.CharacterResurrect(ref client.Character);
            client.Character.ApplySpell(15007);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private ulong GetNewGUID()
        {
            WorldServiceLocator._WorldServer.CreatureGUIDCounter = (ulong)(WorldServiceLocator._WorldServer.CreatureGUIDCounter + 1m);
            return WorldServiceLocator._WorldServer.CreatureGUIDCounter;
        }

        public Dictionary<int, NPCText> NPCTexts = new Dictionary<int, NPCText>();

        public class NPCText
        {
            public byte Count = 1;
            public int TextID = 0;
            public float[] Probability = new float[] { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f };
            public int[] Language = new int[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            public string[] TextLine1 = new string[] { "", "", "", "", "", "", "", "" };
            public string[] TextLine2 = new string[] { "", "", "", "", "", "", "", "" };
            public int[] Emote1 = new int[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            public int[] Emote2 = new int[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            public int[] Emote3 = new int[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            public int[] EmoteDelay1 = new int[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            public int[] EmoteDelay2 = new int[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            public int[] EmoteDelay3 = new int[] { 0, 0, 0, 0, 0, 0, 0, 0 };

            public NPCText(int _TextID)
            {
                TextID = _TextID;
                var MySQLQuery = new DataTable();
                WorldServiceLocator._WorldServer.WorldDatabase.Query(string.Format("SELECT * FROM npc_text WHERE ID = {0};", (object)TextID), ref MySQLQuery);
                if (MySQLQuery.Rows.Count > 0)
                {
                    for (int i = 0; i <= 7; i++)
                    {
                        Probability[i] = Conversions.ToSingle(MySQLQuery.Rows[0]["prob" + i + ""]);
                        if (Information.IsDBNull(MySQLQuery.Rows[0]["text" + i + "_0"]) == false)
                        {
                            TextLine1[i] = Conversions.ToString(MySQLQuery.Rows[0]["text" + i + "_0"]);
                        }

                        if (Information.IsDBNull(MySQLQuery.Rows[0]["text" + i + "_1"]) == false)
                        {
                            TextLine2[i] = Conversions.ToString(MySQLQuery.Rows[0]["text" + i + "_1"]);
                        }

                        if (Information.IsDBNull(MySQLQuery.Rows[0]["lang" + i + ""]) == false)
                        {
                            Language[i] = Conversions.ToInteger(MySQLQuery.Rows[0]["lang" + i + ""]);
                        }

                        if (Information.IsDBNull(MySQLQuery.Rows[0]["em" + i + "_0_delay"]) == false)
                        {
                            EmoteDelay1[i] = Conversions.ToInteger(MySQLQuery.Rows[0]["em" + i + "_0_delay"]);
                        }

                        if (Information.IsDBNull(MySQLQuery.Rows[0]["em" + i + "_0"]) == false)
                        {
                            Emote1[i] = Conversions.ToInteger(MySQLQuery.Rows[0]["em" + i + "_0"]);
                        }

                        if (Information.IsDBNull(MySQLQuery.Rows[0]["em" + i + "_1_delay"]) == false)
                        {
                            EmoteDelay2[i] = Conversions.ToInteger(MySQLQuery.Rows[0]["em" + i + "_1_delay"]);
                        }

                        if (Information.IsDBNull(MySQLQuery.Rows[0]["em" + i + "_1"]) == false)
                        {
                            Emote2[i] = Conversions.ToInteger(MySQLQuery.Rows[0]["em" + i + "_1"]);
                        }

                        if (Information.IsDBNull(MySQLQuery.Rows[0]["em" + i + "_2_delay"]) == false)
                        {
                            EmoteDelay3[i] = Conversions.ToInteger(MySQLQuery.Rows[0]["em" + i + "_2_delay"]);
                        }

                        if (Information.IsDBNull(MySQLQuery.Rows[0]["em" + i + "_2"]) == false)
                        {
                            Emote3[i] = Conversions.ToInteger(MySQLQuery.Rows[0]["em" + i + "_2"]);
                        }

                        if (!string.IsNullOrEmpty(TextLine1[i]))
                            Count = (byte)((byte)i + 1);
                    }
                }
                else
                {
                    Probability[0] = 1f;
                    TextLine1[0] = "Hey there, $N. How can I help you?";
                    TextLine2[0] = TextLine1[0];
                    Count = 0;
                }

                WorldServiceLocator._WS_Creatures.NPCTexts.Add(TextID, this);
            }

            public NPCText(int _TextID, string TextLine)
            {
                TextID = _TextID;
                TextLine1[0] = TextLine;
                TextLine2[0] = TextLine;
                Count = 0;
                WorldServiceLocator._WS_Creatures.NPCTexts.Add(TextID, this);
            }
        }
        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        // #Region "WS.Creatures.MonsterSayCombat"
        // Public MonsterSayCombat As New Dictionary(Of Integer, TMonsterSayCombat)
        // Public Class TMonsterSayCombat
        // Public Entry As Integer
        // Public EventNo As Integer
        // Public Chance As Single
        // Public Language As Integer
        // Public Type As Integer
        // Public MonsterName As String
        // Public Text0 As String
        // Public Text1 As String
        // Public Text2 As String
        // Public Text3 As String
        // Public Text4 As String

        // Public Sub New(ByVal Entry_ As Integer, ByVal EventNo_ As Integer, ByVal Chance_ As Single, ByVal Language_ As Integer, ByVal Type_ As Integer, ByVal MonsterName_ As String, ByVal Text0_ As String, ByVal Text1_ As String, ByVal Text2_ As String, ByVal Text3_ As String, ByVal Text4_ As String)
        // Entry = Entry_
        // EventNo = EventNo_
        // Chance = Chance_
        // Language = Language_
        // Type = Type_
        // MonsterName = MonsterName_
        // Text0 = Text0_
        // Text1 = Text1_
        // Text2 = Text2_
        // Text3 = Text3_
        // Text4 = Text4
        // End Sub
        // End Class
        // #End Region

    }

    /* TODO ERROR: Skipped RegionDirectiveTrivia */
    /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
}