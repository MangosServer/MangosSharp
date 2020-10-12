using System;
using System.Collections.Generic;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Player;
using Mangos.World.DataStores;

namespace Mangos.World.Player
{
	public class WS_Player_Initializator
	{
		public int DEFAULT_MAX_LEVEL;

		public int[] XPTable;

		public WS_Player_Initializator()
		{
			DEFAULT_MAX_LEVEL = 60;
			XPTable = new int[checked(DEFAULT_MAX_LEVEL + 1)];
		}

		public int CalculateStartingLIFE(ref WS_PlayerData.CharacterObject objCharacter, int baseLIFE)
		{
			checked
			{
				if (objCharacter.Stamina.Base < 20)
				{
					return baseLIFE + (objCharacter.Stamina.Base - 20);
				}
				return baseLIFE + 10 * (objCharacter.Stamina.Base - 20);
			}
		}

		public int CalculateStartingMANA(ref WS_PlayerData.CharacterObject objCharacter, int baseMANA)
		{
			checked
			{
				if (objCharacter.Intellect.Base < 20)
				{
					return baseMANA + (objCharacter.Intellect.Base - 20);
				}
				return baseMANA + 15 * (objCharacter.Intellect.Base - 20);
			}
		}

		private int gainStat(int level, double a3, double a2, double a1, double a0)
		{
			return checked((int)Math.Round(a3 * (double)level * (double)level * (double)level + a2 * (double)level * (double)level + a1 * (double)level + a0) - (int)Math.Round(a3 * (double)(level - 1) * (double)(level - 1) * (double)(level - 1) + a2 * (double)(level - 1) * (double)(level - 1) + a1 * (double)(level - 1) + a0));
		}

		public void CalculateOnLevelUP(ref WS_PlayerData.CharacterObject objCharacter)
		{
			int baseInt = objCharacter.Intellect.Base;
			int baseSpi = objCharacter.Spirit.Base;
			int baseAgi = objCharacter.Agility.Base;
			int baseLife = objCharacter.Life.Maximum;
			checked
			{
				switch (objCharacter.Classe)
				{
				case Classes.CLASS_DRUID:
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
						objCharacter.Mana.Base += 20 + unchecked((int)objCharacter.Level);
					}
					else
					{
						objCharacter.Mana.Base += 45;
					}
					objCharacter.Strength.Base += gainStat(objCharacter.Level, 2.1E-05, 0.003009, 0.486493, -0.400003);
					objCharacter.Intellect.Base += gainStat(objCharacter.Level, 3.8E-05, 0.005145, 0.871006, -0.832029);
					objCharacter.Agility.Base += gainStat(objCharacter.Level, 4.1E-05, 0.00044, 0.512076, -1.000317);
					objCharacter.Stamina.Base += gainStat(objCharacter.Level, 2.3E-05, 0.003345, 0.56005, -0.562058);
					objCharacter.Spirit.Base += gainStat(objCharacter.Level, 5.9E-05, 0.004044, 1.04, -1.488504);
					break;
				case Classes.CLASS_HUNTER:
					if (objCharacter.Level <= 13)
					{
						objCharacter.Life.Base += 17;
					}
					else
					{
						objCharacter.Life.Base += unchecked((int)objCharacter.Level) + 4;
					}
					if (objCharacter.Level <= 27)
					{
						objCharacter.Mana.Base += 18 + unchecked((int)objCharacter.Level);
					}
					else
					{
						objCharacter.Mana.Base += 45;
					}
					objCharacter.Strength.Base += gainStat(objCharacter.Level, 2.2E-05, 0.0018, 0.407867, -0.550889);
					objCharacter.Intellect.Base += gainStat(objCharacter.Level, 2E-05, 0.003007, 0.505215, -0.500642);
					objCharacter.Agility.Base += gainStat(objCharacter.Level, 4E-05, 0.007416, 1.125108, -1.003045);
					objCharacter.Stamina.Base += gainStat(objCharacter.Level, 3.1E-05, 0.00448, 0.78004, -0.800471);
					objCharacter.Spirit.Base += gainStat(objCharacter.Level, 1.7E-05, 0.003803, 0.536846, -0.490026);
					break;
				case Classes.CLASS_MAGE:
					if (objCharacter.Level <= 25)
					{
						objCharacter.Life.Base += 15;
					}
					else
					{
						objCharacter.Life.Base += unchecked((int)objCharacter.Level) - 8;
					}
					if (objCharacter.Level <= 27)
					{
						objCharacter.Mana.Base += 23 + unchecked((int)objCharacter.Level);
					}
					else
					{
						objCharacter.Mana.Base += 51;
					}
					objCharacter.Strength.Base += gainStat(objCharacter.Level, 2E-06, 0.001003, 0.10089, -0.076055);
					objCharacter.Intellect.Base += gainStat(objCharacter.Level, 4E-05, 0.007416, 1.125108, -1.003045);
					objCharacter.Agility.Base += gainStat(objCharacter.Level, 8E-06, 0.001001, 0.16319, -0.06428);
					objCharacter.Stamina.Base += gainStat(objCharacter.Level, 6E-06, 0.002031, 0.27836, -0.340077);
					objCharacter.Spirit.Base += gainStat(objCharacter.Level, 3.9E-05, 0.006981, 1.09009, -1.00607);
					break;
				case Classes.CLASS_PALADIN:
					if (objCharacter.Level <= 14)
					{
						objCharacter.Life.Base += 18;
					}
					else
					{
						objCharacter.Life.Base += unchecked((int)objCharacter.Level) + 4;
					}
					if (objCharacter.Level <= 25)
					{
						objCharacter.Mana.Base += 17 + unchecked((int)objCharacter.Level);
					}
					else
					{
						objCharacter.Mana.Base += 42;
					}
					objCharacter.Strength.Base += gainStat(objCharacter.Level, 3.7E-05, 0.005455, 0.940039, -1.00009);
					objCharacter.Intellect.Base += gainStat(objCharacter.Level, 2.3E-05, 0.003345, 0.56005, -0.562058);
					objCharacter.Agility.Base += gainStat(objCharacter.Level, 2E-05, 0.003007, 0.505215, -0.500642);
					objCharacter.Stamina.Base += gainStat(objCharacter.Level, 3.8E-05, 0.005145, 0.871006, -0.832029);
					objCharacter.Spirit.Base += gainStat(objCharacter.Level, 3.2E-05, 0.003025, 0.61589, -0.640307);
					break;
				case Classes.CLASS_PRIEST:
					if (objCharacter.Level <= 22)
					{
						objCharacter.Life.Base += 15;
					}
					else
					{
						objCharacter.Life.Base += unchecked((int)objCharacter.Level) - 6;
					}
					if (objCharacter.Level <= 33)
					{
						objCharacter.Mana.Base += 22 + unchecked((int)objCharacter.Level);
					}
					else
					{
						objCharacter.Mana.Base += 54;
					}
					if (objCharacter.Level == 34)
					{
						objCharacter.Mana.Base += 15;
					}
					objCharacter.Strength.Base += gainStat(objCharacter.Level, 8E-06, 0.001001, 0.16319, -0.06428);
					objCharacter.Intellect.Base += gainStat(objCharacter.Level, 3.9E-05, 0.006981, 1.09009, -1.00607);
					objCharacter.Agility.Base += gainStat(objCharacter.Level, 2.2E-05, 2.2E-05, 0.260756, -0.494);
					objCharacter.Stamina.Base += gainStat(objCharacter.Level, 2.4E-05, 0.000981, 0.364935, -0.5709);
					objCharacter.Spirit.Base += gainStat(objCharacter.Level, 4E-05, 0.007416, 1.125108, -1.003045);
					break;
				case Classes.CLASS_ROGUE:
					if (objCharacter.Level <= 15)
					{
						objCharacter.Life.Base += 17;
					}
					else
					{
						objCharacter.Life.Base += unchecked((int)objCharacter.Level) + 2;
					}
					objCharacter.Strength.Base += gainStat(objCharacter.Level, 2.5E-05, 0.00417, 0.654096, -0.601491);
					objCharacter.Intellect.Base += gainStat(objCharacter.Level, 8E-06, 0.001001, 0.16319, -0.06428);
					objCharacter.Agility.Base += gainStat(objCharacter.Level, 3.8E-05, 0.007834, 1.191028, -1.20394);
					objCharacter.Stamina.Base += gainStat(objCharacter.Level, 3.2E-05, 0.003025, 0.61589, -0.640307);
					objCharacter.Spirit.Base += gainStat(objCharacter.Level, 2.4E-05, 0.000981, 0.364935, -0.5709);
					break;
				case Classes.CLASS_SHAMAN:
					if (objCharacter.Level <= 16)
					{
						objCharacter.Life.Base += 17;
					}
					else
					{
						objCharacter.Life.Base += unchecked((int)objCharacter.Level) + 1;
					}
					if (objCharacter.Level <= 32)
					{
						objCharacter.Mana.Base += 19 + unchecked((int)objCharacter.Level);
					}
					else
					{
						objCharacter.Mana.Base += 52;
					}
					objCharacter.Strength.Base += gainStat(objCharacter.Level, 3.5E-05, 0.003641, 0.73431, -0.800626);
					objCharacter.Intellect.Base += gainStat(objCharacter.Level, 3.1E-05, 0.00448, 0.78004, -0.800471);
					objCharacter.Agility.Base += gainStat(objCharacter.Level, 2.2E-05, 0.0018, 0.407867, -0.550889);
					objCharacter.Stamina.Base += gainStat(objCharacter.Level, 2E-05, 0.00603, 0.80957, -0.80922);
					objCharacter.Spirit.Base += gainStat(objCharacter.Level, 3.8E-05, 0.005145, 0.871006, -0.832029);
					break;
				case Classes.CLASS_WARLOCK:
					if (objCharacter.Level <= 17)
					{
						objCharacter.Life.Base += 15;
					}
					else
					{
						objCharacter.Life.Base += unchecked((int)objCharacter.Level) - 2;
					}
					if (objCharacter.Level <= 30)
					{
						objCharacter.Mana.Base += 21 + unchecked((int)objCharacter.Level);
					}
					else
					{
						objCharacter.Mana.Base += 51;
					}
					objCharacter.Strength.Base += gainStat(objCharacter.Level, 6E-06, 0.002031, 0.27836, -0.340077);
					objCharacter.Intellect.Base += gainStat(objCharacter.Level, 5.9E-05, 0.004044, 1.04, -1.488504);
					objCharacter.Agility.Base += gainStat(objCharacter.Level, 2.4E-05, 0.000981, 0.364935, -0.5709);
					objCharacter.Stamina.Base += gainStat(objCharacter.Level, 2.1E-05, 0.003009, 0.486493, -0.400003);
					objCharacter.Spirit.Base += gainStat(objCharacter.Level, 4E-05, 0.006404, 1.038791, -1.039076);
					break;
				case Classes.CLASS_WARRIOR:
					if (objCharacter.Level <= 14)
					{
						objCharacter.Life.Base += 19;
					}
					else
					{
						objCharacter.Life.Base += unchecked((int)objCharacter.Level) + 10;
					}
					objCharacter.Strength.Base += gainStat(objCharacter.Level, 3.9E-05, 0.006902, 1.0800399999999999, -1.051701);
					objCharacter.Intellect.Base += gainStat(objCharacter.Level, 2E-06, 0.001003, 0.10089, -0.076055);
					objCharacter.Agility.Base += gainStat(objCharacter.Level, 2.2E-05, 0.0046, 0.655333, -0.600356);
					objCharacter.Stamina.Base += gainStat(objCharacter.Level, 5.9E-05, 0.004044, 1.04, -1.488504);
					objCharacter.Spirit.Base += gainStat(objCharacter.Level, 6E-06, 0.002031, 0.27836, -0.340077);
					break;
				}
				if (objCharacter.Agility.Base != baseAgi)
				{
					objCharacter.Resistances[0].Base += (objCharacter.Agility.Base - baseAgi) * 2;
				}
				if (objCharacter.Spirit.Base != baseSpi)
				{
					objCharacter.Life.Base += 10 * (objCharacter.Spirit.Base - baseSpi);
				}
				if (objCharacter.Intellect.Base != baseInt && objCharacter.ManaType == ManaTypes.TYPE_MANA)
				{
					objCharacter.Mana.Base += 15 * (objCharacter.Intellect.Base - baseInt);
				}
				objCharacter.Damage.Minimum += 1f;
				objCharacter.RangedDamage.Minimum += 1f;
				objCharacter.Damage.Maximum += 1f;
				objCharacter.RangedDamage.Maximum += 1f;
				if (objCharacter.Level > 9)
				{
					objCharacter.TalentPoints++;
				}
				foreach (KeyValuePair<int, WS_PlayerHelper.TSkill> Skill in objCharacter.Skills)
				{
					if (WorldServiceLocator._WS_DBCDatabase.SkillLines[Skill.Key] == 6)
					{
						Skill.Value.Base += 5;
					}
				}
			}
		}

		public ManaTypes GetClassManaType(Classes Classe)
		{
			switch (Classe)
			{
			case Classes.CLASS_PALADIN:
			case Classes.CLASS_HUNTER:
			case Classes.CLASS_PRIEST:
			case Classes.CLASS_SHAMAN:
			case Classes.CLASS_MAGE:
			case Classes.CLASS_WARLOCK:
			case Classes.CLASS_DRUID:
				return ManaTypes.TYPE_MANA;
			case Classes.CLASS_ROGUE:
				return ManaTypes.TYPE_ENERGY;
			case Classes.CLASS_WARRIOR:
				return ManaTypes.TYPE_RAGE;
			default:
				return ManaTypes.TYPE_MANA;
			}
		}

		public void InitializeReputations(ref WS_PlayerData.CharacterObject objCharacter)
		{
			byte i = 0;
			do
			{
				objCharacter.Reputation[i] = new WS_PlayerHelper.TReputation
				{
					Value = 0,
					Flags = 0
				};
				checked
				{
					foreach (KeyValuePair<int, WS_DBCDatabase.TFaction> tmpFactionInfo in WorldServiceLocator._WS_DBCDatabase.FactionInfo)
					{
						if (tmpFactionInfo.Value.VisibleID != i)
						{
							continue;
						}
						byte j = 0;
						do
						{
							if (WorldServiceLocator._Functions.HaveFlag((uint)tmpFactionInfo.Value.flags[j], (byte)(unchecked((int)objCharacter.Race) - 1)))
							{
								objCharacter.Reputation[i].Flags = tmpFactionInfo.Value.rep_flags[j];
								objCharacter.Reputation[i].Value = tmpFactionInfo.Value.rep_stats[j];
								break;
							}
							j = (byte)unchecked((uint)(j + 1));
						}
						while (unchecked((uint)j) <= 3u);
						break;
					}
					i = (byte)unchecked((uint)(i + 1));
				}
			}
			while ((uint)i <= 63u);
		}
	}
}
