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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.World.Server
{
	public class ReaderWriterLock_Debug : IDisposable
	{
		private readonly string ID;

		private readonly FileStream file;

		private readonly StreamWriter writer;

		private readonly ReaderWriterLock @lock;

		private readonly Queue<string> WriteQueue;

		private bool _disposedValue;

		public ReaderWriterLock_Debug(string s)
		{
			WriteQueue = new Queue<string>();
			ID = s;
			file = new FileStream($"ReaderWriterLock_Debug_{ID}_{DateAndTime.Now.Ticks}.log", FileMode.Create);
			writer = new StreamWriter(file);
			@lock = new ReaderWriterLock();
			StackTrace st = new StackTrace();
			StackFrame[] sf = st.GetFrames();
			WriteLine("NewLock " + ID + " from:");
			StackFrame[] array = sf;
			foreach (StackFrame frame in array)
			{
				WriteLine("\t" + frame.GetMethod()!.Name);
			}
			WriteLine("NewLock " + ID);
			Thread writeThread = new Thread(new ThreadStart(WriteLoop))
			{
				Name = "WriteLoop, ReaderWriterLock_Debug - " + s
			};
			writeThread.Start();
		}

		public void AcquireReaderLock(int t)
		{
			StackTrace st = new StackTrace();
			StackFrame[] sf = st.GetFrames();
			WriteLine("AcquireReaderLock " + ID + " from:");
			StackFrame[] array = sf;
			foreach (StackFrame frame in array)
			{
				WriteLine("\t" + frame.GetMethod()!.Name);
			}
			@lock.AcquireReaderLock(t);
		}

		public void ReleaseReaderLock()
		{
			try
			{
				@lock.ReleaseReaderLock();
				StackTrace st = new StackTrace();
				StackFrame[] sf = st.GetFrames();
				WriteLine("ReleaseReaderLock " + ID + " from:");
				StackFrame[] array = sf;
				foreach (StackFrame frame in array)
				{
					WriteLine("\t" + frame.GetMethod()!.Name);
				}
			}
			catch (Exception ex2)
			{
				ProjectData.SetProjectError(ex2);
				Exception ex = ex2;
				WriteLine("ReleaseReaderLock " + ID + " is not freed!");
				ProjectData.ClearProjectError();
			}
		}

		public void AcquireWriterLock(int t)
		{
			StackTrace st = new StackTrace();
			StackFrame[] sf = st.GetFrames();
			WriteLine("AcquireWriterLock " + ID + " from:");
			StackFrame[] array = sf;
			foreach (StackFrame frame in array)
			{
				WriteLine("\t" + frame.GetMethod()!.Name);
			}
			@lock.AcquireWriterLock(t);
		}

		public void ReleaseWriterLock()
		{
			try
			{
				@lock.ReleaseWriterLock();
				StackTrace st = new StackTrace();
				StackFrame[] sf = st.GetFrames();
				WriteLine("ReleaseWriterLock " + ID + " from:");
				StackFrame[] array = sf;
				foreach (StackFrame frame in array)
				{
					WriteLine("\t" + frame.GetMethod()!.Name);
				}
			}
			catch (Exception ex2)
			{
				ProjectData.SetProjectError(ex2);
				Exception ex = ex2;
				WriteLine("ReleaseWriterLock " + ID + " is not freed!");
				ProjectData.ClearProjectError();
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
			{
				WriteQueue.Enqueue(str);
			}
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
					{
						str = WriteQueue.Dequeue();
					}
					writer.WriteLine(str);
					i = checked(i + 1);
				}
				if (i > 0)
				{
					writer.Flush();
				}
				Thread.Sleep(100);
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposedValue)
			{
				if (disposing)
				{
				}
				writer.Dispose();
				file.Dispose();
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
			this.Dispose();
		}
	}
}
