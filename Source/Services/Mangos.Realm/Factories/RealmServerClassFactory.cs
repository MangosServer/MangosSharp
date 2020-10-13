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

using global;
using Mangos.Configuration;
using Mangos.Loggers;

namespace Mangos.Realm.Factories
{
    public class RealmServerClassFactory
    {
        private readonly Global_Constants _Global_Constants;
        private readonly ClientClassFactory _ClientClassFactory;
        private readonly IConfigurationProvider<RealmServerConfiguration> configurationProvider;
		private readonly ILogger logger;

		public RealmServerClassFactory(
			Global_Constants globalConstants,
			ClientClassFactory clientClassFactory,
			IConfigurationProvider<RealmServerConfiguration> configurationProvider, 
			ILogger logger)
		{
			_Global_Constants = globalConstants;
			_ClientClassFactory = clientClassFactory;
			this.configurationProvider = configurationProvider;
			this.logger = logger;
		}

		public RealmServerClass Create(RealmServer realmServer)
        {
            return new RealmServerClass(_Global_Constants, _ClientClassFactory, realmServer, configurationProvider, logger);
        }
    }
}