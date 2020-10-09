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
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.VisualBasic;

namespace Mangos.World.Server
{
    public class ReaderWriterLock_Debug : IDisposable
    {
        private readonly string ID;
        private readonly FileStream file;
        private readonly StreamWriter writer;
        private readonly ReaderWriterLock @lock;
        private readonly Queue<string> WriteQueue = new Queue<string>();

        public ReaderWriterLock_Debug(string s)
        {
            ID = s;
            file = new FileStream(string.Format("ReaderWriterLock_Debug_{0}_{1}.log", ID, DateAndTime.Now.Ticks), FileMode.Create);
            writer = new StreamWriter(file);
            @lock = new ReaderWriterLock();
            var st = new StackTrace();
            var sf = st.GetFrames();
            WriteLine("NewLock " + ID + " from:");
            foreach (StackFrame frame in sf)
                WriteLine(Constants.vbTab + frame.GetMethod().Name);
            WriteLine("NewLock " + ID);
            var writeThread = new Thread(WriteLoop) { Name = "WriteLoop, ReaderWriterLock_Debug - " + s };
            writeThread.Start();
        }

        public void AcquireReaderLock(int t)
        {
            var st = new StackTrace();
            var sf = st.GetFrames();
            WriteLine("AcquireReaderLock " + ID + " from:");
            foreach (StackFrame frame in sf)
                WriteLine(Constants.vbTab + frame.GetMethod().Name);
            @lock.AcquireReaderLock(t);
        }

        public void ReleaseReaderLock()
        {
            try
            {
                @lock.ReleaseReaderLock();
                var st = new StackTrace();
                var sf = st.GetFrames();
                WriteLine("ReleaseReaderLock " + ID + " from:");
                foreach (StackFrame frame in sf)
                    WriteLine(Constants.vbTab + frame.GetMethod().Name);
            }
            catch (Exception ex)
            {
                WriteLine("ReleaseReaderLock " + ID + " is not freed!");
            }
        }

        public void AcquireWriterLock(int t)
        {
            var st = new StackTrace();
            var sf = st.GetFrames();
            WriteLine("AcquireWriterLock " + ID + " from:");
            foreach (StackFrame frame in sf)
                WriteLine(Constants.vbTab + frame.GetMethod().Name);
            @lock.AcquireWriterLock(t);
        }

        public void ReleaseWriterLock()
        {
            try
            {
                @lock.ReleaseWriterLock();
                var st = new StackTrace();
                var sf = st.GetFrames();
                WriteLine("ReleaseWriterLock " + ID + " from:");
                foreach (StackFrame frame in sf)
                    WriteLine(Constants.vbTab + frame.GetMethod().Name);
            }
            catch (Exception ex)
            {
                WriteLine("ReleaseWriterLock " + ID + " is not freed!");
            }
        }

        public bool IsWriterLockHeld()
        {
            return @lock.IsWriterLockHeld;
        }

        public bool IsReaderLockHeld()
        {
            return @lock.IsReaderLockHeld;
        }

        public void WriteLine(string str)
        {
            lock (WriteQueue)
                WriteQueue.Enqueue(str);
        }

        private void WriteLoop()
        {
            string str = "";
            while (true)
            {
                int i = 0;
                while (WriteQueue.Count > 0)
                {
                    lock (WriteQueue)
                        str = WriteQueue.Dequeue();
                    writer.WriteLine(str);
                    i += 1;
                }

                if (i > 0)
                    writer.Flush();
                Thread.Sleep(100);
            }
        }

        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        private bool _disposedValue; // To detect redundant calls

        // IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                // TODO: set large fields to null.
                writer.Dispose();
                file.Dispose();
            }

            _disposedValue = true;
        }

        // TODO: override Finalize() only if Dispose(ByVal disposing As Boolean) above has code to free unmanaged resources.
        // Protected Overrides Sub Finalize()
        // ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
        // Dispose(False)
        // MyBase.Finalize()
        // End Sub

        // This code added by Visual Basic to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
    }
}