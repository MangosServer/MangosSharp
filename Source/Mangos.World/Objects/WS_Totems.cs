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
using Mangos.Common.Enums.Spell;
using Mangos.World.Player;
using Mangos.World.Spells;
using System.Collections.Generic;

namespace Mangos.World.Objects;

public class WS_Totems
{
    public class TotemObject : WS_Creatures.CreatureObject
    {
        public WS_Base.BaseUnit Caster;

        public int Duration;

        private readonly TotemType Type;

        public TotemObject(int Entry, float PosX, float PosY, float PosZ, float Orientation, int Map, int Duration_ = 0)
            : base(Entry, PosX, PosY, PosZ, Orientation, Map, Duration_)
        {
            Caster = null;
            Duration = 0;
            Type = TotemType.TOTEM_PASSIVE;
            if (aiScript != null)
            {
                aiScript.Dispose();
            }
            aiScript = null;
            Duration = Duration_;
        }

        public void InitSpell(int SpellID)
        {
            ApplySpell(SpellID);
        }

        public void Update()
        {
            checked
            {
                var num = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs - 1;
                for (var i = 0; i <= num; i++)
                {
                    if (ActiveSpells[i] == null)
                    {
                        continue;
                    }
                    if (ActiveSpells[i].SpellDuration == WorldServiceLocator._Global_Constants.SPELL_DURATION_INFINITE)
                    {
                        ActiveSpells[i].SpellDuration = Duration;
                    }
                    if (ActiveSpells[i].SpellDuration != WorldServiceLocator._Global_Constants.SPELL_DURATION_INFINITE)
                    {
                        ActiveSpells[i].SpellDuration -= 1000;
                        byte k = 0;
                        do
                        {
                            if (ActiveSpells[i] != null && ActiveSpells[i].Aura[k] != null && ActiveSpells[i].Aura_Info[k].Amplitude != 0 && checked(Duration - ActiveSpells[i].SpellDuration) % ActiveSpells[i].Aura_Info[k].Amplitude == 0)
                            {
                                var obj = ActiveSpells[i].Aura[k];
                                WS_Base.BaseUnit Target = this;
                                ref var spellCaster = ref ActiveSpells[i].SpellCaster;
                                WS_Base.BaseObject baseObject = spellCaster;
                                obj(ref Target, ref baseObject, ref ActiveSpells[i].Aura_Info[k], ActiveSpells[i].SpellID, ActiveSpells[i].StackCount + 1, AuraAction.AURA_UPDATE);
                                spellCaster = (WS_Base.BaseUnit)baseObject;
                            }
                            k = (byte)unchecked((uint)(k + 1));
                        }
                        while (k <= 2u);
                        if (ActiveSpells[i] != null && ActiveSpells[i].SpellDuration <= 0 && ActiveSpells[i].SpellDuration != WorldServiceLocator._Global_Constants.SPELL_DURATION_INFINITE)
                        {
                            RemoveAura(i, ref ActiveSpells[i].SpellCaster, RemovedByDuration: true);
                        }
                    }
                    byte j = 0;
                    do
                    {
                        if (ActiveSpells[i] != null && ActiveSpells[i].Aura_Info[j] != null && ActiveSpells[i].Aura_Info[j].ID == SpellEffects_Names.SPELL_EFFECT_APPLY_AREA_AURA)
                        {
                            List<WS_Base.BaseUnit> Targets = new();
                            switch (Caster)
                            {
                                case WS_PlayerData.CharacterObject _:
                                    {
                                        var wS_Spells = WorldServiceLocator._WS_Spells;
                                        WS_PlayerData.CharacterObject objCharacter = (WS_PlayerData.CharacterObject)Caster;
                                        Targets = wS_Spells.GetPartyMembersAtPoint(ref objCharacter, ActiveSpells[i].Aura_Info[j].GetRadius, positionX, positionY, positionZ);
                                        break;
                                    }

                                default:
                                    {
                                        var wS_Spells2 = WorldServiceLocator._WS_Spells;
                                        WS_Base.BaseUnit Target = this;
                                        Targets = wS_Spells2.GetFriendAroundMe(ref Target, ActiveSpells[i].Aura_Info[j].GetRadius);
                                        break;
                                    }
                            }
                            foreach (var item in new List<WS_Base.BaseUnit>())
                            {
                                var Unit = item;
                                if (!Unit.HaveAura(ActiveSpells[i].SpellID))
                                {
                                    var wS_Spells3 = WorldServiceLocator._WS_Spells;
                                    WS_Base.BaseObject baseObject = this;
                                    wS_Spells3.ApplyAura(ref Unit, ref baseObject, ref ActiveSpells[i].Aura_Info[j], ActiveSpells[i].SpellID);
                                }
                            }
                        }
                        j = (byte)unchecked((uint)(j + 1));
                    }
                    while (j <= 2u);
                }
            }
        }
    }
}
