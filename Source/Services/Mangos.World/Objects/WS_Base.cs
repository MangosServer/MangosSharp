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
using System.Runtime.InteropServices;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Player;
using Mangos.Common.Enums.Spell;
using Mangos.Common.Enums.Unit;
using Mangos.Common.Globals;
using Mangos.World.Globals;
using Mangos.World.Player;
using Mangos.World.Spells;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.World.Objects
{
    public class WS_Base
    {
        public class BaseObject
        {
            public BaseObject()
            {
                CorpseType = CorpseType.CORPSE_BONES;
            }

            public ulong GUID = 0UL;
            public byte CellX = 0;
            public byte CellY = 0;
            public float positionX = 0f;
            public float positionY = 0f;
            public float positionZ = 0f;
            public float orientation = 0f;
            public uint instance = 0U;
            public uint MapID = 0U;
            public CorpseType CorpseType;
            public int SpawnID = 0;
            public List<ulong> SeenBy = new List<ulong>();
            public float VisibleDistance = WorldServiceLocator._Global_Constants.DEFAULT_DISTANCE_VISIBLE;
            public InvisibilityLevel Invisibility = InvisibilityLevel.VISIBLE;
            public int Invisibility_Value = 0;
            public int Invisibility_Bonus = 0;
            public InvisibilityLevel CanSeeInvisibility = InvisibilityLevel.INIVISIBILITY;
            public int CanSeeInvisibility_Stealth = 0;
            public bool CanSeeStealth = false;
            public int CanSeeInvisibility_Invisibility = 0;

            public virtual bool CanSee(ref BaseObject objCharacter)
            {
                if (GUID == objCharacter.GUID)
                    return false;
                if (instance != objCharacter.instance)
                    return false;

                // DONE: GM and DEAD invisibility
                if (objCharacter.Invisibility > CanSeeInvisibility)
                    return false;
                // DONE: Stealth Detection
                if (objCharacter.Invisibility == InvisibilityLevel.STEALTH && Math.Sqrt(Math.Pow(objCharacter.positionX - positionX, 2d) + Math.Pow(objCharacter.positionY - positionY, 2d)) < WorldServiceLocator._Global_Constants.DEFAULT_DISTANCE_DETECTION)
                    return true;
                // DONE: Check invisibility
                if (objCharacter.Invisibility == InvisibilityLevel.INIVISIBILITY && objCharacter.Invisibility_Value > CanSeeInvisibility_Invisibility)
                    return false;
                if (objCharacter.Invisibility == InvisibilityLevel.STEALTH && objCharacter.Invisibility_Value > CanSeeInvisibility_Stealth)
                    return false;

                // DONE: Check distance
                if (Math.Sqrt(Math.Pow(objCharacter.positionX - positionX, 2d) + Math.Pow(objCharacter.positionY - positionY, 2d)) > objCharacter.VisibleDistance)
                    return false;
                return true;
            }

            public void InvisibilityReset()
            {
                Invisibility = InvisibilityLevel.VISIBLE;
                Invisibility_Value = 0;
                CanSeeInvisibility = InvisibilityLevel.INIVISIBILITY;
                CanSeeInvisibility_Stealth = 0;
                CanSeeInvisibility_Invisibility = 0;
            }

            public void SendPlaySound(int SoundID, bool OnlyToSelf = false)
            {
                var packet = new Packets.PacketClass(OPCODES.SMSG_PLAY_OBJECT_SOUND);
                try
                {
                    packet.AddInt32(SoundID);
                    packet.AddUInt64(GUID);
                    if (OnlyToSelf && this is WS_PlayerData.CharacterObject)
                    {
                        ((WS_PlayerData.CharacterObject)this).client.Send(packet);
                    }
                    else
                    {
                        SendToNearPlayers(ref packet);
                    }
                }
                finally
                {
                    packet.Dispose();
                }
            }

            public void SendToNearPlayers(ref Packets.PacketClass packet, ulong NotTo = 0UL, bool ToSelf = true)
            {
                if (ToSelf && this is WS_PlayerData.CharacterObject && ((WS_PlayerData.CharacterObject)this).client is object)
                    ((WS_PlayerData.CharacterObject)this).client.SendMultiplyPackets(ref packet);
                foreach (ulong objCharacter in SeenBy.ToArray())
                {
                    if (objCharacter != NotTo && WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(objCharacter) && WorldServiceLocator._WorldServer.CHARACTERs[objCharacter].client is object)
                        WorldServiceLocator._WorldServer.CHARACTERs[objCharacter].client.SendMultiplyPackets(ref packet);
                }
            }
        }

        public class BaseUnit : BaseObject
        {
            public const float CombatReach_Base = 2.0f;
            public WS_GameObjects.GameObjectObject OnTransport = null;
            public float transportX = 0.0f;
            public float transportY = 0.0f;
            public float transportZ = 0.0f;
            public float transportO = 0.0f;
            public float BoundingRadius = 0.389f;
            public float CombatReach = 1.5f;
            public int cUnitFlags = (int)UnitFlags.UNIT_FLAG_ATTACKABLE;
            public int cDynamicFlags = 0; // DynamicFlags.UNIT_DYNFLAG_SPECIALINFO

            // <<0                <<8             <<16                <<24
            public int cBytes0 = 0;                       // Race               Classe          Gender              ManaType
            public int cBytes1 = 0;                       // StandState,        PetLoyalty,     ShapeShift,         StealthFlag [CType(Invisibility > InvisibilityLevel.VISIBLE, Integer) * 2 << 24]
            public int cBytes2 = (int)0xEEEEEE00;              // ?                  ?               ?                   ?

            // cBytes0 subfields
            public virtual ManaTypes ManaType
            {
                get
                {
                    return (ManaTypes)((cBytes0 & 0xFF000000) >> 24);
                }

                set
                {
                    cBytes0 = cBytes0 & 0xFFFFFF | value << 24;
                }
            }

            public virtual Genders Gender
            {
                get
                {
                    return (Genders)((cBytes0 & 0xFF0000) >> 16);
                }

                set
                {
                    cBytes0 = cBytes0 & 0xFF00FFFF | value << 16;
                }
            }

            public virtual Classes Classe
            {
                get
                {
                    return (Classes)((cBytes0 & 0xFF00) >> 8);
                }

                set
                {
                    cBytes0 = cBytes0 & 0xFFFF00FF | value << 8;
                }
            }

            public virtual Races Race
            {
                get
                {
                    return (Races)((cBytes0 & 0xFF) >> 0);
                }

                set
                {
                    cBytes0 = cBytes0 & 0xFFFFFF00 | value << 0;
                }
            }

            public string UnitName
            {
                get
                {
                    if (this is WS_PlayerData.CharacterObject)
                    {
                        return ((WS_PlayerData.CharacterObject)this).Name;
                    }
                    else if (this is WS_Creatures.CreatureObject)
                    {
                        return ((WS_Creatures.CreatureObject)this).Name;
                    }
                    else
                    {
                        return "";
                    }
                }
            }

            // cBytes1 subfields
            public virtual byte StandState
            {
                get
                {
                    return (byte)((cBytes1 & 0xFF) >> 0);
                }

                set
                {
                    cBytes1 = (int)(cBytes1 & 0xFFFFFF00 | value << 0);
                }
            }

            public virtual byte PetLoyalty
            {
                get
                {
                    return (byte)((cBytes1 & 0xFF00) >> 8);
                }

                set
                {
                    cBytes1 = (int)(cBytes1 & 0xFFFF00FF | value << 8);
                }
            }

            public virtual ShapeshiftForm ShapeshiftForm
            {
                get
                {
                    return (ShapeshiftForm)((cBytes1 & 0xFF0000) >> 16);
                }

                set
                {
                    cBytes1 = cBytes1 & 0xFF00FFFF | value << 16;
                }
            }

            public byte Level = 0;
            public int Model = 0;
            public int Mount = 0;
            public WS_PlayerHelper.TStatBar Life = new WS_PlayerHelper.TStatBar(1, 1, 0);
            public WS_PlayerHelper.TStatBar Mana = new WS_PlayerHelper.TStatBar(1, 1, 0);
            public float Size = 1.0f;
            public WS_PlayerHelper.TStat[] Resistances = new WS_PlayerHelper.TStat[7];
            public byte SchoolImmunity = 0;
            public uint MechanicImmunity = 0U;
            public uint DispellImmunity = 0U;
            public Dictionary<int, uint> AbsorbSpellLeft = new Dictionary<int, uint>();
            public bool Invulnerable = false;
            public ulong SummonedBy = 0UL;
            public ulong CreatedBy = 0UL;
            public int CreatedBySpell = 0;
            public int cEmoteState = 0;

            // Temporaly variables
            public int AuraState = 0;
            public bool Spell_Silenced = false;
            public bool Spell_Pacifyed = false;
            public float Spell_ThreatModifier = 1.0f;
            public int AttackPowerMods = 0;
            public int AttackPowerModsRanged = 0;
            public List<WS_DynamicObjects.DynamicObjectObject> dynamicObjects = new List<WS_DynamicObjects.DynamicObjectObject>();
            public List<WS_GameObjects.GameObjectObject> gameObjects = new List<WS_GameObjects.GameObjectObject>();

            public virtual void Die(ref BaseUnit Attacker)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "BaseUnit can't die.");
            }

            public virtual void DealDamage(int Damage, [Optional, DefaultParameterValue(null)] ref BaseUnit Attacker)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "No damage dealt.");
            }

            public virtual void Heal(int Damage, [Optional, DefaultParameterValue(null)] ref BaseUnit Attacker)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "No healing done.");
            }

            public virtual void Energize(int Damage, ManaTypes Power, [Optional, DefaultParameterValue(null)] BaseUnit Attacker)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "No mana increase done.");
            }

            public virtual bool IsDead
            {
                get
                {
                    return Life.Current == 0;
                }
            }

            public virtual bool Exist
            {
                get
                {
                    if (this is WS_PlayerData.CharacterObject)
                    {
                        return WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(GUID);
                    }
                    else if (this is WS_Creatures.CreatureObject)
                    {
                        return WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(GUID);
                    }

                    return false;
                }
            }

            public virtual bool IsRooted
            {
                get
                {
                    return cUnitFlags & UnitFlags.UNIT_FLAG_ROOTED;
                }
            }

            public virtual bool IsStunned
            {
                get
                {
                    return cUnitFlags & UnitFlags.UNIT_FLAG_STUNTED;
                }
            }

            public bool IsInFeralForm
            {
                get
                {
                    return ShapeshiftForm == this.ShapeshiftForm.FORM_CAT || ShapeshiftForm == this.ShapeshiftForm.FORM_BEAR || ShapeshiftForm == this.ShapeshiftForm.FORM_DIREBEAR;
                }
            }

            public bool IsPlayer
            {
                get
                {
                    return this is WS_PlayerData.CharacterObject;
                }
            }

            public virtual bool IsFriendlyTo(ref BaseUnit Unit)
            {
                return false;
            }

            public virtual bool IsEnemyTo(ref BaseUnit Unit)
            {
                return false;
            }

            // Spell Aura Managment
            public BaseActiveSpell[] ActiveSpells = new BaseActiveSpell[WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs];
            public int[] ActiveSpells_Flags = new int[WorldServiceLocator._Global_Constants.MAX_AURA_EFFECT_FLAGs];
            public int[] ActiveSpells_Count = new int[WorldServiceLocator._Global_Constants.MAX_AURA_EFFECT_LEVELSs];
            public int[] ActiveSpells_Level = new int[WorldServiceLocator._Global_Constants.MAX_AURA_EFFECT_LEVELSs];

            public void SetAura(int SpellID, int Slot, int Duration, bool SendUpdate = true)
            {
                if (ActiveSpells[Slot] is null)
                    return;
                // DONE: Passive auras are not displayed
                if (Conversions.ToBoolean(SpellID) && WorldServiceLocator._WS_Spells.SPELLs.ContainsKey(SpellID) && WorldServiceLocator._WS_Spells.SPELLs[SpellID].IsPassive)
                    return;

                // DONE: Calculating slots
                int AuraLevel_Slot = Slot / 4;
                int AuraFlag_Slot = Slot >> 3;
                int AuraFlag_SubSlot = (Slot & 7) << 2;
                int AuraFlag_Value = 9 << AuraFlag_SubSlot;
                ActiveSpells_Flags[AuraFlag_Slot] = ActiveSpells_Flags[AuraFlag_Slot] & ~AuraFlag_Value;
                if (SpellID != 0)
                {
                    ActiveSpells_Flags[AuraFlag_Slot] = ActiveSpells_Flags[AuraFlag_Slot] | AuraFlag_Value;
                }

                byte tmpLevel = 0;
                if (Conversions.ToBoolean(SpellID))
                    tmpLevel = (byte)WorldServiceLocator._WS_Spells.SPELLs[SpellID].spellLevel;
                SetAuraStackCount(Slot, 0);
                SetAuraSlotLevel(Slot, tmpLevel);

                // DONE: Sending updates
                if (SendUpdate)
                {
                    if (this is WS_PlayerData.CharacterObject)
                    {
                        ((WS_PlayerData.CharacterObject)this).SetUpdateFlag((int)(EUnitFields.UNIT_FIELD_AURA + Slot), SpellID);
                        ((WS_PlayerData.CharacterObject)this).SetUpdateFlag((int)(EUnitFields.UNIT_FIELD_AURAFLAGS + AuraFlag_Slot), ActiveSpells_Flags[AuraFlag_Slot]);
                        ((WS_PlayerData.CharacterObject)this).SetUpdateFlag((int)(EUnitFields.UNIT_FIELD_AURAAPPLICATIONS + AuraLevel_Slot), ActiveSpells_Count[AuraLevel_Slot]);
                        ((WS_PlayerData.CharacterObject)this).SetUpdateFlag((int)(EUnitFields.UNIT_FIELD_AURALEVELS + AuraLevel_Slot), ActiveSpells_Level[AuraLevel_Slot]);
                        ((WS_PlayerData.CharacterObject)this).SendCharacterUpdate(true);
                        var SMSG_UPDATE_AURA_DURATION = new Packets.PacketClass(OPCODES.SMSG_UPDATE_AURA_DURATION);
                        try
                        {
                            SMSG_UPDATE_AURA_DURATION.AddInt8((byte)Slot);
                            SMSG_UPDATE_AURA_DURATION.AddInt32(Duration);
                            ((WS_PlayerData.CharacterObject)this).client.Send(SMSG_UPDATE_AURA_DURATION);
                        }
                        finally
                        {
                            SMSG_UPDATE_AURA_DURATION.Dispose();
                        }
                    }
                    else
                    {
                        var tmpUpdate = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_PLAYER);
                        var tmpPacket = new Packets.UpdatePacketClass();
                        try
                        {
                            tmpUpdate.SetUpdateFlag((int)(EUnitFields.UNIT_FIELD_AURA + Slot), SpellID);
                            tmpUpdate.SetUpdateFlag((int)(EUnitFields.UNIT_FIELD_AURAFLAGS + AuraFlag_Slot), ActiveSpells_Flags[AuraFlag_Slot]);
                            tmpUpdate.SetUpdateFlag((int)(EUnitFields.UNIT_FIELD_AURAAPPLICATIONS + AuraLevel_Slot), ActiveSpells_Count[AuraLevel_Slot]);
                            tmpUpdate.SetUpdateFlag((int)(EUnitFields.UNIT_FIELD_AURALEVELS + AuraLevel_Slot), ActiveSpells_Level[AuraLevel_Slot]);
                            tmpUpdate.AddToPacket(tmpPacket, ObjectUpdateType.UPDATETYPE_VALUES, (WS_Creatures.CreatureObject)this);
                            Packets.PacketClass argpacket = tmpPacket;
                            SendToNearPlayers(ref argpacket);
                        }
                        finally
                        {
                            tmpPacket.Dispose();
                            tmpUpdate.Dispose();
                        }
                    }
                }
            }

            public void SetAuraStackCount(int Slot, byte Count)
            {
                // NOTE: Stack count is Zero based -> 2 means "Stacked 3 times"

                int AuraFlag_Slot = Slot / 4;
                int AuraFlag_SubSlot = Slot % 4 * 8;
                ActiveSpells_Count[AuraFlag_Slot] = ActiveSpells_Count[AuraFlag_Slot] & ~(0xFF << AuraFlag_SubSlot);
                ActiveSpells_Count[AuraFlag_Slot] = ActiveSpells_Count[AuraFlag_Slot] | Count << AuraFlag_SubSlot;
            }

            public void SetAuraSlotLevel(int Slot, int Level)
            {
                int AuraFlag_Slot = Slot / 4;
                int AuraFlag_SubSlot = Slot % 4 * 8;
                ActiveSpells_Level[AuraFlag_Slot] = ActiveSpells_Level[AuraFlag_Slot] & ~(0xFF << AuraFlag_SubSlot);
                ActiveSpells_Level[AuraFlag_Slot] = ActiveSpells_Level[AuraFlag_Slot] | Level << AuraFlag_SubSlot;
            }

            public bool HaveAura(int SpellID)
            {
                for (byte i = 0, loopTo = (byte)(WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs - 1); i <= loopTo; i++)
                {
                    if (ActiveSpells[i] is object && ActiveSpells[i].SpellID == SpellID)
                        return true;
                }

                return false;
            }

            public bool HaveAuraType(AuraEffects_Names AuraIndex)
            {
                for (int i = 0, loopTo = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs_VISIBLE - 1; i <= loopTo; i++)
                {
                    if (ActiveSpells[i] is object)
                    {
                        for (byte j = 0; j <= 2; j++)
                        {
                            if (ActiveSpells[i].Aura_Info[j] is object && ActiveSpells[i].Aura_Info[j].ApplyAuraIndex == AuraIndex)
                            {
                                return true;
                            }
                        }
                    }
                }

                return false;
            }

            public bool HaveVisibleAura(int SpellID)
            {
                for (byte i = 0, loopTo = (byte)(WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs_VISIBLE - 1); i <= loopTo; i++)
                {
                    if (ActiveSpells[i] is object && ActiveSpells[i].SpellID == SpellID)
                        return true;
                }

                return false;
            }

            public bool HavePassiveAura(int SpellID)
            {
                for (byte i = (byte)WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs_VISIBLE, loopTo = (byte)(WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs - 1); i <= loopTo; i++)
                {
                    if (ActiveSpells[i] is object && ActiveSpells[i].SpellID == SpellID)
                        return true;
                }

                return false;
            }

            public void RemoveAura(int Slot, ref BaseUnit Caster, bool RemovedByDuration = false, bool SendUpdate = true)
            {
                // DONE: Removing SpellAura
                AuraAction RemoveAction = AuraAction.AURA_REMOVE;
                if (RemovedByDuration)
                    RemoveAction = AuraAction.AURA_REMOVEBYDURATION;
                if (ActiveSpells[Slot] is object)
                {
                    for (byte j = 0; j <= 2; j++)
                    {
                        if (ActiveSpells[Slot].Aura[j] is object)
                        {
                            var argTarget = this;
                            ActiveSpells[Slot].Aura[(int)j].Invoke(ref argTarget, ref (BaseObject)Caster, ref ActiveSpells[Slot].Aura_Info[(int)j], ActiveSpells[Slot].SpellID, ActiveSpells[Slot].StackCount + 1, RemoveAction);
                        }
                    }
                }

                if (SendUpdate && Slot < WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs_VISIBLE)
                    SetAura(0, Slot, 0);
                ActiveSpells[Slot] = null;
            }

            public void RemoveAuraBySpell(int SpellID)
            {
                // DONE: Real aura removing
                for (int i = 0, loopTo = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs - 1; i <= loopTo; i++)
                {
                    if (ActiveSpells[i] is object && ActiveSpells[i].SpellID == SpellID)
                    {
                        RemoveAura(i, ref ActiveSpells[i].SpellCaster);

                        // DONE: Removing additional spell auras (Mind Vision)
                        if (this is WS_PlayerData.CharacterObject && ((WS_PlayerData.CharacterObject)this).DuelArbiter != 0m && ((WS_PlayerData.CharacterObject)this).DuelPartner is null)
                        {
                            WorldServiceLocator._WorldServer.WORLD_CREATUREs[((WS_PlayerData.CharacterObject)this).DuelArbiter].RemoveAuraBySpell(SpellID);
                            ((WS_PlayerData.CharacterObject)this).DuelArbiter = 0UL;
                        }

                        return;
                    }
                }
            }

            public void RemoveAurasOfType(AuraEffects_Names AuraIndex, [Optional, DefaultParameterValue(0)] ref int NotSpellID)
            {
                // DONE: Removing SpellAuras of a certain type
                for (int i = 0, loopTo = (byte)(WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs_VISIBLE - 1); i <= loopTo; i++)
                {
                    if (ActiveSpells[i] is object && ActiveSpells[i].SpellID != NotSpellID)
                    {
                        for (byte j = 0; j <= 2; j++)
                        {
                            if (ActiveSpells[i].Aura_Info[j] is object && ActiveSpells[i].Aura_Info[j].ApplyAuraIndex == AuraIndex)
                            {
                                RemoveAura(i, ref ActiveSpells[i].SpellCaster);
                                break;
                            }
                        }
                    }
                }
            }

            public void RemoveAurasByMechanic(int Mechanic)
            {
                // DONE: Removing SpellAuras of a certain mechanic
                for (int i = 0, loopTo = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs_VISIBLE - 1; i <= loopTo; i++)
                {
                    if (ActiveSpells[i] is object && WorldServiceLocator._WS_Spells.SPELLs[ActiveSpells[i].SpellID].Mechanic == Mechanic)
                    {
                        RemoveAura(i, ref ActiveSpells[i].SpellCaster);
                    }
                }
            }

            public void RemoveAurasByDispellType(int DispellType, int Amount)
            {
                // DONE: Removing SpellAuras of a certain dispelltype
                for (int i = 0, loopTo = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs_VISIBLE - 1; i <= loopTo; i++)
                {
                    if (ActiveSpells[i] is object && WorldServiceLocator._WS_Spells.SPELLs[ActiveSpells[i].SpellID].DispellType == DispellType)
                    {
                        RemoveAura(i, ref ActiveSpells[i].SpellCaster);
                        Amount -= 1;
                        if (Amount <= 0)
                            break;
                    }
                }
            }

            public void RemoveAurasByInterruptFlag(int AuraInterruptFlag)
            {
                // DONE: Removing SpellAuras with a certain interruptflag
                for (int i = 0, loopTo = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs_VISIBLE - 1; i <= loopTo; i++)
                {
                    if (ActiveSpells[i] is object)
                    {
                        if (WorldServiceLocator._WS_Spells.SPELLs.ContainsKey(ActiveSpells[i].SpellID) && Conversions.ToBoolean(WorldServiceLocator._WS_Spells.SPELLs[ActiveSpells[i].SpellID].auraInterruptFlags & AuraInterruptFlag))
                        {
                            if ((WorldServiceLocator._WS_Spells.SPELLs[ActiveSpells[i].SpellID].procFlags & SpellAuraProcFlags.AURA_PROC_REMOVEONUSE) == 0)
                            {
                                RemoveAura(i, ref ActiveSpells[i].SpellCaster);
                            }
                        }
                    }
                }

                // DONE: Interrupt channeled spells
                if (this is WS_PlayerData.CharacterObject)
                {
                    {
                        var withBlock = (WS_PlayerData.CharacterObject)this;
                        if (withBlock.spellCasted[CurrentSpellTypes.CURRENT_CHANNELED_SPELL] is object && withBlock.spellCasted[CurrentSpellTypes.CURRENT_CHANNELED_SPELL].Finished == false && (withBlock.spellCasted[CurrentSpellTypes.CURRENT_CHANNELED_SPELL].SpellInfo.channelInterruptFlags & AuraInterruptFlag) != 0)
                        {
                            withBlock.FinishSpell(CurrentSpellTypes.CURRENT_CHANNELED_SPELL);
                        }
                    }
                }
                else if (this is WS_Creatures.CreatureObject)
                {
                    {
                        var withBlock1 = (WS_Creatures.CreatureObject)this;
                        if (withBlock1.SpellCasted is object && (withBlock1.SpellCasted.SpellInfo.channelInterruptFlags & AuraInterruptFlag) != 0)
                        {
                            withBlock1.StopCasting();
                        }
                    }
                }
            }

            public int GetAuraModifier(AuraEffects_Names AuraIndex)
            {
                int Modifier = 0;
                for (int i = 0, loopTo = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs_VISIBLE - 1; i <= loopTo; i++)
                {
                    if (ActiveSpells[i] is object)
                    {
                        for (byte j = 0; j <= 2; j++)
                        {
                            if (ActiveSpells[i].Aura_Info[j] is object && ActiveSpells[i].Aura_Info[j].ApplyAuraIndex == AuraIndex)
                            {
                                Modifier += ActiveSpells[i].Aura_Info[j].get_GetValue(Level, 0);
                            }
                        }
                    }
                }

                return Modifier;
            }

            public int GetAuraModifierByMiscMask(AuraEffects_Names AuraIndex, int Mask)
            {
                int Modifier = 0;
                for (int i = 0, loopTo = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs_VISIBLE - 1; i <= loopTo; i++)
                {
                    if (ActiveSpells[i] is object)
                    {
                        for (byte j = 0; j <= 2; j++)
                        {
                            if (ActiveSpells[i].Aura_Info[j] is object && ActiveSpells[i].Aura_Info[j].ApplyAuraIndex == AuraIndex && (ActiveSpells[i].Aura_Info[j].MiscValue & Mask) == Mask)
                            {
                                Modifier += ActiveSpells[i].Aura_Info[j].get_GetValue(Level, 0);
                            }
                        }
                    }
                }

                return Modifier;
            }

            public void AddAura(int SpellID, int Duration, ref BaseUnit Caster)
            {
                int AuraStart = 0;
                int AuraEnd = WorldServiceLocator._Global_Constants.MAX_POSITIVE_AURA_EFFECTs - 1;
                if (WorldServiceLocator._WS_Spells.SPELLs[SpellID].IsPassive)
                {
                    AuraStart = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs_VISIBLE;
                    AuraEnd = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs;
                }
                else if (WorldServiceLocator._WS_Spells.SPELLs[SpellID].IsNegative)
                {
                    AuraStart = WorldServiceLocator._Global_Constants.MAX_POSITIVE_AURA_EFFECTs;
                    AuraEnd = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs_VISIBLE - 1;
                }

                // Try to remove spells that can't be used at the same time as this one
                try
                {
                    if (!WorldServiceLocator._WS_Spells.SPELLs[SpellID].IsPassive)
                    {
                        var SpellInfo = WorldServiceLocator._WS_Spells.SPELLs[SpellID];
                        for (int slot = 0, loopTo = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs_VISIBLE - 1; slot <= loopTo; slot++)
                        {
                            if (ActiveSpells[slot] is object && ActiveSpells[slot].GetSpellInfo.Target == SpellInfo.Target && ActiveSpells[slot].GetSpellInfo.Category == SpellInfo.Category && ActiveSpells[slot].GetSpellInfo.SpellIconID == SpellInfo.SpellIconID && ActiveSpells[slot].GetSpellInfo.SpellVisual == SpellInfo.SpellVisual && ActiveSpells[slot].GetSpellInfo.Attributes == SpellInfo.Attributes && ActiveSpells[slot].GetSpellInfo.AttributesEx == SpellInfo.AttributesEx && ActiveSpells[slot].GetSpellInfo.AttributesEx2 == SpellInfo.AttributesEx2)
                            {
                                RemoveAura(slot, ref ActiveSpells[slot].SpellCaster);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "ERROR ADDING AURA!{0}{1}", Environment.NewLine, ex.ToString());
                }

                for (int slot = AuraStart, loopTo1 = AuraEnd; slot <= loopTo1; slot++)
                {
                    if (ActiveSpells[slot] is null)
                    {
                        // DONE: Adding New SpellAura
                        ActiveSpells[slot] = new BaseActiveSpell(SpellID, Duration) { SpellCaster = Caster };
                        if (slot < WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs_VISIBLE)
                            SetAura(SpellID, slot, Duration);
                        break;
                    }
                }

                if (this is WS_PlayerData.CharacterObject)
                {
                    ((WS_PlayerData.CharacterObject)this).GroupUpdateFlag = ((WS_PlayerData.CharacterObject)this).GroupUpdateFlag | (uint)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_AURAS;
                }
                else if (this is WS_Pets.PetObject && ((WS_Pets.PetObject)this).Owner is WS_PlayerData.CharacterObject)
                {
                    ((WS_PlayerData.CharacterObject)((WS_Pets.PetObject)this).Owner).GroupUpdateFlag = ((WS_PlayerData.CharacterObject)((WS_Pets.PetObject)this).Owner).GroupUpdateFlag | (uint)Globals.Functions.PartyMemberStatsFlag.GROUP_UPDATE_FLAG_PET_AURAS;
                }
            }

            public void UpdateAura(int Slot)
            {
                if (ActiveSpells[Slot] is null)
                    return;
                if (Slot >= WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs_VISIBLE)
                    return;
                int AuraFlag_Slot = Slot / 4;
                int AuraFlag_SubSlot = Slot % 4 * 8;
                SetAuraStackCount(Slot, (byte)ActiveSpells[Slot].StackCount);
                if (this is WS_PlayerData.CharacterObject)
                {
                    ((WS_PlayerData.CharacterObject)this).SetUpdateFlag((int)(EUnitFields.UNIT_FIELD_AURAAPPLICATIONS + AuraFlag_Slot), ActiveSpells_Count[AuraFlag_Slot]);
                    ((WS_PlayerData.CharacterObject)this).SendCharacterUpdate(true);
                    var SMSG_UPDATE_AURA_DURATION = new Packets.PacketClass(OPCODES.SMSG_UPDATE_AURA_DURATION);
                    try
                    {
                        SMSG_UPDATE_AURA_DURATION.AddInt8((byte)Slot);
                        SMSG_UPDATE_AURA_DURATION.AddInt32(ActiveSpells[Slot].SpellDuration);
                        ((WS_PlayerData.CharacterObject)this).client.Send(SMSG_UPDATE_AURA_DURATION);
                    }
                    finally
                    {
                        SMSG_UPDATE_AURA_DURATION.Dispose();
                    }
                }
                else
                {
                    var tmpUpdate = new Packets.UpdateClass(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_PLAYER);
                    var tmpPacket = new Packets.UpdatePacketClass();
                    try
                    {
                        tmpUpdate.SetUpdateFlag((int)(EUnitFields.UNIT_FIELD_AURAAPPLICATIONS + AuraFlag_Slot), ActiveSpells_Count[AuraFlag_Slot]);
                        tmpUpdate.AddToPacket(tmpPacket, ObjectUpdateType.UPDATETYPE_VALUES, (WS_Creatures.CreatureObject)this);
                        Packets.PacketClass argpacket = tmpPacket;
                        SendToNearPlayers(ref argpacket);
                    }
                    finally
                    {
                        tmpPacket.Dispose();
                        tmpUpdate.Dispose();
                    }
                }
            }

            public void DoEmote(int EmoteID)
            {
                var packet = new Packets.PacketClass(OPCODES.SMSG_EMOTE);
                try
                {
                    packet.AddInt32(EmoteID);
                    packet.AddUInt64(GUID);
                    SendToNearPlayers(ref packet);
                }
                finally
                {
                    packet.Dispose();
                }
            }

            public void DealSpellDamage(BaseUnit Caster, WS_Spells.SpellEffect EffectInfo, int SpellID, int Damage, DamageTypes DamageType, SpellType SpellType)
            {
                bool IsHeal = false;
                bool IsDot = false;
                switch (SpellType)
                {
                    case var @case when @case == SpellType.SPELL_TYPE_HEAL:
                        {
                            IsHeal = true;
                            break;
                        }

                    case var case1 when case1 == SpellType.SPELL_TYPE_HEALDOT:
                        {
                            IsHeal = true;
                            IsDot = true;
                            break;
                        }

                    case var case2 when case2 == SpellType.SPELL_TYPE_DOT:
                        {
                            IsDot = true;
                            break;
                        }
                }
                // If SpellType = SpellType.SPELL_TYPE_HEAL Or SpellType = SpellType.SPELL_TYPE_HEALDOT Then
                // IsHeal = True
                // End If
                // If SpellType = SpellType.SPELL_TYPE_DOT OrElse SpellType = SpellType.SPELL_TYPE_HEALDOT Then
                // IsDot = True
                // End If

                int SpellDamageBenefit = 0;
                if (Caster is WS_PlayerData.CharacterObject)
                {
                    {
                        int PenaltyFactor = 0;
                        int EffectCount = 0;
                        for (int i = 0; i <= 2; i++)
                        {
                            if (WorldServiceLocator._WS_Spells.SPELLs[SpellID].SpellEffects[i] is object)
                                EffectCount += 1;
                        }

                        if (EffectCount > 1)
                            PenaltyFactor = 5;
                        int SpellDamage;
                        if (IsHeal)
                        {
                            SpellDamage = ((WS_PlayerData.CharacterObject)Caster).healing.Value;
                        }
                        else
                        {
                            SpellDamage = ((WS_PlayerData.CharacterObject)Caster).spellDamage[DamageType].Value;
                        }

                        if (IsDot)
                        {
                            int TickAmount = (int)(WorldServiceLocator._WS_Spells.SPELLs[SpellID].GetDuration / (double)EffectInfo.Amplitude);
                            if (TickAmount < 5)
                                TickAmount = 5;
                            SpellDamageBenefit = SpellDamage / TickAmount;
                        }
                        else
                        {
                            int CastTime = WorldServiceLocator._WS_Spells.SPELLs[SpellID].GetCastTime;
                            if (CastTime < 1500)
                                CastTime = 1500;
                            if (CastTime > 3500)
                                CastTime = 3500;
                            SpellDamageBenefit = (int)Conversion.Fix(SpellDamage * (CastTime / 1000.0f) * ((100 - PenaltyFactor) / 100d) / 3.5d);
                        }

                        if (WorldServiceLocator._WS_Spells.SPELLs[SpellID].IsAOE)
                            SpellDamageBenefit /= 3;
                    }
                }

                Damage += SpellDamageBenefit;

                // TODO: Crit
                bool IsCrit = false;
                if (!IsDot)
                {
                    if (Caster is WS_PlayerData.CharacterObject)
                    {
                        // TODO: Get crit with only the same spell school
                        if (WorldServiceLocator._Functions.RollChance(((WS_PlayerData.CharacterObject)Caster).GetCriticalWithSpells))
                        {
                            Damage = (int)Conversion.Fix(1.5f * Damage);
                            IsCrit = true;
                        }
                    }
                }

                int Resist = 0;
                int Absorb = 0;
                if (!IsHeal)
                {
                    // DONE: Damage reduction
                    float DamageReduction = GetDamageReduction(ref Caster, DamageType, Damage);
                    Damage = (int)(Damage - Damage * DamageReduction);

                    // DONE: Resist
                    if (Damage > 0)
                    {
                        Resist = (int)GetResist(ref Caster, DamageType, Damage);
                        if (Resist > 0)
                            Damage -= Resist;
                    }

                    // DONE: Absorb
                    if (Damage > 0)
                    {
                        Absorb = GetAbsorb(DamageType, Damage);
                        if (Absorb > 0)
                            Damage -= Absorb;
                    }

                    DealDamage(Damage, ref Caster);
                }
                else
                {
                    Heal(Damage, ref Caster);
                }

                // DONE: Send log
                switch (SpellType)
                {
                    case var case3 when case3 == SpellType.SPELL_TYPE_NONMELEE:
                        {
                            var argTarget = this;
                            WorldServiceLocator._WS_Spells.SendNonMeleeDamageLog(ref Caster, ref argTarget, SpellID, (int)DamageType, Damage, Resist, Absorb, IsCrit);
                            break;
                        }

                    case var case4 when case4 == SpellType.SPELL_TYPE_DOT:
                        {
                            var argTarget1 = this;
                            WorldServiceLocator._WS_Spells.SendPeriodicAuraLog(Caster, argTarget1, SpellID, (int)DamageType, Damage, EffectInfo.ApplyAuraIndex);
                            break;
                        }

                    case var case5 when case5 == SpellType.SPELL_TYPE_HEAL:
                        {
                            var argTarget2 = this;
                            WorldServiceLocator._WS_Spells.SendHealSpellLog(ref Caster, ref argTarget2, SpellID, Damage, IsCrit);
                            break;
                        }

                    case var case6 when case6 == SpellType.SPELL_TYPE_HEALDOT:
                        {
                            var argTarget3 = this;
                            WorldServiceLocator._WS_Spells.SendPeriodicAuraLog(Caster, argTarget3, SpellID, (int)DamageType, Damage, EffectInfo.ApplyAuraIndex);
                            break;
                        }
                }
            }

            public SpellMissInfo GetMagicSpellHitResult(ref BaseUnit Caster, WS_Spells.SpellInfo Spell)
            {
                if (IsDead)
                    return SpellMissInfo.SPELL_MISS_NONE; // Can't miss dead target
                int lchance = this is WS_PlayerData.CharacterObject ? 7 : 11;
                int leveldiff = Level - Caster.Level;
                int modHitChance;
                if (leveldiff < 3)
                {
                    modHitChance = 96 - leveldiff;
                }
                else
                {
                    modHitChance = 94 - (leveldiff - 2) * lchance;
                }

                // Increase from attacker SPELL_AURA_MOD_INCREASES_SPELL_PCT_TO_HIT auras
                modHitChance += Caster.GetAuraModifierByMiscMask(AuraEffects_Names.SPELL_AURA_MOD_INCREASES_SPELL_PCT_TO_HIT, (int)Spell.SchoolMask);

                // Chance hit from victim SPELL_AURA_MOD_ATTACKER_SPELL_HIT_CHANCE auras
                modHitChance += GetAuraModifierByMiscMask(AuraEffects_Names.SPELL_AURA_MOD_ATTACKER_SPELL_HIT_CHANCE, (int)Spell.SchoolMask);

                // Reduce spell hit chance for Area of effect spells from victim SPELL_AURA_MOD_AOE_AVOIDANCE aura
                if (Spell.IsAOE)
                    modHitChance -= GetAuraModifier(AuraEffects_Names.SPELL_AURA_MOD_AOE_AVOIDANCE);

                // Reduce spell hit chance for dispel mechanic spells from victim SPELL_AURA_MOD_DISPEL_RESIST
                if (Spell.IsDispell)
                    modHitChance -= GetAuraModifier(AuraEffects_Names.SPELL_AURA_MOD_DISPEL_RESIST);

                // Chance resist mechanic (select max value from every mechanic spell effect)
                int resist_mech = 0;
                if (Spell.Mechanic > 0)
                {
                    resist_mech = GetAuraModifierByMiscMask(AuraEffects_Names.SPELL_AURA_MOD_MECHANIC_RESISTANCE, Spell.Mechanic);
                }

                for (int i = 0; i <= 2; i++)
                {
                    if (Spell.SpellEffects[i] is object && Spell.SpellEffects[i].Mechanic > 0)
                    {
                        int temp = GetAuraModifierByMiscMask(AuraEffects_Names.SPELL_AURA_MOD_MECHANIC_RESISTANCE, Spell.SpellEffects[i].Mechanic);
                        if (resist_mech < temp)
                            resist_mech = temp;
                    }
                }

                modHitChance -= resist_mech;

                // Chance resist debuff
                modHitChance -= GetAuraModifierByMiscMask(AuraEffects_Names.SPELL_AURA_MOD_DEBUFF_RESISTANCE, Spell.DispellType);
                int HitChance = modHitChance * 100;
                // Increase hit chance from attacker SPELL_AURA_MOD_SPELL_HIT_CHANCE and attacker ratings
                // HitChance += int32(m_modSpellHitChance*100.0f);

                if (HitChance < 100)
                {
                    HitChance = 100;
                }
                else if (HitChance > 10000)
                {
                    HitChance = 10000;
                }

                int tmp = 10000 - HitChance;
                int rand = WorldServiceLocator._WorldServer.Rnd.Next(0, 10001);
                if (rand < tmp)
                    return SpellMissInfo.SPELL_MISS_RESIST;
                return SpellMissInfo.SPELL_MISS_NONE;
            }

            public SpellMissInfo GetMeleeSpellHitResult(ref BaseUnit Caster, WS_Spells.SpellInfo Spell)
            {
                WeaponAttackType attType = WeaponAttackType.BASE_ATTACK;
                if (Spell.DamageType == SpellDamageType.SPELL_DMG_TYPE_RANGED)
                    attType = WeaponAttackType.RANGED_ATTACK;

                // bonus from skills is 0.04% per skill Diff
                var argVictim = this;
                int attackerWeaponSkill = Caster.GetWeaponSkill(attType, ref argVictim);
                int skillDiff = attackerWeaponSkill - Level * 5;
                int fullSkillDiff = attackerWeaponSkill - GetDefenceSkill(ref Caster);
                int roll = WorldServiceLocator._WorldServer.Rnd.Next(0, 10001);
                int missChance = (int)Conversion.Fix(0.0f * 100.0f);

                // Roll miss
                int tmp = missChance;
                if (roll < tmp)
                    return SpellMissInfo.SPELL_MISS_MISS;

                // Chance resist mechanic (select max value from every mechanic spell effect)
                int resist_mech = 0;
                if (Spell.Mechanic > 0)
                {
                    resist_mech = GetAuraModifierByMiscMask(AuraEffects_Names.SPELL_AURA_MOD_MECHANIC_RESISTANCE, Spell.Mechanic);
                }

                for (int i = 0; i <= 2; i++)
                {
                    if (Spell.SpellEffects[i] is object && Spell.SpellEffects[i].Mechanic > 0)
                    {
                        int temp = GetAuraModifierByMiscMask(AuraEffects_Names.SPELL_AURA_MOD_MECHANIC_RESISTANCE, Spell.SpellEffects[i].Mechanic);
                        if (resist_mech < temp)
                            resist_mech = temp;
                    }
                }

                tmp += resist_mech;
                if (roll < tmp)
                    return SpellMissInfo.SPELL_MISS_RESIST;

                // Same spells cannot be parry/dodge
                if (Spell.Attributes & SpellAttributes.SPELL_ATTR_CANT_BLOCK)
                    return SpellMissInfo.SPELL_MISS_NONE;

                // TODO: Dodge and parry!

                return SpellMissInfo.SPELL_MISS_NONE;
            }

            public int GetDefenceSkill(ref BaseUnit Attacker)
            {
                if (this is WS_PlayerData.CharacterObject)
                {
                    int value;
                    {
                        var withBlock = (WS_PlayerData.CharacterObject)this;
                        // in PvP use full skill instead current skill value
                        if (Attacker.IsPlayer)
                        {
                            value = withBlock.Skills(SKILL_IDs.SKILL_DEFENSE).MaximumWithBonus;
                        }
                        else
                        {
                            value = withBlock.Skills(SKILL_IDs.SKILL_DEFENSE).CurrentWithBonus;
                        }
                    }

                    return value;
                }
                else
                {
                    return Level * 5;
                }
            }

            public int GetWeaponSkill(WeaponAttackType attType, ref BaseUnit Victim)
            {
                if (this is WS_PlayerData.CharacterObject)
                {
                    int value;
                    {
                        var withBlock = (WS_PlayerData.CharacterObject)this;
                        ItemObject item = null;
                        switch (attType)
                        {
                            case var @case when @case == WeaponAttackType.BASE_ATTACK:
                                {
                                    if (withBlock.Items.ContainsKey((byte)EquipmentSlots.EQUIPMENT_SLOT_MAINHAND))
                                        item = withBlock.Items(EquipmentSlots.EQUIPMENT_SLOT_MAINHAND);
                                    break;
                                }

                            case var case1 when case1 == WeaponAttackType.OFF_ATTACK:
                                {
                                    if (withBlock.Items.ContainsKey((byte)EquipmentSlots.EQUIPMENT_SLOT_OFFHAND))
                                        item = withBlock.Items(EquipmentSlots.EQUIPMENT_SLOT_OFFHAND);
                                    break;
                                }

                            case var case2 when case2 == WeaponAttackType.RANGED_ATTACK:
                                {
                                    if (withBlock.Items.ContainsKey((byte)EquipmentSlots.EQUIPMENT_SLOT_RANGED))
                                        item = withBlock.Items(EquipmentSlots.EQUIPMENT_SLOT_RANGED);
                                    break;
                                }
                        }

                        // feral or unarmed skill only for base attack
                        if (attType != WeaponAttackType.BASE_ATTACK && item is null)
                            return 0;

                        // always maximized SKILL_FERAL_COMBAT in fact
                        if (IsInFeralForm)
                            return Level * 5;

                        // weapon skill or (unarmed for base attack)
                        int skill = item is null ? (int)SKILL_IDs.SKILL_UNARMED : item.GetSkill;

                        // in PvP use full skill instead current skill value
                        if (Victim.IsPlayer)
                        {
                            value = withBlock.Skills[skill].MaximumWithBonus;
                        }
                        else
                        {
                            value = withBlock.Skills[skill].CurrentWithBonus;
                        }
                    }

                    return value;
                }
                else
                {
                    return Level * 5;
                }
            }

            public float GetDamageReduction(ref BaseUnit t, DamageTypes School, int Damage)
            {
                float DamageReduction;
                if (School == DamageTypes.DMG_PHYSICAL)
                {
                    DamageReduction = (float)(Resistances[0].Base / (double)(Resistances[0].Base + 400 + 85 * Level));
                }
                else
                {
                    int effectiveResistanceRating = t.Resistances[School].Base + Math.Max((t.Level - Level) * 5, 0);
                    DamageReduction = (float)(effectiveResistanceRating / (double)(Level * 5) * 0.75d);
                }

                if (DamageReduction > 0.75f)
                {
                    DamageReduction = 0.75f;
                }
                else if (DamageReduction < 0.0f)
                {
                    DamageReduction = 0.0f;
                }

                return DamageReduction;
            }

            public float GetResist(ref BaseUnit t, DamageTypes School, int Damage)
            {
                float damageReduction = GetDamageReduction(ref t, School, Damage);

                // DONE: Partial resist
                int[] partialChances;
                if (damageReduction < 0.15f)
                {
                    partialChances = new int[] { 33, 11, 2, 0 };
                }
                else if (damageReduction < 0.3f)
                {
                    partialChances = new int[] { 49, 24, 6, 1 };
                }
                else if (damageReduction < 0.45f)
                {
                    partialChances = new int[] { 26, 48, 18, 1 };
                }
                else if (damageReduction < 0.6f)
                {
                    partialChances = new int[] { 14, 40, 34, 11 };
                }
                else
                {
                    partialChances = new int[] { 3, 16, 55, 25 };
                }

                int ran = WorldServiceLocator._WorldServer.Rnd.Next(0, 101);
                int m = 0;
                int val = 0;
                for (int i = 0; i <= 3; i++)
                {
                    val += partialChances[i];
                    if (ran > val)
                    {
                        m += 1;
                    }
                    else
                    {
                        break;
                    }
                }

                if (m == 0)
                {
                    return 0f;
                }
                else if (m == 4)
                {
                    return Damage;
                }
                else
                {
                    return (float)(Damage * m / 4d);
                }
            }

            public int GetAbsorb(DamageTypes School, int Damage)
            {
                var ListChange = new Dictionary<int, uint>();
                int StartDmg = Damage;
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Damage: {0} [{1}]", Damage, School);
                foreach (KeyValuePair<int, uint> tmpSpell in AbsorbSpellLeft)
                {
                    int Schools = (int)(tmpSpell.Value >> 0x17U);
                    int AbsorbDamage = (int)(tmpSpell.Value & 0x7FFFFFL);
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Spell: {0} [{1}]", AbsorbDamage, Schools);
                    if (WorldServiceLocator._Functions.HaveFlag((uint)Schools, (byte)School))
                    {
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Apmongo, yes?!");
                        if (Damage == AbsorbDamage)
                        {
                            ListChange.Add(tmpSpell.Key, 0U);
                            Damage = 0;
                            break;
                        }
                        else if (Damage > AbsorbDamage)
                        {
                            ListChange.Add(tmpSpell.Key, 0U);
                            Damage -= AbsorbDamage;
                        }
                        else
                        {
                            AbsorbDamage -= Damage;
                            Damage = 0;
                            ListChange.Add(tmpSpell.Key, (uint)AbsorbDamage);
                            break;
                        }
                    }
                    else if (Schools & (1 << School))
                    {
                        throw new Exception("AHA?!");
                    }
                }

                // First remove
                foreach (KeyValuePair<int, uint> Change in ListChange)
                {
                    if (Change.Value == 0L)
                    {
                        RemoveAuraBySpell(Change.Key);
                        if (AbsorbSpellLeft.ContainsKey(Change.Key))
                            AbsorbSpellLeft.Remove(Change.Key);
                    }
                }

                // And then change
                foreach (KeyValuePair<int, uint> Change in ListChange)
                {
                    if (Change.Value != 0L)
                    {
                        AbsorbSpellLeft[Change.Key] = Change.Value;
                    }
                }

                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Absorbed: {0}", StartDmg - Damage);
                return StartDmg - Damage;
            }

            public BaseUnit()
            {
                for (byte i = (byte)DamageTypes.DMG_PHYSICAL, loopTo = (byte)DamageTypes.DMG_ARCANE; i <= loopTo; i++)
                    Resistances[i] = new WS_PlayerHelper.TStat();
            }
        }

        public class BaseActiveSpell
        {
            public int SpellID = 0;
            public int SpellDuration = 0;
            public BaseUnit SpellCaster = null;
            public byte Flags = 0;
            public byte Level = 0;
            public int StackCount = 0;
            public int[] Values = new int[] { 0, 0, 0 };
            public WS_Spells.ApplyAuraHandler[] Aura = new WS_Spells.ApplyAuraHandler[] { null, null, null };
            public WS_Spells.SpellEffect[] Aura_Info = new WS_Spells.SpellEffect[] { null, null, null };

            public BaseActiveSpell(int ID, int Duration)
            {
                SpellID = ID;
                SpellDuration = Duration;
            }

            public WS_Spells.SpellInfo GetSpellInfo
            {
                get
                {
                    return WorldServiceLocator._WS_Spells.SPELLs[SpellID];
                }
            }
        }
    }
}