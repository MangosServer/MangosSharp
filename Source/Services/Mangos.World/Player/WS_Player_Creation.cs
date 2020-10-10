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
using Mangos.Common.Enums.Character;
using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Misc;
using Mangos.World.Objects;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.World.Player
{
    public class WS_Player_Creation
    {
        public int CreateCharacter(string Account, string Name, byte Race, byte Classe, byte Gender, byte Skin, byte Face, byte HairStyle, byte HairColor, byte FacialHair, byte OutfitID)
        {
            var Character = new WS_PlayerData.CharacterObject();
            var MySQLQuery = new DataTable();

            // DONE: Make name capitalized as on official
            Character.Name = WorldServiceLocator._Functions.CapitalizeName(ref Name);
            Character.Race = (Common.Enums.Player.Races)Race;
            Character.Classe = (Common.Enums.Player.Classes)Classe;
            Character.Gender = (Common.Enums.Player.Genders)Gender;
            Character.Skin = Skin;
            Character.Face = Face;
            Character.HairStyle = HairStyle;
            Character.HairColor = HairColor;
            Character.FacialHair = FacialHair;

            // DONE: Query Access Level and Account ID
            WorldServiceLocator._WorldServer.AccountDatabase.Query(string.Format("SELECT id, gmlevel FROM account WHERE username = \"{0}\";", Account), MySQLQuery);
            int Account_ID = Conversions.ToInteger(MySQLQuery.Rows[0]["id"]);
            AccessLevel Account_Access = (AccessLevel)MySQLQuery.Rows[0]["gmlevel"];
            Character.Access = Account_Access;
            if (!WorldServiceLocator._Functions.ValidateName(Character.Name))
            {
                return (int)CharResponse.CHAR_NAME_INVALID_CHARACTER;
            }

            // DONE: Name In Use
            try
            {
                MySQLQuery.Clear();
                WorldServiceLocator._WorldServer.CharacterDatabase.Query(string.Format("SELECT char_name FROM characters WHERE char_name = \"{0}\";", Character.Name), MySQLQuery);
                if (MySQLQuery.Rows.Count > 0)
                {
                    return (int)CharResponse.CHAR_CREATE_NAME_IN_USE;
                }
            }
            catch
            {
                return (int)CharResponse.CHAR_CREATE_FAILED;
            }

            // DONE: Check for disabled class/race, only for non GM/Admin
            if (WorldServiceLocator._Global_Constants.SERVER_CONFIG_DISABLED_CLASSES(Character.Classe - 1) == true || WorldServiceLocator._Global_Constants.SERVER_CONFIG_DISABLED_RACES(Character.Race - 1) == true && Account_Access < AccessLevel.GameMaster)
            {
                return (int)CharResponse.CHAR_CREATE_DISABLED;
            }

            // DONE: Check for both horde and alliance
            // TODO: Only if it's a pvp realm
            if (Account_Access <= AccessLevel.Player)
            {
                MySQLQuery.Clear();
                WorldServiceLocator._WorldServer.CharacterDatabase.Query(string.Format("SELECT char_race FROM characters WHERE account_id = \"{0}\" LIMIT 1;", (object)Account_ID), MySQLQuery);
                if (MySQLQuery.Rows.Count > 0)
                {
                    if (Character.IsHorde != WorldServiceLocator._Functions.GetCharacterSide(Conversions.ToByte(MySQLQuery.Rows[0]["char_race"])))
                    {
                        return (int)CharResponse.CHAR_CREATE_PVP_TEAMS_VIOLATION;
                    }
                }
            }

            // DONE: Check for MAX characters limit on this realm
            MySQLQuery.Clear();
            WorldServiceLocator._WorldServer.CharacterDatabase.Query(string.Format("SELECT char_name FROM characters WHERE account_id = \"{0}\";", (object)Account_ID), MySQLQuery);
            if (MySQLQuery.Rows.Count >= 10)
            {
                return (int)CharResponse.CHAR_CREATE_SERVER_LIMIT;
            }

            // DONE: Check for max characters in total on all realms
            MySQLQuery.Clear();
            WorldServiceLocator._WorldServer.CharacterDatabase.Query(string.Format("SELECT char_name FROM characters WHERE account_id = \"{0}\";", (object)Account_ID), MySQLQuery);
            if (MySQLQuery.Rows.Count >= 10)
            {
                return (int)CharResponse.CHAR_CREATE_ACCOUNT_LIMIT;
            }

            // DONE: Generate GUID, MySQL Auto generation
            // DONE: Create Char
            try
            {
                WorldServiceLocator._WS_Player_Initializator.InitializeReputations(ref Character);
                CreateCharacter(ref Character);
                Character.SaveAsNewCharacter(Account_ID);
                CreateCharacterSpells(ref Character);
                CreateCharacterItems(ref Character);
            }

            // _WorldServer.Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_CHAR_CREATE [{2}]", client.IP, client.Port, Character.Name)
            catch (Exception err)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "Error initializing character! {0} {1}", Environment.NewLine, err.ToString());
                return (int)CharResponse.CHAR_CREATE_FAILED;
            }
            finally
            {
                Character.Dispose();
            }

            return (int)CharResponse.CHAR_CREATE_SUCCESS;
        }

        public void CreateCharacter(ref WS_PlayerData.CharacterObject objCharacter)
        {
            var CreateInfo = new DataTable();
            var CreateInfoBars = new DataTable();
            var CreateInfoSkills = new DataTable();
            var LevelStats = new DataTable();
            var ClassLevelStats = new DataTable();
            WorldServiceLocator._WorldServer.WorldDatabase.Query(string.Format("SELECT * FROM playercreateinfo WHERE race = {0} AND class = {1};", Conversions.ToInteger(objCharacter.Race), Conversions.ToInteger(objCharacter.Classe)), CreateInfo);
            if (CreateInfo.Rows.Count <= 0)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "No information found in playercreateinfo table for Race: {0}, Class: {1}", objCharacter.Race, objCharacter.Classe);
            }

            WorldServiceLocator._WorldServer.WorldDatabase.Query(string.Format("SELECT * FROM playercreateinfo_action WHERE race = {0} AND class = {1} ORDER BY button;", Conversions.ToInteger(objCharacter.Race), Conversions.ToInteger(objCharacter.Classe)), CreateInfoBars);
            if (CreateInfoBars.Rows.Count <= 0)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "No information found in playercreateinfo_action table for Race: {0}, Class: {1}", objCharacter.Race, objCharacter.Classe);
            }

            WorldServiceLocator._WorldServer.WorldDatabase.Query(string.Format("SELECT * FROM playercreateinfo_skill WHERE race = {0} AND class = {1};", Conversions.ToInteger(objCharacter.Race), Conversions.ToInteger(objCharacter.Classe)), CreateInfoSkills);
            if (CreateInfoSkills.Rows.Count <= 0)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "No information found in playercreateinfo_skill table for Race: {0}, Class: {1}", objCharacter.Race, objCharacter.Classe);
            }

            WorldServiceLocator._WorldServer.WorldDatabase.Query(string.Format("SELECT * FROM player_levelstats WHERE race = {0} AND class = {1} AND level = {2};", Conversions.ToInteger(objCharacter.Race), Conversions.ToInteger(objCharacter.Classe), Conversions.ToInteger(objCharacter.Level)), LevelStats);
            if (LevelStats.Rows.Count <= 0)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "No information found in player_levelstats table for Race: {0}, Class: {1}, Level: {2}", objCharacter.Race, objCharacter.Classe, objCharacter.Level);
            }

            WorldServiceLocator._WorldServer.WorldDatabase.Query(string.Format("SELECT * FROM player_classlevelstats WHERE class = {0} AND level = {1};", Conversions.ToInteger(objCharacter.Classe), Conversions.ToInteger(objCharacter.Level)), ClassLevelStats);
            if (ClassLevelStats.Rows.Count <= 0)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "No information found in player_classlevelstats table for Class: {0}, Level: {1}", objCharacter.Classe, objCharacter.Level);
            }

            // Initialize Character Variables
            objCharacter.Copper = 0U;
            objCharacter.XP = 0;
            objCharacter.Size = 1.0f;
            objCharacter.Life.Base = 0;
            objCharacter.Life.Current = 0;
            objCharacter.Mana.Base = 0;
            objCharacter.Mana.Current = 0;
            objCharacter.Rage.Current = 0;
            objCharacter.Rage.Base = 0;
            objCharacter.Energy.Current = 0;
            objCharacter.Energy.Base = 0;
            objCharacter.ManaType = WorldServiceLocator._WS_Player_Initializator.GetClassManaType(objCharacter.Classe);

            // Set Character Create Information
            objCharacter.Model = WorldServiceLocator._Functions.GetRaceModel(objCharacter.Race, (int)objCharacter.Gender);
            objCharacter.Faction = WorldServiceLocator._WS_DBCDatabase.CharRaces(objCharacter.Race).FactionID;
            objCharacter.MapID = Conversions.ToUInteger(CreateInfo.Rows[0]["map"]);
            objCharacter.ZoneID = Conversions.ToInteger(CreateInfo.Rows[0]["zone"]);
            objCharacter.positionX = Conversions.ToSingle(CreateInfo.Rows[0]["position_x"]);
            objCharacter.positionY = Conversions.ToSingle(CreateInfo.Rows[0]["position_y"]);
            objCharacter.positionZ = Conversions.ToSingle(CreateInfo.Rows[0]["position_z"]);
            objCharacter.orientation = Conversions.ToSingle(CreateInfo.Rows[0]["orientation"]);
            objCharacter.bindpoint_map_id = (int)objCharacter.MapID;
            objCharacter.bindpoint_zone_id = objCharacter.ZoneID;
            objCharacter.bindpoint_positionX = objCharacter.positionX;
            objCharacter.bindpoint_positionY = objCharacter.positionY;
            objCharacter.bindpoint_positionZ = objCharacter.positionZ;
            objCharacter.Strength.Base = Conversions.ToInteger(LevelStats.Rows[0]["str"]);
            objCharacter.Agility.Base = Conversions.ToInteger(LevelStats.Rows[0]["agi"]);
            objCharacter.Stamina.Base = Conversions.ToInteger(LevelStats.Rows[0]["sta"]);
            objCharacter.Intellect.Base = Conversions.ToInteger(LevelStats.Rows[0]["inte"]);
            objCharacter.Spirit.Base = Conversions.ToInteger(LevelStats.Rows[0]["spi"]);
            objCharacter.Life.Base = Conversions.ToInteger(ClassLevelStats.Rows[0]["basehp"]);
            objCharacter.Life.Current = objCharacter.Life.Maximum;
            switch (objCharacter.ManaType)
            {
                case var @case when @case == ManaTypes.TYPE_MANA:
                    {
                        objCharacter.Mana.Base = Conversions.ToInteger(ClassLevelStats.Rows[0]["basemana"]);
                        objCharacter.Mana.Current = objCharacter.Mana.Maximum;
                        break;
                    }

                case var case1 when case1 == ManaTypes.TYPE_RAGE:
                    {
                        objCharacter.Rage.Base = Conversions.ToInteger(ClassLevelStats.Rows[0]["basemana"]);
                        objCharacter.Rage.Current = 0;
                        break;
                    }

                case var case2 when case2 == ManaTypes.TYPE_ENERGY:
                    {
                        objCharacter.Energy.Base = Conversions.ToInteger(ClassLevelStats.Rows[0]["basemana"]);
                        objCharacter.Energy.Current = 0;
                        break;
                    }
            }

            // TODO: Get damage min and maximum
            objCharacter.Damage.Minimum = 5f;
            objCharacter.Damage.Maximum = 10f;

            // Set Player Create Skills
            foreach (DataRow SkillRow in CreateInfoSkills.Rows)
                objCharacter.LearnSkill(Conversions.ToInteger(SkillRow["Skill"]), Conversions.ToShort(SkillRow["SkillMin"]), Conversions.ToShort(SkillRow["SkillMax"]));

            // Set Player Taxi Zones
            for (int i = 0; i <= 31; i++)
            {
                if (Conversions.ToBoolean((long)WorldServiceLocator._WS_DBCDatabase.CharRaces(objCharacter.Race).TaxiMask & (long)(1 << i)))
                {
                    objCharacter.TaxiZones.Set(i + 1, true);
                }
            }

            // Set Player Create Action Buttons
            foreach (DataRow BarRow in CreateInfoBars.Rows)
            {
                if (Conversions.ToBoolean(Operators.ConditionalCompareObjectGreater(BarRow["action"], 0, false)))
                {
                    int ButtonPos = Conversions.ToInteger(BarRow["button"]);
                    objCharacter.ActionButtons[(byte)ButtonPos] = new WS_PlayerHelper.TActionButton(Conversions.ToInteger(BarRow["action"]), Conversions.ToByte(BarRow["type"]), 0);
                }
            }
        }

        public void CreateCharacterSpells(ref WS_PlayerData.CharacterObject objCharacter)
        {
            var CreateInfoSpells = new DataTable();
            WorldServiceLocator._WorldServer.WorldDatabase.Query(string.Format("SELECT * FROM playercreateinfo_spell WHERE race = {0} AND class = {1};", Conversions.ToInteger(objCharacter.Race), Conversions.ToInteger(objCharacter.Classe)), CreateInfoSpells);
            if (CreateInfoSpells.Rows.Count <= 0)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "No information found in playercreateinfo_spell table Race: {0}, Class: {1}", objCharacter.Race, objCharacter.Classe);
            }

            // Set Player Create Spells
            foreach (DataRow SpellRow in CreateInfoSpells.Rows)
                objCharacter.LearnSpell(Conversions.ToInteger(SpellRow["Spell"]));
        }

        public void CreateCharacterItems(ref WS_PlayerData.CharacterObject objCharacter)
        {
            var CreateInfoItems = new DataTable();
            WorldServiceLocator._WorldServer.WorldDatabase.Query(string.Format("SELECT * FROM playercreateinfo_item WHERE race = {0} AND class = {1};", Conversions.ToInteger(objCharacter.Race), Conversions.ToInteger(objCharacter.Classe)), CreateInfoItems);
            if (CreateInfoItems.Rows.Count <= 0)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.FAILED, "No information found in playercreateinfo_item table for Race: {0}, Class: {1}", objCharacter.Race, objCharacter.Classe);
            }

            // Set Player Create Items
            var Items = new Dictionary<int, int>();
            var Used = new List<int>();
            foreach (DataRow ItemRow in CreateInfoItems.Rows)
                Items.Add(Conversions.ToInteger(ItemRow["itemid"]), Conversions.ToInteger(ItemRow["amount"]));

            // First add bags
            foreach (KeyValuePair<int, int> Item in Items)
            {
                if (WorldServiceLocator._WorldServer.ITEMDatabase.ContainsKey(Item.Key) == false)
                {
                    var newItem = new WS_Items.ItemInfo(Item.Key);
                    // The New does a an add to the .Containskey collection above
                }

                if (WorldServiceLocator._WorldServer.ITEMDatabase[Item.Key].ContainerSlots > 0)
                {
                    var Slots = WorldServiceLocator._WorldServer.ITEMDatabase[Item.Key].GetSlots;
                    foreach (byte tmpSlot in Slots)
                    {
                        if (!objCharacter.Items.ContainsKey(tmpSlot))
                        {
                            objCharacter.ItemADD(Item.Key, 0, tmpSlot, Item.Value);
                            Used.Add(Item.Key);
                            break;
                        }
                    }
                }
            }

            // Then add the rest of the items
            foreach (KeyValuePair<int, int> Item in Items)
            {
                if (Used.Contains(Item.Key))
                    continue;
                var Slots = WorldServiceLocator._WorldServer.ITEMDatabase[Item.Key].GetSlots;
                foreach (byte tmpSlot in Slots)
                {
                    if (!objCharacter.Items.ContainsKey(tmpSlot))
                    {
                        objCharacter.ItemADD(Item.Key, 0, tmpSlot, Item.Value);
                        goto NextItem;
                    }
                }

                objCharacter.ItemADD(Item.Key, 255, 255, Item.Value);
            NextItem:
                ;
            }
        }
    }
}