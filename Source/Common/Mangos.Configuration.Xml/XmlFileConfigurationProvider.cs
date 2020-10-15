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

using Mangos.Loggers;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Mangos.Configuration.Xml
{
    public class XmlFileConfigurationProvider<T> : IConfigurationProvider<T>
    {
        private readonly ILogger logger;

        private readonly string filePath;

        public XmlFileConfigurationProvider(ILogger logger, string filePath)
        {
            this.logger = logger;
            this.filePath = filePath;
        }

        public T GetConfiguration()
        {
            using var streamReader = new StreamReader(filePath);
            var xmlSerializer = new XmlSerializer(typeof(T));
            T configuration = (T)xmlSerializer.Deserialize(streamReader);
            return configuration;
        }

        public ValueTask<T> GetConfigurationAsync()
        {
            try
            {
                using var streamReader = new StreamReader(filePath);
                var xmlSerializer = new XmlSerializer(typeof(T));
                T configuration = (T)xmlSerializer.Deserialize(streamReader);
                logger.Debug("Get XML Configuration for {0}", typeof(T).FullName);
                return new ValueTask<T>(configuration);
            }
            catch(Exception ex)
            {
                logger.Error("Unable to load XML configuration for {0}", ex, typeof(T).FullName);
                throw;
            }
        }
    }
}