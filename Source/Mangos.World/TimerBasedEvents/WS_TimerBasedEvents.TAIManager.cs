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
using Mangos.World.Objects;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Mangos.World.Server;

public partial class WS_TimerBasedEvents
{
    public class TAIManager : IDisposable
    {
        public Timer AIManagerTimer;

        private bool AIManagerWorking;

        public const int UPDATE_TIMER = 1000;

        private bool _disposedValue;

        public TAIManager()
        {
            AIManagerTimer = null;
            AIManagerWorking = false;
            AIManagerTimer = new Timer(Update, null, 10000, 1000);
        }

        private void Update(object state)
        {
            if (AIManagerWorking)
            {
                return;
            }
            var StartTime = WorldServiceLocator._NativeMethods.timeGetTime("");
            AIManagerWorking = true;
            try
            {
                WorldServiceLocator._WorldServer.WORLD_TRANSPORTs_Lock.AcquireReaderLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
                foreach (var wORLD_TRANSPORT in WorldServiceLocator._WorldServer.WORLD_TRANSPORTs)
                {
                    wORLD_TRANSPORT.Value.Update();
                }
            }
            catch (Exception ex5)
            {
                ProjectData.SetProjectError(ex5);
                var ex4 = ex5;
                WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "Error updating transports.{0}{1}", Environment.NewLine, ex4.ToString());
                ProjectData.ClearProjectError();
            }
            finally
            {
                WorldServiceLocator._WorldServer.WORLD_TRANSPORTs_Lock.ReleaseReaderLock();
            }
            checked
            {
                try
                {
                    WorldServiceLocator._WorldServer.WORLD_CREATUREs_Lock.AcquireReaderLock(WorldServiceLocator._Global_Constants.DEFAULT_LOCK_TIMEOUT);
                    try
                    {
                        long num = WorldServiceLocator._WorldServer.WORLD_CREATUREsKeys.Count - 1;
                        for (var i = 0L; i <= num; i++)
                        {
                            if (WorldServiceLocator._WorldServer.WORLD_CREATUREs[Conversions.ToULong(WorldServiceLocator._WorldServer.WORLD_CREATUREsKeys[(int)i])] != null && WorldServiceLocator._WorldServer.WORLD_CREATUREs[Conversions.ToULong(WorldServiceLocator._WorldServer.WORLD_CREATUREsKeys[(int)i])].aiScript != null)
                            {
                                WorldServiceLocator._WorldServer.WORLD_CREATUREs[Conversions.ToULong(WorldServiceLocator._WorldServer.WORLD_CREATUREsKeys[(int)i])].aiScript.DoThink();
                            }
                        }
                    }
                    catch (Exception ex6)
                    {
                        ProjectData.SetProjectError(ex6);
                        var ex3 = ex6;
                        WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "Error updating AI.{0}{1}", Environment.NewLine, ex3.ToString());
                        ProjectData.ClearProjectError();
                    }
                    finally
                    {
                        WorldServiceLocator._WorldServer.WORLD_CREATUREs_Lock.ReleaseReaderLock();
                    }
                }
                catch (ApplicationException ex7)
                {
                    ProjectData.SetProjectError(ex7);
                    var ex2 = ex7;
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.WARNING, "Update: AI Manager timed out");
                    ProjectData.ClearProjectError();
                }
                catch (Exception ex8)
                {
                    ProjectData.SetProjectError(ex8);
                    var ex = ex8;
                    WorldServiceLocator._WorldServer.Log.WriteLine(LogType.CRITICAL, "Error updating AI.{0}{1}", Environment.NewLine, ex.ToString());
                    ProjectData.ClearProjectError();
                }
                AIManagerWorking = false;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                AIManagerTimer.Dispose();
                AIManagerTimer = null;
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
