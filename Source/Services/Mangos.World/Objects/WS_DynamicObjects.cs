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
using Mangos.Common.Enums.Global;
using Mangos.Common.Globals;
using Mangos.World.Globals;
using Mangos.World.Server;
using Mangos.World.Spells;

namespace Mangos.World.Objects
{
    public class WS_DynamicObjects
    {
        private ulong GetNewGUID()
        {
            ulong GetNewGUIDRet = default;
            WorldServiceLocator._WorldServer.DynamicObjectsGUIDCounter = (ulong)(WorldServiceLocator._WorldServer.DynamicObjectsGUIDCounter + 1m);
            GetNewGUIDRet = WorldServiceLocator._WorldServer.DynamicObjectsGUIDCounter;
            return GetNewGUIDRet;
        }

        public class DynamicObjectObject : WS_Base.BaseObject, IDisposable
        {
            public int SpellID = 0;
            public List<WS_Spells.SpellEffect> Effects = new List<WS_Spells.SpellEffect>();
            public int Duration = 0;
            public float Radius = 0f;
            public WS_Base.BaseUnit Caster;
            public int CastTime = 0;
            public int Bytes = 1;

            /* TODO ERROR: Skipped RegionDirectiveTrivia */
            private bool _disposedValue; // To detect redundant calls

            // IDisposable
            protected virtual void Dispose(bool disposing)
            {
                if (!_disposedValue)
                {
                    // TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                    // TODO: set large fields to null.
                    WorldServiceLocator._WorldServer.WORLD_DYNAMICOBJECTs.Remove(GUID);
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
            public DynamicObjectObject(ref WS_Base.BaseUnit Caster_, int SpellID_, float PosX, float PosY, float PosZ, int Duration_, float Radius_)
            {
                GUID = WorldServiceLocator._WS_DynamicObjects.GetNewGUID();
                WorldServiceLocator._WorldServer.WORLD_DYNAMICOBJECTs.Add(GUID, this);
                Caster = Caster_;
                SpellID = SpellID_;
                positionX = PosX;
                positionY = PosY;
                positionZ = PosZ;
                orientation = 0f;
                MapID = Caster.MapID;
                instance = Caster.instance;
                Duration = Duration_;
                Radius = Radius_;
                CastTime = WorldServiceLocator._NativeMethods.timeGetTime("");
            }

            public void FillAllUpdateFlags(ref Packets.UpdateClass Update)
            {
                Update.SetUpdateFlag(EObjectFields.OBJECT_FIELD_GUID, GUID);
                Update.SetUpdateFlag(EObjectFields.OBJECT_FIELD_TYPE, ObjectType.TYPE_DYNAMICOBJECT + ObjectType.TYPE_OBJECT);
                Update.SetUpdateFlag(EObjectFields.OBJECT_FIELD_SCALE_X, 0.5f * Radius);
                Update.SetUpdateFlag(EDynamicObjectFields.DYNAMICOBJECT_CASTER, Caster.GUID);
                Update.SetUpdateFlag(EDynamicObjectFields.DYNAMICOBJECT_BYTES, Bytes);
                Update.SetUpdateFlag(EDynamicObjectFields.DYNAMICOBJECT_SPELLID, SpellID);
                Update.SetUpdateFlag(EDynamicObjectFields.DYNAMICOBJECT_RADIUS, Radius);
                Update.SetUpdateFlag(EDynamicObjectFields.DYNAMICOBJECT_POS_X, positionX);
                Update.SetUpdateFlag(EDynamicObjectFields.DYNAMICOBJECT_POS_Y, positionY);
                Update.SetUpdateFlag(EDynamicObjectFields.DYNAMICOBJECT_POS_Z, positionZ);
                Update.SetUpdateFlag(EDynamicObjectFields.DYNAMICOBJECT_FACING, orientation);
                // Update.SetUpdateFlag(EDynamicObjectFields.DYNAMICOBJECT_CASTTIME, CastTime)
            }

            public void AddToWorld()
            {
                WorldServiceLocator._WS_Maps.GetMapTile(positionX, positionY, ref CellX, ref CellY);
                if (WorldServiceLocator._WS_Maps.Maps[MapID].Tiles[CellX, CellY] is null)
                    WorldServiceLocator._WS_CharMovement.MAP_Load(CellX, CellY, MapID);
                try
                {
                    WorldServiceLocator._WS_Maps.Maps[MapID].Tiles[CellX, CellY].DynamicObjectsHere.Add(GUID);
                }
                catch
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "AddToWorld failed MapId: {0} Tile XY: {1} {2} GUID: {3}", MapID, CellX, CellY, GUID);
                    return;
                }

                ulong[] list;
                // DONE: Sending to players in nearby cells
                var packet = new Packets.PacketClass(OPCODES.SMSG_UPDATE_OBJECT);
                packet.AddInt32(1);
                packet.AddInt8(0);
                var tmpUpdate = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_DYNAMICOBJECT);
                FillAllUpdateFlags(ref tmpUpdate);
                tmpUpdate.AddToPacket(packet, ObjectUpdateType.UPDATETYPE_CREATE_OBJECT_SELF, this);
                tmpUpdate.Dispose();
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
                                    if (WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(plGUID) && WorldServiceLocator._WorldServer.CHARACTERs[plGUID].CanSee(ref argobjCharacter))
                                    {
                                        WorldServiceLocator._WorldServer.CHARACTERs[plGUID].client.SendMultiplyPackets(ref packet);
                                        WorldServiceLocator._WorldServer.CHARACTERs[plGUID].dynamicObjectsNear.Add(GUID);
                                        SeenBy.Add(plGUID);
                                    }
                                }
                            }
                        }
                    }
                }

                packet.Dispose();
            }

            public void RemoveFromWorld()
            {
                WorldServiceLocator._WS_Maps.GetMapTile(positionX, positionY, ref CellX, ref CellY);
                WorldServiceLocator._WS_Maps.Maps[MapID].Tiles[CellX, CellY].DynamicObjectsHere.Remove(GUID);

                // DONE: Remove the dynamic object from players that can see the object
                foreach (ulong plGUID in SeenBy.ToArray())
                {
                    if (WorldServiceLocator._WorldServer.CHARACTERs[plGUID].dynamicObjectsNear.Contains(GUID))
                    {
                        WorldServiceLocator._WorldServer.CHARACTERs[plGUID].guidsForRemoving_Lock.AcquireWriterLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
                        WorldServiceLocator._WorldServer.CHARACTERs[plGUID].guidsForRemoving.Add(GUID);
                        WorldServiceLocator._WorldServer.CHARACTERs[plGUID].guidsForRemoving_Lock.ReleaseWriterLock();
                        WorldServiceLocator._WorldServer.CHARACTERs[plGUID].dynamicObjectsNear.Remove(GUID);
                    }
                }
            }

            public void AddEffect(WS_Spells.SpellEffect EffectInfo)
            {
                Effects.Add(EffectInfo);
            }

            public void RemoveEffect(WS_Spells.SpellEffect EffectInfo)
            {
                Effects.Remove(EffectInfo);
            }

            public bool Update()
            {
                // DONE: Remove if caster doesn't exist
                if (Caster is null)
                {
                    return true;
                }

                // DONE: Tick down
                bool DeleteThis = false;
                if (Duration > WS_TimerBasedEvents.TSpellManager.UPDATE_TIMER)
                {
                    Duration -= WS_TimerBasedEvents.TSpellManager.UPDATE_TIMER;
                }
                else
                {
                    DeleteThis = true;
                }

                // DONE: Do the spell
                foreach (WS_Spells.SpellEffect Effect in Effects)
                {
                    if (Effect.GetRadius == 0f)
                    {
                        if (Effect.Amplitude == 0 || (WorldServiceLocator._WS_Spells.SPELLs[SpellID].GetDuration - Duration) % Effect.Amplitude == 0)
                        {
                            var argCaster = this;
                            WorldServiceLocator._WS_Spells.AURAs[Effect.ApplyAuraIndex].Invoke(ref Caster, ref argCaster, ref Effect, SpellID, 1, AuraAction.AURA_UPDATE);
                        }
                    }
                    else
                    {
                        var Targets = WorldServiceLocator._WS_Spells.GetEnemyAtPoint(ref Caster, positionX, positionY, positionZ, Effect.GetRadius);
                        foreach (WS_Base.BaseUnit Target in Targets)
                        {
                            if (Effect.Amplitude == 0 || (WorldServiceLocator._WS_Spells.SPELLs[SpellID].GetDuration - Duration) % Effect.Amplitude == 0)
                            {
                                var argCaster1 = this;
                                WorldServiceLocator._WS_Spells.AURAs[Effect.ApplyAuraIndex].Invoke(ref Target, ref argCaster1, ref Effect, SpellID, 1, AuraAction.AURA_UPDATE);
                            }
                        }
                    }
                }

                // DONE: Remove when done
                if (DeleteThis)
                {
                    Caster.dynamicObjects.Remove(this);
                    return true;
                }

                return false;
            }

            public void Spawn()
            {
                AddToWorld();

                // DONE: Send spawn animation
                var packet = new Packets.PacketClass(OPCODES.SMSG_GAMEOBJECT_SPAWN_ANIM);
                packet.AddUInt64(GUID);
                SendToNearPlayers(ref packet);
                packet.Dispose();
            }

            public void Delete()
            {
                // DONE: Remove the reference between the dynamic object and the caster
                if (Caster is object && Caster.dynamicObjects.Contains(this))
                    Caster.dynamicObjects.Remove(this);

                // DONE: Send despawn animation
                var packet = new Packets.PacketClass(OPCODES.SMSG_GAMEOBJECT_DESPAWN_ANIM);
                packet.AddUInt64(GUID);
                SendToNearPlayers(ref packet);
                packet.Dispose();
                RemoveFromWorld();
                Dispose();
            }
        }
    }
}