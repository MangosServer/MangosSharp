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
using Mangos.Common.Enums.Player;
using Mangos.World.DataStores;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.World.Player
{
    public class WS_Player_Initializator
    {
        public WS_Player_Initializator()
        {
            XPTable = new int[DEFAULT_MAX_LEVEL + 1];
        }

        public int DEFAULT_MAX_LEVEL = 60; // Max Player Level
        public int[] XPTable; // Max XPTable Level from Database

        public int CalculateStartingLIFE(ref WS_PlayerData.CharacterObject objCharacter, int baseLIFE)
        {
            if (objCharacter.Stamina.Base < 20)
            {
                return baseLIFE + (objCharacter.Stamina.Base - 20);
            }
            else
            {
                return baseLIFE + 10 * (objCharacter.Stamina.Base - 20);
            }
        }

        public int CalculateStartingMANA(ref WS_PlayerData.CharacterObject objCharacter, int baseMANA)
        {
            if (objCharacter.Intellect.Base < 20)
            {
                return baseMANA + (objCharacter.Intellect.Base - 20);
            }
            else
            {
                return baseMANA + 15 * (objCharacter.Intellect.Base - 20);
            }
        }

        private int gainStat(int level, double a3, double a2, double a1, double a0)
        {
            return Conversions.ToInteger(Math.Round(a3 * level * level * level + a2 * level * level + a1 * level + a0)) - Conversions.ToInteger(Math.Round(a3 * (level - 1) * (level - 1) * (level - 1) + a2 * (level - 1) * (level - 1) + a1 * (level - 1) + a0));
        }

        public void CalculateOnLevelUP(ref WS_PlayerData.CharacterObject objCharacter)
        {
            int baseInt = objCharacter.Intellect.Base;
            // Dim baseStr As Integer = objCharacter.Strength.Base
            // Dim baseSta As Integer = objCharacter.Stamina.Base
            int baseSpi = objCharacter.Spirit.Base;
            int baseAgi = objCharacter.Agility.Base;
            // Dim baseMana As Integer = objCharacter.Mana.Maximum
            int baseLife = objCharacter.Life.Maximum;
            switch (objCharacter.Classe)
            {
                case var @case when @case == Classes.CLASS_DRUID:
                    {
                        if (objCharacter.Level <= 17)
                        {
                            objCharacter.Life.Base += 17;
                        }
                        else
                        {
                            objCharacter.Life.Base += objCharacter.Level;
                        }

                        if (objCharacter.Level <= 25)
                        {
                            objCharacter.Mana.Base += 20 + objCharacter.Level;
                        }
                        else
                        {
                            objCharacter.Mana.Base += 45;
                        }

                        objCharacter.Strength.Base += gainStat(objCharacter.Level, 0.000021d, 0.003009d, 0.486493d, -0.400003d);
                        objCharacter.Intellect.Base += gainStat(objCharacter.Level, 0.000038d, 0.005145d, 0.871006d, -0.832029d);
                        objCharacter.Agility.Base += gainStat(objCharacter.Level, 0.000041d, 0.00044d, 0.512076d, -1.000317d);
                        objCharacter.Stamina.Base += gainStat(objCharacter.Level, 0.000023d, 0.003345d, 0.56005d, -0.562058d);
                        objCharacter.Spirit.Base += gainStat(objCharacter.Level, 0.000059d, 0.004044d, 1.04d, -1.488504d);
                        break;
                    }

                case var case1 when case1 == Classes.CLASS_HUNTER:
                    {
                        if (objCharacter.Level <= 13)
                        {
                            objCharacter.Life.Base += 17;
                        }
                        else
                        {
                            objCharacter.Life.Base += objCharacter.Level + 4;
                        }

                        if (objCharacter.Level <= 27)
                        {
                            objCharacter.Mana.Base += 18 + objCharacter.Level;
                        }
                        else
                        {
                            objCharacter.Mana.Base += 45;
                        }

                        objCharacter.Strength.Base += gainStat(objCharacter.Level, 0.000022d, 0.0018d, 0.407867d, -0.550889d);
                        objCharacter.Intellect.Base += gainStat(objCharacter.Level, 0.00002d, 0.003007d, 0.505215d, -0.500642d);
                        objCharacter.Agility.Base += gainStat(objCharacter.Level, 0.00004d, 0.007416d, 1.125108d, -1.003045d);
                        objCharacter.Stamina.Base += gainStat(objCharacter.Level, 0.000031d, 0.00448d, 0.78004d, -0.800471d);
                        objCharacter.Spirit.Base += gainStat(objCharacter.Level, 0.000017d, 0.003803d, 0.536846d, -0.490026d);
                        break;
                    }

                case var case2 when case2 == Classes.CLASS_MAGE:
                    {
                        if (objCharacter.Level <= 25)
                        {
                            objCharacter.Life.Base += 15;
                        }
                        else
                        {
                            objCharacter.Life.Base += objCharacter.Level - 8;
                        }

                        if (objCharacter.Level <= 27)
                        {
                            objCharacter.Mana.Base += 23 + objCharacter.Level;
                        }
                        else
                        {
                            objCharacter.Mana.Base += 51;
                        }

                        objCharacter.Strength.Base += gainStat(objCharacter.Level, 0.000002d, 0.001003d, 0.10089d, -0.076055d);
                        objCharacter.Intellect.Base += gainStat(objCharacter.Level, 0.00004d, 0.007416d, 1.125108d, -1.003045d);
                        objCharacter.Agility.Base += gainStat(objCharacter.Level, 0.000008d, 0.001001d, 0.16319d, -0.06428d);
                        objCharacter.Stamina.Base += gainStat(objCharacter.Level, 0.000006d, 0.002031d, 0.27836d, -0.340077d);
                        objCharacter.Spirit.Base += gainStat(objCharacter.Level, 0.000039d, 0.006981d, 1.09009d, -1.00607d);
                        break;
                    }

                case var case3 when case3 == Classes.CLASS_PALADIN:
                    {
                        if (objCharacter.Level <= 14)
                        {
                            objCharacter.Life.Base += 18;
                        }
                        else
                        {
                            objCharacter.Life.Base += objCharacter.Level + 4;
                        }

                        if (objCharacter.Level <= 25)
                        {
                            objCharacter.Mana.Base += 17 + objCharacter.Level;
                        }
                        else
                        {
                            objCharacter.Mana.Base += 42;
                        }

                        objCharacter.Strength.Base += gainStat(objCharacter.Level, 0.000037d, 0.005455d, 0.940039d, -1.00009d);
                        objCharacter.Intellect.Base += gainStat(objCharacter.Level, 0.000023d, 0.003345d, 0.56005d, -0.562058d);
                        objCharacter.Agility.Base += gainStat(objCharacter.Level, 0.00002d, 0.003007d, 0.505215d, -0.500642d);
                        objCharacter.Stamina.Base += gainStat(objCharacter.Level, 0.000038d, 0.005145d, 0.871006d, -0.832029d);
                        objCharacter.Spirit.Base += gainStat(objCharacter.Level, 0.000032d, 0.003025d, 0.61589d, -0.640307d);
                        break;
                    }

                case var case4 when case4 == Classes.CLASS_PRIEST:
                    {
                        if (objCharacter.Level <= 22)
                        {
                            objCharacter.Life.Base += 15;
                        }
                        else
                        {
                            objCharacter.Life.Base += objCharacter.Level - 6;
                        }

                        if (objCharacter.Level <= 33)
                        {
                            objCharacter.Mana.Base += 22 + objCharacter.Level;
                        }
                        else
                        {
                            objCharacter.Mana.Base += 54;
                        }

                        if (objCharacter.Level == 34)
                            objCharacter.Mana.Base += 15;
                        objCharacter.Strength.Base += gainStat(objCharacter.Level, 0.000008d, 0.001001d, 0.16319d, -0.06428d);
                        objCharacter.Intellect.Base += gainStat(objCharacter.Level, 0.000039d, 0.006981d, 1.09009d, -1.00607d);
                        objCharacter.Agility.Base += gainStat(objCharacter.Level, 0.000022d, 0.000022d, 0.260756d, -0.494d);
                        objCharacter.Stamina.Base += gainStat(objCharacter.Level, 0.000024d, 0.000981d, 0.364935d, -0.5709d);
                        objCharacter.Spirit.Base += gainStat(objCharacter.Level, 0.00004d, 0.007416d, 1.125108d, -1.003045d);
                        break;
                    }

                case var case5 when case5 == Classes.CLASS_ROGUE:
                    {
                        if (objCharacter.Level <= 15)
                        {
                            objCharacter.Life.Base += 17;
                        }
                        else
                        {
                            objCharacter.Life.Base += objCharacter.Level + 2;
                        }

                        objCharacter.Strength.Base += gainStat(objCharacter.Level, 0.000025d, 0.00417d, 0.654096d, -0.601491d);
                        objCharacter.Intellect.Base += gainStat(objCharacter.Level, 0.000008d, 0.001001d, 0.16319d, -0.06428d);
                        objCharacter.Agility.Base += gainStat(objCharacter.Level, 0.000038d, 0.007834d, 1.191028d, -1.20394d);
                        objCharacter.Stamina.Base += gainStat(objCharacter.Level, 0.000032d, 0.003025d, 0.61589d, -0.640307d);
                        objCharacter.Spirit.Base += gainStat(objCharacter.Level, 0.000024d, 0.000981d, 0.364935d, -0.5709d);
                        break;
                    }

                case var case6 when case6 == Classes.CLASS_SHAMAN:
                    {
                        if (objCharacter.Level <= 16)
                        {
                            objCharacter.Life.Base += 17;
                        }
                        else
                        {
                            objCharacter.Life.Base += objCharacter.Level + 1;
                        }

                        if (objCharacter.Level <= 32)
                        {
                            objCharacter.Mana.Base += 19 + objCharacter.Level;
                        }
                        else
                        {
                            objCharacter.Mana.Base += 52;
                        }

                        objCharacter.Strength.Base += gainStat(objCharacter.Level, 0.000035d, 0.003641d, 0.73431d, -0.800626d);
                        objCharacter.Intellect.Base += gainStat(objCharacter.Level, 0.000031d, 0.00448d, 0.78004d, -0.800471d);
                        objCharacter.Agility.Base += gainStat(objCharacter.Level, 0.000022d, 0.0018d, 0.407867d, -0.550889d);
                        objCharacter.Stamina.Base += gainStat(objCharacter.Level, 0.00002d, 0.00603d, 0.80957d, -0.80922d);
                        objCharacter.Spirit.Base += gainStat(objCharacter.Level, 0.000038d, 0.005145d, 0.871006d, -0.832029d);
                        break;
                    }

                case var case7 when case7 == Classes.CLASS_WARLOCK:
                    {
                        if (objCharacter.Level <= 17)
                        {
                            objCharacter.Life.Base += 15;
                        }
                        else
                        {
                            objCharacter.Life.Base += objCharacter.Level - 2;
                        }

                        if (objCharacter.Level <= 30)
                        {
                            objCharacter.Mana.Base += 21 + objCharacter.Level;
                        }
                        else
                        {
                            objCharacter.Mana.Base += 51;
                        }

                        objCharacter.Strength.Base += gainStat(objCharacter.Level, 0.000006d, 0.002031d, 0.27836d, -0.340077d);
                        objCharacter.Intellect.Base += gainStat(objCharacter.Level, 0.000059d, 0.004044d, 1.04d, -1.488504d);
                        objCharacter.Agility.Base += gainStat(objCharacter.Level, 0.000024d, 0.000981d, 0.364935d, -0.5709d);
                        objCharacter.Stamina.Base += gainStat(objCharacter.Level, 0.000021d, 0.003009d, 0.486493d, -0.400003d);
                        objCharacter.Spirit.Base += gainStat(objCharacter.Level, 0.00004d, 0.006404d, 1.038791d, -1.039076d);
                        break;
                    }

                case var case8 when case8 == Classes.CLASS_WARRIOR:
                    {
                        if (objCharacter.Level <= 14)
                        {
                            objCharacter.Life.Base += 19;
                        }
                        else
                        {
                            objCharacter.Life.Base += objCharacter.Level + 10;
                        }

                        objCharacter.Strength.Base += gainStat(objCharacter.Level, 0.000039d, 0.006902d, 1.08004d, -1.051701d);
                        objCharacter.Intellect.Base += gainStat(objCharacter.Level, 0.000002d, 0.001003d, 0.10089d, -0.076055d);
                        objCharacter.Agility.Base += gainStat(objCharacter.Level, 0.000022d, 0.0046d, 0.655333d, -0.600356d);
                        objCharacter.Stamina.Base += gainStat(objCharacter.Level, 0.000059d, 0.004044d, 1.04d, -1.488504d);
                        objCharacter.Spirit.Base += gainStat(objCharacter.Level, 0.000006d, 0.002031d, 0.27836d, -0.340077d);
                        break;
                    }
            }

            // Calculate new spi/int gain
            if (objCharacter.Agility.Base != baseAgi)
                objCharacter.Resistances[DamageTypes.DMG_PHYSICAL].Base += (objCharacter.Agility.Base - baseAgi) * 2;
            if (objCharacter.Spirit.Base != baseSpi)
                objCharacter.Life.Base += 10 * (objCharacter.Spirit.Base - baseSpi);
            if (objCharacter.Intellect.Base != baseInt && objCharacter.ManaType == ManaTypes.TYPE_MANA)
                objCharacter.Mana.Base += 15 * (objCharacter.Intellect.Base - baseInt);
            objCharacter.Damage.Minimum += 1f;
            objCharacter.RangedDamage.Minimum += 1f;
            objCharacter.Damage.Maximum += 1f;
            objCharacter.RangedDamage.Maximum += 1f;
            if (objCharacter.Level > 9)
                objCharacter.TalentPoints = (byte)(objCharacter.TalentPoints + 1);
            foreach (KeyValuePair<int, WS_PlayerHelper.TSkill> Skill in objCharacter.Skills)
            {
                if (WorldServiceLocator._WS_DBCDatabase.SkillLines[Skill.Key] == SKILL_LineCategory.WEAPON_SKILLS)
                {
                    Skill.Value.Base = (short)(Skill.Value.Base + 5);
                }
            }
        }

        public ManaTypes GetClassManaType(Classes Classe)
        {
            switch (Classe)
            {
                case var @case when @case == Classes.CLASS_DRUID:
                case var case1 when case1 == Classes.CLASS_HUNTER:
                case var case2 when case2 == Classes.CLASS_MAGE:
                case var case3 when case3 == Classes.CLASS_PALADIN:
                case var case4 when case4 == Classes.CLASS_PRIEST:
                case var case5 when case5 == Classes.CLASS_SHAMAN:
                case var case6 when case6 == Classes.CLASS_WARLOCK:
                    {
                        return ManaTypes.TYPE_MANA;
                    }

                case var case7 when case7 == Classes.CLASS_ROGUE:
                    {
                        return ManaTypes.TYPE_ENERGY;
                    }

                case var case8 when case8 == Classes.CLASS_WARRIOR:
                    {
                        return ManaTypes.TYPE_RAGE;
                    }

                default:
                    {
                        return ManaTypes.TYPE_MANA;
                    }
            }
        }

        public void InitializeReputations(ref WS_PlayerData.CharacterObject objCharacter)
        {
            for (byte i = 0; i <= 63; i++)
            {
                objCharacter.Reputation[i] = new WS_PlayerHelper.TReputation()
                {
                    Value = 0,
                    Flags = 0
                };
                foreach (KeyValuePair<int, WS_DBCDatabase.TFaction> tmpFactionInfo in WorldServiceLocator._WS_DBCDatabase.FactionInfo)
                {
                    if (tmpFactionInfo.Value.VisibleID == i)
                    {
                        for (byte j = 0; j <= 3; j++)
                        {
                            if (WorldServiceLocator._Functions.HaveFlag((uint)tmpFactionInfo.Value.flags[(int)j], (byte)(objCharacter.Race - 1)))
                            {
                                objCharacter.Reputation[i].Flags = tmpFactionInfo.Value.rep_flags[j];
                                objCharacter.Reputation[i].Value = tmpFactionInfo.Value.rep_stats[j];
                                break;
                            }
                        }

                        break;
                    }
                }
            }
        }
    }
}