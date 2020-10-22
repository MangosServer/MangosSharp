//
//  Copyright (C) 2013-2020 getMaNGOS <https://getmangos.eu>
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

using System.Threading.Tasks;
using Mangos.Loggers;

namespace Mangos.Configuration.Store
{
    public class StoredConfigurationProvider<T> : IConfigurationProvider<T>
    {
        private readonly ILogger logger;

        private readonly IConfigurationProvider<T> configurationProvider;
        private T _configuration;

        public StoredConfigurationProvider(ILogger logger, IConfigurationProvider<T> configurationProvider)
        {
            this.logger = logger;
            this.configurationProvider = configurationProvider;
        }

        public T GetConfiguration()
        {
            if (_configuration is null)
            {
                _configuration = configurationProvider.GetConfiguration();
            }
            return _configuration;
        }

        public async ValueTask<T> GetConfigurationAsync()
        {
            if (_configuration is null)
            {
                _configuration = await configurationProvider.GetConfigurationAsync();
                logger.Debug("Save configuration {0} in store", typeof(T).FullName);
            }
            return _configuration;
        }
    }
}