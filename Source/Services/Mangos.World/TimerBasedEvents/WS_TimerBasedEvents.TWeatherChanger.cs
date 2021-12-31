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

using Mangos.Common.Enums.Global;
using Mangos.World.Weather;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Mangos.World.Server;

public partial class WS_TimerBasedEvents
{
    public class TWeatherChanger : IDisposable
    {
        public Timer WeatherTimer;

        private bool WeatherWorking;

        public int UPDATE_TIMER;

        private bool _disposedValue;

        public TWeatherChanger()
        {
            WeatherTimer = null;
            WeatherWorking = false;
            UPDATE_TIMER = WorldServiceLocator._ConfigurationProvider.GetConfiguration().WeatherTimer;
            WeatherTimer = new Timer(Update, null, 10000, UPDATE_TIMER);
        }

        private void Update(object state)
        {
            if (WeatherWorking)
            {
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "Update: Weather changer skipping update");
                return;
            }
            WeatherWorking = true;
            foreach (var weatherZone in WorldServiceLocator._WS_Weather.WeatherZones)
            {
                weatherZone.Value.Update();
            }
            WeatherWorking = false;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                WeatherTimer.Dispose();
                WeatherTimer = null;
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
            Dispose();
        }
    }
}
