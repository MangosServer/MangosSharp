namespace Mangos.Loggers
{
	public interface ILogger
    {
		void Debug(string format, params object[] args);

		void Message(string format, params object[] args);

		void Warning(string format, params object[] args);

		void Error(string format, params object[] args);
	}
}
