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
using Mangos.Common.Enums.Player;
using Mangos.Common.Enums.Spell;
using Mangos.Common.Globals;
using Mangos.World.Globals;
using Mangos.World.Player;
using Mangos.World.Spells;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;

namespace Mangos.World.Objects;

public class WS_Base
{
    public class BaseObject
    {
        public ulong GUID;

        public byte CellX;

        public byte CellY;

        public float positionX;

        public float positionY;

        public float positionZ;

        public float orientation;

        public uint instance;

        public uint MapID;

        public CorpseType CorpseType;

        public int SpawnID;

        public List<ulong> SeenBy;

        public float VisibleDistance;

        public InvisibilityLevel Invisibility;

        public int Invisibility_Value;

        public int Invisibility_Bonus;

        public InvisibilityLevel CanSeeInvisibility;

        public int CanSeeInvisibility_Stealth;

        public bool CanSeeStealth;

        public int CanSeeInvisibility_Invisibility;

        public BaseObject()
        {
            GUID = 0uL;
            CellX = 0;
            CellY = 0;
            positionX = 0f;
            positionY = 0f;
            positionZ = 0f;
            orientation = 0f;
            instance = 0u;
            MapID = 0u;
            CorpseType = CorpseType.CORPSE_BONES;
            SpawnID = 0;
            SeenBy = new List<ulong>();
            VisibleDistance = WorldServiceLocator._Global_Constants.DEFAULT_DISTANCE_VISIBLE;
            Invisibility = InvisibilityLevel.VISIBLE;
            Invisibility_Value = 0;
            Invisibility_Bonus = 0;
            CanSeeInvisibility = InvisibilityLevel.INIVISIBILITY;
            CanSeeInvisibility_Stealth = 0;
            CanSeeStealth = false;
            CanSeeInvisibility_Invisibility = 0;
        }

        public virtual bool CanSee(ref BaseObject objCharacter)
        {
            if (GUID == objCharacter.GUID)
            {
                return false;
            }
            if (instance != objCharacter.instance)
            {
                return false;
            }
            if (objCharacter.Invisibility > CanSeeInvisibility)
            {
                return false;
            }
            if (objCharacter.Invisibility == InvisibilityLevel.STEALTH && Math.Sqrt(Math.Pow(objCharacter.positionX - positionX, 2.0) + Math.Pow(objCharacter.positionY - positionY, 2.0)) < WorldServiceLocator._Global_Constants.DEFAULT_DISTANCE_DETECTION)
            {
                return true;
            }
            if (objCharacter.Invisibility == InvisibilityLevel.INIVISIBILITY && objCharacter.Invisibility_Value > CanSeeInvisibility_Invisibility)
            {
                return false;
            }
            if (objCharacter.Invisibility == InvisibilityLevel.STEALTH && objCharacter.Invisibility_Value > CanSeeInvisibility_Stealth)
            {
                return false;
            }
            return Math.Sqrt(Math.Pow(objCharacter.positionX - positionX, 2.0) + Math.Pow(objCharacter.positionY - positionY, 2.0)) <= objCharacter.VisibleDistance;
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
            Packets.PacketClass packet = new(Opcodes.SMSG_PLAY_OBJECT_SOUND);
            try
            {
                packet.AddInt32(SoundID);
                packet.AddUInt64(GUID);
                if (OnlyToSelf && this is WS_PlayerData.CharacterObject @object)
                {
                    @object.client.Send(ref packet);
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

        public void SendToNearPlayers(ref Packets.PacketClass packet, ulong NotTo = 0uL, bool ToSelf = true)
        {
            if (ToSelf && this is WS_PlayerData.CharacterObject @object && @object.client != null)
            {
                @object.client.SendMultiplyPackets(ref packet);
            }
            var array = SeenBy.ToArray();
            foreach (var objCharacter in array)
            {
                if (objCharacter != NotTo && WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(objCharacter) && WorldServiceLocator._WorldServer.CHARACTERs[objCharacter].client != null)
                {
                    WorldServiceLocator._WorldServer.CHARACTERs[objCharacter].client.SendMultiplyPackets(ref packet);
                }
            }
        }
    }

    public class BaseUnit : BaseObject
    {
        public const float CombatReach_Base = 2f;

        public WS_GameObjects.GameObject OnTransport;

        public float transportX;

        public float transportY;

        public float transportZ;

        public float transportO;

        public float BoundingRadius;

        public float CombatReach;

        public int cUnitFlags;

        public int cDynamicFlags;

        public int cBytes0;

        public int cBytes1;

        public int cBytes2;

        public byte Level;

        public int Model;

        public int Mount;

        public WS_PlayerHelper.TStatBar Life;

        public WS_PlayerHelper.TStatBar Mana;

        public float Size;

        public WS_PlayerHelper.TStat[] Resistances;

        public byte SchoolImmunity;

        public uint MechanicImmunity;

        public uint DispellImmunity;

        public Dictionary<int, uint> AbsorbSpellLeft;

        public bool Invulnerable;

        public ulong SummonedBy;

        public ulong CreatedBy;

        public int CreatedBySpell;

        public int cEmoteState;

        public int AuraState;

        public bool Spell_Silenced;

        public bool Spell_Pacifyed;

        public float Spell_ThreatModifier;

        public int AttackPowerMods;

        public int AttackPowerModsRanged;

        public List<WS_DynamicObjects.DynamicObject> dynamicObjects;

        public List<WS_GameObjects.GameObject> gameObjects;

        public BaseActiveSpell[] ActiveSpells;

        public int[] ActiveSpells_Flags;

        public int[] ActiveSpells_Count;

        public int[] ActiveSpells_Level;

        public virtual ManaTypes ManaType
        {
            get => (ManaTypes)((cBytes0 & -16777216) >> 24);
            set => cBytes0 = (cBytes0 & 0xFFFFFF) | ((int)value << 24);
        }

        public virtual Genders Gender
        {
            get => (Genders)checked((byte)((cBytes0 & 0xFF0000) >> 16));
            set => cBytes0 = (cBytes0 & -16711681) | (int)((uint)value << 16);
        }

        public virtual Classes Classe
        {
            get => (Classes)checked((byte)((cBytes0 & 0xFF00) >> 8));
            set => cBytes0 = (cBytes0 & -65281) | (int)((uint)value << 8);
        }

        public virtual Races Race
        {
            get => (Races)checked((byte)((cBytes0 & 0xFF) >> 0));
            set => cBytes0 = (cBytes0 & -256) | (int)((uint)value << 0);
        }

        public string UnitName => this switch
        {
            WS_PlayerData.CharacterObject _ => ((WS_PlayerData.CharacterObject)this).Name,
            WS_Creatures.CreatureObject _ => ((WS_Creatures.CreatureObject)this).Name,
            _ => ""
        };

        public virtual byte StandState
        {
            get => checked((byte)((cBytes1 & 0xFF) >> 0));
            set => cBytes1 = (cBytes1 & -256) | (value << 0);
        }

        public virtual byte PetLoyalty
        {
            get => checked((byte)((cBytes1 & 0xFF00) >> 8));
            set => cBytes1 = (cBytes1 & -65281) | (value << 8);
        }

        public virtual ShapeshiftForm ShapeshiftForm
        {
            get => (ShapeshiftForm)checked((byte)((cBytes1 & 0xFF0000) >> 16));
            set => cBytes1 = (cBytes1 & -16711681) | (int)((uint)value << 16);
        }

        public virtual bool IsDead => Life.Current == 0;

        public virtual bool Exist
        {
            get
            {
                if (this is WS_PlayerData.CharacterObject)
                {
                    return WorldServiceLocator._WorldServer.CHARACTERs.ContainsKey(GUID);
                }
                return this is WS_Creatures.CreatureObject && WorldServiceLocator._WorldServer.WORLD_CREATUREs.ContainsKey(GUID);
            }
        }

        public virtual bool IsRooted => (cUnitFlags & 0x10000) != 0;

        public virtual bool IsStunned => (cUnitFlags & 0x40000) != 0;

        public bool IsInFeralForm => ShapeshiftForm is ShapeshiftForm.FORM_CAT or ShapeshiftForm.FORM_BEAR or ShapeshiftForm.FORM_DIREBEAR;

        public bool IsPlayer => this is WS_PlayerData.CharacterObject;

        public virtual void Die(ref BaseUnit Attacker)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "BaseUnit can't die.");
        }

        public virtual void DealDamage(int Damage, BaseUnit Attacker = null)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "No damage dealt.");
        }

        public virtual void Heal(int Damage, BaseUnit Attacker = null)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "No healing done.");
        }

        public virtual void Energize(int Damage, ManaTypes Power, BaseUnit Attacker = null)
        {
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "No mana increase done.");
        }

        public virtual bool IsFriendlyTo(ref BaseUnit Unit)
        {
            return false;
        }

        public virtual bool IsEnemyTo(ref BaseUnit Unit)
        {
            return false;
        }

        public void SetAura(int SpellID, int Slot, int Duration, bool SendUpdate = true)
        {
            if (ActiveSpells[Slot] == null || (SpellID != 0 && WorldServiceLocator._WS_Spells.SPELLs.ContainsKey(SpellID) && WorldServiceLocator._WS_Spells.SPELLs[SpellID].IsPassive))
            {
                return;
            }
            var AuraLevel_Slot = Slot / 4;
            var AuraFlag_Slot = Slot >> 3;
            var AuraFlag_SubSlot = (Slot & 7) << 2;
            var AuraFlag_Value = 9 << AuraFlag_SubSlot;
            ActiveSpells_Flags[AuraFlag_Slot] &= ~AuraFlag_Value;
            if (SpellID != 0)
            {
                ActiveSpells_Flags[AuraFlag_Slot] |= AuraFlag_Value;
            }
            byte tmpLevel = 0;
            checked
            {
                if (SpellID != 0)
                {
                    tmpLevel = (byte)WorldServiceLocator._WS_Spells.SPELLs[SpellID].spellLevel;
                }
                SetAuraStackCount(Slot, 0);
                SetAuraSlotLevel(Slot, tmpLevel);
                if (!SendUpdate)
                {
                    return;
                }
                if (this is WS_PlayerData.CharacterObject @object)
                {
                    @object.SetUpdateFlag(47 + Slot, SpellID);
                    @object.SetUpdateFlag(95 + AuraFlag_Slot, ActiveSpells_Flags[AuraFlag_Slot]);
                    @object.SetUpdateFlag(113 + AuraLevel_Slot, ActiveSpells_Count[AuraLevel_Slot]);
                    @object.SetUpdateFlag(101 + AuraLevel_Slot, ActiveSpells_Level[AuraLevel_Slot]);
                    @object.SendCharacterUpdate();
                    Packets.PacketClass SMSG_UPDATE_AURA_DURATION = new(Opcodes.SMSG_UPDATE_AURA_DURATION);
                    try
                    {
                        SMSG_UPDATE_AURA_DURATION.AddInt8((byte)Slot);
                        SMSG_UPDATE_AURA_DURATION.AddInt32(Duration);
                        @object.client.Send(ref SMSG_UPDATE_AURA_DURATION);
                    }
                    finally
                    {
                        SMSG_UPDATE_AURA_DURATION.Dispose();
                    }
                    return;
                }
                Packets.UpdateClass tmpUpdate = new(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_PLAYER);
                Packets.UpdatePacketClass tmpPacket = new();
                try
                {
                    tmpUpdate.SetUpdateFlag(47 + Slot, SpellID);
                    tmpUpdate.SetUpdateFlag(95 + AuraFlag_Slot, ActiveSpells_Flags[AuraFlag_Slot]);
                    tmpUpdate.SetUpdateFlag(113 + AuraLevel_Slot, ActiveSpells_Count[AuraLevel_Slot]);
                    tmpUpdate.SetUpdateFlag(101 + AuraLevel_Slot, ActiveSpells_Level[AuraLevel_Slot]);
                    Packets.PacketClass packet = tmpPacket;
                    WS_Creatures.CreatureObject updateObject = (WS_Creatures.CreatureObject)this;
                    tmpUpdate.AddToPacket(ref packet, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
                    tmpPacket = (Packets.UpdatePacketClass)packet;
                    packet = tmpPacket;
                    SendToNearPlayers(ref packet);
                    tmpPacket = (Packets.UpdatePacketClass)packet;
                }
                finally
                {
                    tmpPacket.Dispose();
                    tmpUpdate.Dispose();
                }
            }
        }

        public void SetAuraStackCount(int Slot, byte Count)
        {
            var AuraFlag_Slot = Slot / 4;
            checked
            {
                var AuraFlag_SubSlot = Slot % 4 * 8;
                ActiveSpells_Count[AuraFlag_Slot] &= ~(255 << AuraFlag_SubSlot);
                ActiveSpells_Count[AuraFlag_Slot] |= Count << AuraFlag_SubSlot;
            }
        }

        public void SetAuraSlotLevel(int Slot, int Level)
        {
            var AuraFlag_Slot = Slot / 4;
            checked
            {
                var AuraFlag_SubSlot = Slot % 4 * 8;
                ActiveSpells_Level[AuraFlag_Slot] &= ~(255 << AuraFlag_SubSlot);
                ActiveSpells_Level[AuraFlag_Slot] |= Level << AuraFlag_SubSlot;
            }
        }

        public bool HaveAura(int SpellID)
        {
            checked
            {
                var b = (byte)(WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs - 1);
                byte i = 0;
                while (i <= (uint)b)
                {
                    if (ActiveSpells[i] != null && ActiveSpells[i].SpellID == SpellID)
                    {
                        return true;
                    }
                    i = (byte)unchecked((uint)(i + 1));
                }
                return false;
            }
        }

        public bool HaveAuraType(AuraEffects_Names AuraIndex)
        {
            checked
            {
                var num = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs_VISIBLE - 1;
                for (var i = 0; i <= num; i++)
                {
                    if (ActiveSpells[i] == null)
                    {
                        continue;
                    }
                    byte j = 0;
                    do
                    {
                        if (ActiveSpells[i].Aura_Info[j] != null && ActiveSpells[i].Aura_Info[j].ApplyAuraIndex == (int)AuraIndex)
                        {
                            return true;
                        }
                        j = (byte)unchecked((uint)(j + 1));
                    }
                    while (j <= 2u);
                }
                return false;
            }
        }

        public bool HaveVisibleAura(int SpellID)
        {
            checked
            {
                var b = (byte)(WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs_VISIBLE - 1);
                byte i = 0;
                while (i <= (uint)b)
                {
                    if (ActiveSpells[i] != null && ActiveSpells[i].SpellID == SpellID)
                    {
                        return true;
                    }
                    i = (byte)unchecked((uint)(i + 1));
                }
                return false;
            }
        }

        public bool HavePassiveAura(int SpellID)
        {
            checked
            {
                var b = (byte)WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs_VISIBLE;
                var b2 = (byte)(WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs - 1);
                var i = b;
                while (i <= (uint)b2)
                {
                    if (ActiveSpells[i] != null && ActiveSpells[i].SpellID == SpellID)
                    {
                        return true;
                    }
                    i = (byte)unchecked((uint)(i + 1));
                }
                return false;
            }
        }

        public void RemoveAura(int Slot, ref BaseUnit Caster, bool RemovedByDuration = false, bool SendUpdate = true)
        {
            var RemoveAction = AuraAction.AURA_REMOVE;
            if (RemovedByDuration)
            {
                RemoveAction = AuraAction.AURA_REMOVEBYDURATION;
            }
            if (ActiveSpells[Slot] != null)
            {
                byte i = 0;
                do
                {
                    checked
                    {
                        if (ActiveSpells[Slot].Aura[i] != null)
                        {
                            var obj = ActiveSpells[Slot].Aura[i];
                            var Target = this;
                            BaseObject Caster2 = Caster;
                            obj(ref Target, ref Caster2, ref ActiveSpells[Slot].Aura_Info[i], ActiveSpells[Slot].SpellID, ActiveSpells[Slot].StackCount + 1, RemoveAction);
                            Caster = (BaseUnit)Caster2;
                        }
                        i = (byte)unchecked((uint)(i + 1));
                    }
                }
                while (i <= 2u);
            }
            if (SendUpdate && Slot < WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs_VISIBLE)
            {
                SetAura(0, Slot, 0);
            }
            ActiveSpells[Slot] = null;
        }

        public void RemoveAuraBySpell(int SpellID)
        {
            checked
            {
                var num = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs - 1;
                for (var i = 0; i <= num; i++)
                {
                    if (ActiveSpells[i] != null && ActiveSpells[i].SpellID == SpellID)
                    {
                        RemoveAura(i, ref ActiveSpells[i].SpellCaster);
                        if (this is WS_PlayerData.CharacterObject @object && decimal.Compare(new decimal(@object.DuelArbiter), 0m) != 0 && @object.DuelPartner == null)
                        {
                            WorldServiceLocator._WorldServer.WORLD_CREATUREs[@object.DuelArbiter].RemoveAuraBySpell(SpellID);
                            @object.DuelArbiter = 0uL;
                        }
                        break;
                    }
                }
            }
        }

        public void RemoveAurasOfType(AuraEffects_Names AuraIndex, int NotSpellID = 0)
        {
            checked
            {
                var num = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs_VISIBLE - 1;
                for (var i = 0; i <= num; i++)
                {
                    if (ActiveSpells[i] == null || ActiveSpells[i].SpellID == NotSpellID)
                    {
                        continue;
                    }
                    byte j = 0;
                    do
                    {
                        if (ActiveSpells[i].Aura_Info[j] != null && ActiveSpells[i].Aura_Info[j].ApplyAuraIndex == (int)AuraIndex)
                        {
                            RemoveAura(i, ref ActiveSpells[i].SpellCaster);
                            break;
                        }
                        j = (byte)unchecked((uint)(j + 1));
                    }
                    while (j <= 2u);
                }
            }
        }

        public void RemoveAurasByMechanic(int Mechanic)
        {
            checked
            {
                var num = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs_VISIBLE - 1;
                for (var i = 0; i <= num; i++)
                {
                    if (ActiveSpells[i] != null && WorldServiceLocator._WS_Spells.SPELLs[ActiveSpells[i].SpellID].Mechanic == Mechanic)
                    {
                        RemoveAura(i, ref ActiveSpells[i].SpellCaster);
                    }
                }
            }
        }

        public void RemoveAurasByDispellType(int DispellType, int Amount)
        {
            checked
            {
                var num = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs_VISIBLE - 1;
                for (var i = 0; i <= num; i++)
                {
                    if (ActiveSpells[i] != null && WorldServiceLocator._WS_Spells.SPELLs[ActiveSpells[i].SpellID].DispellType == DispellType)
                    {
                        RemoveAura(i, ref ActiveSpells[i].SpellCaster);
                        Amount--;
                        if (Amount <= 0)
                        {
                            break;
                        }
                    }
                }
            }
        }

        public void RemoveAurasByInterruptFlag(int AuraInterruptFlag)
        {
            checked
            {
                var num = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs_VISIBLE - 1;
                for (var i = 0; i <= num; i++)
                {
                    if (ActiveSpells[i] != null && WorldServiceLocator._WS_Spells.SPELLs.ContainsKey(ActiveSpells[i].SpellID) && (WorldServiceLocator._WS_Spells.SPELLs[ActiveSpells[i].SpellID].auraInterruptFlags & AuraInterruptFlag) != 0 && (WorldServiceLocator._WS_Spells.SPELLs[ActiveSpells[i].SpellID].procFlags & 0x8000000) == 0)
                    {
                        RemoveAura(i, ref ActiveSpells[i].SpellCaster);
                    }
                }
                if (this is WS_PlayerData.CharacterObject characterObject)
                {
                    if (characterObject.spellCasted[3] != null && !characterObject.spellCasted[3].Finished && (characterObject.spellCasted[3].SpellInfo.channelInterruptFlags & AuraInterruptFlag) != 0)
                    {
                        characterObject.FinishSpell(CurrentSpellTypes.CURRENT_CHANNELED_SPELL);
                    }
                }
                else if (this is WS_Creatures.CreatureObject creatureObject)
                {
                    if (creatureObject.SpellCasted != null && (creatureObject.SpellCasted.SpellInfo.channelInterruptFlags & AuraInterruptFlag) != 0)
                    {
                        creatureObject.StopCasting();
                    }
                }
            }
        }

        public int GetAuraModifier(AuraEffects_Names AuraIndex)
        {
            var Modifier = 0;
            checked
            {
                var num = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs_VISIBLE - 1;
                for (var i = 0; i <= num; i++)
                {
                    if (ActiveSpells[i] == null)
                    {
                        continue;
                    }
                    byte j = 0;
                    do
                    {
                        if (ActiveSpells[i].Aura_Info[j] != null && ActiveSpells[i].Aura_Info[j].ApplyAuraIndex == (int)AuraIndex)
                        {
                            Modifier += ActiveSpells[i].Aura_Info[j].GetValue(Level, 0);
                        }
                        j = (byte)unchecked((uint)(j + 1));
                    }
                    while (j <= 2u);
                }
                return Modifier;
            }
        }

        public int GetAuraModifierByMiscMask(AuraEffects_Names AuraIndex, int Mask)
        {
            var Modifier = 0;
            checked
            {
                var num = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs_VISIBLE - 1;
                for (var i = 0; i <= num; i++)
                {
                    if (ActiveSpells[i] == null)
                    {
                        continue;
                    }
                    byte j = 0;
                    do
                    {
                        if (ActiveSpells[i].Aura_Info[j] != null && ActiveSpells[i].Aura_Info[j].ApplyAuraIndex == (int)AuraIndex && (ActiveSpells[i].Aura_Info[j].MiscValue & Mask) == Mask)
                        {
                            Modifier += ActiveSpells[i].Aura_Info[j].GetValue(Level, 0);
                        }
                        j = (byte)unchecked((uint)(j + 1));
                    }
                    while (j <= 2u);
                }
                return Modifier;
            }
        }

        public void AddAura(int SpellID, int Duration, ref BaseUnit Caster)
        {
            var AuraStart = 0;
            checked
            {
                var AuraEnd = WorldServiceLocator._Global_Constants.MAX_POSITIVE_AURA_EFFECTs - 1;
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
                try
                {
                    if (!WorldServiceLocator._WS_Spells.SPELLs[SpellID].IsPassive)
                    {
                        var SpellInfo = WorldServiceLocator._WS_Spells.SPELLs[SpellID];
                        var num = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs_VISIBLE - 1;
                        for (var slot2 = 0; slot2 <= num; slot2++)
                        {
                            if (ActiveSpells[slot2] != null && ActiveSpells[slot2].GetSpellInfo.Target == SpellInfo.Target && ActiveSpells[slot2].GetSpellInfo.Category == SpellInfo.Category && ActiveSpells[slot2].GetSpellInfo.SpellIconID == SpellInfo.SpellIconID && ActiveSpells[slot2].GetSpellInfo.SpellVisual == SpellInfo.SpellVisual && ActiveSpells[slot2].GetSpellInfo.Attributes == SpellInfo.Attributes && ActiveSpells[slot2].GetSpellInfo.AttributesEx == SpellInfo.AttributesEx && ActiveSpells[slot2].GetSpellInfo.AttributesEx2 == SpellInfo.AttributesEx2)
                            {
                                RemoveAura(slot2, ref ActiveSpells[slot2].SpellCaster);
                            }
                        }
                    }
                }
                catch (Exception ex2)
                {
                    ProjectData.SetProjectError(ex2);
                    var ex = ex2;
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "ERROR ADDING AURA!{0}{1}", Environment.NewLine, ex.ToString());
                    ProjectData.ClearProjectError();
                }
                var num2 = AuraStart;
                var num3 = AuraEnd;
                for (var slot = num2; slot <= num3; slot++)
                {
                    if (ActiveSpells[slot] == null)
                    {
                        ActiveSpells[slot] = new BaseActiveSpell(SpellID, Duration)
                        {
                            SpellCaster = Caster
                        };
                        if (slot < WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs_VISIBLE)
                        {
                            SetAura(SpellID, slot, Duration);
                        }
                        break;
                    }
                }
                switch (this)
                {
                    case WS_PlayerData.CharacterObject _:
                        ((WS_PlayerData.CharacterObject)this).GroupUpdateFlag = ((WS_PlayerData.CharacterObject)this).GroupUpdateFlag | 0x200u;
                        break;

                    case WS_Pets.PetObject _ when ((WS_Pets.PetObject)this).Owner is WS_PlayerData.CharacterObject @object:
                        @object.GroupUpdateFlag |= 0x40000u;
                        break;
                }
            }
        }

        public void UpdateAura(int Slot)
        {
            if (ActiveSpells[Slot] == null || Slot >= WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs_VISIBLE)
            {
                return;
            }
            var AuraFlag_Slot = Slot / 4;
            checked
            {
                var AuraFlag_SubSlot = Slot % 4 * 8;
                SetAuraStackCount(Slot, (byte)ActiveSpells[Slot].StackCount);
                if (this is WS_PlayerData.CharacterObject @object)
                {
                    @object.SetUpdateFlag(113 + AuraFlag_Slot, ActiveSpells_Count[AuraFlag_Slot]);
                    @object.SendCharacterUpdate();
                    Packets.PacketClass SMSG_UPDATE_AURA_DURATION = new(Opcodes.SMSG_UPDATE_AURA_DURATION);
                    try
                    {
                        SMSG_UPDATE_AURA_DURATION.AddInt8((byte)Slot);
                        SMSG_UPDATE_AURA_DURATION.AddInt32(ActiveSpells[Slot].SpellDuration);
                        @object.client.Send(ref SMSG_UPDATE_AURA_DURATION);
                    }
                    finally
                    {
                        SMSG_UPDATE_AURA_DURATION.Dispose();
                    }
                    return;
                }
                Packets.UpdateClass tmpUpdate = new(WorldServiceLocator._Global_Constants.FIELD_MASK_SIZE_PLAYER);
                Packets.UpdatePacketClass tmpPacket = new();
                try
                {
                    tmpUpdate.SetUpdateFlag(113 + AuraFlag_Slot, ActiveSpells_Count[AuraFlag_Slot]);
                    Packets.PacketClass packet = tmpPacket;
                    WS_Creatures.CreatureObject updateObject = (WS_Creatures.CreatureObject)this;
                    tmpUpdate.AddToPacket(ref packet, ObjectUpdateType.UPDATETYPE_VALUES, ref updateObject);
                    tmpPacket = (Packets.UpdatePacketClass)packet;
                    packet = tmpPacket;
                    SendToNearPlayers(ref packet);
                    tmpPacket = (Packets.UpdatePacketClass)packet;
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
            Packets.PacketClass packet = new(Opcodes.SMSG_EMOTE);
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

        public void DealSpellDamage(ref BaseUnit Caster, ref WS_Spells.SpellEffect EffectInfo, int SpellID, int Damage, DamageTypes DamageType, SpellType SpellType)
        {
            var IsHeal = false;
            var IsDot = false;
            switch (SpellType)
            {
                case SpellType.SPELL_TYPE_HEAL:
                    IsHeal = true;
                    break;

                case SpellType.SPELL_TYPE_HEALDOT:
                    IsHeal = true;
                    IsDot = true;
                    break;

                case SpellType.SPELL_TYPE_DOT:
                    IsDot = true;
                    break;
            }
            var SpellDamageBenefit = 0;
            bool IsCrit;
            int Resist;
            int Absorb;
            checked
            {
                if (Caster is WS_PlayerData.CharacterObject @object)
                {
                    var PenaltyFactor = 0;
                    var EffectCount = 0;
                    var i = 0;
                    do
                    {
                        if (WorldServiceLocator._WS_Spells.SPELLs[SpellID].SpellEffects[i] != null)
                        {
                            EffectCount++;
                        }
                        i++;
                    }
                    while (i <= 2);
                    if (EffectCount > 1)
                    {
                        PenaltyFactor = 5;
                    }
                    var SpellDamage = (!IsHeal) ? @object.spellDamage[(uint)DamageType].Value : @object.healing.Value;
                    if (IsDot)
                    {
                        var TickAmount = (int)Math.Round(WorldServiceLocator._WS_Spells.SPELLs[SpellID].GetDuration / (double)EffectInfo.Amplitude);
                        if (TickAmount < 5)
                        {
                            TickAmount = 5;
                        }
                        SpellDamageBenefit = SpellDamage / TickAmount;
                    }
                    else
                    {
                        var CastTime = WorldServiceLocator._WS_Spells.SPELLs[SpellID].GetCastTime;
                        if (CastTime < 1500)
                        {
                            CastTime = 1500;
                        }
                        if (CastTime > 3500)
                        {
                            CastTime = 3500;
                        }
                        SpellDamageBenefit = (int)(SpellDamage * (CastTime / 1000f) * ((100 - PenaltyFactor) / 100.0) / 3.5);
                    }
                    if (WorldServiceLocator._WS_Spells.SPELLs[SpellID].IsAOE)
                    {
                        SpellDamageBenefit /= 3;
                    }
                }
                Damage += SpellDamageBenefit;
                IsCrit = false;
                if (!IsDot && Caster is WS_PlayerData.CharacterObject object1 && WorldServiceLocator._Functions.RollChance(object1.GetCriticalWithSpells))
                {
                    Damage = (int)(1.5f * Damage);
                    IsCrit = true;
                }
                Resist = 0;
                Absorb = 0;
                if (!IsHeal)
                {
                    var DamageReduction = GetDamageReduction(ref Caster, DamageType, Damage);
                    Damage = (int)Math.Round(Damage - (Damage * DamageReduction));
                    if (Damage > 0)
                    {
                        Resist = (int)Math.Round(GetResist(ref Caster, DamageType, Damage));
                        if (Resist > 0)
                        {
                            Damage -= Resist;
                        }
                    }
                    if (Damage > 0)
                    {
                        Absorb = GetAbsorb(DamageType, Damage);
                        if (Absorb > 0)
                        {
                            Damage -= Absorb;
                        }
                    }
                    DealDamage(Damage, Caster);
                }
                else
                {
                    Heal(Damage, Caster);
                }
            }
            switch (SpellType)
            {
                case SpellType.SPELL_TYPE_NONMELEE:
                    {
                        var wS_Spells4 = WorldServiceLocator._WS_Spells;
                        var Target = this;
                        wS_Spells4.SendNonMeleeDamageLog(ref Caster, ref Target, SpellID, (int)DamageType, Damage, Resist, Absorb, IsCrit);
                        break;
                    }
                case SpellType.SPELL_TYPE_DOT:
                    {
                        var wS_Spells3 = WorldServiceLocator._WS_Spells;
                        var Target = this;
                        wS_Spells3.SendPeriodicAuraLog(ref Caster, ref Target, SpellID, (int)DamageType, Damage, EffectInfo.ApplyAuraIndex);
                        break;
                    }
                case SpellType.SPELL_TYPE_HEAL:
                    {
                        var wS_Spells2 = WorldServiceLocator._WS_Spells;
                        var Target = this;
                        wS_Spells2.SendHealSpellLog(ref Caster, ref Target, SpellID, Damage, IsCrit);
                        break;
                    }
                case SpellType.SPELL_TYPE_HEALDOT:
                    {
                        var wS_Spells = WorldServiceLocator._WS_Spells;
                        var Target = this;
                        wS_Spells.SendPeriodicAuraLog(ref Caster, ref Target, SpellID, (int)DamageType, Damage, EffectInfo.ApplyAuraIndex);
                        break;
                    }
            }
        }

        public SpellMissInfo GetMagicSpellHitResult(ref BaseUnit Caster, WS_Spells.SpellInfo Spell)
        {
            if (IsDead)
            {
                return SpellMissInfo.SPELL_MISS_NONE;
            }
            var lchance = (this is WS_PlayerData.CharacterObject) ? 7 : 11;
            checked
            {
                var leveldiff = Level - Caster.Level;
                var modHitChance = (leveldiff >= 3) ? (94 - ((leveldiff - 2) * lchance)) : (96 - leveldiff);
                modHitChance += Caster.GetAuraModifierByMiscMask(AuraEffects_Names.SPELL_AURA_MOD_INCREASES_SPELL_PCT_TO_HIT, (int)Spell.SchoolMask);
                modHitChance += GetAuraModifierByMiscMask(AuraEffects_Names.SPELL_AURA_MOD_ATTACKER_SPELL_HIT_CHANCE, (int)Spell.SchoolMask);
                if (Spell.IsAOE)
                {
                    modHitChance -= GetAuraModifier(AuraEffects_Names.SPELL_AURA_MOD_AOE_AVOIDANCE);
                }
                if (Spell.IsDispell)
                {
                    modHitChance -= GetAuraModifier(AuraEffects_Names.SPELL_AURA_MOD_DISPEL_RESIST);
                }
                var resist_mech = 0;
                if (Spell.Mechanic > 0)
                {
                    resist_mech = GetAuraModifierByMiscMask(AuraEffects_Names.SPELL_AURA_MOD_MECHANIC_RESISTANCE, Spell.Mechanic);
                }
                var i = 0;
                do
                {
                    if (Spell.SpellEffects[i] != null && Spell.SpellEffects[i].Mechanic > 0)
                    {
                        var temp = GetAuraModifierByMiscMask(AuraEffects_Names.SPELL_AURA_MOD_MECHANIC_RESISTANCE, Spell.SpellEffects[i].Mechanic);
                        if (resist_mech < temp)
                        {
                            resist_mech = temp;
                        }
                    }
                    i++;
                }
                while (i <= 2);
                modHitChance -= resist_mech;
                modHitChance -= GetAuraModifierByMiscMask(AuraEffects_Names.SPELL_AURA_MOD_DEBUFF_RESISTANCE, Spell.DispellType);
                var HitChance = modHitChance * 100;
                if (HitChance < 100)
                {
                    HitChance = 100;
                }
                else if (HitChance > 10000)
                {
                    HitChance = 10000;
                }
                var tmp = 10000 - HitChance;
                var rand = WorldServiceLocator._WorldServer.Rnd.Next(0, 10001);
                return rand < tmp ? SpellMissInfo.SPELL_MISS_RESIST : SpellMissInfo.SPELL_MISS_NONE;
            }
        }

        public SpellMissInfo GetMeleeSpellHitResult(ref BaseUnit Caster, WS_Spells.SpellInfo Spell)
        {
            var attType = WeaponAttackType.BASE_ATTACK;
            if (Spell.DamageType == 3)
            {
                attType = WeaponAttackType.RANGED_ATTACK;
            }
            var obj = Caster;
            var attType2 = attType;
            var Victim = this;
            var attackerWeaponSkill = obj.GetWeaponSkill(attType2, ref Victim);
            checked
            {
                var skillDiff = attackerWeaponSkill - (Level * 5);
                var fullSkillDiff = attackerWeaponSkill - GetDefenceSkill(ref Caster);
                var roll = WorldServiceLocator._WorldServer.Rnd.Next(0, 10001);
                var missChance = 0;
                var tmp = missChance;
                if (roll < tmp)
                {
                    return SpellMissInfo.SPELL_MISS_MISS;
                }
                var resist_mech = 0;
                if (Spell.Mechanic > 0)
                {
                    resist_mech = GetAuraModifierByMiscMask(AuraEffects_Names.SPELL_AURA_MOD_MECHANIC_RESISTANCE, Spell.Mechanic);
                }
                var i = 0;
                do
                {
                    if (Spell.SpellEffects[i] != null && Spell.SpellEffects[i].Mechanic > 0)
                    {
                        var temp = GetAuraModifierByMiscMask(AuraEffects_Names.SPELL_AURA_MOD_MECHANIC_RESISTANCE, Spell.SpellEffects[i].Mechanic);
                        if (resist_mech < temp)
                        {
                            resist_mech = temp;
                        }
                    }
                    i++;
                }
                while (i <= 2);
                tmp += resist_mech;
                if (roll < tmp)
                {
                    return SpellMissInfo.SPELL_MISS_RESIST;
                }
            }
            return ((uint)Spell.Attributes & 0x200000u) != 0 ? SpellMissInfo.SPELL_MISS_NONE : SpellMissInfo.SPELL_MISS_NONE;
        }

        public int GetDefenceSkill(ref BaseUnit Attacker)
        {
            if (this is WS_PlayerData.CharacterObject characterObject)
            {
                var value = (!Attacker.IsPlayer) ? characterObject.Skills[95].CurrentWithBonus : characterObject.Skills[95].MaximumWithBonus;
                return value;
            }
            checked
            {
                return Level * 5;
            }
        }

        public int GetWeaponSkill(WeaponAttackType attType, ref BaseUnit Victim)
        {
            checked
            {
                if (this is WS_PlayerData.CharacterObject characterObject)
                {
                    ItemObject item = null;
                    switch (attType)
                    {
                        case WeaponAttackType.BASE_ATTACK:
                            if (characterObject.Items.ContainsKey(15))
                            {
                                item = characterObject.Items[15];
                            }
                            break;

                        case WeaponAttackType.OFF_ATTACK:
                            if (characterObject.Items.ContainsKey(16))
                            {
                                item = characterObject.Items[16];
                            }
                            break;

                        case WeaponAttackType.RANGED_ATTACK:
                            if (characterObject.Items.ContainsKey(17))
                            {
                                item = characterObject.Items[17];
                            }
                            break;
                    }
                    if (attType != 0 && item == null)
                    {
                        return 0;
                    }
                    if (IsInFeralForm)
                    {
                        return Level * 5;
                    }
                    var skill = item?.GetSkill ?? 162;
                    var value = (!Victim.IsPlayer) ? characterObject.Skills[skill].CurrentWithBonus : characterObject.Skills[skill].MaximumWithBonus;
                    return value;
                }
                return Level * 5;
            }
        }

        public float GetDamageReduction(ref BaseUnit t, DamageTypes School, int Damage)
        {
            checked
            {
                float DamageReduction;
                if (School == DamageTypes.DMG_PHYSICAL)
                {
                    DamageReduction = (float)(Resistances[0].Base / (double)(Resistances[0].Base + 400 + (85 * Level)));
                }
                else
                {
                    var effectiveResistanceRating = t.Resistances[(uint)School].Base + Math.Max((t.Level - Level) * 5, 0);
                    DamageReduction = (float)(effectiveResistanceRating / (double)(Level * 5) * 0.75);
                }
                if (DamageReduction > 0.75f)
                {
                    DamageReduction = 0.75f;
                }
                else if (DamageReduction < 0f)
                {
                    DamageReduction = 0f;
                }
                return DamageReduction;
            }
        }

        public float GetResist(ref BaseUnit t, DamageTypes School, int Damage)
        {
            var damageReduction = GetDamageReduction(ref t, School, Damage);
            var partialChances = (damageReduction < 0.15f) ? new int[4]
            {
                    33,
                    11,
                    2,
                    0
            } : ((damageReduction < 0.3f) ? new int[4]
            {
                    49,
                    24,
                    6,
                    1
            } : ((damageReduction < 0.45f) ? new int[4]
            {
                    26,
                    48,
                    18,
                    1
            } : ((!(damageReduction < 0.6f)) ? new int[4]
            {
                    3,
                    16,
                    55,
                    25
            } : new int[4]
            {
                    14,
                    40,
                    34,
                    11
            })));
            var ran = WorldServiceLocator._WorldServer.Rnd.Next(0, 101);
            var j = 0;
            var val = 0;
            var i = 0;
            checked
            {
                do
                {
                    val += partialChances[i];
                    if (ran > val)
                    {
                        j++;
                        i++;
                        continue;
                    }
                    break;
                }
                while (i <= 3);
                return j switch
                {
                    0 => 0f,
                    4 => Damage,
                    _ => (float)(Damage * j / 4.0),
                };
            }
        }

        public int GetAbsorb(DamageTypes School, int Damage)
        {
            Dictionary<int, uint> ListChange = new();
            var StartDmg = Damage;
            WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Damage: {0} [{1}]", Damage, School);
            checked
            {
                foreach (var tmpSpell in AbsorbSpellLeft)
                {
                    var Schools = (int)(tmpSpell.Value >> 23);
                    var AbsorbDamage = (int)(tmpSpell.Value & 0x7FFFFFL);
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Spell: {0} [{1}]", AbsorbDamage, Schools);
                    if (WorldServiceLocator._Functions.HaveFlag((uint)Schools, (byte)School))
                    {
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Apmongo, yes?!");
                        if (Damage == AbsorbDamage)
                        {
                            ListChange.Add(tmpSpell.Key, 0u);
                            Damage = 0;
                            break;
                        }
                        if (Damage <= AbsorbDamage)
                        {
                            AbsorbDamage -= Damage;
                            Damage = 0;
                            ListChange.Add(tmpSpell.Key, (uint)AbsorbDamage);
                            break;
                        }
                        ListChange.Add(tmpSpell.Key, 0u);
                        Damage -= AbsorbDamage;
                    }
                    else if ((Schools & (1 << (int)School)) != 0)
                    {
                        throw new Exception("AHA?!");
                    }
                }
            }
            foreach (var Change2 in ListChange)
            {
                if ((ulong)Change2.Value == 0)
                {
                    RemoveAuraBySpell(Change2.Key);
                    if (AbsorbSpellLeft.ContainsKey(Change2.Key))
                    {
                        AbsorbSpellLeft.Remove(Change2.Key);
                    }
                }
            }
            foreach (var Change in ListChange)
            {
                if ((ulong)Change.Value != 0)
                {
                    AbsorbSpellLeft[Change.Key] = Change.Value;
                }
            }
            checked
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.DEBUG, "Absorbed: {0}", StartDmg - Damage);
                return StartDmg - Damage;
            }
        }

        public BaseUnit()
        {
            OnTransport = null;
            transportX = 0f;
            transportY = 0f;
            transportZ = 0f;
            transportO = 0f;
            BoundingRadius = 0.389f;
            CombatReach = 1.5f;
            cUnitFlags = 8;
            cDynamicFlags = 0;
            cBytes0 = 0;
            cBytes1 = 0;
            cBytes2 = -286331392;
            Level = 0;
            Model = 0;
            Mount = 0;
            Life = new WS_PlayerHelper.TStatBar(1, 1, 0);
            Mana = new WS_PlayerHelper.TStatBar(1, 1, 0);
            Size = 1f;
            Resistances = new WS_PlayerHelper.TStat[7];
            SchoolImmunity = 0;
            MechanicImmunity = 0u;
            DispellImmunity = 0u;
            AbsorbSpellLeft = new Dictionary<int, uint>();
            Invulnerable = false;
            SummonedBy = 0uL;
            CreatedBy = 0uL;
            CreatedBySpell = 0;
            cEmoteState = 0;
            AuraState = 0;
            Spell_Silenced = false;
            Spell_Pacifyed = false;
            Spell_ThreatModifier = 1f;
            AttackPowerMods = 0;
            AttackPowerModsRanged = 0;
            dynamicObjects = new List<WS_DynamicObjects.DynamicObject>();
            gameObjects = new List<WS_GameObjects.GameObject>();
            checked
            {
                ActiveSpells = new BaseActiveSpell[WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs - 1 + 1];
                ActiveSpells_Flags = new int[WorldServiceLocator._Global_Constants.MAX_AURA_EFFECT_FLAGs - 1 + 1];
                ActiveSpells_Count = new int[WorldServiceLocator._Global_Constants.MAX_AURA_EFFECT_LEVELSs - 1 + 1];
                ActiveSpells_Level = new int[WorldServiceLocator._Global_Constants.MAX_AURA_EFFECT_LEVELSs - 1 + 1];
                byte i = 0;
                do
                {
                    Resistances[i] = new WS_PlayerHelper.TStat();
                    i = (byte)unchecked((uint)(i + 1));
                }
                while (i <= 6u);
            }
        }
    }

    public class BaseActiveSpell
    {
        public int SpellID;

        public int SpellDuration;

        public BaseUnit SpellCaster;

        public byte Flags;

        public byte Level;

        public int StackCount;

        public int[] Values;

        public WS_Spells.ApplyAuraHandler[] Aura;

        public WS_Spells.SpellEffect[] Aura_Info;

        public WS_Spells.SpellInfo GetSpellInfo => WorldServiceLocator._WS_Spells.SPELLs[SpellID];

        public BaseActiveSpell(int ID, int Duration)
        {
            SpellID = 0;
            SpellDuration = 0;
            SpellCaster = null;
            Flags = 0;
            Level = 0;
            StackCount = 0;
            Values = new int[3];
            Aura = new WS_Spells.ApplyAuraHandler[3];
            Aura_Info = new WS_Spells.SpellEffect[3];
            SpellID = ID;
            SpellDuration = Duration;
        }
    }
}
