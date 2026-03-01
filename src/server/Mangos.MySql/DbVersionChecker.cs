//
// Copyright (C) 2013-2025 getMaNGOS <https://www.getmangos.eu>
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
using Mangos.Common.Globals;
using Mangos.Logging;
using System;
using System.Data;

namespace Mangos.MySql;

public class DbVersionChecker
{
    private readonly MangosGlobalConstants mangosGlobalConstants;
    private readonly IMangosLogger logger;

    public DbVersionChecker(IMangosLogger logger, MangosGlobalConstants mangosGlobalConstants)
    {
        this.logger = logger;
        this.mangosGlobalConstants = mangosGlobalConstants;
    }

    public bool CheckRequiredDbVersion(SQL thisDatabase, ServerDb thisServerDb)
    {
        logger.Information($"[DB] Checking database version for {thisServerDb} database '{thisDatabase.SQLDBName}'");
        logger.Debug($"[DB] Querying db_version table in '{thisDatabase.SQLDBName}'");
        DataTable mySqlQuery = new();
        thisDatabase.Query("SELECT `version`,`structure`,`content` FROM db_version ORDER BY VERSION DESC, structure DESC, content DESC LIMIT 0,1", ref mySqlQuery);
        logger.Debug($"[DB] db_version query returned {mySqlQuery.Rows.Count} row(s)");

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
                    logger.Debug($"[DB] Expected Realm DB version: Rev{coreDbVersion}.{coreDbStructure}.{coreDbContent}");
                    break;
                }

            case ServerDb.Character:
                {
                    coreDbVersion = mangosGlobalConstants.RevisionDbCharactersVersion;
                    coreDbStructure = mangosGlobalConstants.RevisionDbCharactersStructure;
                    coreDbContent = mangosGlobalConstants.RevisionDbCharactersContent;
                    logger.Debug($"[DB] Expected Character DB version: Rev{coreDbVersion}.{coreDbStructure}.{coreDbContent}");
                    break;
                }

            case ServerDb.World:
                {
                    coreDbVersion = mangosGlobalConstants.RevisionDbMangosVersion;
                    coreDbStructure = mangosGlobalConstants.RevisionDbMangosStructure;
                    coreDbContent = mangosGlobalConstants.RevisionDbMangosContent;
                    logger.Debug($"[DB] Expected World DB version: Rev{coreDbVersion}.{coreDbStructure}.{coreDbContent}");
                    break;
                }

            case 0:
                {
                    logger.Warning(string.Format("[DB] Default switch fallback has occured with an error, data output: ThisServerDb {0}, CoreDbVersion {1}, CoreDbContent {2}, CoreDbVersion {3}", thisServerDb, coreDbVersion, coreDbContent, coreDbVersion));
                    break;
                }
        }

        if (mySqlQuery.Rows.Count > 0)
        {
            var dbVersion = mySqlQuery.Rows[0].As<int>("version");
            var dbStructure = mySqlQuery.Rows[0].As<int>("structure");
            var dbContent = mySqlQuery.Rows[0].As<int>("content");

            if (dbVersion == coreDbVersion && dbStructure == coreDbStructure && dbContent == coreDbContent)
            {
                logger.Trace(string.Format("[{0}] Db Version Matched", DateTime.Now.ToString("hh:mm:ss")));
                return true;
            }

            if (dbVersion == coreDbVersion && dbStructure == coreDbStructure && dbContent != coreDbContent)
            {
                logger.Warning("--------------------------------------------------------------");
                logger.Warning("-- WARNING: CONTENT VERSION MISMATCH                        --");
                logger.Warning("--------------------------------------------------------------");
                logger.Warning("Your Database " + thisDatabase.SQLDBName + " requires updating.");
                logger.Warning(string.Format("You have: Rev{0}.{1}.{2}, however the core expects Rev{3}.{4}.{5}", dbVersion, dbStructure, dbContent, coreDbVersion, coreDbStructure, coreDbContent));
                logger.Warning("The server will run, but you may be missing some database fixes");
                return true;
            }

            logger.Error("--------------------------------------------------------------");
            logger.Error("-- FATAL ERROR: VERSION MISMATCH                            --");
            logger.Error("--------------------------------------------------------------");
            logger.Error("Your Database " + thisDatabase.SQLDBName + " requires updating.");
            logger.Error(string.Format("You have: Rev{0}.{1}.{2}, however the core expects Rev{3}.{4}.{5}", dbVersion, dbStructure, dbContent, coreDbVersion, coreDbStructure, coreDbContent));
            logger.Error("The server is unable to run until the required updates are run");
            logger.Error("--------------------------------------------------------------");
            logger.Error(string.Format("You must apply all updates after Rev{1}.{2}.{3} ", coreDbVersion, coreDbStructure, coreDbContent));
            logger.Error("These updates are included in the sql/updates folder.");
            logger.Error("--------------------------------------------------------------");
            return false;
        }

        logger.Trace("--------------------------------------------------------------");
        logger.Trace("The table `db_version` in database " + thisDatabase.SQLDBName + " is missing");
        logger.Trace("--------------------------------------------------------------");
        logger.Trace(string.Format("MaNGOSVB cannot find the version info required, please update"));
        logger.Trace(string.Format("your database to check that the db is up to date."));
        logger.Trace(string.Format("your database to Rev{0}.{1}.{2} ", coreDbVersion, coreDbStructure, coreDbContent));
        return false;
    }
}
