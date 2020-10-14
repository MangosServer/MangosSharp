using System;

namespace Mangos.Loggers.Console
{
	public class ConsoleLogger : ILogger
	{
		private static readonly object _lockObject = new object();

		public void Debug(string format, params object[] args)
		{
			Write(ConsoleColor.Gray, format, args);
		}

		public void Message(string format, params object[] args)
		{
			Write(ConsoleColor.White, format, args);
		}

		public void Warning(string format, params object[] args)
		{
			Write(ConsoleColor.Yellow, format, args);
		}

		public void Error(string format, params object[] args)
		{
			Write(ConsoleColor.Red, format, args);
		}

		private void Write(ConsoleColor color, string format, object[] args)
		{
			lock (_lockObject)
			{
				System.Console.ForegroundColor = color;
				System.Console.WriteLine(Format(format, args));
			}
		}

		private string Format(string format, object[] args)
		{
			return string.Format("[{0}] {1}", DateTime.Now, string.Format(format, args));
		}
	}
}