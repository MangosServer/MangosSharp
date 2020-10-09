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

using System.Collections.Generic;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Spell;
using Mangos.World.Player;
using Mangos.World.Server;

namespace Mangos.World.Objects
{
    public class WS_Totems
    {
        public class TotemObject : WS_Creatures.CreatureObject
        {
            public WS_Base.BaseUnit Caster = null;
            public int Duration = 0;
            private readonly TotemType Type = TotemType.TOTEM_PASSIVE;

            public TotemObject(int Entry, float PosX, float PosY, float PosZ, float Orientation, int Map, int Duration_ = 0) : base(Entry, PosX, PosY, PosZ, Orientation, Map, Duration_)
            {
                if (aiScript is object)
                    aiScript.Dispose();
                aiScript = null;
                Duration = Duration_;
            }

            public void InitSpell(int SpellID)
            {
                ApplySpell(SpellID);
            }

            public void Update()
            {
                for (int i = 0, loopTo = WorldServiceLocator._Global_Constants.MAX_AURA_EFFECTs - 1; i <= loopTo; i++)
                {
                    if (ActiveSpells[i] is object)
                    {
                        if (ActiveSpells[i].SpellDuration == WorldServiceLocator._Global_Constants.SPELL_DURATION_INFINITE)
                            ActiveSpells[i].SpellDuration = Duration;

                        // DONE: Count aura duration
                        if (ActiveSpells[i].SpellDuration != WorldServiceLocator._Global_Constants.SPELL_DURATION_INFINITE)
                        {
                            ActiveSpells[i].SpellDuration -= WS_TimerBasedEvents.TSpellManager.UPDATE_TIMER;

                            // DONE: Cast aura (check if: there is aura; aura is periodic; time for next activation)
                            for (byte j = 0; j <= 2; j++)
                            {
                                if (ActiveSpells[i] is object && ActiveSpells[i].Aura[j] is object && ActiveSpells[i].Aura_Info[j].Amplitude != 0 && (Duration - ActiveSpells[i].SpellDuration) % ActiveSpells[i].Aura_Info[j].Amplitude == 0)
                                {
                                    var argTarget = this;
                                    ActiveSpells[i].Aura[(int)j].Invoke(ref argTarget, ref ActiveSpells[i].SpellCaster, ref ActiveSpells[i].Aura_Info[(int)j], ActiveSpells[i].SpellID, ActiveSpells[i].StackCount + 1, AuraAction.AURA_UPDATE);
                                }
                            }

                            // DONE: Remove finished aura
                            if (ActiveSpells[i] is object && ActiveSpells[i].SpellDuration <= 0 && ActiveSpells[i].SpellDuration != WorldServiceLocator._Global_Constants.SPELL_DURATION_INFINITE)
                                RemoveAura(i, ref ActiveSpells[i].SpellCaster, true);
                        }

                        for (byte j = 0; j <= 2; j++)
                        {
                            if (ActiveSpells[i] is object && ActiveSpells[i].Aura_Info[j] is object)
                            {
                                if (ActiveSpells[i].Aura_Info[j].ID == SpellEffects_Names.SPELL_EFFECT_APPLY_AREA_AURA)
                                {
                                    var Targets = new List<WS_Base.BaseUnit>();
                                    if (Caster is WS_PlayerData.CharacterObject)
                                    {
                                        List<WS_Base.BaseUnit> localGetPartyMembersAtPoint() { WS_PlayerData.CharacterObject argobjCharacter = (WS_PlayerData.CharacterObject)Caster; var ret = WorldServiceLocator._WS_Spells.GetPartyMembersAtPoint(ref argobjCharacter, ActiveSpells[i].Aura_Info[j].GetRadius, positionX, positionY, positionZ); return ret; }

                                        Targets = localGetPartyMembersAtPoint();
                                    }
                                    else
                                    {
                                        WS_Base.BaseUnit argobjCharacter = this;
                                        Targets = WorldServiceLocator._WS_Spells.GetFriendAroundMe(ref argobjCharacter, ActiveSpells[i].Aura_Info[j].GetRadius);
                                    }

                                    foreach (WS_Base.BaseUnit Unit in new List<WS_Base.BaseUnit>())
                                    {
                                        if (Unit.HaveAura(ActiveSpells[i].SpellID) == false)
                                        {
                                            WS_Base.BaseObject argCaster = this;
                                            WorldServiceLocator._WS_Spells.ApplyAura(ref Unit, ref argCaster, ref ActiveSpells[i].Aura_Info[j], ActiveSpells[i].SpellID);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}