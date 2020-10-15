//
//  Copyright (C) 2013-2020 getMaNGOS <https:\\getmangos.eu>
//  
//  This program is free software. You can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation. either version 2 of the License, or
//  (at your option) any later version.
//  
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY. Without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//  
//  You should have received a copy of the GNU General Public License
//  along with this program. If not, write to the Free Software
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//

using System;
using Mangos.Common.Enums.Global;
using Microsoft.VisualBasic;

namespace Mangos.Cluster.DataStores
{
    public class WS_DBCLoad
    {
        public void InitializeInternalDatabase()
        {
            InitializeLoadDbCs();
            try
            {
                // Set all characters offline
                ClusterServiceLocator._WorldCluster.GetCharacterDatabase().Update("UPDATE characters SET char_online = 0;");
            }
            catch (Exception e)
            {
                ClusterServiceLocator._WorldCluster.Log.WriteLine(LogType.FAILED, "Internal database initialization failed! [{0}]{1}{2}", e.Message, Constants.vbCrLf, e.ToString());
            }
        }

        private void InitializeLoadDbCs()
        {
            ClusterServiceLocator._WS_DBCDatabase.InitializeMaps();
            ClusterServiceLocator._WS_DBCDatabase.InitializeChatChannels();
            ClusterServiceLocator._WS_DBCDatabase.InitializeBattlegrounds();
            ClusterServiceLocator._WS_DBCDatabase.InitializeWorldSafeLocs();
            ClusterServiceLocator._WS_DBCDatabase.InitializeCharRaces();
            ClusterServiceLocator._WS_DBCDatabase.InitializeCharClasses();
        }
    }
}