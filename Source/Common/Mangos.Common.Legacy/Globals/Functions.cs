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

// Note: Temp place holder

using Mangos.Common.Enums.Global;
using Mangos.Common.Enums.Player;
using Mangos.Common.Globals;
using Mangos.Loggers;
using Microsoft.VisualBasic;
using System.Data;

namespace Mangos.Common.Legacy.Globals;

public class Functions
{
    private readonly MangosGlobalConstants mangosGlobalConstants;
    private readonly ILogger logger;

    public Functions(ILogger logger, MangosGlobalConstants mangosGlobalConstants)
    {
        this.logger = logger;
        this.mangosGlobalConstants = mangosGlobalConstants;
    }

    public bool GuidIsCreature(ulong guid)
    {
        return GuidHigh2(guid) == mangosGlobalConstants.GUID_UNIT;
    }

    public bool GuidIsPet(ulong guid)
    {
        return GuidHigh2(guid) == mangosGlobalConstants.GUID_PET;
    }

    public bool GuidIsItem(ulong guid)
    {
        return GuidHigh2(guid) == mangosGlobalConstants.GUID_ITEM;
    }

    public bool GuidIsGameObject(ulong guid)
    {
        return GuidHigh2(guid) == mangosGlobalConstants.GUID_GAMEOBJECT;
    }

    public bool GuidIsDnyamicObject(ulong guid)
    {
        return GuidHigh2(guid) == mangosGlobalConstants.GUID_DYNAMICOBJECT;
    }

    public bool GuidIsTransport(ulong guid)
    {
        return GuidHigh2(guid) == mangosGlobalConstants.GUID_TRANSPORT;
    }

    public bool GuidIsMoTransport(ulong guid)
    {
        return GuidHigh2(guid) == mangosGlobalConstants.GUID_MO_TRANSPORT;
    }

    public bool GuidIsCorpse(ulong guid)
    {
        return GuidHigh2(guid) == mangosGlobalConstants.GUID_CORPSE;
    }

    public bool GuidIsPlayer(ulong guid)
    {
        return GuidHigh2(guid) == mangosGlobalConstants.GUID_PLAYER;
    }

    public ulong GuidHigh2(ulong guid)
    {
        return guid & mangosGlobalConstants.GUID_MASK_HIGH;
    }

    public uint GuidHigh(ulong guid)
    {
        return (uint)((guid & mangosGlobalConstants.GUID_MASK_HIGH) >> 32);
    }

    public uint GuidLow(ulong guid)
    {
        return (uint)(guid & mangosGlobalConstants.GUID_MASK_LOW);
    }

    public int GetShapeshiftModel(ShapeshiftForm form, Races race, int model)
    {
        switch (form)
        {
            case ShapeshiftForm.FORM_CAT:
                {
                    if (race == Races.RACE_NIGHT_ELF)
                    {
                        return 892;
                    }

                    if (race == Races.RACE_TAUREN)
                    {
                        return 8571;
                    }

                    break;
                }

            case ShapeshiftForm.FORM_BEAR:
            case ShapeshiftForm.FORM_DIREBEAR:
                {
                    if (race == Races.RACE_NIGHT_ELF)
                    {
                        return 2281;
                    }

                    if (race == Races.RACE_TAUREN)
                    {
                        return 2289;
                    }

                    break;
                }

            case ShapeshiftForm.FORM_MOONKIN:
                {
                    if (race == Races.RACE_NIGHT_ELF)
                    {
                        return 15374;
                    }

                    if (race == Races.RACE_TAUREN)
                    {
                        return 15375;
                    }

                    break;
                }

            case ShapeshiftForm.FORM_TRAVEL:
                {
                    return 632;
                }

            case ShapeshiftForm.FORM_AQUA:
                {
                    return 2428;
                }

            case ShapeshiftForm.FORM_FLIGHT:
                {
                    if (race == Races.RACE_NIGHT_ELF)
                    {
                        return 20857;
                    }

                    if (race == Races.RACE_TAUREN)
                    {
                        return 20872;
                    }

                    break;
                }

            case ShapeshiftForm.FORM_SWIFT:
                {
                    if (race == Races.RACE_NIGHT_ELF)
                    {
                        return 21243;
                    }

                    if (race == Races.RACE_TAUREN)
                    {
                        return 21244;
                    }

                    break;
                }

            case ShapeshiftForm.FORM_GHOUL:
                {
                    return race == Races.RACE_NIGHT_ELF ? 10045 : model;
                }

            case ShapeshiftForm.FORM_CREATUREBEAR:
                {
                    return 902;
                }

            case ShapeshiftForm.FORM_GHOSTWOLF:
                {
                    return 4613;
                }

            case ShapeshiftForm.FORM_SPIRITOFREDEMPTION:
                {
                    return 12824;
                }

            default:
                {
                    return model;
                }
                // Case ShapeshiftForm.FORM_CREATURECAT
                // Case ShapeshiftForm.FORM_AMBIENT
                // Case ShapeshiftForm.FORM_SHADOW
        }

        return default;
    }

    public ManaTypes GetShapeshiftManaType(ShapeshiftForm form, ManaTypes manaType)
    {
        switch (form)
        {
            case ShapeshiftForm.FORM_CAT:
            case ShapeshiftForm.FORM_STEALTH:
                {
                    return ManaTypes.TYPE_ENERGY;
                }

            case ShapeshiftForm.FORM_AQUA:
            case ShapeshiftForm.FORM_TRAVEL:
            case ShapeshiftForm.FORM_MOONKIN:
            case var @case when @case == ShapeshiftForm.FORM_MOONKIN:
            case var case1 when case1 == ShapeshiftForm.FORM_MOONKIN:
            case ShapeshiftForm.FORM_SPIRITOFREDEMPTION:
            case ShapeshiftForm.FORM_FLIGHT:
            case ShapeshiftForm.FORM_SWIFT:
                {
                    return ManaTypes.TYPE_MANA;
                }

            case ShapeshiftForm.FORM_BEAR:
            case ShapeshiftForm.FORM_DIREBEAR:
                {
                    return ManaTypes.TYPE_RAGE;
                }

            default:
                {
                    return manaType;
                }
        }
    }

    public bool CheckRequiredDbVersion(SQL thisDatabase, ServerDb thisServerDb)
    {
        DataTable mySqlQuery = new();
        // thisDatabase.Query(String.Format("SELECT column_name FROM information_schema.columns WHERE table_name='" & thisTableName & "'  AND TABLE_SCHEMA='" & thisDatabase.SQLDBName & "'"), mySqlQuery)
        thisDatabase.Query("SELECT `version`,`structure`,`content` FROM db_version ORDER BY VERSION DESC, structure DESC, content DESC LIMIT 0,1", ref mySqlQuery);
        // Check database version against code version

        var coreDbVersion = 0;
        var coreDbStructure = 0;
        var coreDbContent = 0;
        switch (thisServerDb)
        {
            case ServerDb.Realm:
                {
                    coreDbVersion = mangosGlobalConstants.RevisionDbRealmVersion;
                    coreDbStructure = mangosGlobalConstants.RevisionDbRealmStructure;
                    coreDbContent = mangosGlobalConstants.RevisionDbRealmContent;
                    break;
                }

            case ServerDb.Character:
                {
                    coreDbVersion = mangosGlobalConstants.RevisionDbCharactersVersion;
                    coreDbStructure = mangosGlobalConstants.RevisionDbCharactersStructure;
                    coreDbContent = mangosGlobalConstants.RevisionDbCharactersContent;
                    break;
                }

            case ServerDb.World:
                {
                    coreDbVersion = mangosGlobalConstants.RevisionDbMangosVersion;
                    coreDbStructure = mangosGlobalConstants.RevisionDbMangosStructure;
                    coreDbContent = mangosGlobalConstants.RevisionDbMangosContent;
                    break;
                }

            case 0:
                {
                    logger.Warning("Default switch fallback has occured with an error, data output: ThisServerDb {0}, CoreDbVersion {1}, CoreDbContent {2}, CoreDbVersion {3}", thisServerDb, coreDbVersion, coreDbContent, coreDbVersion);
                    break;
                }
        }

        if (mySqlQuery.Rows.Count > 0)
        {
            // For Each row As DataRow In mySqlQuery.Rows
            // dtVersion = row.Item("column_name").ToString
            // Next
            var dbVersion = mySqlQuery.Rows[0].As<int>("version");
            var dbStructure = mySqlQuery.Rows[0].As<int>("structure");
            var dbContent = mySqlQuery.Rows[0].As<int>("content");

            // NOTES: Version or Structure mismatch is a hard error, Content mismatch as a warning

            if (dbVersion == coreDbVersion && dbStructure == coreDbStructure && dbContent == coreDbContent) // Full Match
            {
                logger.Debug("[{0}] Db Version Matched", Strings.Format(DateAndTime.TimeOfDay, "hh:mm:ss"));
                return true;
            }

            if (dbVersion == coreDbVersion && dbStructure == coreDbStructure && dbContent != coreDbContent) // Content MisMatch, only a warning
            {
                logger.Warning("--------------------------------------------------------------");
                logger.Warning("-- WARNING: CONTENT VERSION MISMATCH                        --");
                logger.Warning("--------------------------------------------------------------");
                logger.Warning("Your Database " + thisDatabase.SQLDBName + " requires updating.");
                logger.Warning("You have: Rev{0}.{1}.{2}, however the core expects Rev{3}.{4}.{5}", dbVersion, dbStructure, dbContent, coreDbVersion, coreDbStructure, coreDbContent);
                logger.Warning("The server will run, but you may be missing some database fixes");
                return true;
            }

            logger.Error("--------------------------------------------------------------");
            logger.Error("-- FATAL ERROR: VERSION MISMATCH                            --");
            logger.Error("--------------------------------------------------------------");
            logger.Error("Your Database " + thisDatabase.SQLDBName + " requires updating.");
            logger.Error("You have: Rev{0}.{1}.{2}, however the core expects Rev{3}.{4}.{5}", dbVersion, dbStructure, dbContent, coreDbVersion, coreDbStructure, coreDbContent);
            logger.Error("The server is unable to run until the required updates are run");
            logger.Error("--------------------------------------------------------------");
            logger.Error("You must apply all updates after Rev{1}.{2}.{3} ", coreDbVersion, coreDbStructure, coreDbContent);
            logger.Error("These updates are included in the sql/updates folder.");
            logger.Error("--------------------------------------------------------------");
            return false;
        }

        logger.Debug("--------------------------------------------------------------");
        logger.Debug("The table `db_version` in database " + thisDatabase.SQLDBName + " is missing");
        logger.Debug("--------------------------------------------------------------");
        logger.Debug("MaNGOSVB cannot find the version info required, please update", "hh:mm:ss");
        logger.Debug("your database to check that the db is up to date.", "hh:mm:ss");
        logger.Debug("your database to Rev{0}.{1}.{2} ", coreDbVersion, coreDbStructure, coreDbContent);
        return false;
    }
}
