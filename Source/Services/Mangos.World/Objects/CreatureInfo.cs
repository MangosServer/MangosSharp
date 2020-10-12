using System;
using System.Data;
using System.IO;
using System.Runtime.CompilerServices;
using Mangos.Common.Enums.Global;
using Mangos.World.Globals;
using Mangos.World.Gossip;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.World.Objects
{
	public class CreatureInfo : IDisposable
	{
		private bool _disposedValue;

		private readonly bool found_;

		public int Id;

		public string Name;

		public string SubName;

		public float Size;

		public int ModelA1;

		public int ModelA2;

		public int ModelH1;

		public int ModelH2;

		public int MinLife;

		public int MaxLife;

		public int MinMana;

		public int MaxMana;

		public byte ManaType;

		public short Faction;

		public byte CreatureType;

		public byte CreatureFamily;

		public byte Elite;

		public byte HonorRank;

		public WS_Items.TDamage Damage;

		public WS_Items.TDamage RangedDamage;

		public int AttackPower;

		public int RangedAttackPower;

		public int[] Resistances;

		public float WalkSpeed;

		public float RunSpeed;

		public short BaseAttackTime;

		public short BaseRangedAttackTime;

		public int cNpcFlags;

		public int DynFlags;

		public int cFlags;

		public uint TypeFlags;

		public byte LevelMin;

		public byte LevelMax;

		public byte Leader;

		public int TrainerType;

		public int TrainerSpell;

		public byte Classe;

		public byte Race;

		public int PetSpellDataID;

		public int[] Spells;

		public int LootID;

		public int SkinLootID;

		public int PocketLootID;

		public uint MinGold;

		public uint MaxGold;

		public int EquipmentID;

		public uint MechanicImmune;

		public float UnkFloat1;

		public float UnkFloat2;

		public string AIScriptSource;

		public TBaseTalk TalkScript;

		public int Life => WorldServiceLocator._WorldServer.Rnd.Next(MinLife, MaxLife);

		public int Mana => WorldServiceLocator._WorldServer.Rnd.Next(MinMana, MaxMana);

		public int GetRandomModel
		{
			get
			{
				int[] modelIDs = new int[4];
				int current = 0;
				checked
				{
					if (ModelA1 != 0)
					{
						modelIDs[current] = ModelA1;
						current++;
					}
					if (ModelA2 != 0)
					{
						modelIDs[current] = ModelA2;
						current++;
					}
					if (ModelH1 != 0)
					{
						modelIDs[current] = ModelH1;
						current++;
					}
					if (ModelH2 != 0)
					{
						modelIDs[current] = ModelH2;
						current++;
					}
					if (current == 0)
					{
						return 0;
					}
					return modelIDs[WorldServiceLocator._WorldServer.Rnd.Next(0, current)];
				}
			}
		}

		public int GetFirstModel
		{
			get
			{
				if (ModelA1 != 0)
				{
					return ModelA1;
				}
				if (ModelA2 != 0)
				{
					return ModelA2;
				}
				if (ModelH1 != 0)
				{
					return ModelH1;
				}
				if (ModelH2 != 0)
				{
					return ModelH2;
				}
				return 0;
			}
		}

		public CreatureInfo(int CreatureID)
			: this()
		{
			Id = CreatureID;
			WorldServiceLocator._WorldServer.CREATURESDatabase.Add(Id, this);
			DataTable MySQLQuery = new DataTable();
			WorldServiceLocator._WorldServer.WorldDatabase.Query($"SELECT * FROM creature_template LEFT JOIN creature_template_spells ON creature_template.entry = creature_template_spells.`entry` WHERE creature_template.entry = {CreatureID};", ref MySQLQuery);
			if (MySQLQuery.Rows.Count == 0)
			{
				WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "CreatureID {0} not found in SQL database.", CreatureID);
				found_ = false;
				return;
			}
			found_ = true;
			ModelA1 = Conversions.ToInteger(MySQLQuery.Rows[0]["modelid1"]);
			ModelA2 = Conversions.ToInteger(MySQLQuery.Rows[0]["Modelid2"]);
			ModelH1 = Conversions.ToInteger(MySQLQuery.Rows[0]["modelid3"]);
			ModelH2 = Conversions.ToInteger(MySQLQuery.Rows[0]["modelid4"]);
			Name = Conversions.ToString(MySQLQuery.Rows[0]["name"]);
			try
			{
				SubName = Conversions.ToString(MySQLQuery.Rows[0]["subname"]);
			}
			catch (Exception projectError)
			{
				ProjectData.SetProjectError(projectError);
				SubName = "";
				ProjectData.ClearProjectError();
			}
			Size = Conversions.ToSingle(MySQLQuery.Rows[0]["scale"]);
			MinLife = Conversions.ToInteger(MySQLQuery.Rows[0]["MinLevelHealth"]);
			MaxLife = Conversions.ToInteger(MySQLQuery.Rows[0]["MaxLevelHealth"]);
			MinMana = Conversions.ToInteger(MySQLQuery.Rows[0]["MinLevelMana"]);
			MaxMana = Conversions.ToInteger(MySQLQuery.Rows[0]["MaxLevelMana"]);
			ManaType = 0;
			Faction = Conversions.ToShort(MySQLQuery.Rows[0]["factionAlliance"]);
			Elite = Conversions.ToByte(MySQLQuery.Rows[0]["rank"]);
			Damage.Maximum = Conversions.ToSingle(MySQLQuery.Rows[0]["MaxMeleeDmg"]);
			RangedDamage.Maximum = Conversions.ToSingle(MySQLQuery.Rows[0]["MaxRangedDmg"]);
			Damage.Minimum = Conversions.ToSingle(MySQLQuery.Rows[0]["MinMeleeDmg"]);
			RangedDamage.Minimum = Conversions.ToSingle(MySQLQuery.Rows[0]["MinRangedDmg"]);
			AttackPower = Conversions.ToInteger(MySQLQuery.Rows[0]["MeleeAttackPower"]);
			RangedAttackPower = Conversions.ToInteger(MySQLQuery.Rows[0]["RangedAttackPower"]);
			WalkSpeed = Conversions.ToSingle(MySQLQuery.Rows[0]["SpeedWalk"]);
			RunSpeed = Conversions.ToSingle(MySQLQuery.Rows[0]["SpeedRun"]);
			BaseAttackTime = Conversions.ToShort(MySQLQuery.Rows[0]["MeleeBaseAttackTime"]);
			BaseRangedAttackTime = Conversions.ToShort(MySQLQuery.Rows[0]["RangedBaseAttackTime"]);
			cNpcFlags = Conversions.ToInteger(MySQLQuery.Rows[0]["NpcFlags"]);
			DynFlags = Conversions.ToInteger(MySQLQuery.Rows[0]["DynamicFlags"]);
			cFlags = Conversions.ToInteger(MySQLQuery.Rows[0]["UnitFlags"]);
			TypeFlags = Conversions.ToUInteger(MySQLQuery.Rows[0]["CreatureTypeFlags"]);
			CreatureType = Conversions.ToByte(MySQLQuery.Rows[0]["CreatureType"]);
			CreatureFamily = Conversions.ToByte(MySQLQuery.Rows[0]["Family"]);
			LevelMin = Conversions.ToByte(MySQLQuery.Rows[0]["MinLevel"]);
			LevelMax = Conversions.ToByte(MySQLQuery.Rows[0]["MaxLevel"]);
			TrainerType = Conversions.ToInteger(MySQLQuery.Rows[0]["TrainerType"]);
			TrainerSpell = Conversions.ToInteger(MySQLQuery.Rows[0]["TrainerSpell"]);
			Classe = Conversions.ToByte(MySQLQuery.Rows[0]["TrainerClass"]);
			Race = Conversions.ToByte(MySQLQuery.Rows[0]["TrainerRace"]);
			Leader = Conversions.ToByte(MySQLQuery.Rows[0]["RacialLeader"]);
			if (!Information.IsDBNull(RuntimeHelpers.GetObjectValue(MySQLQuery.Rows[0]["spell1"])))
			{
				Spells[0] = Conversions.ToInteger(MySQLQuery.Rows[0]["spell1"]);
			}
			else
			{
				Spells[0] = 0;
			}
			if (!Information.IsDBNull(RuntimeHelpers.GetObjectValue(MySQLQuery.Rows[0]["spell2"])))
			{
				Spells[1] = Conversions.ToInteger(MySQLQuery.Rows[0]["spell2"]);
			}
			else
			{
				Spells[1] = 0;
			}
			if (!Information.IsDBNull(RuntimeHelpers.GetObjectValue(MySQLQuery.Rows[0]["spell3"])))
			{
				Spells[2] = Conversions.ToInteger(MySQLQuery.Rows[0]["spell3"]);
			}
			else
			{
				Spells[2] = 0;
			}
			if (!Information.IsDBNull(RuntimeHelpers.GetObjectValue(MySQLQuery.Rows[0]["spell4"])))
			{
				Spells[3] = Conversions.ToInteger(MySQLQuery.Rows[0]["spell4"]);
			}
			else
			{
				Spells[3] = 0;
			}
			PetSpellDataID = Conversions.ToInteger(MySQLQuery.Rows[0]["PetSpellDataId"]);
			LootID = Conversions.ToInteger(MySQLQuery.Rows[0]["LootId"]);
			SkinLootID = Conversions.ToInteger(MySQLQuery.Rows[0]["SkinningLootId"]);
			PocketLootID = Conversions.ToInteger(MySQLQuery.Rows[0]["PickpocketLootId"]);
			MinGold = Conversions.ToUInteger(MySQLQuery.Rows[0]["MinLootGold"]);
			MaxGold = Conversions.ToUInteger(MySQLQuery.Rows[0]["MaxLootGold"]);
			Resistances[0] = Conversions.ToInteger(MySQLQuery.Rows[0]["Armor"]);
			Resistances[1] = Conversions.ToInteger(MySQLQuery.Rows[0]["ResistanceHoly"]);
			Resistances[2] = Conversions.ToInteger(MySQLQuery.Rows[0]["ResistanceFire"]);
			Resistances[3] = Conversions.ToInteger(MySQLQuery.Rows[0]["ResistanceNature"]);
			Resistances[4] = Conversions.ToInteger(MySQLQuery.Rows[0]["ResistanceFrost"]);
			Resistances[5] = Conversions.ToInteger(MySQLQuery.Rows[0]["ResistanceShadow"]);
			Resistances[6] = Conversions.ToInteger(MySQLQuery.Rows[0]["ResistanceArcane"]);
			EquipmentID = Conversions.ToInteger(MySQLQuery.Rows[0]["EquipmentTemplateId"]);
			MechanicImmune = Conversions.ToUInteger(MySQLQuery.Rows[0]["SchoolImmuneMask"]);
			if (File.Exists("scripts\\gossip\\" + WorldServiceLocator._Functions.FixName(Name) + ".vb"))
			{
				ScriptedObject tmpScript = new ScriptedObject("scripts\\gossip\\" + WorldServiceLocator._Functions.FixName(Name) + ".vb", "", InMemory: true);
				TalkScript = (TBaseTalk)tmpScript.InvokeConstructor("TalkScript");
				tmpScript.Dispose();
			}
			else if (((uint)cNpcFlags & 0x10u) != 0)
			{
				TalkScript = new WS_NPCs.TDefaultTalk();
			}
			else if (((uint)cNpcFlags & 0x40u) != 0)
			{
				TalkScript = new WS_GuardGossip.TGuardTalk();
			}
			else if (cNpcFlags == 0)
			{
				TalkScript = null;
			}
			else if (cNpcFlags == 1)
			{
				TalkScript = new WS_NPCs.TDefaultTalk();
			}
			else
			{
				TalkScript = new WS_NPCs.TDefaultTalk();
			}
		}

		public CreatureInfo()
		{
			found_ = false;
			Id = 0;
			Name = "MISSING_CREATURE_INFO";
			SubName = "";
			Size = 1f;
			ModelA1 = 262;
			ModelA2 = 0;
			ModelH1 = 0;
			ModelH2 = 0;
			MinLife = 1;
			MaxLife = 1;
			MinMana = 1;
			MaxMana = 1;
			ManaType = 0;
			Faction = 0;
			CreatureType = 0;
			CreatureFamily = 0;
			Elite = 0;
			HonorRank = 0;
			Damage = new WS_Items.TDamage();
			RangedDamage = new WS_Items.TDamage();
			AttackPower = 0;
			RangedAttackPower = 0;
			Resistances = new int[7];
			WalkSpeed = WorldServiceLocator._Global_Constants.UNIT_NORMAL_WALK_SPEED;
			RunSpeed = WorldServiceLocator._Global_Constants.UNIT_NORMAL_RUN_SPEED;
			BaseAttackTime = 2000;
			BaseRangedAttackTime = 2000;
			LevelMin = 1;
			LevelMax = 1;
			Leader = 0;
			TrainerSpell = 0;
			Classe = 0;
			Race = 0;
			PetSpellDataID = 0;
			Spells = new int[4];
			LootID = 0;
			SkinLootID = 0;
			PocketLootID = 0;
			MinGold = 0u;
			MaxGold = 0u;
			EquipmentID = 0;
			MechanicImmune = 0u;
			UnkFloat1 = 1f;
			UnkFloat2 = 1f;
			AIScriptSource = "";
			TalkScript = null;
			Damage.Minimum = 0.8f * (float)BaseAttackTime / 1000f * ((float)(int)LevelMin * 10f);
			Damage.Maximum = 1.2f * (float)BaseAttackTime / 1000f * ((float)(int)LevelMax * 10f);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposedValue)
			{
				WorldServiceLocator._WorldServer.CREATURESDatabase.Remove(Id);
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
